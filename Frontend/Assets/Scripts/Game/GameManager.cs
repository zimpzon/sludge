using Assets.Scripts.Game;
using DG.Tweening;
using Sludge;
using Sludge.Colors;
using Sludge.PlayerInputs;
using Sludge.Shared;
using Sludge.SludgeObjects;
using Sludge.UI;
using Sludge.Utility;
using System.Collections;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

// First script to run
public class GameManager : MonoBehaviour
{
    // Level switching:
    // LoadLevel() is the only way in
    // StartLevel() resets and starts what was loaded.

    public static PlayerSample[] PlayerSamples = new PlayerSample[30000];

    public static readonly string Version = "0.1b";

    public const double TickSize = 0.008;
    public const int TickSizeMs = 8;
    public const double TicksPerSecond = 1000.0 / TickSizeMs;

    public Vector3 PlayerLandStartOffset = new Vector3(25, -16);
    public float PlayerLandRotationSpeed = 500;
    public float PlayerLandDuration = 0.5f;
    public float PlayerLandMaxScaleAdd = 1;

    public Transform CameraRoot;
    public Tilemap Tilemap;
    public Tilemap PillTilemap;
    public ColorSchemeScriptableObject CurrentColorScheme;
    public ColorSchemeScriptableObject CurrentUiColorScheme;
    public ColorSchemeListScriptableObject ColorSchemeList;

    public GameObject RoundsActionPanel;
    public TextMeshProUGUI TextRoundsAction;
    public Image GoldScoreRoundsAction;

    public ParticleSystem DustParticles;
    public ParticleSystem CompletedParticles;
    public ParticleSystem MarkerParticles;

    public TMP_Text TextTimer;
    public TMP_Text TextLevelName;

    public Material OutlineMaterial;

    public static string ClientId;
    public static PlayerInput PlayerInput;
    public static GameManager I;

    public Player Player;
    public SludgeObject[] SludgeObjects;
    public SlimeBomb[] SlimeBombs;
    public ParticleSystem[] SlimeBombsHighlight;

    public double UnityTime;
    public double EngineTime;
    public int EngineTimeMs;
    public int FrameCounter;
    public int Keys;
    LevelData currentLevelData = new LevelData();
    UiLevel currentUiLevel;
    LevelElements levelElements;
    LevelSettings levelSettings;
    bool levelComplete;
    RoundResult latestRoundResult;

    void Awake()
    {
        I = this;
        SetDefaultColorScheme();

        Startup.StaticInit();
        PlayerInput = new PlayerInput();
        levelElements = (LevelElements)Resources.FindObjectsOfTypeAll(typeof(LevelElements)).First();
        levelSettings = (LevelSettings)Resources.FindObjectsOfTypeAll(typeof(LevelSettings)).First();
        Player = FindFirstObjectByType<Player>();
    }

    public void KillEnemy(GameObject goEnemy)
    {
        SoundManager.Play(FxList.Instance.EnemyDie);
        DustParticles.transform.position = goEnemy.transform.position;
        DustParticles.Emit(4);

        // If enemy implements IEnemy kill it using that (can customize death). Else just disable the whole go.
        var iEnemy = goEnemy.GetComponent<IEnemy>();
        if (iEnemy != null)
        {
            iEnemy.Kill();
            return;
        }

        SludgeUtil.SetActiveRecursive(goEnemy, false);
    }

    public void StartLevel()
    {
        StartCoroutine(BetweenRoundsLoop());
    }

    // levelSelectedFromLevelSelect is NULL when starting a level directly in editor, else the level selected in menus.
    public void LoadLevel(UiLevel levelSelectedFromLevelSelect)
    {
        var levelData = levelSelectedFromLevelSelect?.LevelData;

        // Total hack: The player dies if the new level has a collider at his OLD start position. The same thing could happen to other objects sensitive to collision!
        Tilemap.gameObject.SetActive(false);

        if (levelData != null)
        {
            LevelDeserializer.Run(levelData, levelElements, levelSettings);
            currentLevelData = levelData;
            currentUiLevel = levelSelectedFromLevelSelect;
            TextLevelName.text = levelData.LevelName;
        }
        else
        {
            // Starting game from current scene in editor
            TextLevelName.text = "(started from editor)";

            SludgeObjects = FindObjectsByType<SludgeObject>(FindObjectsSortMode.None);

            // Simulate level load when starting directly from editor
            foreach (var obj in SludgeObjects)
            {
                foreach (var modifier in obj.Modifiers)
                    modifier.OnLoaded();
            }
        }

        Player.SetHomePosition();

        SludgeObjects = FindObjectsByType<SludgeObject>(FindObjectsSortMode.None);
        SlimeBombs = SludgeObjects.Where(o => o is SlimeBomb).Cast<SlimeBomb>().ToArray();
        SlimeBombsHighlight = SlimeBombs.Select(b => b.transform.Find("HighlightParticles").GetComponent<ParticleSystem>()).ToArray();

        PillTilemap.gameObject.GetComponent<PillSnapshot>().Push();

        ResetLevel();

        Tilemap.gameObject.SetActive(true);
    }

    void GoToNextLevel()
    {
        StopAllCoroutines();

        currentUiLevel = currentUiLevel.Next;
        if (currentUiLevel.LevelData.Namespace == PlayerProgress.LevelNamespace.Casual)
        {
            UiLogic.Instance.lastSelectedCasualLevelId = currentUiLevel.LevelData.LevelId;
        }
        else if (currentUiLevel.LevelData.Namespace == PlayerProgress.LevelNamespace.Hard)
        {
            UiLogic.Instance.lastSelectedHardLevelId = currentUiLevel.LevelData.LevelId;
        }
        else
        {
            Debug.LogError("cannot change level without a namespace");
            return;
        }

        LoadLevel(currentUiLevel);
        StartLevel();
    }

    bool CanGoToNextLevel()
    {
        bool wasStartedFromEditor = currentLevelData.Namespace == PlayerProgress.LevelNamespace.NotSet;
        return wasStartedFromEditor ? false : PlayerProgress.IsLevelCompleted(currentLevelData.Namespace, currentLevelData.LevelId);
    }

    StringBuilder betweenRoundsSb = new StringBuilder();
    void ShowBetweenRoundsActionsText(bool show)
    {
        RoundsActionPanel.SetActive(show);
        if (!show)
            return;

        RectTransform panel = RoundsActionPanel.GetComponent<RectTransform>();
        panel.DOKill(complete: true);
        panel.anchoredPosition = new Vector2(-125f, panel.anchoredPosition.y); // Start slightly off-screen to the left
        panel.DOAnchorPosX(1f, 0.1f).SetEase(Ease.InSine); // Animate to visible position with a bounce

        var savedStats = PlayerProgress.GetSavedStats(currentLevelData.Namespace, currentLevelData.LevelId);
        bool canGoToNextLevel = CanGoToNextLevel();

        string timePart = latestRoundResult.Completed ? $"{latestRoundResult.Time,6:0.000}" : "     -";
        string bestPart = savedStats.BestTime >= 0 ? $"{savedStats.BestTime,6:0.000}" : "     -";
        betweenRoundsSb.Clear();
        betweenRoundsSb.AppendLine($"Time\t{timePart}");
        betweenRoundsSb.AppendLine($"Best\t{bestPart}");
        betweenRoundsSb.AppendLine($"Gold\t{currentLevelData.TargetTime,6:0.000}");
        betweenRoundsSb.AppendLine($"Attempts\t{savedStats.Attempts,6}");
        betweenRoundsSb.AppendLine();
        betweenRoundsSb.AppendLine("Retry\tMove");
        betweenRoundsSb.AppendLine($"Next\t{(canGoToNextLevel ? "Select btn" : "<locked>")}");
        betweenRoundsSb.AppendLine("Menu\tBack btn");
        betweenRoundsSb.AppendLine("Reset\tBack btn");
        TextRoundsAction.SetText(betweenRoundsSb);
        if (currentUiLevel != null)
            GoldScoreRoundsAction.enabled = currentUiLevel.HasGoldTime;
    }

    IEnumerator BetweenRoundsLoop(string replayId = null)
    {
        int attempts = 0;
        bool lastRoundCancelled = false;
        bool abort = false;

        ShowBetweenRoundsActionsText(true);
        UpdateTimer(-1);

        while (true)
        {
            bool startRound = false;

            ResetLevel();

            yield return RevealPlayer(landing: false);

            while (startRound == false)
            {
                PlayerInput.GetHumanInput();

                bool startPlaying = PlayerInput.Up > 0 || PlayerInput.Down > 0 || PlayerInput.Left > 0 || PlayerInput.Right > 0 || PlayerInput.JumpActive();
                if (startPlaying)
                {
                    startRound = true;
                    PlayerInput.ClearState(); // Make sure starting round with a tap jump will "eat" the tap when round stars. Eg. we want to start with a jump in that case.
                }

                if (PlayerInput.IsTapped(PlayerInput.InputType.Select) && CanGoToNextLevel())
                {
                    GoToNextLevel();
                    abort = true;
                    break;
                }

                if (PlayerInput.IsTapped(PlayerInput.InputType.Back))
                {
                    ShowBetweenRoundsActionsText(show: false);
                    UiLogic.Instance.BackFromGame();
                    StopAllCoroutines();
                }

                yield return null;
            }
            ShowBetweenRoundsActionsText(false);

            if (abort)
                break;

            yield return Playing();
            attempts++;
            lastRoundCancelled = latestRoundResult.Cancelled;

            ShowBetweenRoundsActionsText(show: true);

            if (!lastRoundCancelled)
            {
                float afterRoundDelay = latestRoundResult.Completed ? 1.5f : 1.0f;
                yield return new WaitForSeconds(afterRoundDelay);
            }
        }
    }

    IEnumerator RevealPlayer(bool landing)
    {
        Player.DisableCollisions(true);

        float t = 1.0f;

        Vector3 targetPos = Player.transform.position;
        Vector3 baseScale = Player.transform.localScale;
        Quaternion baseRotation = Player.transform.rotation;

        if (landing)
        {
            SoundManager.Play(FxList.Instance.PlayerLanding);
            ShakeCamera(PlayerLandDuration, strength: 0.5f);

            while (t >= 0)
            {
                Vector3 pos = targetPos + PlayerLandStartOffset * t;
                Player.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, 0, t * PlayerLandRotationSpeed + baseRotation.eulerAngles.z));
                Player.transform.localScale = baseScale + Vector3.one * PlayerLandMaxScaleAdd * t;
                Player.SetAlpha(Mathf.Clamp01(1.0f - (t * 2.0f)));

                t -= Time.deltaTime / PlayerLandDuration;
                yield return null;
            }

            SoundManager.Play(FxList.Instance.PlayerLanded);
            ShakeCamera(duration: 0.5f, strength: 0.5f);
        }
        else
        {
            // not landing, just showing up
        }

        Player.SetAlpha(1.0f);
        Player.transform.SetPositionAndRotation(targetPos, Quaternion.identity);
        Player.transform.localScale = baseScale;
        Player.transform.rotation = baseRotation;

        Player.DisableCollisions(false);
    }

    Tweener cameraTweener;
    public void ShakeCamera(float duration, float strength)
    {
        if (cameraTweener != null)
            cameraTweener.Kill(complete: true);

        cameraTweener = CameraRoot.DOShakePosition(duration, strength);
    }

    public void OnPillEaten()
    {
    }

    public void UpdateTimer(float time)
    {
        if (time < 0)
            TextTimer.SetText("0.000 sec", time);
        else
            TextTimer.SetText("{0:0.000} sec", time);
    }

    void ResetLevel()
    {
        EngineTime = 0;
        EngineTimeMs = 0;
        FrameCounter = 0;
        UnityTime = 0;
        latestRoundResult = new RoundResult();

        levelComplete = false;
        Keys = 0;

        PillManager.Reset(PillTilemap.gameObject.GetComponent<PillSnapshot>().TotalPills);

        var pickupSequences = SludgeObjects.Where(o => o is PickupSequence).Cast<PickupSequence>().ToList();
        PickupSequenceManager.Reset(pickupSequences);

        BulletManager.Instance.Reset();
        CellAntManager.Instance.Reset();

        LevelCells.Instance.UpdateFrom(Tilemap);

        PillTilemap.gameObject.GetComponent<PillSnapshot>().Pop();

        for (int i = 0; i < SludgeObjects.Length; ++i)
            SludgeUtil.SetActiveRecursive(SludgeObjects[i].gameObject, true);

        for (int i = 0; i < SludgeObjects.Length; ++i)
            SludgeObjects[i].Reset();

        Player.Prepare();
        Player.SetAlpha(0.0f);

        UpdateSludgeObjects();
    }

    public void OnActivatingBomb()
    {
        //SetHighlightedObjects(bombActivated: true);
    }

    IEnumerator Playing()
    {
        SoundManager.Play(FxList.Instance.StartRound);
        Player.RoundStartTime = Time.time;

        while (Player.Alive)
        {
            UnityTime += Time.deltaTime;

            while (EngineTime <= UnityTime)
            {
                PlayerInput.GetHumanInput();
                DoTick();
            }

            UpdateTimer((float)EngineTime);

            if (levelComplete)
                break;

            if (PlayerInput.BackActive() || PlayerInput.RestartKey())
            {
                latestRoundResult.Cancelled = true;
                yield break;
            }

            yield return null;
        }

        latestRoundResult.Time = (float)EngineTime;
        UpdateTimer(latestRoundResult.Time);
        latestRoundResult.Completed = levelComplete;
        latestRoundResult.LevelNamespace = UiLogic.Instance.latestSelectedLevelNamespace;
        latestRoundResult.LevelId = UiLogic.Instance.latestSelectedLevelNamespace == PlayerProgress.LevelNamespace.Casual ?
            UiLogic.Instance.lastSelectedCasualLevelId : UiLogic.Instance.lastSelectedHardLevelId;

        // Check for new best and new gold score
        var savedStats = PlayerProgress.GetSavedStats(currentLevelData.Namespace, currentLevelData.LevelId);
        bool hadGoldScoreBefore = currentUiLevel?.HasGoldTime ?? false; // If started from editor
        savedStats = PlayerProgress.UpdateWithRoundResult(latestRoundResult, out bool newBestTime);

        bool gotGoldScore = latestRoundResult.Completed && latestRoundResult.Time <= currentLevelData.TargetTime;
        bool gotFirstGoldScore = gotGoldScore && !hadGoldScoreBefore;

        var scoreDisplaySb = new StringBuilder();
        if (gotFirstGoldScore)
        {
            // First gold for this level
            if (currentUiLevel != null)
                currentUiLevel.HasGoldTime = true;
            Debug.Log("First gold for this level");
            scoreDisplaySb.AppendLine("Gold score!");
        } else if (gotGoldScore)
        {
            // Gold score but not for the first time on this level
            Debug.Log("Gold, but not first");
            scoreDisplaySb.AppendLine("Gold score!");
        }

        if (newBestTime)
        {
            // New personal best for this level
            Debug.Log("New personal best time");
            scoreDisplaySb.AppendLine("New personal best!");
        }

        if (scoreDisplaySb.Length > 0)
        {
            var exitLogic = FindFirstObjectByType<ModExitLogic>();
            exitLogic.SetScoreText(scoreDisplaySb.ToString());
        }
        else
        {
            // Completed, but nothing special happened
            //var exitLogic = FindFirstObjectByType<ModExitLogic>();
            //exitLogic.SetScoreText("testing");
        }

        if (latestRoundResult.Completed)
        {
            SoundManager.Play(FxList.Instance.LevelComplete);
        }
        else
        {
            // dead, did not complete level
        }

        // TODO: (pwe) update playfab here?
    }

    public void LevelCompleted()
    {
        var pos = Player.transform.position;
        CompletedParticles.transform.position = pos;
        CompletedParticles.Emit(30);

        MarkerParticles.transform.position = pos;
        MarkerParticles.Emit(30);

        levelComplete = true;
    }

    public void KeyPickup(Key key)
    {
        DustParticles.transform.position = key.transform.position;
        DustParticles.Emit(1);
        SoundManager.Play(FxList.Instance.KeyPickup);
        Keys++;
    }

    void UpdateAll()
    {
        UpdatePlayer();
        UpdateSludgeObjects();
        BulletManager.Instance.EngineTick();
        CellAntManager.Instance.EngineTick();
    }

    void UpdateSludgeObjects()
    {
        for (int i = 0; i < SludgeObjects.Length; ++i)
        {
            if (SludgeObjects[i].gameObject.activeSelf)
                SludgeObjects[i].EngineTick();
        }
    }

    void UpdatePlayer()
    {
        Player.EngineTick();
    }

    private void OnValidate()
    {
        SetDefaultColorScheme();
    }

    private void SetDefaultColorScheme()
    {
        if (I is null)
            return;

        var defaultScheme = ColorSchemeList.ColorSchemes.Where(s => s.IsDefault).First();
        if (defaultScheme == null)
        {
            Debug.LogError("No default color scheme found, missing one with Default = true");
        }
        SetColorScheme(defaultScheme);
    }

    public void SetColorScheme(ColorSchemeScriptableObject colorScheme)
    {
        if (colorScheme == null)
        {
            Debug.LogError("Trying set set NULL color scheme");
            return;
        }

        Debug.Log($"Setting color scheme: [{colorScheme.name}]");
        ColorScheme.ApplyColors(colorScheme);
    }

    private void CheckChangeColorScheme(PlayerInput input)
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            SetColorScheme(ColorSchemeList.GetNext());
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            SetColorScheme(ColorSchemeList.GetPrev());
        }
    }

    private void Update()
    {
        //DebugLinesScript.Instance.SetLine("currentLevelData.Namespace", currentLevelData.Namespace);
        //DebugLinesScript.Instance.SetLine("UiLogic.Instance.lastSelectedCasualLevelId", UiLogic.Instance.lastSelectedCasualLevelId);
        //DebugLinesScript.Instance.SetLine("UiLogic.Instance.lastSelectedHardLevelId", UiLogic.Instance.lastSelectedHardLevelId);

        CheckChangeColorScheme(PlayerInput);

        // Out of Tweens: search for TODO TWEEN to eventually replace later.
        //DebugLinesScript.Instance.SetLine("TotalPlayingTweens", DOTween.TotalPlayingTweens());
    }

    void DoTick()
    {
        Player.Position = Player.transform.position;

        EngineTimeMs = FrameCounter * TickSizeMs;
        EngineTime = EngineTimeMs * 0.001;

        UpdateAll();

        FrameCounter++;

        Physics2D.Simulate((float)TickSize);
    }
}

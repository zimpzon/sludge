using Sludge;
using Sludge.Colors;
using Sludge.PlayerInputs;
using Sludge.Replays;
using Sludge.Shared;
using Sludge.SludgeObjects;
using Sludge.UI;
using Sludge.Utility;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

// First script to run
public class GameManager : MonoBehaviour
{
    public static PlayerSample[] PlayerSamples = new PlayerSample[30000];

    public static readonly string Version = "0.1b";

    const double TimePillBonusTime = -1.0;
    public const double TickSize = 0.016;
    public const int TickSizeMs = 16;
    public const double TicksPerSecond = 1000.0 / TickSizeMs;

    public Transform CameraRoot;
    public Image TimeBarLeft;
    public Image TimeBarRight;
    public Tilemap Tilemap;
    public Tilemap PillTilemap;
    public ColorSchemeScriptableObject CurrentColorScheme;
    public ColorSchemeScriptableObject CurrentUiColorScheme;
    public ColorSchemeListScriptableObject ColorSchemeList;

    public GameObject ButtonStartRound;
    public GameObject ButtonWatchReplay;
    public GameObject ButtonGoToNextLevel;

    public ParticleSystem DeathParticles;
    public ParticleSystem DustParticles;
    public ParticleSystem CompletedParticles;
    public ParticleSystem MarkerParticles;

    public TMP_Text TextLevelStatus;

    public TMP_Text TextLevelTime;
    public TMP_Text TextLevelName;
    public TMP_Text TextLevelMasterTime;
    public Material OutlineMaterial;

    public static string ClientId;
    public static PlayerInput PlayerInput;
    public static LevelReplay LevelReplay = new LevelReplay();
    public static GameManager Instance;

    public Player Player;
    public SludgeObject[] SludgeObjects;
    public SlimeBomb[] SlimeBombs;
    public ParticleSystem[] SlimeBombsHighlight;
    public Exit[] Exits;
    public ParticleSystem[] ExitsHighlight;

    public bool IsReplay;
    public double UnityTime;
    public double EngineTime;
    public int EngineTimeMs;
    public int FrameCounter;
    public int Keys;
    LevelData currentLevelData = new LevelData { TimeSeconds = 30, EliteCompletionTimeSeconds = 20, };
    PlayerProgress.LevelProgress currentLevelProgress = new PlayerProgress.LevelProgress();
    UiLevel currentUiLevel;
    bool levelJustMastered;
    double roundTime;
    LevelElements levelElements;
    LevelSettings levelSettings;
    bool levelComplete;
    RoundResult latestRoundResult;

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        SetColorScheme(CurrentColorScheme);
    }

    void Awake()
    {
        Instance = this;
        Startup.StaticInit();
        PlayerInput = new PlayerInput();
        levelElements = (LevelElements)Resources.FindObjectsOfTypeAll(typeof(LevelElements)).First();
        levelSettings = (LevelSettings)Resources.FindObjectsOfTypeAll(typeof(LevelSettings)).First();
        Player = FindObjectOfType<Player>();

        OnValidate();
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

    public void SetColorScheme(ColorSchemeScriptableObject scheme)
    {
        CurrentColorScheme = scheme;
        ColorScheme.ApplyColors(scheme);
        ColorScheme.ApplyUiColors(scheme);
    }

    // Level switching:
    // LoadLevel() is the only way in
    // StartLevel() resets and starts what was loaded.

    public void StartLevel()
    {
        StartCoroutine(BetweenRoundsLoop());
    }

    public void LoadLevel(UiLevel uiLevel)
    {
        var levelData = uiLevel?.LevelData;

        // Total hack: The player dies if the new level has a collider at his OLD start position. The same thing could happen to other objects sensitive to collision!
        Tilemap.gameObject.SetActive(false);

        if (levelData != null)
        {
            Debug.Log($"Loading level: {levelData.Name}");
            LevelDeserializer.Run(levelData, levelElements, levelSettings);
            currentLevelData = levelData;
            currentUiLevel = uiLevel;
        }
        else
        {
            // Starting game from current scene in editor
            currentLevelData.Name = levelSettings.LevelName;
            currentLevelData.TimeSeconds = levelSettings.StartTimeSeconds;
            currentLevelData.EliteCompletionTimeSeconds = levelSettings.EliteCompletionTimeSeconds;
            levelSettings.ColorSchemeName = levelSettings.ColorScheme.name;
            currentLevelData.ColorSchemeName = levelSettings.ColorSchemeName;

            SludgeObjects = FindObjectsOfType<SludgeObject>();

            // Simulate level load when starting directly from editor
            foreach (var obj in SludgeObjects)
            {
                foreach (var modifier in obj.Modifiers)
                    modifier.OnLoaded();
            }
        }
        
        if (!string.IsNullOrWhiteSpace(levelSettings.ColorSchemeName))
        {
            var colorScheme = ColorSchemeList.ColorSchemes.Where(s => s.name == levelSettings.ColorSchemeName).FirstOrDefault();
            if (colorScheme != null)
            {
                SetColorScheme(colorScheme);
                levelSettings.ColorScheme = colorScheme;
            }
            else
            {
                Debug.LogError($"Colorscheme saved in level not found: {levelSettings.ColorSchemeName}");
            }
        }
        else
        {
            // No color scheme selected, use default
            var defaultColorScheme = ColorSchemeList.ColorSchemes.Where(s => s?.name == "Default").FirstOrDefault();
            if (defaultColorScheme != null)
                SetColorScheme(defaultColorScheme);
        }

        TextLevelName.text = currentLevelData.Name;
        TextLevelMasterTime.text = $"Complete: {currentLevelData.EliteCompletionTimeSeconds:0.000}";
        Player.SetHomePosition();

        SludgeObjects = FindObjectsOfType<SludgeObject>();
        Exits = SludgeObjects.Where(o => o is Exit).Cast<Exit>().ToArray();
        ExitsHighlight = Exits.Select(e => e.transform.Find("HighlightParticles").GetComponent<ParticleSystem>()).ToArray();
        SlimeBombs = SludgeObjects.Where(o => o is SlimeBomb).Cast<SlimeBomb>().ToArray();
        SlimeBombsHighlight = SlimeBombs.Select(b => b.transform.Find("HighlightParticles").GetComponent<ParticleSystem>()).ToArray();

        ResetLevel();
        SetScoreText();

        Tilemap.gameObject.SetActive(true);
    }

    void SetHighlightedObjects(bool bombActivated)
    {
        bool highlightExits = bombActivated || SlimeBombs.Length == 0;
        bool highlightBombs = !highlightExits;

        for (int i = 0; i < Exits.Length; ++i)
        {
            SludgeUtil.EnableEmission(ExitsHighlight[i], highlightExits);
            if (highlightExits)
                Exits[i].Activate();
        }

        for (int i = 0; i < SlimeBombs.Length; ++i)
        {
            SludgeUtil.EnableEmission(SlimeBombsHighlight[i], highlightBombs);
        }
    }

    void SetMenuButtonActive(GameObject go, bool active)
    {
        // Button face
        go.GetComponent<UiSchemeColorApplier>().SetBrightnessOffset(active ? 0 : -0.25f);
        // Button text
        go.GetComponentInChildren<TMP_Text>().gameObject.GetComponent<UiSchemeColorApplier>().SetBrightnessOffset(active ? 0 : -0.5f);

        go.GetComponent<UiNavigation>().Enabled = active;
    }

    void SetScoreText(RoundResult roundResult = null)
    {
        var highlightColor = ColorScheme.GetColor(CurrentColorScheme, SchemeColor.UiTextHighlighted);

        TextLevelStatus.text = "";
        var prevLevelProgress = PlayerProgress.GetLevelProgress(currentLevelData.UniqueId);
        if (roundResult?.Completed == true)
        {
            // Reached exit
            TextLevelStatus.text += roundResult.IsEliteTime ? "- You swiftly escaped -\n" : "- You barely escaped -\n";
            bool completedJustNow = roundResult.IsEliteTime && prevLevelProgress.LevelStatus < PlayerProgress.LevelStatus.Completed;
            if (completedJustNow)
                QuickText.Instance.ShowText("Chamber completed!");
        }
        else
        {
            // Dead or just arrived on level
            bool levelCompleted = prevLevelProgress.LevelStatus == PlayerProgress.LevelStatus.Completed;
            TextLevelStatus.text += !levelCompleted ?
                $"Escape in {SludgeUtil.ColorWrap($"{levelSettings.EliteCompletionTimeSeconds:0.000}", highlightColor)} seconds to complete chamber" :
                "- You have completed this chamber -";
        }
    }

    void GoToNextLevel()
    {
        // TODO: Some transition to next level?
        StopAllCoroutines();
        UiLogic.Instance.latestSelectedLevelUniqueId = currentUiLevel.Next.LevelData.UniqueId;
        LoadLevel(currentUiLevel.Next);
        StartLevel();
    }

    IEnumerator BetweenRoundsLoop(string replayId = null)
    {
        levelJustMastered = false;

        GameObject latestSelection = ButtonStartRound.gameObject;
        UpdateTime();

        while (true)
        {
            GC.Collect();

            currentLevelProgress = PlayerProgress.GetLevelProgress(currentLevelData.UniqueId);
            bool canGoToNextLevel = currentLevelProgress.LevelStatus >= PlayerProgress.LevelStatus.Escaped && currentUiLevel.Next != null;

            SetMenuButtonActive(ButtonStartRound, true);
            SetMenuButtonActive(ButtonWatchReplay, LevelReplay.HasReplay(currentLevelData.UniqueId));
            SetMenuButtonActive(ButtonGoToNextLevel, canGoToNextLevel);

            // Select 'next level' if player got access just now. Else keep last selection.
            var selectedButton = levelJustMastered && canGoToNextLevel ? ButtonGoToNextLevel : latestSelection;
            UiLogic.Instance.SetSelectionMarker(selectedButton);
            latestSelection = selectedButton;

            bool startReplay = false;
            bool startRound = false;

            UiNavigation.OnNavigationChanged = (go) => latestSelection = go;
            UiNavigation.OnNavigationSelected = (go) =>
            {
                if (go == ButtonStartRound)
                {
                    startRound = true;
                }
                else if (go == ButtonWatchReplay && go.GetComponent<UiNavigation>().Enabled)
                {
                    startReplay = true;
                }
                else if (go == ButtonGoToNextLevel && go.GetComponent<UiNavigation>().Enabled)
                {
                    GoToNextLevel();
                };
            };

            ResetLevel();

            yield return UiPanels.Instance.ShowPanel(UiPanel.BetweenRoundsMenu);

            PlayerInput.PauseInput(0.3f);

            while (startReplay == false && startRound == false)
            {
                // Menu selection loop - start new round, start replay, etc.
                PlayerInput.GetHumanInput();
                UiLogic.Instance.DoUiNavigation(PlayerInput);

                if (PlayerInput.Up > 0 && latestSelection == ButtonStartRound)
                {
                    startRound = true;
                }

                if (PlayerInput.Up > 0 && latestSelection == ButtonGoToNextLevel)
                {
                    GoToNextLevel();
                    yield break;
                }

                if (PlayerInput.IsTapped(PlayerInput.InputType.Back))
                {
                    QuickText.Instance.Hide();
                    StopAllCoroutines();
                    UiPanels.Instance.HidePanel(UiPanel.BetweenRoundsMenu);
                    UiLogic.Instance.BackFromGame();
                }

                yield return null;
            }

            UiPanels.Instance.HidePanel(UiPanel.BetweenRoundsMenu);
            UiLogic.Instance.SetSelectionMarker(null);

            yield return Playing(isReplay: startReplay);
        }
    }

    void ResetLevel()
    {
        Debug.Log($"ResetLevel()");

        EngineTime = 0;
        EngineTimeMs = 0;
        FrameCounter = 0;
        UnityTime = 0;
        roundTime = 0;
        latestRoundResult = new RoundResult();

        levelComplete = false;
        Keys = 0;

        BulletManager.Instance.Reset();
        CellAntManager.Instance.Reset();

        Debug.Log($"Tilemap size: {Tilemap.size} ({Tilemap.cellBounds})");
        LevelCells.Instance.ResetToTilemap(Tilemap);
        PillCells.Instance.ResetToTilemap(PillTilemap);

        for (int i = 0; i < SludgeObjects.Length; ++i)
            SludgeUtil.SetActiveRecursive(SludgeObjects[i].gameObject, true);

        for (int i = 0; i < SludgeObjects.Length; ++i)
            SludgeObjects[i].Reset();

        Player.Prepare();

        UpdateSludgeObjects();
        SetHighlightedObjects(bombActivated: false);
    }

    public void OnActivatingBomb()
    {
        SetHighlightedObjects(bombActivated: true);
    }

    IEnumerator Playing(bool isReplay)
    {
        SoundManager.Play(FxList.Instance.StartRound);

        IsReplay = isReplay;
        if (isReplay)
        {
            LevelReplay.BeginReplay();
            QuickText.Instance.ShowText("replay");
        }
        else
        {
            LevelReplay.BeginRecording(currentLevelData.UniqueId);
        }

        while (Player.Alive)
        {
            UnityTime += Time.deltaTime;

            while (EngineTime <= UnityTime)
            {
                PlayerInput.GetHumanInput();

                DoTick(isReplay);
                latestRoundResult.RoundTotalTime += TickSize;
            }

            if (levelComplete)
                break;

            if (PlayerInput.BackActive() || Input.GetKeyDown(KeyCode.R))
            {
                latestRoundResult.Cancelled = true;
                QuickText.Instance.ShowText(isReplay ? "replay cancelled" : "restart");
                yield break;
            }

            yield return null;
        }

        roundTime = SludgeUtil.Stabilize(roundTime);
        bool playerCompletedRound = !isReplay && !latestRoundResult.Cancelled;
        if (playerCompletedRound)
            LevelReplay.CommitReplay();

        latestRoundResult.ClientId = ClientId;
        latestRoundResult.Version = Version;
        latestRoundResult.Platform = Application.platform.ToString();
        latestRoundResult.UnixTimestamp = SludgeUtil.UnixTimeNow();
        latestRoundResult.UniqueId = UnityEngine.Random.Range(1 << 28, 1 << 29).ToString("X").ToUpper();

        latestRoundResult.LevelId = currentLevelData.UniqueId;
        latestRoundResult.LevelName = currentLevelData.Name;
        latestRoundResult.IsReplay = isReplay;
        latestRoundResult.Completed = levelComplete;
        latestRoundResult.EndTime = roundTime;
        latestRoundResult.RoundTotalTime = SludgeUtil.Stabilize(latestRoundResult.RoundTotalTime);
        latestRoundResult.OutOfTime = roundTime >= currentLevelData.TimeSeconds;
        latestRoundResult.IsEliteTime = levelComplete && EngineTime <= levelSettings.EliteCompletionTimeSeconds;
        latestRoundResult.ReplayData = latestRoundResult.Cancelled ? null : LevelReplay.LatestCommittedToReplayString();

        levelJustMastered = currentLevelProgress.LevelStatus < PlayerProgress.LevelStatus.Escaped && latestRoundResult.Completed;
        if (latestRoundResult.Completed && latestRoundResult.IsEliteTime)
        {
            SoundManager.Play(FxList.Instance.LevelCompleteGood);
            QuickText.Instance.ShowText("Completed!");
        }
        else if (latestRoundResult.Completed && !latestRoundResult.IsEliteTime)
        {
            SoundManager.Play(FxList.Instance.LevelCompleteBad);
        }

        if (latestRoundResult.OutOfTime)
            QuickText.Instance.ShowText("time ran out");

        SetScoreText(latestRoundResult);
        PlayerProgress.UpdateLevelStatus(latestRoundResult);

        UiLogic.Instance.CalcProgression();
        latestRoundResult.ProgressionAfter = UiLogic.Instance.GameProgressPct;

        Analytics.Instance.SaveStats(latestRoundResult);

        yield return new WaitForSeconds(1.0f);
    }

    public void LevelCompleted(Exit exit)
    {
        CompletedParticles.transform.position = exit.transform.position;
        CompletedParticles.Emit(11);

        MarkerParticles.transform.position = exit.transform.position;
        MarkerParticles.Emit(1);

        SludgeUtil.EnableEmission(exit.transform.Find("HighlightParticles").GetComponent<ParticleSystem>(), enabled: false, clearParticles: true);
        levelComplete = true;
    }

    public void KeyPickup(Key key)
    {
        DustParticles.transform.position = key.transform.position;
        DustParticles.Emit(1);
        SoundManager.Play(FxList.Instance.KeyPickup);
        Keys++;
    }

    public void TimePillPickup(TimePill pill)
    {
        DustParticles.transform.position = pill.transform.position;
        DustParticles.Emit(1);
        SoundManager.Play(FxList.Instance.TimePillPickup);
        roundTime += TimePillBonusTime;
        if (roundTime < 0)
            roundTime = 0;
    }

    void UpdateAll()
    {
        UpdateTime();
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

    void UpdateTime()
    {
        if (roundTime >= currentLevelData.TimeSeconds)
        {
            roundTime = currentLevelData.TimeSeconds;
            Player.Kill();
        }

        int timeIdx = (int)(roundTime * 1000.0);
        timeIdx = Mathf.Clamp(timeIdx, 0, Strings.TimeStrings.Length - 1);

        TextLevelTime.text = Strings.TimeStrings[timeIdx];
        double maxTime = currentLevelData.TimeSeconds;
        double fillAmount = roundTime / maxTime;
        float fillBar = 1 - (float)fillAmount;
        TimeBarLeft.fillAmount = fillBar;
        TimeBarRight.fillAmount = fillBar;
    }

    void DoTick(bool isReplay)
    {
        if (isReplay)
        {
            int state = LevelReplay.GetReplayState(FrameCounter);
            PlayerInput.SetState(state);
        }
        else
        {
            LevelReplay.RecordState(PlayerInput.GetState(), FrameCounter);
        }

        Player.Position = Player.transform.position;

        EngineTimeMs = FrameCounter * TickSizeMs;
        EngineTime = EngineTimeMs * 0.001;
        roundTime += TickSize;

        UpdateAll();
        Physics2D.Simulate((float)TickSize);

        FrameCounter++;
    }
}

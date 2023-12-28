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

// First script to run
public class GameManager : MonoBehaviour
{
    // Level switching:
    // LoadLevel() is the only way in
    // StartLevel() resets and starts what was loaded.

    public static PlayerSample[] PlayerSamples = new PlayerSample[30000];

    public static readonly string Version = "0.1b";

    const double TimePillBonusTime = -1.0;
    public const double TickSize = 0.016;
    public const int TickSizeMs = 16;
    public const double TicksPerSecond = 1000.0 / TickSizeMs;

    public Transform CameraRoot;
    public Tilemap Tilemap;
    public Tilemap PillTilemap;
    public ColorSchemeScriptableObject CurrentColorScheme;
    public ColorSchemeScriptableObject CurrentUiColorScheme;
    public ColorSchemeListScriptableObject ColorSchemeList;

    public GameObject ButtonStartRound;

    public ParticleSystem DeathParticles;
    public ParticleSystem DustParticles;
    public ParticleSystem CompletedParticles;
    public ParticleSystem MarkerParticles;

    public Material OutlineMaterial;

    public static string ClientId;
    public static PlayerInput PlayerInput;
    public static GameManager I;

    public Player Player;
    public SludgeObject[] SludgeObjects;
    public SlimeBomb[] SlimeBombs;
    public ParticleSystem[] SlimeBombsHighlight;
    public Exit[] Exits;
    public ParticleSystem[] ExitsHighlight;

    public double UnityTime;
    public double EngineTime;
    public int EngineTimeMs;
    public int FrameCounter;
    public int Keys;
    LevelData currentLevelData = new LevelData { EliteCompletionTimeSeconds = 60, };
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
        I = this;
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

        Player.SetHomePosition();

        SludgeObjects = FindObjectsOfType<SludgeObject>();
        Exits = SludgeObjects.Where(o => o is Exit).Cast<Exit>().ToArray();
        ExitsHighlight = Exits.Select(e => e.transform.Find("HighlightParticles").GetComponent<ParticleSystem>()).ToArray();
        SlimeBombs = SludgeObjects.Where(o => o is SlimeBomb).Cast<SlimeBomb>().ToArray();
        SlimeBombsHighlight = SlimeBombs.Select(b => b.transform.Find("HighlightParticles").GetComponent<ParticleSystem>()).ToArray();

        PillTilemap.gameObject.GetComponent<PillSnapshot>().Push();

        ResetLevel();

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

        UpdateTime();

        while (true)
        {
            GC.Collect();

            currentLevelProgress = PlayerProgress.GetLevelProgress(currentLevelData.UniqueId);
            bool canGoToNextLevel = currentLevelProgress.LevelStatus >= PlayerProgress.LevelStatus.Escaped && currentUiLevel.Next != null;

            SetMenuButtonActive(ButtonStartRound, true);

            var selectedButton = ButtonStartRound;
            UiLogic.Instance.SetSelectionMarker(selectedButton);

            bool startRound = false;

            UiNavigation.OnNavigationSelected = (go) =>
            {
                if (go == ButtonStartRound)
                {
                    startRound = true;
                }
            };

            ResetLevel();

            yield return UiPanels.Instance.ShowPanel(UiPanel.BetweenRoundsMenu);

            PlayerInput.PauseInput(0.3f);

            while (startRound == false)
            {
                PlayerInput.GetHumanInput();
                UiLogic.Instance.DoUiNavigation(PlayerInput);

                if (PlayerInput.Up > 0 || PlayerInput.Down > 0 || PlayerInput.Left > 0 || PlayerInput.Right > 0)
                {
                    startRound = true;
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

            yield return Playing();
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
        LevelCells.Instance.UpdateFrom(Tilemap);

        Debug.Log($"Pillmap size: {Tilemap.size} ({Tilemap.cellBounds})");
        PillTilemap.gameObject.GetComponent<PillSnapshot>().Pop();

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

    IEnumerator Playing()
    {
        SoundManager.Play(FxList.Instance.StartRound);

        while (Player.Alive)
        {
            UnityTime += Time.deltaTime;

            while (EngineTime <= UnityTime)
            {
                PlayerInput.GetHumanInput();
                DoTick();

                latestRoundResult.RoundTotalTime += TickSize;
            }

            if (levelComplete)
                break;

            if (PlayerInput.BackActive() || Input.GetKeyDown(KeyCode.R))
            {
                latestRoundResult.Cancelled = true;
                QuickText.Instance.ShowText("restart");
                yield break;
            }

            yield return null;
        }

        roundTime = SludgeUtil.Stabilize(roundTime);

        latestRoundResult.ClientId = ClientId;
        latestRoundResult.Version = Version;
        latestRoundResult.Platform = Application.platform.ToString();
        latestRoundResult.UnixTimestamp = SludgeUtil.UnixTimeNow();
        latestRoundResult.UniqueId = UnityEngine.Random.Range(1 << 28, 1 << 29).ToString("X").ToUpper();

        latestRoundResult.LevelId = currentLevelData.UniqueId;
        latestRoundResult.LevelName = currentLevelData.Name;
        latestRoundResult.Completed = levelComplete;
        latestRoundResult.EndTime = roundTime;
        latestRoundResult.RoundTotalTime = SludgeUtil.Stabilize(latestRoundResult.RoundTotalTime);
        latestRoundResult.IsEliteTime = levelComplete && EngineTime <= levelSettings.EliteCompletionTimeSeconds;

        if (latestRoundResult.Completed)
        {
            SoundManager.Play(FxList.Instance.LevelCompleteGood);
            QuickText.Instance.ShowText("Completed!");

            PlayerProgress.UpdateLevelStatus(latestRoundResult);
            UiLogic.Instance.CalcProgression();
        }
        else
        {
            // dead
        }

        latestRoundResult.ProgressionAfter = UiLogic.Instance.GameProgressPct;

        Analytics.Instance.SaveStats(latestRoundResult);

        yield return new WaitForSeconds(0.75f);
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
        int timeIdx = (int)(roundTime * 1000.0);
        timeIdx = Mathf.Clamp(timeIdx, 0, Strings.TimeStrings.Length - 1);
    }

    void DoTick()
    {
        Player.Position = Player.transform.position;

        EngineTimeMs = FrameCounter * TickSizeMs;
        EngineTime = EngineTimeMs * 0.001;
        roundTime += TickSize;

        UpdateAll();
        Physics2D.Simulate((float)TickSize);

        FrameCounter++;
    }
}

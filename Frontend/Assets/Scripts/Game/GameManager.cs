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
    const double TimePillBonusTime = -1.0;
    public const double TickSize = 0.016;
    public const int TickSizeMs = 16;
    public const double TicksPerSecond = 1000.0 / TickSizeMs;

    public Transform CameraRoot;
    public Image TimeBarLeft;
    public Image TimeBarRight;
    public Tilemap Tilemap;
    public ColorSchemeScriptableObject CurrentColorScheme;
    public ColorSchemeScriptableObject CurrentUiColorScheme;
    public ColorSchemeListScriptableObject ColorSchemeList;

    public GameObject ButtonStartRound;
    public GameObject ButtonWatchReplay;
    public GameObject ButtonGoToNextLevel;

    public ParticleSystem DeathParticles;
    public ParticleSystem DustParticles;
    public ParticleSystem HighlightParticles;

    public TMP_Text TextStatsComments;
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
    public Exit[] Exits;

    public double UnityTime;
    public double EngineTime;
    public int EngineTimeMs;
    public int FrameCounter;
    public int Keys;
    LevelData currentLevelData = new LevelData { TimeSeconds = 30, EliteCompletionTimeSeconds = 20, };
    double roundTime;
    LevelElements levelElements;
    LevelSettings levelSettings;
    bool levelComplete;
    RoundResult latestRoundResult;

    private void OnValidate()
    {
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
        DustParticles.transform.position = goEnemy.transform.position;
        DustParticles.Emit(4);
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

    public void LoadLevel(LevelData levelData)
    {
        // Total hack: The player dies if the new level has a collider at his OLD start position. The same thing could happen to other objects sensitive to collision!
        Tilemap.gameObject.SetActive(false);

        if (levelData != null)
        {
            Debug.Log($"Loading level: {levelData.Name}");
            LevelDeserializer.Run(levelData, levelElements, levelSettings);
            currentLevelData = levelData;
        }
        else
        {
            // Starting game from current scene in editor
            currentLevelData.Name = levelSettings.LevelName;
            currentLevelData.TimeSeconds = levelSettings.StartTimeSeconds;
            currentLevelData.EliteCompletionTimeSeconds = levelSettings.EliteCompletionTimeSeconds;

            SludgeObjects = FindObjectsOfType<SludgeObject>();

            // Simulate level load when starting directly from editor
            foreach (var obj in SludgeObjects)
            {
                foreach (var modifier in obj.Modifiers)
                    modifier.OnLoaded();
            }
        }

        TextLevelName.text = currentLevelData.Name;
        TextLevelMasterTime.text = $"Master {currentLevelData.EliteCompletionTimeSeconds:0.000}s";
        Player.SetHomePosition();

        SludgeObjects = FindObjectsOfType<SludgeObject>();
        Exits = SludgeObjects.Where(o => o is Exit).Cast<Exit>().ToArray();
        SlimeBombs = SludgeObjects.Where(o => o is SlimeBomb).Cast<SlimeBomb>().ToArray();

        ResetLevel();

        Tilemap.gameObject.SetActive(true);
    }

    void SetHighlightedObjects(bool bombActivated)
    {
        bool highlightExits = bombActivated || SlimeBombs.Length == 0;
        bool highlightBombs = !highlightExits;

        foreach (var exit in Exits)
        {
            SludgeUtil.EnableEmission(exit.transform.Find("HighlightParticles").GetComponent<ParticleSystem>(), highlightExits);
            if (highlightExits)
                exit.Activate();
        }

        foreach (var bomb in SlimeBombs)
            SludgeUtil.EnableEmission(bomb.transform.Find("HighlightParticles").GetComponent<ParticleSystem>(), highlightBombs);
    }

    void SetMenuButtonActive(GameObject go, bool active)
    {
        // Button face
        go.GetComponent<UiSchemeColorApplier>().SetBrightnessOffset(active ? 0 : -0.25f);
        // Button text
        go.GetComponentInChildren<TMP_Text>().gameObject.GetComponent<UiSchemeColorApplier>().SetBrightnessOffset(active ? 0 : -0.5f);

        go.GetComponent<UiNavigation>().Enabled = active;
    }

    void SetScoreText(RoundResult roundResult)
    {
        if (roundResult.Completed)
        {
            TextStatsComments.text = roundResult.IsEliteTime ? "- Level Mastered! -" : "- Well Done -";
        }
        else
        {
            TextStatsComments.text = roundResult.OutOfTime ? "- Out Of Time -" : "";
        }
    }

    IEnumerator BetweenRoundsLoop()
    {
        GameObject latestSelection = ButtonStartRound.gameObject;
        TextLevelStatus.text = $"Elite Time: {levelSettings.EliteCompletionTimeSeconds.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)}";
        TextStatsComments.text = "";
        UpdateTime();

        while (true)
        {
            GC.Collect();

            SetMenuButtonActive(ButtonStartRound, true);
            SetMenuButtonActive(ButtonWatchReplay, LevelReplay.HasReplay());
            SetMenuButtonActive(ButtonGoToNextLevel, false);

            UiLogic.Instance.SetSelectionMarker(latestSelection);

            bool startReplay = false;
            bool startRound = false;

            UiNavigation.OnNavigationChanged = (go) => latestSelection = go;
            UiNavigation.OnNavigationSelected = (go) =>
            {
                if (go == ButtonStartRound) {
                    startRound = true;
                } else if (go == ButtonWatchReplay && go.GetComponent<UiNavigation>().Enabled) {
                    startReplay = true;
                }
            };

            ResetLevel();

            UiPanels.Instance.ShowPanel(UiPanel.BetweenRoundsMenu);
            PlayerInput.ClearState();

            while (startReplay == false && startRound == false)
            {
                // Menu selection loop - start new round, start replay, etc.
                PlayerInput.GetHumanInput();
                UiLogic.Instance.DoUiNavigation(PlayerInput);

                if (PlayerInput.Up > 0 && latestSelection == ButtonStartRound)
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

        for (int i = 0; i < SludgeObjects.Length; ++i)
        {
            SludgeUtil.SetActiveRecursive(SludgeObjects[i].gameObject, true);
            SludgeObjects[i].Reset();
        }

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
        if (isReplay)
        {
            LevelReplay.BeginReplay();
            QuickText.Instance.ShowText("replay");
        }
        else
        {
            LevelReplay.BeginRecording();
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

            if (PlayerInput.BackActive())
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
        latestRoundResult.LevelId = currentLevelData.UniqueId;
        latestRoundResult.LevelName = currentLevelData.Name;
        latestRoundResult.IsReplay = isReplay;
        latestRoundResult.Completed = levelComplete;
        latestRoundResult.EndTime = roundTime;
        latestRoundResult.RoundTotalTime = SludgeUtil.Stabilize(latestRoundResult.RoundTotalTime);
        latestRoundResult.OutOfTime = roundTime >= currentLevelData.TimeSeconds;
        latestRoundResult.IsEliteTime = levelComplete && EngineTime <= levelSettings.EliteCompletionTimeSeconds;
        latestRoundResult.ReplayData = latestRoundResult.Cancelled ? null : LevelReplay.LatestCommittedToReplayString();

        if (latestRoundResult.OutOfTime)
            QuickText.Instance.ShowText("time ran out");

        Analytics.Instance.SaveStats(latestRoundResult);

        SetScoreText(latestRoundResult);

        PlayerProgress.UpdateLevelStatus(latestRoundResult);

        yield return new WaitForSeconds(1.0f);
    }

    public void LevelCompleted(Exit exit)
    {
        levelComplete = true;
    }

    public void KeyPickup(Key key)
    {
        Keys++;
    }

    public void TimePillPickup(TimePill key)
    {
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

        EngineTimeMs = FrameCounter * TickSizeMs;
        EngineTime = EngineTimeMs * 0.001;
        roundTime += TickSize;

        UpdateAll();
        Physics2D.Simulate((float)TickSize);

        FrameCounter++;
    }
}

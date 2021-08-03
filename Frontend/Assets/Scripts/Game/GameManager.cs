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
    const double TimePillBonusTime = 1.0;
    public const double TickSize = 0.016;
    public const int TickSizeMs = 16;
    public const double TicksPerSecond = 1000.0 / TickSizeMs;
    
    public Image TimeBarLeft;
    public Image TimeBarRight;
    public Tilemap Tilemap;
    public ColorSchemeScriptableObject CurrentColorScheme;
    public ColorSchemeListScriptableObject ColorSchemeList;

    public GameObject ButtonStartRound;
    public GameObject ButtonWatchReplay;
    public GameObject ButtonGoToNextLevel;

    public TMP_Text TextStatsComments;
    public TMP_Text TextLevelStatus;

    public TMP_Text TextLevelTime;
    public TMP_Text TextLevelName;
    public Material OutlineMaterial;

    public static PlayerInput PlayerInput = new PlayerInput();
    public static LevelReplay LevelReplay = new LevelReplay();
    public static GameManager Instance;

    public Player Player;
    public SludgeObject[] SludgeObjects;
    public double UnityTime;
    public double EngineTime;
    public int EngineTimeMs;
    public int FrameCounter;
    public int Keys;
    LevelData currentLevelData = new LevelData { StartTimeSeconds = 30, EliteCompletionTimeSeconds = 20, };
    double timeLeft;
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

        levelElements = (LevelElements)Resources.FindObjectsOfTypeAll(typeof(LevelElements)).First();
        levelSettings = (LevelSettings)Resources.FindObjectsOfTypeAll(typeof(LevelSettings)).First();
        Player = FindObjectOfType<Player>();

        OnValidate();
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
            currentLevelData.StartTimeSeconds = levelSettings.StartTimeSeconds;
            currentLevelData.EliteCompletionTimeSeconds = levelSettings.EliteCompletionTimeSeconds;

            SludgeObjects = FindObjectsOfType<SludgeObject>();

            // Simulate level load
            foreach (var obj in SludgeObjects)
            {
                foreach (var modifier in obj.Modifiers)
                    modifier.OnLoaded();
            }
        }

        TextLevelName.text = currentLevelData.Name;
        Player.SetHomePosition();

        SludgeObjects = FindObjectsOfType<SludgeObject>();
        ResetLevel();

        Tilemap.gameObject.SetActive(true);
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
            PlayerInput.Clearstate();
            yield return null; // Unity input state

            while (startReplay == false && startRound == false)
            {
                // Menu selection loop - start new round, start replay, etc.
                PlayerInput.GetHumanInput();
                UiLogic.Instance.DoUiNavigation(PlayerInput);

                if (PlayerInput.Up > 0 && latestSelection == ButtonStartRound)
                {
                    startRound = true;
                }

                if (PlayerInput.BackTap)
                {
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
        timeLeft = currentLevelData.StartTimeSeconds;
        latestRoundResult = new RoundResult();

        levelComplete = false;
        Keys = 0;

        BulletManager.Instance.Reset();

        for (int i = 0; i < SludgeObjects.Length; ++i)
            SludgeObjects[i].Reset();

        Player.Prepare();

        UpdateSludgeObjects();
    }

    IEnumerator Playing(bool isReplay)
    {
        if (isReplay)
            LevelReplay.BeginReplay();
        else
            LevelReplay.BeginRecording();

        while (Player.Alive)
        {
            UnityTime += Time.deltaTime;
            while (EngineTime <= UnityTime)
                DoTick(isReplay);

            if (levelComplete)
                break;

            yield return null;
        }

        latestRoundResult.Completed = levelComplete;
        latestRoundResult.EndTime = timeLeft;

        latestRoundResult.OutOfTime = timeLeft <= 0;
        latestRoundResult.IsEliteTime = EngineTime <= levelSettings.EliteCompletionTimeSeconds;

        SetScoreText(latestRoundResult);

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
        timeLeft += TimePillBonusTime;
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
            SludgeObjects[i].EngineTick();
    }

    void UpdatePlayer()
    {
        Player.EngineTick();
    }

    const float MaxTime = 30;

    void UpdateTime()
    {
        if (timeLeft <= 0)
        {
            timeLeft = 0;
            Player.Kill();
        }

        int timeIdx = (int)(timeLeft * 1000.0);
        timeIdx = Mathf.Clamp(timeIdx, 0, Strings.TimeStrings.Length - 1);

        TextLevelTime.text = Strings.TimeStrings[timeIdx];
        float fillAmount = (float)(timeLeft / MaxTime);
        TimeBarLeft.fillAmount = fillAmount;
        TimeBarRight.fillAmount = fillAmount;
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
            PlayerInput.GetHumanInput();
            LevelReplay.RecordState(PlayerInput.GetState(), FrameCounter);
        }

        EngineTimeMs = FrameCounter * TickSizeMs;
        EngineTime = EngineTimeMs * 0.001;
        timeLeft -= TickSize;

        UpdateAll();
        Physics2D.Simulate((float)TickSize);

        FrameCounter++;
    }
}

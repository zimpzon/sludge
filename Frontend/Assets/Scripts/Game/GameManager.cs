using Sludge.Colors;
using Sludge.PlayerInputs;
using Sludge.Replays;
using Sludge.Shared;
using Sludge.SludgeObjects;
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
    public Image TimeBar;
    public Tilemap Tilemap;
    public ColorSchemeScriptableObject UiColorScheme;
    public ColorSchemeScriptableObject ColorScheme;
    public TMP_Text TextStatus;
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
    public const double TickSize = 0.016;
    public const int TickSizeMs = 16;
    public const double TicksPerSecond = 1000 / TickSizeMs;
    public int Keys;
    LevelData currentLevelData = new LevelData { StartTimeSeconds = 30, EliteCompletionTimeSeconds = 20, Name = "(started from editor)" };
    double timeLeft;

    LevelElements levelElements;
    LevelSettings levelSettings;
    bool levelComplete;

    private void OnValidate()
    {
        UpdateColors(ColorScheme);
        UpdateUiColors(UiColorScheme);
        OutlineMaterial.color = Sludge.Colors.ColorScheme.GetColor(ColorScheme, SchemeColor.Edges);
    }

    public static void SetStatusText(string text)
    {
        Instance.TextStatus.text = text;
    }

    void Awake()
    {
        Startup.StaticInit();

        Instance = this;
        OnValidate();
    }

    public void SetScheme(ColorSchemeScriptableObject scheme)
    {
        ColorScheme = scheme;
        UiColorScheme = scheme;
        UpdateColors(ColorScheme);
        UpdateUiColors(UiColorScheme);
    }

    void UpdateColors(ColorSchemeScriptableObject scheme)
    {
        var allColorAppliers = FindObjectsOfType<SchemeColorApplier>(includeInactive: true);
        foreach (var applier in allColorAppliers)
            applier.ApplyColor(scheme);
    }

    void UpdateUiColors(ColorSchemeScriptableObject scheme)
    {
        var allUiColorAppliers = FindObjectsOfType<UiSchemeColorApplier>(includeInactive: true);
        foreach (var applier in allUiColorAppliers)
            applier.ApplyColor(scheme);
    }

    private void Start()
    {
        levelElements = (LevelElements)Resources.FindObjectsOfTypeAll(typeof(LevelElements)).First();
        levelSettings = (LevelSettings)Resources.FindObjectsOfTypeAll(typeof(LevelSettings)).First();
        Player = FindObjectOfType<Player>();
    }

    public void StartLevel()
    {
        ResetLevel();
        StartCoroutine(LevelLoop());
    }

    public void LoadLevel(LevelData levelData)
    {
        if (levelData != null)
        {
            LevelDeserializer.Run(levelData, levelElements, levelSettings);
            currentLevelData = levelData;
        }

        TextLevelName.text = currentLevelData.Name;
        Player.SetHomePosition();
        SludgeObjects = FindObjectsOfType<SludgeObject>();
    }

    IEnumerator LevelLoop()
    {
        while (true)
        {
            GC.Collect();

            if (LevelReplay.HasReplay())
            {
                SetStatusText("<Press W to start, R to replay or back to exit>");
            }
            else
            {
                SetStatusText("<Press W to start>");
            }

            ResetLevel();

            bool? isReplay = null;
            while (isReplay == null)
            {
                PlayerInput.GetHumanInput();
                if (PlayerInput.Up > 0)
                {
                    SetStatusText("");
                    isReplay = false;
                }
                else if (Input.GetKey(KeyCode.R))
                {
                    SetStatusText("<Replay>");
                    isReplay = true;
                }
                else if (PlayerInput.BackTap)
                {
                    StopAllCoroutines();
                }

                yield return null;
            }

            yield return Playing(isReplay.Value);
        }
    }

    void ResetLevel()
    {
        EngineTime = 0;
        EngineTimeMs = 0;
        FrameCounter = 0;
        UnityTime = 0;

        levelComplete = false;
        Keys = 0;

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

        timeLeft = currentLevelData.StartTimeSeconds;

        while (Player.Alive)
        {
            UnityTime += Time.deltaTime;
            while (EngineTime <= UnityTime)
                DoTick(isReplay);

            if (levelComplete)
            {
                SetStatusText("Level complete!");
                yield return new WaitForSeconds(1);
                break;
            }

            yield return null;
        }

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
        timeLeft += 2;
    }

    void UpdateAll()
    {
        UpdateTime();
        UpdatePlayer();
        UpdateSludgeObjects();
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

    const float MaxTime = 60;

    void UpdateTime()
    {
        if (timeLeft <= 0)
        {
            timeLeft = 0;
            Player.Kill();
        }

        int timeIdx = Mathf.Min(9999, (int)(timeLeft * TicksPerSecond));
        if (timeIdx >= Strings.TimeStrings.Length)
            timeIdx = Strings.TimeStrings.Length - 1;

        TextLevelTime.text = Strings.TimeStrings[timeIdx];
        TimeBar.fillAmount = (float)(timeLeft / MaxTime);
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

        FrameCounter++;
    }
}

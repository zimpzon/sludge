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

// OnLevelCompleted: Compare with best. Show how close to elite. Make player want elite.
// "DARK" LEVELS? Super meat Boy has the level layouts repeated with higher difficulty.

// First script to run
public class GameManager : MonoBehaviour
{
    public Image TimeBarLeft;
    public Image TimeBarRight;
    public Tilemap Tilemap;
    public ColorSchemeScriptableObject CurrentColorScheme;
    public ColorSchemeListScriptableObject ColorSchemeList;
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
        SetColorScheme(CurrentColorScheme);
    }

    public static void SetStatusText(string text)
    {
        Instance.TextStatus.text = text;
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
        StartCoroutine(LevelLoop());
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
            currentLevelData.StartTimeSeconds = levelSettings.StartTimeSeconds;
            currentLevelData.EliteCompletionTimeSeconds = levelSettings.EliteCompletionTimeSeconds;
        }

        TextLevelName.text = currentLevelData.Name;
        Player.SetHomePosition();

        SludgeObjects = FindObjectsOfType<SludgeObject>();
        ResetLevel();

        Tilemap.gameObject.SetActive(true);
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
        Debug.Log($"ResetLevel()");
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

        FrameCounter++;
    }
}

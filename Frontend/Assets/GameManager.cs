using Sludge.PlayerInputs;
using Sludge.Replays;
using Sludge.SludgeObjects;
using SludgeColors;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    static string[] TimeStrings = new string[6250]; // 0.00 to 99.99 (100 / 0.016)

    static GameManager()
    {
        for (int i = 0; i < TimeStrings.Length; ++i)
            TimeStrings[i] = ((i * TickSizeMs) / 1000.0f).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
    }

    public Canvas CanvasMainMenu;
    public Canvas CanvasMyLevels;
    public Canvas CanvasLevelEditor;
    public Canvas CanvasGame;

    public Tilemap Tilemap;
    public TileListScriptableObject TileList;
    public ColorSchemeScriptableObject UiColorScheme;
    public ColorSchemeScriptableObject ColorScheme;
    public TMP_Text TextStatus;
    public TMP_Text TextLevelTime;

    public static PlayerInput PlayerInput = new PlayerInput();
    public static LevelReplay LevelReplay = new LevelReplay();
    public static GameManager Instance;
    public static Vector3 PlayerPos;
    public Player Player;
    public SludgeObject[] SludgeObjects;
    public double UnityTime;
    public double EngineTime;
    public int EngineTimeMs;
    public int FrameCounter;
    public const double TickSize = 0.016;
    public const int TickSizeMs = 16;
    public const double TicksPerSecond = 1000 / TickSizeMs;
    private Vector3 playerStartPos;
    public int Keys;
    bool levelComplete;

    public static void SetStatusText(string text)
    {
        Instance.TextStatus.text = text;
    }

    void SmartStartup()
    {
        // If multiple canvases are enabled pick just one and disable the rest.
        Canvas selectedCanvas = null;
        if (CanvasMainMenu?.isActiveAndEnabled == true) selectedCanvas = CanvasMainMenu;
        else if (CanvasMyLevels?.isActiveAndEnabled == true) selectedCanvas = CanvasMyLevels;
        else if (CanvasLevelEditor?.isActiveAndEnabled == true) selectedCanvas = CanvasLevelEditor;
        else if (CanvasGame?.isActiveAndEnabled == true) selectedCanvas = CanvasGame;

        if (selectedCanvas != null)
        {
            CanvasMainMenu.gameObject.SetActive(false);
            CanvasMyLevels.gameObject.SetActive(false);
            CanvasLevelEditor.gameObject.SetActive(false);
            CanvasGame.gameObject.SetActive(false);

            selectedCanvas.gameObject.SetActive(true);
        }
    }

    void Awake()
    {
        Instance = this;

        //Player = FindObjectOfType<Player>();
        //playerStartPos = Player.transform.position;
        //SludgeObjects = FindObjectsOfType<SludgeObject>();

        //StartCoroutine(LevelLoop());
    }

    void Start()
    {
        SmartStartup();
    }

    IEnumerator LevelLoop()
    {
        while (true)
        {
            GC.Collect();

            if (LevelReplay.HasReplay())
            {
                SetStatusText("<Press W to start or R to replay>");
            }
            else
            {
                SetStatusText("<Press W to start>");
            }

            ResetLevel();

            bool ? isReplay = null;
            while (isReplay == null)
            {
                if (Input.GetAxisRaw("Vertical") > 0)
                {
                    SetStatusText("");
                    isReplay = false;
                }
                else if (Input.GetKey(KeyCode.R))
                {
                    SetStatusText("<Replay>");
                    isReplay = true;
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

        Player.Prepare(playerStartPos);
        PlayerPos = Player.transform.position;

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
            {
                SetStatusText("Level complete!");
                yield return new WaitForSeconds(1);
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(1.0f);
    }

    void UpdateAll()
    {
        UpdatePlayer();
        PlayerPos = Player.transform.position;

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

    public void LevelCompleted(Exit exit)
    {
        levelComplete = true;
    }

    public void KeyPickup(Key key)
    {
        Keys++;
    }

    void DoTick(bool isReplay)
    {
        int timeIdx = Mathf.Min(9999, FrameCounter);
        TextLevelTime.text = TimeStrings[timeIdx];

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

        UpdateAll();

        FrameCounter++;
    }
}

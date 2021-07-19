using Sludge.Editor;
using Sludge.PlayerInputs;
using Sludge.Replays;
using Sludge.SludgeObjects;
using SludgeColors;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    static string[] TimeStrings = new string[6250]; // 0.00 to 99.99 (100 / 0.016)

    static LevelManager()
    {
        for (int i = 0; i < TimeStrings.Length; ++i)
            TimeStrings[i] = ((i * TickSizeMs) / 1000.0f).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
    }

    public Tilemap Tilemap;
    public TileListScriptableObject TileList;
    public ColorSchemeScriptableObject ColorScheme;
    public TMP_Text TextStatus;
    public TMP_Text TextLevelTime;
    public TMP_InputField TextReplayData;

    public static PlayerInput PlayerInput = new PlayerInput();
    public static LevelReplay LevelReplay = new LevelReplay();
    public static LevelManager Instance;
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
    EditorLogic editor = new EditorLogic();

    public static void SetStatusText(string text)
    {
        Instance.TextStatus.text = text;
    }

    void Awake()
    {
        Instance = this;
        Player = FindObjectOfType<Player>();
        playerStartPos = Player.transform.position;
        SludgeObjects = FindObjectsOfType<SludgeObject>();

        StartCoroutine(LevelLoop());
    }

    IEnumerator LevelLoop()
    {
        while (true)
        {
            GC.Collect();

            //TextReplayData.gameObject.SetActive(LevelReplay.HasReplay());

            if (LevelReplay.HasReplay())
            {
                SetStatusText("<Press W to start or R to replay>");
                TextReplayData.text = LevelReplay.ToReplayString();
            }
            else
            {
                SetStatusText("<Press W to start>");
            }

            ResetLevel();
            yield return editor.EditorLoop();

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
                    LevelReplay.FromString(TextReplayData.text);
                }

                yield return null;
            }

            TextReplayData.gameObject.SetActive(false);

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

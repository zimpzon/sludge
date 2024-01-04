using Assets.Scripts.Game;
using DG.Tweening;
using Sludge;
using Sludge.Colors;
using Sludge.PlayerInputs;
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

    public const double TickSize = 0.016;
    public const int TickSizeMs = 16;
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

    public GameObject ButtonStartRound;

    public ParticleSystem DustParticles;
    public ParticleSystem CompletedParticles;
    public ParticleSystem MarkerParticles;

    public TMP_Text TextPillsLeft;
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
        ApplyColorScheme(scheme);
    }

    public static void ApplyColorScheme(ColorSchemeScriptableObject scheme)
    {
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
            LevelDeserializer.Run(levelData, levelElements, levelSettings);
            currentLevelData = levelData;
            currentUiLevel = uiLevel;
            TextLevelName.text = levelData.LevelName;
        }
        else
        {
            // Starting game from current scene in editor
            TextLevelName.text = "(started from editor)";

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
        SlimeBombs = SludgeObjects.Where(o => o is SlimeBomb).Cast<SlimeBomb>().ToArray();
        SlimeBombsHighlight = SlimeBombs.Select(b => b.transform.Find("HighlightParticles").GetComponent<ParticleSystem>()).ToArray();

        PillTilemap.gameObject.GetComponent<PillSnapshot>().Push();

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

    void GoToNextLevel()
    {
        StopAllCoroutines();
        LoadLevel(currentUiLevel.Next);
        StartLevel();
    }

    IEnumerator BetweenRoundsLoop(string replayId = null)
    {
        int attempts = 0;

        while (true)
        {
            GC.Collect();

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

            yield return RevealPlayer(landing: true);

            while (startRound == false)
            {
                PlayerInput.GetHumanInput();
                UiLogic.Instance.DoUiNavigation(PlayerInput);

                UiLogic.CheckChangeColorScheme(PlayerInput);

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
            attempts++;

            float afterRoundDelay = latestRoundResult.Completed ? 1.5f : 1.0f;
            yield return new WaitForSeconds(afterRoundDelay);
        }
    }

    IEnumerator RevealPlayer(bool landing)
    {
        Player.AvoidCollisions(true);

        float t = 1.0f;

        Vector3 targetPos = Player.transform.position;
        Vector3 baseScale = Player.transform.localScale;
        Quaternion baseRotation = Player.transform.rotation;

        if (landing)
        {
            SoundManager.Play(FxList.Instance.PlayerLanding);
            CameraRoot.DOShakePosition(PlayerLandDuration, 0.1f);

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
            CameraRoot.DOKill();
            CameraRoot.DOShakePosition(0.5f, 0.5f);
        }
        else
        {
            // not landing, just showing up
        }

        Player.SetAlpha(1.0f);
        Player.transform.SetPositionAndRotation(targetPos, Quaternion.identity);
        Player.transform.localScale = baseScale;
        Player.transform.rotation = baseRotation;

        Player.AvoidCollisions(false);
    }

    public void OnPillEaten()
    {
        UpdatePillsLeft();
    }

    public void UpdatePillsLeft()
    {
        TextPillsLeft.text = $"{PillManager.PillsLeft}/{PillManager.TotalPills}";
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
        UpdatePillsLeft();

        var pickupSequences = SludgeObjects.Where(o => o is PickupSequence).Cast<PickupSequence>().ToList();
        PickupSequenceManager.Reset(pickupSequences);

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

        while (Player.Alive)
        {
            UnityTime += Time.deltaTime;

            while (EngineTime <= UnityTime)
            {
                PlayerInput.GetHumanInput();
                DoTick();
            }

            if (levelComplete)
                break;

            if (PlayerInput.BackActive() || PlayerInput.RestartKey())
            {
                latestRoundResult.Cancelled = true;
                QuickText.Instance.ShowText("restart");
                yield break;
            }

            yield return null;
        }

        latestRoundResult.Completed = levelComplete;

        if (latestRoundResult.Completed)
        {
            SoundManager.Play(FxList.Instance.LevelComplete);
            QuickText.Instance.ShowText("Completed!");

            PlayerProgress.UpdateProgress(latestRoundResult);
        }
        else
        {
            // dead, did not complete level
        }

        Analytics.Instance.SaveStats(latestRoundResult);
    }

    public void LevelCompleted()
    {
        var pos = Player.transform.position;
        CompletedParticles.transform.position = pos;
        CompletedParticles.Emit(11);

        MarkerParticles.transform.position = pos;
        MarkerParticles.Emit(1);

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

    void DoTick()
    {
        Player.Position = Player.transform.position;

        EngineTimeMs = FrameCounter * TickSizeMs;
        EngineTime = EngineTimeMs * 0.001;

        UpdateAll();

        FrameCounter++;
    }
}

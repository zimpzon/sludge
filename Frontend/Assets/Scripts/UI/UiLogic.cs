using Sludge.Colors;
using Sludge.PlayerInputs;
using Sludge.Shared;
using Sludge.Utility;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Sludge.UI
{
	// Second script to run
	public class UiLogic : MonoBehaviour
	{
		public static UiLogic Instance;
		public bool StartCurrentScene = false;
		public UiLevelsLayout LevelLayout;
		public GameObject ButtonPlay;
		public GameObject ButtonControls;
		public GameObject ButtonExit;
		public GameObject GameRoot;
		public UiSelectionMarker UiSelectionMarker;
		public TMP_Text TextProgression;

		public int LevelCount;
		public int LevelsCompletedCount;
		public int LevelsEliteCount;
		public double GameProgressPct;

		string latestSelectedLevelUniqueId;

		private void Awake()
        {
			Instance = this;
			UiSelectionMarker.gameObject.SetActive(true);

			if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
				ButtonExit.SetActive(false);
            }

			CalcProgression();
			LevelLayout.CreateLevelsSelection(LevelList.Levels);

			UiPanels.Instance.Init();
			ColorScheme.ApplyUiColors(GameManager.Instance.CurrentUiColorScheme);
			UiPanels.Instance.SetAllActive(false);
		}

		public void CalcProgression()
        {
			GameProgressPct = SludgeUtil.CalcProgression(out LevelsCompletedCount, out LevelsEliteCount, out LevelCount);
			TextProgression.text = $"Progression: {GameProgressPct:0}%";
		}

		private void Start()
        {
			StopAllCoroutines();

			if (StartCurrentScene)
			{
				UiPanels.Instance.ShowPanel(UiPanel.Game, instant: true);
				StartCoroutine(PlayLoop(uiLevel: null));
			}
			else
			{
				StartCoroutine(MainMenuLoop());
			}
		}

		public void SetSelectionMarker(GameObject uiObject)
        {
			UiSelectionMarker.SetTarget(uiObject);
			UiSelectionMarker.gameObject.SetActive(uiObject == null ? false : true);
		}

        public void PlayClick()
		{
			StopAllCoroutines();
			StartCoroutine(LevelSelectLoop());
		}

		public void ControlsClick()
		{
			StopAllCoroutines();
			StartCoroutine(ControlsLoop());
		}

		public void DoUiNavigation(PlayerInput playerInput)
        {
			UiNavigation.TryMove(UiSelectionMarker, playerInput);
		}

		public void ExitClick()
		{
			Application.Quit();
		}

		IEnumerator MainMenuLoop()
		{
			SetSelectionMarker(ButtonPlay);

			UiPanels.Instance.ShowBackground();
			UiPanels.Instance.HidePanel(UiPanel.Game);
			UiPanels.Instance.HidePanel(UiPanel.LevelSelect);
			UiPanels.Instance.HidePanel(UiPanel.BetweenRoundsMenu);
			UiPanels.Instance.HidePanel(UiPanel.Settings);
			UiPanels.Instance.HidePanel(UiPanel.Settings2);

			UiPanels.Instance.ShowPanel(UiPanel.MainMenu);

			UiNavigation.OnNavigationChanged = null;
			UiNavigation.OnNavigationSelected = (go) =>
			{
				if (go == ButtonPlay)
					PlayClick();
				else if (go == ButtonControls)
					ControlsClick();
				else if (go == ButtonExit)
					ExitClick();
			};

			// Allow dynamic renderscale now that we are up and running
			RenderSize.AllowDownsizingRenderScale = true;

			while (true)
			{
				GameManager.PlayerInput.GetHumanInput();
				CheckChangeColorScheme(GameManager.PlayerInput);
				DoUiNavigation(GameManager.PlayerInput);
                yield return null;
			}
		}

		IEnumerator PlayLoop(UiLevel uiLevel)
		{
			GameManager.Instance.LoadLevel(uiLevel);
			UiPanels.Instance.HideBackground();

			UiPanels.Instance.ShowPanel(UiPanel.Game);
			UiPanels.Instance.HidePanel(UiPanel.MainMenu);

			SetSelectionMarker(null);
			GameManager.Instance.StartLevel();

			while (true)
			{
				// Wait for game sequence to end. Important: Only game loop calls GetHumanInput since coroutine ticks and engine ticks are not synced.
				yield return null;
			}
		}

		public void BackFromGame()
        {
			StopAllCoroutines();
			CalcProgression();
			LevelLayout.UpdateVisualHints();
			ColorScheme.ApplyUiColors(GameManager.Instance.CurrentUiColorScheme);

			UiPanels.Instance.HidePanel(UiPanel.Game);
			UiPanels.Instance.ShowPanel(UiPanel.MainMenu);
			UiPanels.Instance.ShowPanel(UiPanel.LevelSelect);
			UiPanels.Instance.ShowBackground();
			StartCoroutine(LevelSelectLoop());
		}

		void CheckChangeColorScheme(PlayerInput input)
        {
			if (input.IsTapped(PlayerInput.InputType.ColorNext))
			{
				GameManager.Instance.SetColorScheme(GameManager.Instance.ColorSchemeList.GetNext());
			}
			if (input.IsTapped(PlayerInput.InputType.ColorPrev))
			{
				GameManager.Instance.SetColorScheme(GameManager.Instance.ColorSchemeList.GetPrev());
			}
		}

		IEnumerator ControlsLoop()
		{
			UiNavigation.OnNavigationChanged = null;
			UiNavigation.OnNavigationSelected = null;
			yield return UiPanels.Instance.ShowPanel(UiPanel.Settings);

			while (true)
			{
				GameManager.PlayerInput.GetHumanInput();
				CheckChangeColorScheme(GameManager.PlayerInput);

				if (GameManager.PlayerInput.IsTapped(PlayerInput.InputType.Back))
				{
					UiPanels.Instance.HidePanel(UiPanel.Settings);
					StopAllCoroutines();
					StartCoroutine(MainMenuLoop());
					break;
				}

				yield return null;
			}
		}

		IEnumerator LevelSelectLoop()
		{
			UiNavigation.OnNavigationChanged = null;
			UiNavigation.OnNavigationSelected = null;

			yield return UiPanels.Instance.ShowPanel(UiPanel.LevelSelect);

			double charsShown = 0;
			double charRevealSpeed = 150;
			var uilevelSelection = UiPanels.Instance.PanelLevelSelect.GetComponent<UiLevelSelection>();

			UiNavigation.OnNavigationSelected = (go) =>
			{
				var uiLevel = go.GetComponent<UiLevel>();
				if (!uiLevel.IsUnlocked)
                {
					// TODO: Nope-sound
					return;
                }

				UiPanels.Instance.HidePanel(UiPanel.LevelSelect);
				StopAllCoroutines();
				StartCoroutine(PlayLoop(uiLevel));
			};

			UiNavigation.OnNavigationChanged = OnNavigationChanged;

			void OnNavigationChanged(GameObject go)
            {
				var uiLevel = go.GetComponent<UiLevel>();
				var levelData = uiLevel.LevelData;
				var levelStatus = PlayerProgress.GetLevelProgress(levelData.UniqueId);
				var difficulty = uiLevel.LevelData.Difficulty;
				string levelText;
				string statsText;
				string timingsText = "";
				string otherText = "";
				if (uiLevel.IsUnlocked)
                {
					timingsText = $"Time limit\t<mspace=0.5em>{levelData.TimeSeconds,6:0.000}s</mspace>\nComplete\t<mspace=0.5em>{levelData.EliteCompletionTimeSeconds,6:0.000}s</mspace>\nYour best\t<mspace=0.5em>-.---s</mspace>";
					otherText = $"Attempts\t0";

					levelText = $"{LevelData.DifficultyIds[(int)difficulty]} {(uiLevel.LevelIndex + 1):00} - {levelData.Name}";
					if (uiLevel.Status == PlayerProgress.LevelStatus.NotCompleted || uiLevel.Status == PlayerProgress.LevelStatus.Escaped)
					{
						Color masteredColor = ColorScheme.GetColor(GameManager.Instance.CurrentUiColorScheme, SchemeColor.UiLevelMastered);
						statsText = $"Escape in {SludgeUtil.ColorWrap($"{levelData.EliteCompletionTimeSeconds:0.000}", masteredColor)}s to complete chamber";
					}
					else
                    {
						statsText = "You have completed this chamber";
                    }
                }
				else
                {
					levelText = "<Locked>";
					statsText = "Escape more chambers to unlock";
				}

				uilevelSelection.TextLevelName.text = levelText;
				uilevelSelection.TextLevelInfo.text = statsText;
				uilevelSelection.TextLevelTimings.text = timingsText;
				uilevelSelection.TextLevelOtherInfo.text = otherText;

				charsShown = 0;
				latestSelectedLevelUniqueId = levelData.UniqueId;
			}

			// Reselect latest selected
			if (latestSelectedLevelUniqueId == null)
				latestSelectedLevelUniqueId = LevelLayout.LevelItems[0].levelScript.LevelData.UniqueId;

			var level = LevelLayout.GetLevelFromUniqueId(latestSelectedLevelUniqueId);
			SetSelectionMarker(level.go);
			OnNavigationChanged(level.go);

			while (true)
            {
				GameManager.PlayerInput.GetHumanInput();
				CheckChangeColorScheme(GameManager.PlayerInput);
				DoUiNavigation(GameManager.PlayerInput);

				int intCharsShown = (int)charsShown;
				uilevelSelection.TextLevelName.maxVisibleCharacters = intCharsShown >> 1;
				uilevelSelection.TextLevelInfo.maxVisibleCharacters = intCharsShown;
				uilevelSelection.TextLevelTimings.maxVisibleCharacters = intCharsShown;
				uilevelSelection.TextLevelOtherInfo.maxVisibleCharacters = intCharsShown;

				charsShown += charRevealSpeed * Time.deltaTime;

				if (GameManager.PlayerInput.IsTapped(PlayerInput.InputType.Back))
                {
					UiPanels.Instance.HidePanel(UiPanel.LevelSelect);
					StopAllCoroutines();
					StartCoroutine(MainMenuLoop());
					break;
				}

				yield return null;
			}
		}
	}
}
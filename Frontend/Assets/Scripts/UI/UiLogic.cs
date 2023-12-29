using Sludge.Colors;
using Sludge.PlayerInputs;
using Sludge.Utility;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Sludge.UI
{
	// Second script to run
	public class UiLogic : MonoBehaviour
	{
		public static UiLogic Instance;
		public bool StartCurrentScene = false;
		public UiLevelsLayout LevelLayoutCasual;
		public UiLevelsLayout LevelLayoutHard;
		public GameObject ButtonPlayCasual;
        public GameObject ButtonPlayHard;
        public GameObject ButtonControls;
		public GameObject ButtonExit;
		public GameObject GameRoot;
		public UiSelectionMarker UiSelectionMarker;
		public TMP_Text TextWorldWideAttempts;

		public static long WorldWideAttempts;
		public int LevelCount;
		public int LevelsCompletedCount;
		public int LevelsEliteCount;
		public double GameProgressPct = -1;

		public int latestSelectedCasualLevelId = -1;
		public int latestSelectedHardLevelId = -1;
		public PlayerProgress.LevelNamespace latestSelectedLevelNamespace;

		private void Awake()
        {
			Instance = this;
			UiSelectionMarker.gameObject.SetActive(true);

			if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
				ButtonExit.SetActive(false);
            }

			LevelLayoutCasual.CreateLevelsSelection(LevelList.CasualLevels, PlayerProgress.LevelNamespace.Casual);
			LevelLayoutHard.CreateLevelsSelection(LevelList.HardLevels, PlayerProgress.LevelNamespace.Hard);

			UiPanels.Instance.Init();
			ColorScheme.ApplyUiColors(GameManager.I.CurrentUiColorScheme);
			UiPanels.Instance.SetAllActive(false);
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
				UpdateWorldWideAttempts();
				StartCoroutine(MainMenuLoop());
			}
		}

		public void SetSelectionMarker(GameObject uiObject)
        {
			UiSelectionMarker.SetTarget(uiObject);
			UiSelectionMarker.gameObject.SetActive(uiObject == null ? false : true);
		}

		void UpdateWorldWideAttempts()
        {
			StartCoroutine(GetWorldWideAttempts());
        }

		IEnumerator GetWorldWideAttempts()
		{
			using var request = UnityWebRequest.Get("https://sludgefunctions.azurewebsites.net/api/world-wide-attempts");
			yield return request.SendWebRequest();

			string response = request.downloadHandler.text;
			if (!long.TryParse(response, out long totalAttempts))
            {
				Debug.LogWarning($"Getting world wide attempts returned invalid data: '{response}'");
				TextWorldWideAttempts.text = $"World wide attempts: ?";
				yield break;
			}

			TextWorldWideAttempts.text = $"World wide attempts: {totalAttempts}";
		}

		public void ShowCasualLevelsClick()
		{
			StopAllCoroutines();
			StartCoroutine(LevelSelectLoop(PlayerProgress.LevelNamespace.Casual));
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
			SetSelectionMarker(ButtonPlayCasual);

			UiPanels.Instance.ShowBackground();
			UiPanels.Instance.HidePanel(UiPanel.Game);
			UiPanels.Instance.HidePanel(UiPanel.LevelSelect);
			UiPanels.Instance.HidePanel(UiPanel.BetweenRoundsMenu);
			UiPanels.Instance.HidePanel(UiPanel.Settings);

			UiPanels.Instance.ShowPanel(UiPanel.MainMenu);
			
			UiNavigation.OnNavigationChanged = null;
			UiNavigation.OnNavigationSelected = (go) =>
			{
                if (go == ButtonPlayCasual)
					ShowCasualLevelsClick();
				else if (go == ButtonControls)
					ControlsClick();
				else if (go == ButtonExit)
					ExitClick();
			};

			while (true)
			{
				GameManager.PlayerInput.GetHumanInput();
				CheckChangeColorScheme(GameManager.PlayerInput);
				DoUiNavigation(GameManager.PlayerInput);

				if (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.RightControl))
					PlayerPrefs.DeleteAll();

                yield return null;
			}
		}

		IEnumerator PlayLoop(UiLevel uiLevel)
		{
			GameManager.I.LoadLevel(uiLevel);
			UiPanels.Instance.HideBackground();

			UiPanels.Instance.ShowPanel(UiPanel.Game);
			UiPanels.Instance.HidePanel(UiPanel.MainMenu);

			SetSelectionMarker(null);
			GameManager.I.StartLevel();

			while (true)
			{
				// Wait for game sequence to end. Important: Only game loop calls GetHumanInput since coroutine ticks and engine ticks are not synced.
				yield return null;
			}
		}

		public void BackFromGame()
        {
			StopAllCoroutines();
			UpdateWorldWideAttempts();

			LevelLayoutCasual.UpdateVisualHints();
			LevelLayoutHard.UpdateVisualHints();

			ColorScheme.ApplyUiColors(GameManager.I.CurrentUiColorScheme);

			UiPanels.Instance.HidePanel(UiPanel.Game);
			UiPanels.Instance.ShowPanel(UiPanel.MainMenu);
			UiPanels.Instance.ShowPanel(UiPanel.LevelSelect);
			UiPanels.Instance.ShowBackground();
			StartCoroutine(LevelSelectLoop(latestSelectedLevelNamespace));
		}

		public static void CheckChangeColorScheme(PlayerInput input)
        {
			if (input.IsTapped(PlayerInput.InputType.ColorNext))
			{
				GameManager.I.SetColorScheme(GameManager.I.ColorSchemeList.GetNext());
			}
			if (input.IsTapped(PlayerInput.InputType.ColorPrev))
			{
				GameManager.I.SetColorScheme(GameManager.I.ColorSchemeList.GetPrev());
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

		IEnumerator LevelSelectLoop(PlayerProgress.LevelNamespace levelNamespace)
		{
			latestSelectedLevelNamespace = levelNamespace;

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
				string levelText;
				if (uiLevel.IsUnlocked)
                {
					levelText = $"{uiLevel.LevelData.Namespace}{(uiLevel.LevelIndex + 1):000}";
                }
				else
                {
					levelText = "<Locked>";
				}

				uilevelSelection.TextLevelName.text = levelText;

				charsShown = 0;
				if (latestSelectedLevelNamespace == PlayerProgress.LevelNamespace.Casual)
					latestSelectedCasualLevelId = levelData.LevelId;
				else
					latestSelectedHardLevelId = levelData.LevelId;
			}

			LevelItem level = latestSelectedLevelNamespace == PlayerProgress.LevelNamespace.Casual ? LevelLayoutCasual.GetLevelFromId(latestSelectedCasualLevelId) : LevelLayoutHard.GetLevelFromId(latestSelectedHardLevelId);
			SetSelectionMarker(level.go);
			OnNavigationChanged(level.go);

			while (true)
            {
				GameManager.PlayerInput.GetHumanInput();
				CheckChangeColorScheme(GameManager.PlayerInput);
				DoUiNavigation(GameManager.PlayerInput);

				int intCharsShown = (int)charsShown;
				uilevelSelection.TextLevelName.maxVisibleCharacters = intCharsShown >> 1;

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
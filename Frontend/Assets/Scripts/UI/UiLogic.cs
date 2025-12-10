using DG.Tweening;
using Sludge.PlayerInputs;
using Sludge.Utility;
using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using static UiNavigation;

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

		[NonSerialized] public UiNavigationGroup ActiveNavigationGroup;

		public static long WorldWideAttempts;
        [NonSerialized] public int LevelCount;
        [NonSerialized] public int LevelsCompletedCount;
        [NonSerialized] public int LevelsEliteCount;
        [NonSerialized] public double GameProgressPct = -1;
		[NonSerialized] public PlayerProgress.LevelNamespace latestSelectedLevelNamespace;
        [NonSerialized] public int lastSelectedCasualLevelId = -1;
        [NonSerialized] public int lastSelectedHardLevelId = -1;

        private void Awake()
        {
			Instance = this;
			UiSelectionMarker.gameObject.SetActive(true);

            ButtonExit.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);

			LevelLayoutCasual.CreateLevelsSelection(LevelList.CasualLevels, PlayerProgress.LevelNamespace.Casual);
			LevelLayoutHard.CreateLevelsSelection(LevelList.HardLevels, PlayerProgress.LevelNamespace.Hard);

			UiPanels.Instance.Init();
			UiPanels.Instance.SetAllActive(false);
		}

		private void Start()
        {
			StopAllCoroutines();

            Playfab.Login();

            if (StartCurrentScene)
			{
				UiPanels.Instance.ShowPanel(UiPanel.Game, instant: true);
				StartCoroutine(PlayLoop(uiLevel: null));
			}
			else
			{
				//UpdateWorldWideAttempts();
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

		public void ShowLevelsSelect(PlayerProgress.LevelNamespace levelSelection)
		{
			StopAllCoroutines();
			StartCoroutine(LevelSelectLoop(levelSelection));
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

		GameObject mainMenuLatestSelection;

		IEnumerator MainMenuLoop()
		{
			ActiveNavigationGroup = UiNavigationGroup.MainMenu;

			var selection = mainMenuLatestSelection ?? ButtonPlayCasual;
			SetSelectionMarker(selection);

			UiPanels.Instance.ShowBackground();
			yield return UiPanels.Instance.HidePanel(UiPanel.Game);
			yield return UiPanels.Instance.HidePanel(UiPanel.LevelSelect);
			yield return UiPanels.Instance.HidePanel(UiPanel.Settings);

			UiPanels.Instance.ShowPanel(UiPanel.MainMenu);

			UiNavigation.OnNavigationChanged = (go) =>
			{
				go.transform.DOKill();
                go.transform.DOPunchScale(Vector3.one * 0.05f, 0.3f); // TODO TWEEN
            };

			UiNavigation.OnNavigationSelected = (go) =>
			{
				mainMenuLatestSelection = go;

                if (go == ButtonPlayCasual)
					ShowLevelsSelect(PlayerProgress.LevelNamespace.Casual);
                else if (go == ButtonPlayHard)
                    ShowLevelsSelect(PlayerProgress.LevelNamespace.Hard);
                else if (go == ButtonControls)
					ControlsClick();
				else if (go == ButtonExit)
					ExitClick();
			};

			while (true)
			{
				GameManager.PlayerInput.GetHumanInput();
				DoUiNavigation(GameManager.PlayerInput);

				CheckCheats();
                yield return null;
			}
		}

		void CheckCheats()
		{
				if (Input.GetKeyDown(KeyCode.D) && Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.RightControl))
					PlayerPrefs.DeleteAll();

                if (Input.GetKeyDown(KeyCode.U) && Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.RightControl))
				{
					for (int i = 0; i < LevelList.CasualLevels.Count; i++)
					{
						var level = LevelList.CasualLevels[i];
						PlayerProgress.saveGame.CasualLevelsSeen[level.LevelId] = new PlayerProgress.LevelStats { Completions = 1 };

					}
                    for (int i = 0; i < LevelList.HardLevels.Count; i++)
                    {
                        var level = LevelList.HardLevels[i];
                        PlayerProgress.saveGame.HardLevelsSeen[level.LevelId] = new PlayerProgress.LevelStats { Completions = 1 };

                    }
                }

		}

		IEnumerator PlayLoop(UiLevel uiLevel)
		{ 
            ActiveNavigationGroup = UiNavigationGroup.InGame;

            GameManager.I.LoadLevel(uiLevel);
			UiPanels.Instance.HideBackground();

            yield return UiPanels.Instance.ShowPanel(UiPanel.Game);
			yield return UiPanels.Instance.HidePanel(UiPanel.MainMenu);

			SetSelectionMarker(null);
			GameManager.I.StartLevel();

			while (true)
			{
                // Wait for game sequence to end. When does this exit? The rest runs in coroutines. So StopAllCoroutines?
				// Important: Only game loop calls GetHumanInput since coroutine ticks and engine ticks are not synced.
                yield return null;
			}
		}

		public void BackFromGame()
        {
            //UpdateWorldWideAttempts();

            GameManager.IsInMenu = true;

            LevelLayoutCasual.UpdateVisualHints();
			LevelLayoutHard.UpdateVisualHints();
			GameManager.I.UpdateTextComplete();

			UiPanels.Instance.HidePanel(UiPanel.Game);
			UiPanels.Instance.ShowPanel(UiPanel.MainMenu);
			UiPanels.Instance.ShowPanel(UiPanel.LevelSelect);
			UiPanels.Instance.ShowBackground();

			StopAllCoroutines();
            StartCoroutine(LevelSelectLoop(latestSelectedLevelNamespace));
		}

		IEnumerator ControlsLoop()
		{
            ActiveNavigationGroup = UiNavigationGroup.Settings;

            UiNavigation.OnNavigationChanged = null;
			UiNavigation.OnNavigationSelected = null;
            
			SoundManager.Play(FxList.Instance.UiShowMenu);
            yield return UiPanels.Instance.ShowPanel(UiPanel.Settings);

			//UiPanels.Instance.PanelSettings.transform.DOKill();
			//UiPanels.Instance.PanelSettings.transform.DOPunchPosition(Vector3.up * 4, 0.3f); // TODO TWEEN

            while (true)
			{
				GameManager.PlayerInput.GetHumanInput();

				if (GameManager.PlayerInput.IsTapped(PlayerInput.InputType.Back))
				{
                    SoundManager.Play(FxList.Instance.UiHideMenu);
                    UiPanels.Instance.HidePanel(UiPanel.Settings);
					StopAllCoroutines();
					StartCoroutine(MainMenuLoop());
					break;
				}

				yield return null;
			}
		}

		StringBuilder sb = new StringBuilder();
		void UpdateSelectedLevelStats(UiLevel uiLevel)
		{
            var savedStats = PlayerProgress.GetSavedStats(uiLevel.LevelData.Namespace, uiLevel.LevelData.LevelId);
            string bestPart = savedStats.BestTime >= 0 ? $"{savedStats.BestTime,8:0.000}" : "       -";

            sb.Clear();
            sb.AppendLine($"Gold time\t{uiLevel.LevelData.TargetTime,8:0.000}");
            sb.AppendLine($"Best time\t{bestPart}");
            sb.AppendLine($"Attempts\t{savedStats.Attempts,8}");

            GameManager.I.TextSelectedLevelStats.text = sb.ToString();
		}

		IEnumerator LevelSelectLoop(PlayerProgress.LevelNamespace levelNamespace)
		{
            ActiveNavigationGroup = UiNavigationGroup.LevelSelect;
            latestSelectedLevelNamespace = levelNamespace;

            UiNavigation.OnNavigationChanged = null;
			UiNavigation.OnNavigationSelected = null;

            var uilevelSelection = UiPanels.Instance.PanelLevelSelect.GetComponent<UiLevelSelection>();
            uilevelSelection.TextLevelNamespace.text = PlayerProgress.NamespaceDisplayName(latestSelectedLevelNamespace);
            uilevelSelection.TextLevelName.text = "";

            SoundManager.Play(FxList.Instance.UiShowMenu);
            yield return UiPanels.Instance.ShowPanel(UiPanel.LevelSelect);

			//UiPanels.Instance.PanelLevelSelect.transform.DOKill();
			//UiPanels.Instance.PanelLevelSelect.transform.DOPunchPosition(Vector3.left * 4, 0.3f); // TODO TWEEN

            double charsShown = 0;
			double charRevealSpeed = 150;
            double statsCharsShown = 0;

            UiNavigation.OnNavigationSelected = (go) =>
			{
				var uiLevel = go.GetComponent<UiLevel>();
				if (!uiLevel.IsUnlocked)
                {
					SoundManager.Play(FxList.Instance.UiNope);
					return;
                }

				UiPanels.Instance.HidePanel(UiPanel.LevelSelect);
				StopAllCoroutines();
				StartCoroutine(PlayLoop(uiLevel));
			};

			UiNavigation.OnNavigationChanged = OnNavigationChanged;

			void OnNavigationChanged(GameObject go)
            {
                go.transform.DOKill();
                go.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f); // TODO TWEEN

                var uiLevel = go.GetComponent<UiLevel>();
                if (latestSelectedLevelNamespace == PlayerProgress.LevelNamespace.Casual)
				{
                    lastSelectedCasualLevelId = uiLevel.LevelIndex + 1;
                }
                else if (latestSelectedLevelNamespace == PlayerProgress.LevelNamespace.Hard)
                {
                    lastSelectedHardLevelId = uiLevel.LevelIndex + 1;
                }

                var levelData = uiLevel.LevelData;
				string levelText;
				if (uiLevel.IsUnlocked)
                {
					levelText = $"{uiLevel.LevelData.LevelName}";
                }
				else
                {
					levelText = "<Locked>";
				}

                UpdateSelectedLevelStats(uiLevel);

                uilevelSelection.TextLevelName.text = levelText;

				charsShown = 0;
                statsCharsShown = 0;
			}

            // show either casual or hard levels
            LevelLayoutCasual.gameObject.SetActive(latestSelectedLevelNamespace == PlayerProgress.LevelNamespace.Casual);
            LevelLayoutHard.gameObject.SetActive(latestSelectedLevelNamespace == PlayerProgress.LevelNamespace.Hard);
			
			// select a level in the current namespace/difficulty
            LevelItem level = latestSelectedLevelNamespace == PlayerProgress.LevelNamespace.Casual ?
				LevelLayoutCasual.GetLevelFromId(lastSelectedCasualLevelId) :
				LevelLayoutHard.GetLevelFromId(lastSelectedHardLevelId);

            SetSelectionMarker(level.go);
			OnNavigationChanged(level.go);

			while (true)
            {
				GameManager.PlayerInput.GetHumanInput();
				DoUiNavigation(GameManager.PlayerInput);

				int intCharsShown = (int)charsShown;
				uilevelSelection.TextLevelName.maxVisibleCharacters = intCharsShown >> 1;

				int intStatsCharsShown = (int)statsCharsShown;
				GameManager.I.TextSelectedLevelStats.maxVisibleCharacters = intStatsCharsShown;

				charsShown += charRevealSpeed * Time.deltaTime;
                statsCharsShown += charRevealSpeed * Time.deltaTime;

				if (GameManager.PlayerInput.IsTapped(PlayerInput.InputType.Back))
                {
                    SoundManager.Play(FxList.Instance.UiHideMenu);
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
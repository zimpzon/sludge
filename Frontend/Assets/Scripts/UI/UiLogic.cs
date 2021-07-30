using Sludge.PlayerInputs;
using System.Collections;
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
		public GameObject ButtonExit;
		public GameObject GameRoot;
		public UiSelectionMarker UiSelectionMarker;

		string latestSelectedLevelId;
		PlayerInput playerInput = new PlayerInput();

		private void Awake()
        {
			Instance = this;

			if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
				ButtonExit.SetActive(false);
            }

			LevelLayout.CreateLevelsSelection(LevelList.Levels);
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
			UiPanels.Instance.ShowPanel(UiPanel.MainMenu);

			UiNavigation.OnNavigationChanged = null;
			UiNavigation.OnNavigationSelected = (go) =>
			{
				if (go == ButtonPlay)
					PlayClick();
				else if (go == ButtonExit)
					ExitClick();
			};

			while (true)
			{
				CheckInput(playerInput);
				DoUiNavigation(playerInput);
                yield return null;
			}
		}

		IEnumerator PlayLoop(UiLevel uiLevel)
		{
			GameManager.Instance.LoadLevel(uiLevel?.levelData);
			UiPanels.Instance.HideBackground();

			UiPanels.Instance.ShowPanel(UiPanel.Game);
			UiPanels.Instance.HidePanel(UiPanel.MainMenu);

			SetSelectionMarker(null);
			GameManager.Instance.StartLevel();

			while (true)
			{
				// Wait for game sequence to end
				yield return null;
			}
		}

		public void BackFromGame()
        {
			StopAllCoroutines();
			UiPanels.Instance.HidePanel(UiPanel.Game);
			UiPanels.Instance.ShowPanel(UiPanel.MainMenu);
			UiPanels.Instance.ShowPanel(UiPanel.LevelSelect);
			UiPanels.Instance.ShowBackground();
			StartCoroutine(LevelSelectLoop());
		}

		void ReselectLatestLevel()
        {
			if (latestSelectedLevelId == null)
				latestSelectedLevelId = LevelList.Levels[0].Id;

			var level = LevelLayout.GetLevelFromId(latestSelectedLevelId);
			SetSelectionMarker(level.go);
		}

		void CheckChangeColorScheme(PlayerInput input)
        {
			if (input.ColorNextTap)
			{
				GameManager.Instance.SetColorScheme(GameManager.Instance.ColorSchemeList.GetNext());
			}
			else if (input.ColorPrevTap)
			{
				GameManager.Instance.SetColorScheme(GameManager.Instance.ColorSchemeList.GetPrev());
			}
		}

		void CheckInput(PlayerInput input)
        {
			input.GetHumanInput();
			CheckChangeColorScheme(input);
        }

		IEnumerator LevelSelectLoop()
		{
			UiNavigation.OnNavigationChanged = null;
			UiNavigation.OnNavigationSelected = null;

			yield return UiPanels.Instance.ShowPanel(UiPanel.LevelSelect);
			ReselectLatestLevel();

			UiNavigation.OnNavigationSelected = (go) =>
			{
				UiPanels.Instance.HidePanel(UiPanel.LevelSelect);
				var uiLevel = go.GetComponent<UiLevel>(); 
				StartCoroutine(PlayLoop(uiLevel));
			};

			UiNavigation.OnNavigationChanged = (go) =>
			{
				var uiLevel = go.GetComponent<UiLevel>();
				UiPanels.Instance.PanelLevelSelect.GetComponent<UiLevelSelection>().TextLevelName.text = uiLevel.levelData.Name;
				latestSelectedLevelId = uiLevel.levelData.Id;
			};

			while (true)
            {
				CheckInput(playerInput);
				DoUiNavigation(playerInput);

				if (playerInput.BackTap)
                {
					UiPanels.Instance.HidePanel(UiPanel.LevelSelect);
					StartCoroutine(MainMenuLoop());
					break;
				}

				yield return null;
			}
		}
	}
}
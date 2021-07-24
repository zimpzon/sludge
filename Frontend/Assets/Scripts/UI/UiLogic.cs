using Sludge.PlayerInputs;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Sludge.Utility;

namespace Sludge.Editor
{
	public class UiLogic : MonoBehaviour
	{
		public GameObject PanelBackground;
		public GameObject PanelMainMenu;
		public GameObject PanelLevelSelect;
		public GameObject PanelGame;
		public UiLevelsLayout LevelLayout;
		public GameObject ButtonPlay;
		public GameObject ButtonExit;
		public GameObject GameRoot;
		public UiSelectionMarker UiSelectionMarker;

		Vector2 panelLevelSelectHidePos;
		Vector2 panelLevelSelectShowPos = new Vector3(110, -46);
		Vector2 panelMainMenuHidePos;
		Vector2 panelMainMenuShowPos;

		PlayerInput playerInput = new PlayerInput();

		private void Awake()
        {
			Startup.StaticInit();

			//if (GameRoot.activeSelf)
   //         {
			//	// Easy debugging
			//	PanelBackground.SetActive(false);
			//	PanelMainMenu.SetActive(false);
			//	PanelLevelSelect.SetActive(false);
			//	PanelGame.SetActive(true);
			//	return;
			//}

			if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
				ButtonExit.SetActive(false);
            }

			PanelBackground.SetActive(true);
			PanelMainMenu.SetActive(true);
			PanelLevelSelect.SetActive(true);

			panelLevelSelectHidePos = PanelLevelSelect.GetComponent<RectTransform>().anchoredPosition;
			panelMainMenuShowPos = PanelMainMenu.GetComponent<RectTransform>().anchoredPosition;
			panelMainMenuHidePos = panelMainMenuShowPos + Vector2.left * 760;
		}

		private void Start()
        {
			UiSelectionMarker.gameObject.SetActive(true);
			LevelLayout.CreateLevels(LevelList.Levels);
			StartCoroutine(MainMenuLoop());
		}

		void SetSelectionMarker(GameObject uiObject)
        {
			UiSelectionMarker.SetTarget(uiObject);
			UiSelectionMarker.gameObject.SetActive(uiObject == null ? false : true);
		}

        public void PlayClick()
		{
			StopAllCoroutines();
			StartCoroutine(LevelSelectLoop());
		}

		void DoUiNavigation(PlayerInput playerInput)
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
                playerInput.GetHumanInput();
				DoUiNavigation(playerInput);
                yield return null;
			}
		}

		IEnumerator PlayLoop(UiLevel uiLevel)
		{
			GameManager.Instance.LoadLevel(uiLevel.levelData);

			var panelMain = PanelMainMenu.GetComponent<RectTransform>();
			PanelBackground.SetActive(false);
			yield return panelMain.DOAnchorPos(panelMainMenuHidePos, 0.1f).SetEase(Ease.OutCubic).WaitForCompletion();

			GameManager.Instance.StartLevel();

			while (true)
			{
				playerInput.GetHumanInput();
				yield return null;
			}
		}

		string latestSelectedLevelId;

		void ReselectLatestLevel()
        {
			if (latestSelectedLevelId == null)
				latestSelectedLevelId = LevelList.Levels[0].Id;

			var level = LevelLayout.GetLevelFromId(latestSelectedLevelId);
			SetSelectionMarker(level.go);
		}

		IEnumerator LevelSelectLoop()
		{
			UiNavigation.OnNavigationChanged = null;
			UiNavigation.OnNavigationSelected = null;

			var panelTrans = PanelLevelSelect.GetComponent<RectTransform>();
			yield return panelTrans.DOAnchorPos(panelLevelSelectShowPos, 0.1f).SetEase(Ease.OutCubic).WaitForCompletion();

			ReselectLatestLevel();

			UiNavigation.OnNavigationSelected = (go) =>
			{
				var uiLevel = go.GetComponent<UiLevel>();
				panelTrans.DOAnchorPos(panelLevelSelectHidePos, 0.1f);

				StartCoroutine(PlayLoop(uiLevel));
			};

			UiNavigation.OnNavigationChanged = (go) =>
			{
				var uiLevel = go.GetComponent<UiLevel>();
				PanelLevelSelect.GetComponent<UiLevelSelection>().TextLevelName.text = uiLevel.levelData.Name;
				latestSelectedLevelId = uiLevel.levelData.Id;
			};

			while (true)
            {
                playerInput.GetHumanInput();
				DoUiNavigation(playerInput);

				if (playerInput.BackTap)
                {
					yield return panelTrans.DOAnchorPos(panelLevelSelectHidePos, 0.1f).WaitForCompletion();
					StartCoroutine(MainMenuLoop());
					break;
				}
				yield return null;
			}
		}
	}
}
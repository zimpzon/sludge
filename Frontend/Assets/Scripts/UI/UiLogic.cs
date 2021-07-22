using Sludge.PlayerInputs;
using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace Sludge.Editor
{
	public class UiLogic : MonoBehaviour
	{
		public GameObject LevelSelectIconPrefab;
		public GameObject PanelBackground;
		public GameObject PanelMainMenu;
		public GameObject PanelLevelSelect;
		public GameObject ButtonPlay;
		public GameObject ButtonExit;
		public UiSelectionMarker UiSelectionMarker;

		PlayerInput playerInput = new PlayerInput();

		private void Awake()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
				ButtonExit.SetActive(false);
            }

			PanelBackground.SetActive(true);
			PanelMainMenu.SetActive(true);
			PanelLevelSelect.SetActive(false);

			SetSelection(ButtonPlay);
			StartCoroutine(MainMenuLoop());

			//PanelMainMenu.GetComponent<RectTransform>().DOAnchorPos(Vector2.down * 400, 0.0f);
			//PanelMainMenu.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 1.0f);
		}

		void SetSelection(GameObject uiObject)
        {
			UiSelectionMarker.target = uiObject;
			UiSelectionMarker.gameObject.SetActive(uiObject == null ? false : true);
		}

        public void PlayClick()
		{
			StopAllCoroutines();

			SetSelection(null);
			PanelLevelSelect.SetActive(true);
			PanelMainMenu.SetActive(false);

			StartCoroutine(LevelSelectLoop());
		}

		public void ExitClick()
		{
			Application.Quit();
		}

		IEnumerator MainMenuLoop()
		{
			while (true)
			{
                playerInput.GetHumanInput();
                if (playerInput.SelectTap)
                {
                    if (UiSelectionMarker.target == ButtonPlay)
                    {
                        PlayClick();
                        break;
                    }
                    else if (UiSelectionMarker.target == ButtonExit)
                    {
                        ExitClick();
                        break;
                    }
                }

                yield return null;
			}
		}

		IEnumerator LevelSelectLoop()
		{
			//var panelTrans = PanelLevelSelect.GetComponent<RectTransform>();

			//// Move into view
			while (true)
			{
				yield return null;
			}

            while (true)
            {
                playerInput.GetHumanInput();
				yield return null;
			}
		}
	}
}
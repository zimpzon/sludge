using UnityEngine;

namespace Sludge.Editor
{
	public class MainMenuLogic : MonoBehaviour
	{
		public void PlayClick()
		{
		}

		public void MyLevelsClick()
        {
			GameManager.Instance.CanvasMainMenu.gameObject.SetActive(false);
			GameManager.Instance.CanvasMyLevels.gameObject.SetActive(true);
		}
	}
}
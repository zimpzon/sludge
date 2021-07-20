using System.Collections;
using TMPro;
using UnityEngine;

namespace Sludge.Editor
{
	public class MainMenuLogic : MonoBehaviour
	{
		public TMP_Text[] menuItems;

		public IEnumerator MainMenuLoop()
		{
			while (true)
			{
				var mousePos = Input.mousePosition;
				Debug.Log(mousePos);
				yield return null;
			}
		}
	}
}
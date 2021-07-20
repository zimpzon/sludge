using System.Collections;
using UnityEngine;

namespace Sludge.Editor
{
	public class EditorLogic : MonoBehaviour
	{
		public IEnumerator EditorLoop()
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
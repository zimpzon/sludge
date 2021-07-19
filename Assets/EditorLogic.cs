using System.Collections;
using UnityEngine;

namespace Sludge.Editor
{
	public class EditorLogic
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
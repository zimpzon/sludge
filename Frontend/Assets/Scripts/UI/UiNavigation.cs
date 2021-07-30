using Sludge.PlayerInputs;
using System;
using UnityEngine;

public class UiNavigation : MonoBehaviour
{
    public GameObject Left;
    public GameObject Right;
    public GameObject Up;
    public GameObject Down;
	public bool Enabled = true;

	public static Action<GameObject> OnNavigationChanged;
	public static Action<GameObject> OnNavigationSelected;

	public static void TryMove(UiSelectionMarker selectionMarker, PlayerInput playerInput)
    {
		if (selectionMarker.target == null)
			return;

		var navComponent = selectionMarker.target.GetComponent<UiNavigation>();
		if (navComponent == null)
		{
			Debug.LogError($"Cannot navigate on gameobject {selectionMarker.target.name}, it is missing component {nameof(UiNavigation)}");
			return;
		}

		if (playerInput.SelectTap && OnNavigationSelected != null)
			OnNavigationSelected(selectionMarker.target);

		if (playerInput.DownTap)
			TryMove(selectionMarker, navComponent.Down);
		if (playerInput.UpTap)
			TryMove(selectionMarker, navComponent.Up);
		if (playerInput.LeftTap)
			TryMove(selectionMarker, navComponent.Left);
		if (playerInput.RightTap)
			TryMove(selectionMarker, navComponent.Right);
	}

	static void TryMove(UiSelectionMarker selectionMarker, GameObject moveTarget)
	{
		if (moveTarget != null && moveTarget.activeSelf)
		{
			selectionMarker.SetTarget(moveTarget);

			if (OnNavigationChanged != null)
				OnNavigationChanged(moveTarget);
		}
	}
}

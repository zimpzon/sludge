using Sludge.PlayerInputs;
using Sludge.UI;
using System;
using UnityEngine;

public class UiNavigation : MonoBehaviour
{
	public enum UiNavigationGroup { None, MainMenu, LevelSelect, Settings, InGame, };

    public GameObject Left;
    public GameObject Right;
    public GameObject Up;
    public GameObject Down;
	public bool Enabled = true;
	public UiNavigationGroup NavigationGroup;

	public static Action<GameObject> OnNavigationChanged;
	public static Action<GameObject> OnNavigationSelected;

	RectTransform myRect;

    private void Awake()
    {
		myRect = GetComponent<RectTransform>();
	}

    private void OnDrawGizmos()
    {
		myRect = GetComponent<RectTransform>();
		Gizmos.DrawWireCube(myRect.position, myRect.sizeDelta);
	}

	private void Update()
    {
		if (NavigationGroup == UiNavigationGroup.None)
			Debug.LogError($"Navigation group not set on {gameObject.name}");

        // Don't react on mouse clicks on buttons that are not active in this context
        if (NavigationGroup != UiLogic.Instance.ActiveNavigationGroup)
            return;

		if (Input.GetMouseButtonDown(0))
        {
			bool isClicked = RectTransformUtility.RectangleContainsScreenPoint(myRect, Input.mousePosition);
			if (isClicked)
            {
				if (UiSelectionMarker.Instance.target == gameObject)
				{
					// Activate
					if (OnNavigationSelected != null)
						OnNavigationSelected(gameObject);
				}
				else
				{
					// Select
					ChangeSelection(UiSelectionMarker.Instance, gameObject);
				}
			}
		}
	}

	static void ChangeSelection(UiSelectionMarker selectionMarker, GameObject newSelection)
	{
        SoundManager.Play(FxList.Instance.UiChangeSelection);

        selectionMarker.SetTarget(newSelection);

        if (OnNavigationChanged != null)
        {
            OnNavigationChanged(newSelection);
        }
    }

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

		if (playerInput.IsTapped(PlayerInput.InputType.Select) && OnNavigationSelected != null)
		{
			OnNavigationSelected(selectionMarker.target);
			return;
		}

		if (playerInput.IsTapped(PlayerInput.InputType.Down))
			TryMove(selectionMarker, navComponent.Down);
		if (playerInput.IsTapped(PlayerInput.InputType.Up))
			TryMove(selectionMarker, navComponent.Up);
		if (playerInput.IsTapped(PlayerInput.InputType.Left))
			TryMove(selectionMarker, navComponent.Left);
		if (playerInput.IsTapped(PlayerInput.InputType.Right))
			TryMove(selectionMarker, navComponent.Right);
	}

	static void TryMove(UiSelectionMarker selectionMarker, GameObject moveTarget)
	{
		if (moveTarget != null && moveTarget.activeSelf)
		{
            ChangeSelection(selectionMarker, moveTarget);
		}
	}
}

using UnityEngine;

[ExecuteInEditMode]
public class UiSelectionMarker : MonoBehaviour
{
    public GameObject target;
    public RectTransform TL;
    public RectTransform TR;
    public RectTransform BL;
    public RectTransform BR;

    RectTransform rectTrans;

    private void OnValidate()
    {
        UpdatePositions(target, 5);
    }

    public void UpdatePositions(GameObject uiObject, float offset)
    {
        if (uiObject == null || rectTrans == null)
            return;

        var targetRect = uiObject.GetComponent<RectTransform>();
        rectTrans.anchoredPosition = targetRect.anchoredPosition;

        float leftBase = (-targetRect.sizeDelta.x / 2) + (TL.rect.width / 4);
        float rightBase = (targetRect.sizeDelta.x / 2) - (TR.rect.width / 4);
        float topBase = (targetRect.sizeDelta.y / 2) - (TL.rect.height / 4);
        float bottomBase = (-targetRect.sizeDelta.y / 2) + (BL.rect.height / 4);

        leftBase -= offset;
        rightBase += offset;
        topBase += offset;
        bottomBase -= offset;

        TL.anchoredPosition = new Vector2(leftBase, topBase);
        TR.anchoredPosition = new Vector2(rightBase, topBase);
        BR.anchoredPosition = new Vector2(rightBase, bottomBase);
        BL.anchoredPosition = new Vector2(leftBase, bottomBase);
    }

    private void Awake()
    {
        rectTrans = GetComponent<RectTransform>();
    }

    void Update()
    {
        float offset = (Mathf.Sin(Time.time * 10) + 1) * 2 + 3;
        UpdatePositions(target, offset);
    }
}

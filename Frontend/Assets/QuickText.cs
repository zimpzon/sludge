using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class QuickText : MonoBehaviour
{
    const float ShowTime = 1.0f;
    const float MoveAmount = 20.0f;

    public static QuickText Instance;

    TextMeshProUGUI quickText;
    Vector2 basePos;
    RectTransform trans;
    Tweener shakeTween;

    private void Awake()
    {
        Instance = this;

        quickText = GetComponentInChildren<TextMeshProUGUI>();
        quickText.enabled = false;

        trans = transform.GetComponent<RectTransform>();
        basePos = trans.anchoredPosition;
    }

    public void Hide()
    {
        quickText.text = "";
        if (shakeTween != null)
            shakeTween.Rewind();

        quickText.enabled = false;
    }

    public void ShowText(string text)
    {
        StopAllCoroutines();
        StartCoroutine(ShowTextCo(text));
    }

    IEnumerator ShowTextCo(string text)
    {
        quickText.text = text;
        quickText.enabled = true;
        if (shakeTween == null)
            shakeTween = quickText.transform.DOShakeScale(0.2f);
        else
            shakeTween.Restart();

        float endTime = Time.time + ShowTime;

        while (Time.time < endTime)
        {
            float t = (endTime - Time.time) / ShowTime;
            var pos = basePos + Vector2.down * (t * MoveAmount);
            trans.anchoredPosition = pos;
            yield return null;
        }

        quickText.text = "(disabled)";
        quickText.enabled = false;
        trans.anchoredPosition = basePos;
    }
}

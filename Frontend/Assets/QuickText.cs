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
    Transform trans;

    private void Awake()
    {
        Instance = this;

        quickText = GetComponentInChildren<TextMeshProUGUI>();
        quickText.enabled = false;

        trans = transform;
        basePos = trans.position;
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
        quickText.transform.DOShakeScale(0.2f);

        float endTime = Time.time + ShowTime;

        while (Time.time < endTime)
        {
            float t = (endTime - Time.time) / ShowTime;
            var pos = basePos + Vector2.down * (t * MoveAmount);
            trans.position = pos;
            yield return null;
        }

        quickText.text = "(disabled)";
        quickText.enabled = false;
        trans.position = basePos;
    }
}

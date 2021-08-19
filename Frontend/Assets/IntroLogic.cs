using DG.Tweening;
using Sludge.PlayerInputs;
using System.Collections;
using TMPro;
using UnityEngine;

public class IntroLogic : MonoBehaviour
{
    public TMP_Text TextLongTimeAgo;
    public RectTransform PanelCrawl;

    Vector3 crawlStartPos = new Vector3(0, -5017, -12759);
    Vector3 crawlEndPos = new Vector3(0, -772, 3082);
    float crawlT;
    PlayerInput input = new PlayerInput();

    private void Awake()
    {
        TextLongTimeAgo.DOFade(0.0f, 0.0f);
        PanelCrawl.anchoredPosition3D = crawlStartPos;
        StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        yield return new WaitForSeconds(0.5f);
        TextLongTimeAgo.DOFade(1.0f, 2.0f);
        yield return new WaitForSeconds(3);
        TextLongTimeAgo.DOFade(0.0f, 2.0f);
        yield return new WaitForSeconds(1);

        while (true)
        {
            crawlT += Time.deltaTime * 0.01f;

            if (input.DownActive())
                crawlT = crawlT - Time.deltaTime * 0.03f;
            if (input.UpActive())
                crawlT = crawlT + Time.deltaTime * 0.02f;

            PanelCrawl.anchoredPosition3D = Vector3.Lerp(crawlStartPos, crawlEndPos, crawlT);
            yield return null;
        }
    }
}

using DG.Tweening;
using UnityEngine;

public class CombatAnnouncer : MonoBehaviour
{
    [SerializeField] private CanvasGroup backgroundCG;
    [SerializeField] private CanvasGroup symbolCG;
    [SerializeField] private CanvasGroup textCG;

    private RectTransform backgroundRT, symbolRT, textRT;

    private void Awake()
    {
        backgroundRT = backgroundCG.GetComponent<RectTransform>();
        symbolRT = symbolCG.GetComponent<RectTransform>();
        textRT = textCG.GetComponent<RectTransform>();

        gameObject.SetActive(false);
    }
    public Sequence Announce()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.SetAutoKill(true);

        Vector2 targetPos = new Vector2(-1280, 0);
        Vector2 backgroundSizeOrigin = backgroundRT.sizeDelta;
        Vector2 symbolPosOrigin = symbolRT.anchoredPosition;
        Vector2 textPosOirigin = textRT.anchoredPosition;
        float symbolAlphaOrigin = symbolCG.alpha;
        float textAlphaOirgin = textCG.alpha;

        backgroundRT.sizeDelta = Vector2.zero;
        symbolRT.anchoredPosition = symbolPosOrigin;
        textRT.anchoredPosition = textPosOirigin;

        symbolCG.alpha = 0f;
        textCG.alpha = 0f;

        gameObject.SetActive(true);

        sequence.Append(textCG.DOFade(textAlphaOirgin, 0.3333f).SetEase(Ease.OutSine));
        sequence.Append(symbolCG.DOFade(symbolAlphaOrigin, 0.3333f).SetEase(Ease.OutSine));
        sequence.Append(backgroundRT.DOSizeDelta(backgroundSizeOrigin, 0.3333f).SetEase(Ease.OutSine));

        sequence.AppendInterval(0.3333f);

        sequence.Append(symbolRT.DOAnchorPos(targetPos, 0.3333f).SetEase(Ease.OutSine));
        sequence.Join(textRT.DOAnchorPos(targetPos, 0.3333f).SetEase(Ease.OutSine));

        sequence.Append(backgroundCG.DOFade(0f, 0.3333f).SetEase(Ease.OutSine));
        sequence.AppendCallback(() =>
        {
            backgroundRT.sizeDelta = Vector2.zero;
            symbolRT.anchoredPosition = Vector2.zero;
            textRT.anchoredPosition = Vector2.zero;
            symbolCG.alpha = 0f;
            textCG.alpha = 0f;

            gameObject.SetActive(false);
        });

        return sequence;
    }
}

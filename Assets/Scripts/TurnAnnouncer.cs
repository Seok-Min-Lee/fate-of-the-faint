using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class TurnAnnouncer : MonoBehaviour
{
    [SerializeField] private CanvasGroup backgroundCG;
    [SerializeField] private TextMeshProUGUI text;

    private RectTransform backgroundRT, textRT;
    private CanvasGroup textCG;
    Vector2 backgroundSizeOrigin;
    private void Awake()
    {
        backgroundRT = backgroundCG.GetComponent<RectTransform>();
        textRT = text.GetComponent<RectTransform>();
        textCG = text.GetComponent<CanvasGroup>();

        backgroundSizeOrigin = backgroundRT.sizeDelta;

        gameObject.SetActive(false);
    }
    public Sequence PlayerTurnAnnounce()
    {
        text.text = "내 턴";
        return Announce();
    }
    public Sequence EnemyTurnAnnounce()
    {
        text.text = "적 턴";
        return Announce();
    }
    private Sequence Announce()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.SetAutoKill(true);

        sequence.AppendCallback(() =>
        {
            backgroundRT.sizeDelta = new Vector2(backgroundSizeOrigin.x, 0);
            backgroundCG.alpha = 1f;
            textRT.localScale = Vector3.one;
            textCG.alpha = 0f;

            gameObject.SetActive(true);
        });

        sequence.Append(textCG.DOFade(1f, 0.3333f).SetEase(Ease.OutSine));
        sequence.Join(textRT.DOScale(Vector3.one * 0.5f, 0.3333f).SetEase(Ease.OutSine));

        sequence.Append(backgroundRT.DOSizeDelta(backgroundSizeOrigin, 0.3333f).SetEase(Ease.OutSine));

        sequence.AppendInterval(0.3333f);

        sequence.Append(backgroundCG.DOFade(0f, 0.3333f).SetEase(Ease.OutSine));
        sequence.Append(textCG.DOFade(0f, 0.3333f).SetEase(Ease.OutSine));

        sequence.AppendCallback(() =>
        {
            gameObject.SetActive(false);
        });

        return sequence;
    }
}

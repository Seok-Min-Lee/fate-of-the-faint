using DG.Tweening;
using TMPro;
using UnityEngine;

public class EntityBlockView : MonoBehaviour
{
    [SerializeField] private CanvasGroup borderCG;
    [SerializeField] private TextMeshProUGUI text;

    private Color textColor;
    public void Start()
    {
        textColor = text.color;
    }
    public Sequence Show(string value)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            borderCG.alpha = 0f;

            text.text = value;
            text.color = Color.clear;
            text.transform.localScale = Vector3.one * 1.25f;

            gameObject.SetActive(true);

        });
        sequence.Append(borderCG.DOFade(1f, 0.5f));

        sequence.Append(text.DOColor(textColor, 0.25f));
        sequence.Join(text.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));

        return sequence;
    }
    public Sequence Hide()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(borderCG.DOFade(0f, 0.5f));
        sequence.Join(text.DOColor(Color.clear, 0.5f));

        sequence.AppendCallback(() => gameObject.SetActive(false));

        return sequence;
    }
    public Sequence Change(string value)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(() => 
        {
            text.text = value;
            text.transform.localScale = Vector3.one * 1.25f;
        });
        sequence.Append(text.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));

        return sequence;
    }
}

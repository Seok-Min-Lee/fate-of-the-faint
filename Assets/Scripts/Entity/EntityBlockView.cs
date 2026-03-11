using DG.Tweening;
using TMPro;
using UnityEngine;

public class EntityBlockView : MonoBehaviour, ITooltip
{
    [SerializeField] private CanvasGroup borderCG;
    [SerializeField] private TextMeshProUGUI text;

    private Color textColor;
    private Sequence sequence;
    public void Init(int value)
    {
        textColor = text.color;

        if (value > 0)
        {
            text.text = value.ToString();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    public Sequence Show(int value)
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            borderCG.alpha = 0f;

            text.text = value.ToString();
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
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.Append(borderCG.DOFade(0f, 0.5f));
        sequence.Join(text.DOColor(Color.clear, 0.5f));

        sequence.AppendCallback(() => gameObject.SetActive(false));

        return sequence;
    }
    public Sequence Change(int value)
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() => 
        {
            text.text = value.ToString();
            text.transform.localScale = Vector3.one * 1.25f;
        });
        sequence.Append(text.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));

        return sequence;
    }

    public void GetTooltip(out string head, out string desc)
    {
        head = "방어";
        desc = "수치만큼 피해를 막습니다";
    }
}

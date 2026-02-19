using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class CardCostView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private Sequence sequence;
    public void Change(string value, Color color)
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            text.text = value;
            text.color = color;
            text.transform.localScale = Vector3.one * 1.25f;
        });
        sequence.Append(text.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
    }
}

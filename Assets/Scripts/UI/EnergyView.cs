using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class EnergyView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    public IEnumerator ChangeCor(int currentValue, int maxValue)
    {
        Change(currentValue, maxValue);

        yield return null;
    }
    private Sequence Change(int currentValue, int maxValue)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            text.text = $"{currentValue}/{maxValue}";
            text.transform.localScale = Vector3.one * 1.25f;
        });
        sequence.Append(text.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));

        return sequence;
    }
}

using DG.Tweening;
using TMPro;
using UnityEngine;

public class EntityHpView : MonoBehaviour
{
    [SerializeField] private RectTransform guage;
    [SerializeField] private TextMeshProUGUI text;

    private RectTransform rectTransform;
    private Color textColor;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        textColor = text.color;
    }

    public Sequence Change(int currentHp, int maxHp)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(guage.DOSizeDelta(new Vector2(rectTransform.sizeDelta.x * currentHp / maxHp, rectTransform.sizeDelta.y), 0.5f));
        sequence.Join(text.DOColor(Color.clear, 0.25f));

        sequence.AppendCallback(() => 
        {
            text.text = currentHp + "/" + maxHp;
            text.transform.localScale = Vector3.one * 1.25f;
        });
        sequence.Append(text.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
        sequence.Join(text.DOColor(textColor, 0.25f));

        return sequence;
    }
}

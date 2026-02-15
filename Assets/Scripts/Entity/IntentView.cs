using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntentView : MonoBehaviour
{
    [SerializeField] private Image symbol;
    [SerializeField] private TextMeshProUGUI text;

    private CanvasGroup canvasGroup;
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    public Sequence Show(Sprite sprite, string text)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            symbol.sprite = sprite;
            this.text.text = text;

            canvasGroup.alpha = 0f;
        });
        sequence.Append(canvasGroup.DOFade(1f, 0.5f));

        return sequence;
    }
    public Sequence Hide()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(canvasGroup.DOFade(0f, 0.5f));

        return sequence;
    }
}

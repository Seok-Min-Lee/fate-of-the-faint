using DG.Tweening;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    private TextMeshProUGUI text;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private DamageTextPool pool;
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }
    public void Spawn(string text, Transform parent, DamageTextPool pool)
    {
        this.text.text = text;
        transform.parent = parent;
        this.pool = pool;

        Vector3 start = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.95f, 1.05f), 0);
        Vector2 end = new Vector2(Random.Range(-1f, 1f), Random.Range(-1.1f, -0.9f));

        Sequence seqeunce = DOTween.Sequence();
        
        seqeunce.AppendCallback(() => 
        {
            rectTransform.localPosition = start;
            transform.localScale = Vector3.one * 2f;
            canvasGroup.alpha = 1f;
        });

        seqeunce.Append(rectTransform.DOLocalMoveX(end.x, 0.5f));
        seqeunce.Join(rectTransform.DOLocalMoveY(end.y, 0.5f).SetEase(Ease.InCubic));
        seqeunce.Join(transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic));
        seqeunce.Join(canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InQuart));

        seqeunce.AppendCallback(() => 
        {
            pool.Push(this); 
        });
    }
}

using DG.Tweening;
using UnityEngine;

public class ScaleLoopImage : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private float min;
    [SerializeField] private float max;

    private void Start()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(() => 
        {
            transform.localScale = Vector3.one * min;
        });
        sequence.Append(transform.DOScale(Vector3.one * max, duration).SetLoops(-1, LoopType.Yoyo));
    }
}

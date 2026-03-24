using DG.Tweening;
using UnityEngine;

public class ScaleLooper : MonoBehaviour
{
    [SerializeField] private float min;
    [SerializeField] private float max;
    [SerializeField] private float duration;
    private void Start()
    {
        Vector3 minScale = Vector3.one * min;
        Vector3 maxScale = Vector3.one * max;
        
        transform.localScale = minScale;

        transform.DOScale(maxScale, duration)
                 .SetEase(Ease.InOutSine)
                 .SetLoops(-1, LoopType.Yoyo);
    }
}

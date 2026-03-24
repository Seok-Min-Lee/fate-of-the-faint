using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AnchoredPositionLooper : MonoBehaviour
{
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 endPosition;
    [SerializeField] private float duration;

    private void Start()
    {
        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = startPosition;

        rect.DOAnchorPos(endPosition, duration)
                 .SetEase(Ease.InOutSine)
                 .SetLoops(-1, LoopType.Yoyo);
    }
}

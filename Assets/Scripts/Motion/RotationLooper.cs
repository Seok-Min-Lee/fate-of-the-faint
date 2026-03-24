using DG.Tweening;
using UnityEngine;

public class RotationLooper : MonoBehaviour
{
    [SerializeField] private Vector3 startRotation;
    [SerializeField] private Vector3 endRotation;
    [SerializeField] private float duration;

    private void Start()
    {
        transform.rotation = Quaternion.Euler(startRotation);

        transform.DORotate(endRotation, duration, RotateMode.FastBeyond360)
                 .SetEase(Ease.InOutSine)
                 .SetLoops(-1, LoopType.Yoyo);
    }
}

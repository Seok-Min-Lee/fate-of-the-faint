using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ImageColorLooper : MonoBehaviour
{
    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;
    [SerializeField] private float duration;
    private void Start()
    {
        Image image = GetComponent<Image>();

        image.color = startColor;

        image.DOColor(endColor, duration)
             .SetEase(Ease.InOutSine)
             .SetLoops(-1, LoopType.Yoyo);
    }
}

using DG.Tweening;
using UnityEngine;

public class EndingCredit : MonoBehaviour
{
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 endPosition;
    
    public RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            return _rectTransform;
        }
    }
    private RectTransform _rectTransform;
    public Tween Play(float duration)
    {
        RectTransform.anchoredPosition = startPosition;

        return RectTransform.DOAnchorPos(endPosition, duration);
    }
}

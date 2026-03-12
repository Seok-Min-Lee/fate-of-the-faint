using DG.Tweening;
using UnityEngine;

public class SideButton : MonoBehaviour
{
    [SerializeField] private Vector2 startPosition;
    [SerializeField] private Vector2 endPosition;
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
    RectTransform _rectTransform;

    public void Show()
    {
        Reset();
        RectTransform.DOAnchorPos(endPosition, 0.5f);
    }
    public void Hide()
    {
        Reset();
    }
    private void Reset()
    {
        RectTransform.anchoredPosition = startPosition;
    }
}

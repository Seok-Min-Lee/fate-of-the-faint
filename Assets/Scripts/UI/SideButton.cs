using DG.Tweening;
using UnityEngine;

public class SideButton : MonoBehaviour
{
    [SerializeField] private Vector2 startPosition;
    [SerializeField] private Vector2 endPosition;

    [SerializeField] private bool showOnEnabled = true;
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
    private Tween tween;
    private void OnEnable()
    {
        if (!showOnEnabled)
        {
            return;
        }

        Show();
    }
    public void Show()
    {
        Reset();

        tween = RectTransform.DOAnchorPos(endPosition, 0.5f);
    }
    public void Hide()
    {
        Reset();
    }
    private void Reset()
    {
        if (tween != null)
        {
            tween.Kill();
        }

        RectTransform.anchoredPosition = startPosition;
    }
}

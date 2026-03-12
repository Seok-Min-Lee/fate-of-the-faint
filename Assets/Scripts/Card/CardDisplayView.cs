using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDisplayView : CardDefaultView, IPointerEnterHandler, IPointerExitHandler
{
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
    public Button Button 
    {
        get
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            return _button;
        }
    }
    private Button _button;
    public int Index { get; private set; }
    private bool isHold;
    private float hoverScale;
    public void Init(int index, CardSO origin, float hoverScale = 1f)
    {
        Index = index;
        base.Init(origin);

        this.hoverScale = hoverScale;
        isHold = hoverScale == 1f;
    }
    public void BindOnClickListener(UnityAction onClick)
    {
        if (onClick == null)
        {
            Button.enabled = false;
            return;
        }

        Button.enabled = true;
        Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(onClick);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHold)
        {
            return;
        }
        Hover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isHold)
        {
            return;
        }
        HoverCancel();
    }
    public void Hover()
    {
        transform.localScale = Vector3.one * hoverScale;
    }
    public void HoverCancel()
    {
        transform.localScale = Vector3.one;
    }
    public void Hold()
    {
        isHold = true;
    }
    public void HoldCancel()
    {
        isHold = false;
    }
}

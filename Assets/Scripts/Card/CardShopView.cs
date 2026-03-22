using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardShopView : CardDefaultView, IShopView, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI priceText;

    [Header("[Fail React]")]
    [SerializeField] private int strength;
    [SerializeField] private float duration;
    [SerializeField] private int vibrato;
    private RectTransform rectTransform
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
    private CanvasGroup canvasGroup
    {
        get
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
            return _canvasGroup;
        }
    }
    private CanvasGroup _canvasGroup;
    public int Price => price;
    private int price;
    private Action<IShopView> onClick;
    public void Init(CardSO data, int price, Action<IShopView> onClick)
    {
        base.Init(data);
        this.price = price;
        this.priceText.text = price.ToString();
        this.onClick = onClick;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * 1.2f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }

    public Tween FailedToPurchase()
    {
        return rectTransform.DOPunchPosition(
            punch: UnityEngine.Random.insideUnitCircle * strength,
            duration: duration,
            vibrato: vibrato
        );
    }

    public Tween SuccessedToPurchase()
    {
        return canvasGroup.DOFade(0f, 0.5f);
    }
}

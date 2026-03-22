using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RelicShopView : RelicDefaultView, IShopView, IPointerClickHandler
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
    public int Price => price;
    private int price;

    private Action<IShopView> onClick;
    public void Init(RelicSO data, TooltipView tooltip, int price, Action<IShopView> onClick)
    {
        base.Init(data, tooltip);

        this.price = price;
        this.priceText.text = price.ToString();
        this.onClick = onClick;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(this);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        transform.localScale = Vector3.one * 1.2f;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        transform.localScale = Vector3.one;
    }
    public void FailedToPurchase()
    {
        rectTransform.DOPunchPosition(
            punch: UnityEngine.Random.insideUnitCircle * strength,
            duration: duration,
            vibrato: vibrato
        );
    }
}

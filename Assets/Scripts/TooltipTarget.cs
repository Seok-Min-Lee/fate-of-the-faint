using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public class TooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform canvas;
    [SerializeField] private TooltipView tooltip;
    [SerializeField] private string head;
    [SerializeField] private string description;
    private RectTransform rectTransform;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.Bind(head, description);

        tooltip.RectTransform.pivot = rectTransform.pivot;
        tooltip.RectTransform.anchorMin = rectTransform.anchorMin;
        tooltip.RectTransform.anchorMax = rectTransform.anchorMax;

        tooltip.RectTransform.anchoredPosition = new Vector2(
            rectTransform.anchoredPosition.x,
            rectTransform.anchoredPosition.y + rectTransform.sizeDelta.y
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.Clear();
    }
}

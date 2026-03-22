using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RelicDefaultView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected Image image;
    public RelicSO Origin { get; private set; }
    public TooltipView Tooltip { get; private set; }
    public void Init(RelicSO origin, TooltipView tooltip)
    {
        Origin = origin;
        Tooltip = tooltip;
        image.sprite = Origin.Icon;
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        Tooltip.Bind(
            name: Origin.DisplayName,
            description: Origin.Description
        );

        Tooltip.transform.position = transform.position;
        Tooltip.transform.parent = transform.parent.parent;
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        Tooltip.Clear();
    }
}

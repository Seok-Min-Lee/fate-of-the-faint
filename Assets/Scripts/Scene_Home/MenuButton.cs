using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Color hoverColor;
    [SerializeField] private Color defaultColor;

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = hoverColor;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = defaultColor;
    }
}

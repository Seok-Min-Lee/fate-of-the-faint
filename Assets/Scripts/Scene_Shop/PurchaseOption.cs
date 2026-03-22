using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PurchaseOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sprite defaultImage;
    [SerializeField] private Sprite hoverImage;
    private Image image
    {
        get
        {
            if (_image == null)
            {
                _image = GetComponent<Image>();
            }
            return _image;
        }
    }
    private Image _image;
    public void Init()
    {
        image.sprite = defaultImage;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        image.sprite = hoverImage;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.sprite = defaultImage;
    }
}

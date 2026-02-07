using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image backgrond;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI name;

    [Header("Hover")]
    [SerializeField] private Color BackgroundColorHover;
    [SerializeField] private Color textColorHover;

    [Header("Default")]
    [SerializeField] private Color backgroundColorDefault;
    [SerializeField] private Color textColorDefault;

    private VictoryWindow pool;
    public void Init(VictoryWindow pool, Sprite image, string text)
    {
        this.pool = pool;

        icon.sprite = image;
        name.text = text;

        backgrond.color = backgroundColorDefault;
        name.color = textColorDefault;

        gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        backgrond.color = BackgroundColorHover;
        name.color = textColorHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgrond.color = backgroundColorDefault;
        name.color = textColorDefault;
    }
    public void OnClick()
    {
        gameObject.SetActive(false);
        pool.Charge(this);
    }
}
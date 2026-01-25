using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] private ViewType type;
    [SerializeField] private TextMeshProUGUI cost;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI desc;
    [SerializeField] private Image image;

    public ViewType Type => type;

    public CardInstance CardInstance { get; private set; }
    public void Init(CardInstance cardInstance)
    {
        CardInstance = cardInstance;

        cost.text = cardInstance.BaseDef.Cost.ToString();
        name.text = cardInstance.BaseDef.Name;
        desc.text = cardInstance.BaseDef.Description;
        image.sprite = cardInstance.BaseDef.Image;
    }
    public enum ViewType
    {
        Attack,
        Skill,
        Power,
    }
}


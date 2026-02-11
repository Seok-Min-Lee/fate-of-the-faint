using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDisplayView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI cost;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI desc;
    [SerializeField] private CardArt[] arts;
    public bool IsIgnorePointer = false;
    public Button Button { get; private set; }
    public int Id { get; private set; }
    public CardSO Origin { get; private set; }
    private float hoverScale;
    private void Awake()
    {
        Button = GetComponent<Button>();
    }
    public void Init(int id, float hoverScale, CardSO origin, bool isButton = false)
    {
        Id = id;
        this.hoverScale = hoverScale;
        this.Origin = origin;

        cost.text = origin.Cost.ToString();
        name.text = origin.Name;
        desc.text = origin.Description;

        for (int i = 0; i < arts.Length; i++)
        {
            if (i == (int)origin.Type)
            {
                arts[i].Activate(origin.Image);
            }
            else
            {
                arts[i].Deactivate();
            }
        }

        Button.enabled = isButton;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsIgnorePointer)
        {
            return;
        }
        transform.localScale = Vector3.one * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsIgnorePointer)
        {
            return;
        }
        transform.localScale = Vector3.one;
    }
}

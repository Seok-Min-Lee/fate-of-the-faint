using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image eventIcon;
    [SerializeField] private Image clearIcon;
    public RectTransform RectTransform { get; private set; }
    public MapNode Node { get; private set; }

    private Sprite normal;
    private Sprite hover;

    public int id;
    public int floor;
    public int col;
    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }
    public void Init(MapNode node, Sprite normal, Sprite hover, bool isVisible)
    {
        this.Node = node;
        this.normal = normal;
        this.hover = hover;

        eventIcon.sprite = normal;
        clearIcon.gameObject.SetActive(node.State == MapNodeState.Cleared);

        gameObject.SetActive(isVisible);

        //
        id = node.Id;
        floor = node.Floor;
        col = node.Col;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Node.State == MapNodeState.None)
        {
            eventIcon.sprite = hover;
            RectTransform.localScale = Vector3.one * 1.25f;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        eventIcon.sprite = normal;
        RectTransform.localScale = Vector3.one;
    }
    public void OnClick()
    {
        Node.State = MapNodeState.Cleared;
        clearIcon.gameObject.SetActive(true);

        eventIcon.raycastTarget = false;
    }
}

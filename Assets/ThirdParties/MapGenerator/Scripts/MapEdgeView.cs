using UnityEngine;

public class MapEdgeView : MonoBehaviour
{
    public RectTransform RectTransform { get; private set; }
    public MapEdge Edge { get; private set; }
    public int fromId;
    public int toId;
    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }
    public void Init(MapEdge edge, bool isVisible)
    {
        Edge = edge;

        gameObject.SetActive(isVisible);

        //
        fromId = edge.FromId;
        toId = edge.ToId;
    }
}

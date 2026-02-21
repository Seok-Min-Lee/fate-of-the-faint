using System;
using System.Collections.Generic;
using UnityEngine;
public class MapBootstrap : MonoBehaviour
{
    public MapGenConfig Config => config;
    [SerializeField] private MapGenConfig config;

    // Custom
    public IReadOnlyList<MapNodeView> Nodes => nodePool.Actives;
    [SerializeField] private MapNodeViewPool nodePool;
    [SerializeField] private MapEdgeViewPool edgePool;
    [SerializeField] private MapNodeIconPair[] nodeIconPairs;

    [SerializeField] private int colSpacing;
    [SerializeField] private int floorSpacing;
    [SerializeField] private int extraSpacing;
    [SerializeField] private int thickness = 5;

    private MapGraph graph;
    public void Init(MapGraph graph)
    {
        this.graph = graph;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // 기존 뷰 반납
        nodePool.RetrieveAll();
        edgePool.RetrieveAll();

        // 노드 뷰 생성
        foreach (MapNode n in graph.Nodes)
        {
            MapNodeView nodeView = nodePool.Pop();

            nodeView.Init(
                node: n,
                normal: nodeIconPairs[(int)n.Type].normal,
                hover: nodeIconPairs[(int)n.Type].hover,
                isVisible: n.Id != 1
            );

            nodeView.RectTransform.anchoredPosition = ToUIPosition(n.Floor, n.Col, n.Extra);
        }

        float linePadding = nodePool.Actives[0].RectTransform.sizeDelta.x * 1.5f;

        // 엣지 뷰 생성
        foreach (MapEdge edge in graph.Edges)
        {
            MapEdgeView edgeView = edgePool.Pop();

            edgeView.Init(edge, edge.FromId != 1);

            MapNode fromNode = graph.GetNode(edge.FromId);
            MapNode toNode = graph.GetNode(edge.ToId);

            DrawLine(
                line: edgeView.RectTransform,
                a: ToUIPosition(fromNode.Floor, fromNode.Col, fromNode.Extra),
                b: ToUIPosition(toNode.Floor, toNode.Col, toNode.Extra),
                padding: linePadding
            );
        }
    }
    private Vector2 ToUIPosition(int floor, int col, Vector2 extra)
    {
        float x = (col - (config.Width - 1) * 0.5f) * colSpacing + extra.x * extraSpacing;
        float y = floor * floorSpacing + extra.y * extraSpacing;

        return new Vector2(x, y);
    }
    private void DrawLine(RectTransform line, Vector2 a, Vector2 b, float padding)
    {
        Vector2 dir = b - a;

        int temp = (int)(dir.magnitude - padding);
        int remain = temp % 16;
        temp += remain < 8 ? -remain : 16 - remain;
        line.sizeDelta = new Vector2(temp, thickness);

        //line.sizeDelta = new Vector2(dir.magnitude - linePadding, thickness);
        line.anchoredPosition = (a + b) * 0.5f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        line.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
[Serializable]
public struct MapNodeIconPair
{
    public MapNodeType type;
    public Sprite normal;
    public Sprite hover;
}
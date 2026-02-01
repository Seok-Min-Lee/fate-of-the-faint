using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class MapBootstrap : MonoBehaviour
{
    public MapGenConfig Config => config;
    [SerializeField] private MapGenConfig config;

    // Custom
    [SerializeField] private Transform nodeParent;
    [SerializeField] private MapNodeView nodePrefab;
    [SerializeField] private MapNodeIconPair[] nodeIconPairs;

    [SerializeField] private Transform edgeParent;
    [SerializeField] private MapEdgeView edgePrefab;

    [SerializeField] private int colSpacing;
    [SerializeField] private int floorSpacing;
    [SerializeField] private int extraSpacing;
    [SerializeField] private int thickness = 5;

    private float linePadding;
    // 
    private Queue<MapNodeView> nodePool = new Queue<MapNodeView>();
    private List<MapNodeView> nodes = new List<MapNodeView>();
    private Queue<MapEdgeView> edgePool = new Queue<MapEdgeView>();
    private List<MapEdgeView> edges = new List<MapEdgeView>();

    private MapGraph graph;
    public void Init(MapGraph graph)
    {
        this.graph = graph;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (nodes.Count > 0)
        {
            // 기존 뷰 풀에 반납
            foreach (MapNodeView node in nodes)
            {
                node.gameObject.SetActive(false);
                nodePool.Enqueue(node);
            }
            nodes.Clear();
        }
        if (edges.Count > 0)
        {
            // 기존 뷰 풀에 반납
            foreach (MapEdgeView line in edges)
            {
                line.gameObject.SetActive(false);
                edgePool.Enqueue(line);
            }
            edges.Clear();
        }
        // 뷰 생성
        foreach (MapNode n in graph.Nodes)
        {
            MapNodeView nodeView;

            nodeView = nodePool.Count > 0 ? 
                        nodePool.Dequeue() :
                        GameObject.Instantiate<MapNodeView>(nodePrefab, nodeParent);
            
            nodeView.Init(
                node: n, 
                normal: nodeIconPairs[(int)n.Type].normal,
                hover: nodeIconPairs[(int)n.Type].hover,
                isVisible: n.Id != 1
            );

            nodeView.RectTransform.anchoredPosition = ToUIPosition(n.Floor, n.Col, n.Extra);

            nodes.Add(nodeView);
        }

        linePadding = nodes[0].RectTransform.sizeDelta.x * 1.5f;
        foreach (MapEdge edge in graph.Edges)
        {
            MapEdgeView edgeView = edgePool.Count > 0 ?
                                    edgeView = edgePool.Dequeue() :
                                    GameObject.Instantiate<MapEdgeView>(edgePrefab, edgeParent);

            edgeView.Init(edge, edge.FromId != 1);

            MapNode fromNode = graph.GetNode(edge.FromId);
            MapNode toNode = graph.GetNode(edge.ToId);

            DrawLine(
                edgeView.RectTransform,
                ToUIPosition(fromNode.Floor, fromNode.Col, fromNode.Extra),
                ToUIPosition(toNode.Floor, toNode.Col, toNode.Extra)
            );

            edges.Add(edgeView);
        }
    }
    private Vector2 ToUIPosition(int floor, int col, Vector2 extra)
    {
        float x = (col - (config.Width - 1) * 0.5f) * colSpacing + extra.x * extraSpacing;
        float y = floor * floorSpacing + extra.y * extraSpacing;

        return new Vector2(x, y);
    }
    private void DrawLine(RectTransform line, Vector2 a, Vector2 b)
    {
        Vector2 dir = b - a;

        int temp = (int)(dir.magnitude - linePadding);
        int remain = temp % 16;
        temp += remain < 8 ? -remain : 16 - remain;
        line.sizeDelta = new Vector2(temp, thickness);

        //line.sizeDelta = new Vector2(dir.magnitude - linePadding, thickness);
        line.anchoredPosition = (a + b) * 0.5f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        line.localRotation = Quaternion.Euler(0, 0, angle);
    }
[Serializable]
public struct MapNodeIconPair
{
    public MapNodeType type;
    public Sprite normal;
    public Sprite hover;
}
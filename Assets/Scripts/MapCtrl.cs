using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class MapCtrl : MonoBehaviour
{
    [SerializeField] private MapBootstrap bootstrap;
    [SerializeField] private ScrollRect scrollRect;
    public MapGraph Graph { get; private set; }
    public MapNode LatestNode { get; private set; }
    private Sequence seq;
    private void Awake()
    {
        if (!PlayManager.Instance.isLoad)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.INIT);
            return;
        }

        // Init
        Graph = PlayManager.Instance.MapGraph;
        LatestNode = PlayManager.Instance.LatestNode;

        bootstrap.Init(Graph);

        // OnClick Process Bind
        foreach (MapNodeView view in bootstrap.Nodes)
        {
            view.Bind((node) => TrySelectNode(node));
        }

        // View Update
        UpdateNodeStates();
        ScrollToIndex(LatestNode?.Floor?? 0);
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CreateMap();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            SaveMap();
        }
    }
#endif
    public bool TrySelectNode(MapNode node)
    {
        bool success = false;

        // Check
        if (LatestNode == null)
        {
            success = node.Floor == 1;
        }
        else
        {
            if (Graph.Adjacency.TryGetValue(LatestNode.Id, out List<int> candidates))
            {
                success = candidates.Contains(node.Id);
            }
        }

        // Process
        if (success)
        {
            LatestNode = node;
            node.State = MapNodeState.Visited;
            PlayManager.Instance.UpdateLatestNode(node);

            UpdateNodeStates();
            SaveMap();

            return true;
        }

        return false;
    }
    public void CreateMap()
    {
        Graph = MapGenerator.Generate(bootstrap.Config);
        bootstrap.Init(Graph);

        PlayManager.Instance.UpdateLatestNode(null);
    }
    public void SaveMap()
    {
        MapData save = MapDataConverter.ToSaveData(Graph, LatestNode?.Id?? -1);
        MapDataIO.SaveToFile(save);
    }
    private void UpdateNodeStates()
    {
        // 새 게임 시작인 경우
        if (LatestNode == null)
        {
            foreach (MapNodeView view in bootstrap.Nodes)
            {
                if (view.Node.Floor == 1)
                {
                    view.Highlight();
                }
            }
            return;
        }

        // 바로 다음 노드 하이라이트
        // 갈 수 없는 노드 하이드
        HashSet<int> possibles = Graph.BFS(LatestNode.Id);
        foreach (MapNodeView view in bootstrap.Nodes)
        {
            if (possibles.Contains(view.id))
            {
                if(view.Node.Floor == LatestNode.Floor + 1)
                {
                    view.Highlight();
                }
            }
            else
            {
                if (view.Node.State != MapNodeState.Visited)
                {
                    view.Node.State = MapNodeState.Impossible;
                    view.Hide();
                }
            }
        }
    }
    private void ScrollToIndex(int floor)
    {
        float duration = 0.5f;

        seq?.Kill();
        seq = DOTween.Sequence();

        float normalized = (float)floor / (bootstrap.Config.Floors - 1);

        seq.Append(DOTween.To(
            () => scrollRect.verticalNormalizedPosition,
            x => scrollRect.verticalNormalizedPosition = x,
            normalized,
            duration
        ));
    }
}

using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapCtrl : MonoBehaviour
{
    [SerializeField] private UICurtain curtain;

    [SerializeField] private MapBootstrap bootstrap;
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private UIWindowManager windowManager;
    public MapGraph Graph { get; private set; }
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

        bootstrap.Init(Graph);

        // OnClick Process Bind
        foreach (MapNodeView view in bootstrap.Nodes)
        {
            view.Bind(
                checkProcess: (node) => TrySelectNode(node),
                successProcess: (node) => LoadSceneByNode(node)
            );
        }

        // View Update
        UpdateNodeStates();
        ScrollToIndex(Graph.LatestNode?.Floor?? 0);
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
            MapData save = MapDataConverter.ToSaveData(Graph);
            MapDataIO.SaveToFile(save);
        }
    }
#endif
    public void OnClickCardDisplay()
    {
        windowManager.ActivateWindow(WindowType.CardDisplay, WindowMode.Single);
    }
    public void OnClickMap()
    {
        windowManager.ActivateWindow(WindowType.Map, WindowMode.Single);
    }
    public void OnClickSetting()
    {
        windowManager.ActivateWindow(WindowType.Setting, WindowMode.Single);
    }
    private bool TrySelectNode(MapNode node)
    {
        bool success = false;

        // Check
        if (Graph.LatestNode == null)
        {
            success = node.Floor == 1;
        }
        else
        {
            if (Graph.Adjacency.TryGetValue(Graph.LatestNode.Id, out List<int> candidates))
            {
                success = candidates.Contains(node.Id);
            }
        }

        // Process
        if (success)
        {
            Graph.SetLatestNode(node);
            node.State = MapNodeState.Visited;

            UpdateNodeStates();

            string key = node.Type switch
            {
                MapNodeType.Combat => PlayRecordKeys.COMBAT_VISIT_COUNT,
                MapNodeType.Elite => PlayRecordKeys.ELITE_VISIT_COUNT,
                MapNodeType.Treasure => PlayRecordKeys.TREASURE_VISIT_COUNT,
                MapNodeType.Rest => PlayRecordKeys.REST_VISIT_COUNT,
                MapNodeType.Shop => PlayRecordKeys.SHOP_VISIT_COUNT,
                _ => string.Empty
            };

            PlayManager.Instance.CurrentData.AddRecord(key, 1);

            return true;
        }

        return false;
    }
    private void LoadSceneByNode(MapNode node)
    {
        curtain.Close().OnComplete(() =>
        {
            switch (node.Type)
            {
                case MapNodeType.Combat:
                case MapNodeType.Elite:
                case MapNodeType.Boss:
                    UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.COMBAT);
                    break;
                case MapNodeType.Treasure:
                    UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.TREASURE);
                    break;
                case MapNodeType.Rest:
                    UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.REST);
                    break;
            }
        });
    }
    public void CreateMap()
    {
        Graph = MapGenerator.Generate(bootstrap.Config);
        bootstrap.Init(Graph);
    }
    private void UpdateNodeStates()
    {
        // 새 게임 시작인 경우
        if (Graph.LatestNode == null)
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
        HashSet<int> possibles = Graph.BFS(Graph.LatestNode.Id);
        foreach (MapNodeView view in bootstrap.Nodes)
        {
            if (possibles.Contains(view.id))
            {
                if(view.Node.Floor == Graph.LatestNode.Floor + 1)
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

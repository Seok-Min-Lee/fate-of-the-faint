using System.Linq;
using UnityEngine;

public class MapCtrl : MonoBehaviour
{
    [SerializeField] private MapBootstrap bootstrap;
    public MapGraph Graph { get; private set; }
    public int CurrentNodeId { get; private set; } = -1;
    //private void Awake()
    //{
    //    Graph = PlayManager.Instance.MapGraph;
    //    bootstrap.Init(Graph);
    //}
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Graph = MapGenerator.Generate(bootstrap.Config);
            bootstrap.Init(Graph);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            SaveRun();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            LoadRun();
            bootstrap.Init(Graph);
        }
    }

    public void SaveRun()
    {
        MapData save = MapDataConverter.ToSaveData(Graph, CurrentNodeId);
        MapDataIO.SaveToFile(save);
    }

    public bool LoadRun()
    {
        if (!MapDataIO.TryLoadFromFile(out MapData save)) 
        {
            return false;
        }

        Graph = MapDataConverter.FromSaveData(save);
        CurrentNodeId = save.currentNodeId;

        return true;
    }
}

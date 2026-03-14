using NUnit.Framework;
using UnityEngine;

public class RunManager : MonoSingleton<RunManager>
{
    [SerializeField] private PlayerSO temp_PlaerSO;
    [SerializeField] private MapGenConfig mapConfig;

    [Header("Catalog")]
    [SerializeField] private PlayerSO[] players;
    [SerializeField] private CardSO[] cards;
    [SerializeField] private RelicSO[] relics;
    [SerializeField] private PotionSO[] potions;

    [SerializeField] private EnemySpawnPlanSO[] enemyPlans;
    [SerializeField] private EnemySpawnPlanSO[] elitePlans;
    [SerializeField] private EnemySpawnPlanSO bossPlan;
    public RunData CurrentData { get; private set; }
    public GameCatalog Catalog { get; private set; }
    public MapGraph MapGraph { get; private set; }
    public bool isLoad { get; private set; }
    private void Awake()
    {
        Catalog = new GameCatalog(
            players: players,
            cards: cards,
            relics: relics,
            potions: potions
        );

        // Play Data Load
#if UNITY_EDITOR
        CurrentData = RunSaveDataIO.TryLoadFromFile(out RunSaveData data) ?
                      RunData.CreateFromSaveData(data, Catalog) :
                      RunData.CreateNew(temp_PlaerSO, 1234, Catalog);

        // Map Data Load
        MapGraph = MapDataIO.TryLoadFromFile(out MapData mapData) ?
                   MapDataConverter.FromSaveData(mapData) :
                   MapGenerator.Generate(mapConfig);
#else
        if (PlaySaveDataIO.TryLoadFromFile(out PlaySaveData data))
        {
            CurrentData = PlayData.CreateFromSaveData(data, Catalog);
        }
        if (MapDataIO.TryLoadFromFile(out MapData mapData))
        {
            MapGraph = MapDataConverter.FromSaveData(mapData);
        }
#endif

        isLoad = true;
    }
    public void SaveData()
    {
        RunSaveData playSave = CurrentData.ToSaveData();
        RunSaveDataIO.SaveToFile(playSave);

        MapData save = MapDataConverter.ToSaveData(MapGraph);
        MapDataIO.SaveToFile(save);
    }
    public void ClearPlayData()
    {
        CurrentData = RunData.CreateNew(temp_PlaerSO, 1234, Catalog);
        MapGraph = MapGenerator.Generate(mapConfig);
    }
    public void RemovePlayData()
    {
        RunSaveDataIO.TryRemoveFromFile();
        CurrentData = null;

        MapDataIO.TryRemoveFromFile();
        MapGraph = null;
    }
    public EnemySpawnPlanSO GetEnemyPlan()
    {
        switch (MapGraph.LatestNode.Type)
        {
            case MapNodeType.Combat:
                return enemyPlans[Random.Range(0, enemyPlans.Length)];

            case MapNodeType.Elite:
                return elitePlans[Random.Range(0, elitePlans.Length)];

            case MapNodeType.Boss:
                return bossPlan;

            default:
#if UNITY_EDITOR
                return bossPlan;
#else
                return null;
#endif
        }
    }
}
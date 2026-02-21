using UnityEngine;

public class PlayManager : MonoSingleton<PlayManager>
{
    [SerializeField] private PlayerSO temp_PlaerSO;
    [SerializeField] private MapGenConfig mapConfig;

    [Header("Catalog")]
    [SerializeField] private PlayerSO[] players;
    [SerializeField] private CardSO[] cards;
    [SerializeField] private RelicSO[] relics;
    [SerializeField] private PotionSO[] potions;

    public PlayData CurrentData { get; private set; }
    public GameCatalog Catalog { get; private set; }
    public MapGraph MapGraph { get; private set; }
    public MapNode LatestNode { get; private set; }
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
        CurrentData = PlaySaveDataIO.TryLoadFromFile(out PlaySaveData data) ?
                      PlayData.CreateFromSaveData(data, Catalog) :
                      PlayData.CreateNew(temp_PlaerSO, 1234, Catalog);

        // Map Data Load
        if (MapDataIO.TryLoadFromFile(out MapData mapData))
        {
            MapGraph = MapDataConverter.FromSaveData(mapData);
            LatestNode = MapGraph.GetNode(mapData.currentNodeId);
        }
        else
        {
            MapGraph = MapGenerator.Generate(mapConfig);
            LatestNode = null;
        }

        isLoad = true;
    }
    public void SavePlayData()
    {
        PlaySaveDataIO.SaveToFile(CurrentData.ToSaveData());
    }
    public void ClearPlayData()
    {
        CurrentData = PlayData.ClearData();
    }
    public void UpdateLatestNode(MapNode node)
    {
        LatestNode = node;
    }
}
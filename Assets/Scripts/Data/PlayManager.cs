using UnityEngine;

public class PlayManager : MonoSingleton<PlayManager>
{
    [SerializeField] private PlayerSO temp_PlaerSO;

    [Header("Catalog")]
    [SerializeField] private PlayerSO[] players;
    [SerializeField] private CardSO[] cards;
    [SerializeField] private RelicSO[] relics;
    [SerializeField] private PotionSO[] potions;

    public PlayData CurrentData { get; private set; }
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

        CurrentData = PlaySaveDataIO.TryLoadFromFile(out PlaySaveData data) ?
                    PlayData.CreateFromSaveData(data, Catalog) :
                    PlayData.CreateNew(temp_PlaerSO, 1234, Catalog);

        isLoad = true;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            PlaySaveDataIO.SaveToFile(CurrentData.ToSaveData());
        }
    }
    public void ClearPlayData()
    {
        CurrentData = PlayData.ClearData();
    }
}
using System.Collections.Generic;
using System.Linq;
public class GameCatalog
{
    public IReadOnlyList<CardSO> CardList => cardDictionary.Values.ToList();
    public IReadOnlyList<RelicSO> RelicList => relicDictionary.Values.ToList();

    private Dictionary<string, PlayerSO> playerDictionary;
    private Dictionary<string, CardSO> cardDictionary;
    private Dictionary<string, RelicSO> relicDictionary;
    private Dictionary<string, PotionSO> potionDictionary;

    public GameCatalog(
        IEnumerable<PlayerSO> players,
        IEnumerable<CardSO> cards,
        IEnumerable<RelicSO> relics,
        IEnumerable<PotionSO> potions
    )
    {
        playerDictionary = players.ToDictionary(key => key.Id, value => value);
        cardDictionary = cards.ToDictionary(key => key.Id, value => value);
        relicDictionary = relics.ToDictionary(key => key.Id, value => value);
        potionDictionary = potions.ToDictionary(key => key.Id, value => value);
    }
    public bool TryGetPlayerSO(string key, out PlayerSO value)
    {
        return playerDictionary.TryGetValue(key, out value);
    }
    public bool TryGetCardSO(string key, out CardSO value)
    {
        return cardDictionary.TryGetValue(key, out value);
    }
    public bool TryGetRelicSO(string key, out RelicSO value)
    {
        return relicDictionary.TryGetValue(key, out value);
    }
    public bool TryGetPotionSO(string key, out PotionSO value)
    {
        return potionDictionary.TryGetValue(key, out value);
    }
}

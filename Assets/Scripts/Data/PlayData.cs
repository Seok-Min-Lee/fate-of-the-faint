using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
public class PlayData
{
    public PlayData
    (
        string playerId,
        int currentHp,
        int maxHp,
        int gold,
        int rewardCardOptionCount,
        RunRngState rngState,
        IEnumerable<int> nodes,
        IEnumerable<CardEntry> cards,
        IEnumerable<RelicInstance> relics,
        IEnumerable<PotionEntry> potions
    )
    {
        PlayerId = playerId;
        CurrentHp = currentHp;
        MaxHp = maxHp;
        Gold = gold;
        RewardCardOptionCount = rewardCardOptionCount;
        RngState = rngState;
        Nodes = new List<int>(nodes);
        Cards = new List<CardEntry>(cards);
        Relics = new List<RelicInstance>(relics);
        Potions = new List<PotionEntry>(potions);
    }
    // Identity
    public string PlayerId { get; private set; }

    // Core
    public int CurrentHp { get; private set; }
    public int MaxHp { get; private set; }
    public int Gold { get; private set; }
    public int RewardCardOptionCount { get; private set; }

    // RNG (재현/리플레이를 원하면 필수)
    public RunRngState RngState { get; private set; }
    public List<int> Nodes {  get; private set; }

    // Inventory
    public List<CardEntry> Cards { get; private set; }
    public List<RelicInstance> Relics { get; private set; }
    public List<PotionEntry> Potions { get; private set; }

    // Create
    public static PlayData CreateNew(PlayerSO player, int seed, GameCatalog catalog)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        // 시작 덱
        List<CardEntry> cards = new List<CardEntry>();
        if (player.StartingCards != null)
        {
            for (int i = 0; i < player.StartingCards.Count; i++)
            {
                CardSO card = player.StartingCards[i];

                if (card != null)
                {
                    cards.Add(new CardEntry(
                        id: card.Id, 
                        subId: i, 
                        origin: card
                    ));
                }
            }
        }

        List<RelicInstance> relics = new List<RelicInstance>();
        if (player.StartingRelics != null)
        {
            for (int i = 0; i < player.StartingRelics.Count; i++)
            {
                RelicSO relic = player.StartingRelics[i];

                if (relic != null)
                {
                    relics.Add(relic.CreateInstance());
                }
            }
        }

        PlayData data = new PlayData(
            playerId: player.Id,
            maxHp: player.MaxHp,
            currentHp: player.MaxHp,
            gold: 0,
            rewardCardOptionCount: 3,
            rngState: new RunRngState(seed),
            nodes: new List<int>(),
            cards: cards,
            relics: relics,
            potions: new List<PotionEntry>()
        );

        return data;
    }
    public static PlayData CreateFromSaveData(PlaySaveData save, GameCatalog catalog)
    {
        if (save == null)
        {
            throw new ArgumentNullException(nameof(save));
        }
        if (catalog == null)
        {
            throw new ArgumentNullException(nameof(catalog));
        }

        if (!catalog.TryGetPlayerSO(save.playerId, out PlayerSO player))
        {
            throw new InvalidOperationException("Player not found for id: " + save.playerId);
        }

        List<CardEntry> cards = new List<CardEntry>();
        for (int i = 0; i < save.cardIds.Count; i++)
        {
            if (catalog.TryGetCardSO(save.cardIds[i], out CardSO card))
            {
                cards.Add(new CardEntry(
                    id: card.Id, 
                    subId: i,
                    origin: card
                ));
            }
            else
            {
                throw new InvalidOperationException("Card not found for id: " + save.playerId);
            }
        }

        List<RelicInstance> relics = new List<RelicInstance>();
        for (int i = 0; i < save.relicIds.Count; i++)
        {
            if (catalog.TryGetRelicSO(save.relicIds[i], out RelicSO relic))
            {
                relics.Add(relic.CreateInstance());
            }
            else
            {
                throw new InvalidOperationException("Relic not found for id: " + save.playerId);
            }
        }

        List<PotionEntry> potions = new List<PotionEntry>();
        for (int i = 0; i < save.potionIds.Count; i++)
        {
            if (catalog.TryGetPotionSO(save.potionIds[i], out PotionSO potion))
            {
                potions.Add(new PotionEntry(
                    id: potion.Id,
                    subId: i,
                    origin: potion
                ));
            }
            else
            {
                throw new InvalidOperationException("Potion not found for id: " + save.playerId);
            }
        }

        // Create New
        PlayData data = new PlayData(
            playerId: player.Id,
            currentHp: Clamp(save.currentHp, 0, save.maxHp),
            maxHp: save.maxHp,
            gold: Math.Max(0, save.gold),
            rewardCardOptionCount: Math.Max(0, save.rewardCardOptionCount),
            nodes: save.nodes,
            rngState: new RunRngState(save.rng.seed, save.rng.calls),
            cards: cards,
            relics: relics,
            potions: potions
        );

        return data;
    }
    public static PlayData ClearData()
    {
        PlayData run = new PlayData(
            playerId: string.Empty,
            currentHp: -1,
            maxHp: -1,
            gold: -1,
            rewardCardOptionCount: -1,
            nodes: null,
            rngState: null,
            cards: null,
            relics: null,
            potions: null
        );

        return run;
    }

    // ─────────────────────────────────────────────
    // Save to DTO
    // ─────────────────────────────────────────────
    public PlaySaveData ToSaveData()
    {
        PlaySaveData save = new PlaySaveData();

        save.playerId = PlayerId;

        save.currentHp = CurrentHp;
        save.maxHp = MaxHp;
        save.gold = Gold;
        save.rewardCardOptionCount = RewardCardOptionCount;

        save.rng = new RunRngStateSaveData();
        save.rng.seed = RngState.Seed;
        save.rng.calls = RngState.Calls;

        save.cardIds = new List<string>(Cards.Select(card => card.Id));
        save.relicIds = new List<string>(Relics.Select(relic => relic.Id));
        save.potionIds = new List<string>(Potions.Select(potion => potion.Id));

        return save;
    }
    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Gold += amount;
    }

    public bool SubtractGold(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (Gold < amount)
        {
            return false;
        }

        Gold -= amount;
        return true;
    }

    public void SetHp(int currentHp, int maxHp)
    {
        MaxHp = Math.Max(1, maxHp);
        CurrentHp = Clamp(currentHp, 0, MaxHp);
    }

    public void AddCard(CardSO cardSO)
    {
        CardEntry newCard = new CardEntry(
            id: cardSO.Id,
            subId: Cards.Max(card => card.SubId),
            origin: cardSO
        );

        Cards.Add(newCard);
    }

    public void RemoveCardFromDeck(string id, int subId)
    {
        if (string.IsNullOrEmpty(id) || subId < 0)
        {
            return;
        }

        foreach (CardEntry card in Cards)
        {
            if (card.Id.Equals(id) && card.SubId == subId)
            {
                Cards.Add(card);
                return;
            }
        }
    }

    public void AddRelic(RelicInstance relic)
    {
        Relics.Add(relic);
    }
    public void RemoveRelic(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        foreach (RelicInstance relic in Relics)
        {
            if (relic.Id.Equals(id))
            {
                Relics.Remove(relic);
                return;
            }
        }
    }

    public void AddPotion(PotionSO potionSO)
    {
        PotionEntry newPotion = new PotionEntry(
            id: potionSO.Id,
            subId: Potions.Max(potion => potion.SubId),
            origin: potionSO
        );

        Potions.Add(newPotion);
    }
    public void RemovePotion(string id, int subId)
    {
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        foreach (PotionEntry potion in Potions)
        {
            if (potion.Id.Equals(id) && potion.SubId == subId)
            {
                Potions.Remove(potion);
            }
        }
    }
    // ─────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────
    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }
}
public class CardEntry 
{
    public CardEntry(string id, int subId, CardSO origin)
    {
        Id = id;
        SubId = subId;
        Origin = origin;
    }
    public string Id { get; private set; }
    public int SubId { get; private set; }
    public CardSO Origin { get; private set; }
}
public class PotionEntry
{
    public PotionEntry(string id, int subId, PotionSO origin)
    {
        Id = id;
        SubId = subId;
        Origin = origin;
    }
    public string Id { get; private set; }
    public int SubId { get; private set; }
    public PotionSO Origin { get; private set; }
}
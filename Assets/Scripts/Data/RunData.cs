using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
public class RunData
{
    public RunData
    (
        string playerId,
        int currentHp,
        int maxHp,
        int gold,
        int energy,
        int rewardCardOptionCount,
        RunRngState rngState,
        IEnumerable<int> nodes,
        IEnumerable<CardEntry> cards,
        IEnumerable<RelicInstance> relics,
        IEnumerable<PotionEntry> potions,
        Dictionary<string, PlayRecord> records
    )
    {
        PlayerId = playerId;
        CurrentHp = currentHp;
        MaxHp = maxHp;
        Gold = gold;
        Energy = energy;
        RewardCardOptionCount = rewardCardOptionCount;
        RngState = rngState;
        Nodes = new List<int>(nodes);
        Cards = new List<CardEntry>(cards);
        Relics = new List<RelicInstance>(relics);
        Potions = new List<PotionEntry>(potions);
        Records = records;
    }
    // Identity
    public string PlayerId { get; private set; }

    // Core
    public int CurrentHp { get; private set; }
    public int MaxHp { get; private set; }
    public int Gold { get; private set; }
    public int Energy { get; private set; }
    public int RewardCardOptionCount { get; private set; }

    // RNG (재현/리플레이를 원하면 필수)
    public RunRngState RngState { get; private set; }
    public List<int> Nodes {  get; private set; }

    // Inventory
    public List<CardEntry> Cards { get; private set; }
    public List<RelicInstance> Relics { get; private set; }
    public List<PotionEntry> Potions { get; private set; }

    public Dictionary<string, PlayRecord> Records { get; private set; }

    // Create
    public static RunData CreateNew(PlayerSO player, int seed, GameCatalog catalog)
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

        RunData data = new RunData(
            playerId: player.Id,
            maxHp: player.MaxHp,
            currentHp: player.MaxHp,
            gold: 0,
            energy: player.BaseEnergy,
            rewardCardOptionCount: 3,
            rngState: new RunRngState(seed),
            nodes: new List<int>(),
            cards: cards,
            relics: relics,
            potions: new List<PotionEntry>(),
            records: new Dictionary<string, PlayRecord>()
        );

        return data;
    }
    public static RunData CreateFromSaveData(RunSaveData save, GameCatalog catalog)
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
                throw new InvalidOperationException("Card not found for id: " + save.cardIds[i]);
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
                throw new InvalidOperationException("Relic not found for id: " + save.relicIds[i]);
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
                throw new InvalidOperationException("Potion not found for id: " + save.potionIds[i]);
            }
        }


        Dictionary<string, PlayRecord> records = new Dictionary<string, PlayRecord>();
        for (int i = 0; i < save.records.Count; i++)
        {
            PlayRecord record = save.records[i];
            if (!records.ContainsKey(record.Id))
            {
                records.Add(record.Id, record);
            }
            else
            {
                throw new InvalidOperationException("Record already exist for id: " + save.records[i]);
            }
        }

        // Create New
        RunData data = new RunData(
            playerId: player.Id,
            currentHp: Clamp(save.currentHp, 0, save.maxHp),
            maxHp: save.maxHp,
            gold: Math.Max(0, save.gold),
            energy: save.energy,
            rewardCardOptionCount: Math.Max(0, save.rewardCardOptionCount),
            nodes: save.nodes,
            rngState: new RunRngState(save.rng.seed, save.rng.calls),
            cards: cards,
            relics: relics,
            potions: potions,
            records: records
        );

        return data;
    }

    // ─────────────────────────────────────────────
    // Save to DTO
    // ─────────────────────────────────────────────
    public RunSaveData ToSaveData()
    {
        RunSaveData save = new RunSaveData();

        save.playerId = PlayerId;

        save.currentHp = CurrentHp;
        save.maxHp = MaxHp;
        save.gold = Gold;

        save.energy = Energy;
        save.rewardCardOptionCount = RewardCardOptionCount;

        save.rng = new RunRngStateSaveData();
        save.rng.seed = RngState.Seed;
        save.rng.calls = RngState.Calls;

        save.cardIds = new List<string>(Cards.Select(card => card.Id));
        save.relicIds = new List<string>(Relics.Select(relic => relic.Id));
        save.potionIds = new List<string>(Potions.Select(potion => potion.Id));

        save.records = new List<PlayRecord>(Records.Values);

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
            subId: Cards.Max(card => card.SubId) + 1,
            origin: cardSO
        );

        Cards.Add(newCard);
    }

    public void RemoveCard(string id, int subId)
    {
        if (string.IsNullOrEmpty(id) || subId < 0)
        {
            return;
        }

        CardEntry card = Cards.Where(x => x.Id.Equals(id) && x.SubId == subId).First();

        if (card == null)
        {
            return;
        }

        Cards.Remove(card);
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
    public void AddRecord(string key, int delta)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        if (Records.ContainsKey(key))
        {
            Records[key].Add(delta);
        }
        else
        {
            PlayRecord newRecord = new PlayRecord(
                id: key,
                value: delta
            );
            Records.Add(key, newRecord);
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
[Serializable]
public class PlayRecord
{
    public PlayRecord(string id, int value) 
    {
        Id = id;
        Value = value;
    }
    public void Add(int delta)
    {
        Value += delta;
    }
    public string Id;// { get; private set; }
    public int Value;// { get; private set; }
}
public static class PlayRecordKeys
{
    public const string ENEMY_KILL_COUNT = "몬스터 처치 수";
    public const string TURN_COUNT = "진행한 턴 수";
    public const string CARD_PLAY_COUNT = "사용한 카드 수";

    public const string COMBAT_VISIT_COUNT = "일반 전투 횟수";
    public const string ELITE_VISIT_COUNT = "엘리트 전투 횟수";
    public const string SHOP_VISIT_COUNT = "상점 방문 횟수";
    public const string TREASURE_VISIT_COUNT = "유물방 방문 횟수";
    public const string REST_VISIT_COUNT = "휴식 횟수";
}
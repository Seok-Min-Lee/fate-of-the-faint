using System;
using System.Collections.Generic;

public sealed class EnemyInstance
{
    // ─────────────────────────────────────────────
    // Identity / Static Data
    // ─────────────────────────────────────────────
    public int Id { get; }
    public EnemySO Data { get; }

    // ─────────────────────────────────────────────
    // Core Stats
    // ─────────────────────────────────────────────
    public int MaxHp { get; }
    public int Hp { get; private set; }
    public int Block { get; private set; }

    // ─────────────────────────────────────────────
    // Status Flags
    // ─────────────────────────────────────────────
    public bool IsDead => Hp <= 0;
    public bool IsEscaping { get; private set; }

    // ─────────────────────────────────────────────
    // Buff / Debuff (간단 모델)
    // ─────────────────────────────────────────────
    private readonly Dictionary<BuffType, int> buffs;

    // ─────────────────────────────────────────────
    // AI / Intent
    // ─────────────────────────────────────────────
    public EnemyActionSO NextAction { get; private set; }

    private readonly Dictionary<string, int> actionCooldowns;
    private readonly Queue<string> recentActionKeys;

    // ─────────────────────────────────────────────
    // ctor
    // ─────────────────────────────────────────────
    public EnemyInstance(int id, EnemySO data, int maxHp)
    {
        Id = id;
        Data = data;

        MaxHp = maxHp;
        Hp = maxHp;
        Block = 0;

        buffs = new Dictionary<BuffType, int>();

        actionCooldowns = new Dictionary<string, int>();
        recentActionKeys = new Queue<string>(8);
    }

    // ─────────────────────────────────────────────
    // AI
    // ─────────────────────────────────────────────
    public void DecideNextAction(Random rng)
    {
        if (rng == null)
        {
            throw new ArgumentNullException(nameof(rng));
        }

        TickCooldowns();

        List<EnemyActionSO> candidates = CollectCandidates();
        if (candidates.Count == 0)
        {
            // 정책상 후보가 0이 되면 안정적으로 전체 actions에서 다시 뽑게 하거나,
            // 첫 action으로 fallback 하는 식으로 처리할 수 있음.
            NextAction = Data.aiPolicy.actions[0];
            RegisterAction(NextAction);
            return;
        }

        NextAction = PickWeighted(candidates, rng);
        RegisterAction(NextAction);
    }

    // ─────────────────────────────────────────────
    // Combat API (CombatSystem에서 호출)
    // ─────────────────────────────────────────────
    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (IsDead)
        {
            return;
        }

        int dmgToBlock = Math.Min(Block, amount);
        Block -= dmgToBlock;

        int remaining = amount - dmgToBlock;
        if (remaining <= 0)
        {
            return;
        }

        Hp = Math.Max(0, Hp - remaining);
    }

    public void GainBlock(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Block += amount;
    }

    public void ClearBlock()
    {
        Block = 0;
    }

    public void ApplyBuff(BuffType type, int delta)
    {
        if (delta == 0)
        {
            return;
        }

        if (!buffs.ContainsKey(type))
        {
            buffs[type] = 0;
        }

        buffs[type] += delta;

        if (buffs[type] <= 0)
        {
            buffs.Remove(type);
        }
    }

    public int GetBuff(BuffType type)
    {
        int value;
        if (!buffs.TryGetValue(type, out value))
        {
            return 0;
        }

        return value;
    }

    public void Escape()
    {
        IsEscaping = true;
    }

    // ─────────────────────────────────────────────
    // AI internals
    // ─────────────────────────────────────────────
    private void TickCooldowns()
    {
        if (actionCooldowns.Count == 0)
        {
            return;
        }

        List<string> keys = new List<string>(actionCooldowns.Keys);

        int i = 0;
        while (i < keys.Count)
        {
            string key = keys[i];

            actionCooldowns[key]--;

            if (actionCooldowns[key] <= 0)
            {
                actionCooldowns.Remove(key);
            }

            i++;
        }
    }

    private List<EnemyActionSO> CollectCandidates()
    {
        List<EnemyActionSO> list = new List<EnemyActionSO>();

        List<EnemyActionSO> actions = Data.aiPolicy.actions;
        int i = 0;
        while (i < actions.Count)
        {
            EnemyActionSO action = actions[i];

            if (actionCooldowns.ContainsKey(action.key))
            {
                i++;
                continue;
            }

            if (IsOverRepeatLimit(action.key))
            {
                i++;
                continue;
            }

            list.Add(action);
            i++;
        }

        return list;
    }

    private bool IsOverRepeatLimit(string actionKey)
    {
        int limit = Data.aiPolicy.globalMaxRepeat;
        if (limit <= 0)
        {
            return false;
        }

        int count = 0;
        foreach (string key in recentActionKeys)
        {
            if (key != actionKey)
            {
                break;
            }

            count++;
        }

        if (count >= limit)
        {
            return true;
        }

        return false;
    }

    private void RegisterAction(EnemyActionSO action)
    {
        recentActionKeys.Enqueue(action.key);

        while (recentActionKeys.Count > 8)
        {
            recentActionKeys.Dequeue();
        }

        if (action.cooldownTurns > 0)
        {
            actionCooldowns[action.key] = action.cooldownTurns;
        }
    }

    private static EnemyActionSO PickWeighted(List<EnemyActionSO> actions, Random rng)
    {
        int total = 0;

        int i = 0;
        while (i < actions.Count)
        {
            EnemyActionSO action = actions[i];
            total += Math.Max(0, action.weight);
            i++;
        }

        if (total <= 0)
        {
            return actions[0];
        }

        int r = rng.Next(0, total);

        int acc = 0;
        int j = 0;
        while (j < actions.Count)
        {
            EnemyActionSO move = actions[j];

            acc += Math.Max(0, move.weight);
            if (r < acc)
            {
                return move;
            }

            j++;
        }

        return actions[0];
    }
}

public enum BuffType
{
    Strength,
    Weak,
    Vulnerable
}

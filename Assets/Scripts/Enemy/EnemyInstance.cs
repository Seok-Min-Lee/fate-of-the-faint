using System;
using System.Collections.Generic;

public class EnemyInstance : EntityInstance
{
    // Identity / Static Data
    public EnemySO Data { get; }
    public EnemyActionSO NextAction { get; private set; }

    private readonly Dictionary<string, int> actionCooldowns;
    private readonly Queue<string> recentActionKeys;

    // ctor
    public EnemyInstance(EnemySO data, int maxHp)
    {
        Id = Guid.NewGuid();
        Data = data;

        MaxHp = maxHp;
        CurrentHp = maxHp;
        Block = 0;

        buffs = new Dictionary<BuffType, int>();

        actionCooldowns = new Dictionary<string, int>();
        recentActionKeys = new Queue<string>(8);
    }

    // AI
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

        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i];

            actionCooldowns[key]--;

            if (actionCooldowns[key] <= 0)
            {
                actionCooldowns.Remove(key);
            }
        }
    }

    private List<EnemyActionSO> CollectCandidates()
    {
        List<EnemyActionSO> list = new List<EnemyActionSO>();

        List<EnemyActionSO> actions = Data.aiPolicy.actions;
        for (int i = 0; i < actions.Count; i++)
        {
            EnemyActionSO action = actions[i];

            if (actionCooldowns.ContainsKey(action.Key))
            {
                continue;
            }

            if (IsOverRepeatLimit(action.Key))
            {
                continue;
            }

            list.Add(action);
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
        recentActionKeys.Enqueue(action.Key);

        while (recentActionKeys.Count > 8)
        {
            recentActionKeys.Dequeue();
        }

        if (action.CooldownTurns > 0)
        {
            actionCooldowns[action.Key] = action.CooldownTurns;
        }
    }

    private static EnemyActionSO PickWeighted(List<EnemyActionSO> actions, Random rng)
    {
        int total = 0;

        int i = 0;
        while (i < actions.Count)
        {
            EnemyActionSO action = actions[i];
            total += Math.Max(0, action.Weight);
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

            acc += Math.Max(0, move.Weight);
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

using System;
using System.Collections.Generic;
using UnityEngine;
public class EntityInstance
{
    public Guid Id { get; protected set; }
    public int MaxHp { get; protected set; }
    public int CurrentHp { get; protected set; }
    public int Block { get; protected set; }
    public bool IsDead => CurrentHp <= 0;
    public IReadOnlyDictionary<BuffType, int> Buffs => buffs;

    protected Dictionary<BuffType, int> buffs;

    public void SetCurrentHp(int value)
    {
        CurrentHp = value;
    }
    public void SetBlock(int value)
    {
        Block = value;
    }
    public void AddBlock(int amount)
    {
        Block += amount;
    }
    public void ApplyBuff(BuffType type, int delta)
    {
        if (!buffs.ContainsKey(type))
        {
            buffs.Add(type, delta);
        }
        else
        {
            buffs[type] += delta;

            if (buffs[type] <= 0)
            {
                buffs.Remove(type);
            }
        }
    }
    public int Getbuff(BuffType type)
    {
        if (!buffs.TryGetValue(type, out int value))
        {
            return -1;
        }

        return value;
    }
}
public enum BuffType
{
    Strength,
    Weak,
    Vulnerable
}
public interface ITargetable
{
    Transform AimPoint { get; }
    EntityInstance Instance { get; }
}
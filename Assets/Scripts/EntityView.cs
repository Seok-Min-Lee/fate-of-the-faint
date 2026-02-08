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

    public void SetCurrentHp(int amount)
    {
        CurrentHp = amount;
    }
}
//public class EntityView : MonoBehaviour, ITargetable
//{
//    [SerializeField] private Transform aimPoint;
//    public Transform AimPoint => aimPoint;
//}
public interface ITargetable
{
    Transform AimPoint { get; }
    EntityInstance Instance { get; }
}
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

    public void ApplyBuff(EventBus eventBus, EventContext context, MotionContext motion, BuffType type, int delta)
    {
        int startAmount = 0;
        if (buffs.TryGetValue(type, out startAmount))
        {
            buffs[type] = Mathf.Max(0, startAmount + delta);
        }
        else
        {
            buffs.Add(type, Mathf.Max(0, delta));
        }

        eventBus.Publish<BuffChanged>(new BuffChanged(
            context: context.RewriteNew(this),
            motion: motion,
            target: this,
            type: type,
            startAmount: startAmount,
            endAmount: buffs[type]
        ));

        if (buffs[type] <= 0)
        {
            buffs.Remove(type);
        }
    }
    public void ChangeBlock(EventBus eventBus, EventContext context, MotionContext motion, int amount)
    {
        if (amount == 0)
        {
            return;
        }

        int startAmount = Block;
        Block = Mathf.Max(0, Block + amount);

        eventBus.Publish<BlockChanged>(new BlockChanged(
            context: context.RewriteNew(this),
            motion: motion,
            target: this,
            startAmount: startAmount,
            endAmount: Block
        ));
    }
    public void ChangeHp(EventBus eventBus, EventContext context, MotionContext motion, int amount)
    {
        int startAmount = CurrentHp;
        CurrentHp = Mathf.Clamp(CurrentHp + amount, 0, MaxHp);

        eventBus.Publish<HpChanged>(new HpChanged(
            context: context.RewriteNew(this),
            motion: motion,
            target: this,
            startAmount: startAmount,
            endAmount: CurrentHp
        ));
    }
    public void Hit(EventBus eventBus, EventContext context, MotionContext motion, int amount)
    {
        int damage = amount;
        if (Block > 0)
        {
            damage -= Block;
            ChangeBlock(eventBus, context, motion, -amount);
        }

        Damage(eventBus, context, motion, damage);
    }
    public void Damage(EventBus eventBus, EventContext context, MotionContext motion, int amount)
    {
        if (amount > 0)
        {
            ChangeHp(eventBus, context, motion, -amount);
        }

        if (CurrentHp <= 0)
        {
            if (this is EnemyInstance enemy)
            {
                context.Combat.AddGold(enemy.GoldReward);
            }

            eventBus.Publish<DeathDeclared>(new DeathDeclared(
                context: context.RewriteNew(this),
                motion: motion,
                target: this
            ));
        }
    }
    public void ModifyDamage(DamageContext context)
    {
        // 공격 보정
        if (context.Source == this)
        {
            if (Buffs.TryGetValue(key: BuffType.Strength, value: out int strength))
            {
                context.Add(strength, this);
            }

            if (Buffs.ContainsKey(BuffType.Weak))
            {
                context.Multiply(0.5f, this);
            }
        }

        // 피격 보정
        if (context.Target == this)
        {
            if (Buffs.ContainsKey(BuffType.Vulnerable))
            {
                context.Multiply(1.5f, this);
            }
        }
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
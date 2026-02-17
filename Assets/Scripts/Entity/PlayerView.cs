using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : EntityView, ITargetable
{
    private CombatManager combatManager;
    private AnimationMonoSystem animationSystem;
    private PlayerInstance instance;
    public Transform AimPoint => aimPoint;
    public EntityInstance Instance => instance;
    public void Init(
        PlayerInstance instance, 
        CombatManager combatManager, 
        AnimationMonoSystem animationSystem, 
        Vector3 position, 
        EntityBuffViewPool buffViewPool,
        DamageTextPool damageTextPool
    )
    {
        this.combatManager = combatManager;
        this.animationSystem = animationSystem;
        this.instance = instance;
        base.buffViewPool = buffViewPool;
        base.damageTextPool = damageTextPool;

        transform.position = position;

        combatManager.CombatSystem.EventBus.Subscribe<CombatStarted>(OnCombatStarted);
        combatManager.CombatSystem.EventBus.Subscribe<CombatEnded>(OnCombatEnded);
        combatManager.CombatSystem.EventBus.Subscribe<DeathDeclared>(OnDeathDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<AttackDeclared>(OnAttackDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<BlockDeclared>(OnBlockDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<HpChanged>(OnHpChanged);
        combatManager.CombatSystem.EventBus.Subscribe<BlockChanged>(OnBlockChanged);
        combatManager.CombatSystem.EventBus.Subscribe<BuffChanged>(OnBuffChanged);

        animator.SetBool(AnimationKeys.PLAYER_ENCOUNTER, true);
        statusCG.alpha = 0f;
        hpView.Init(instance.CurrentHp, instance.MaxHp);
        blockView.Init(instance.Block);
    }
    private void OnDisable()
    {
        combatManager.CombatSystem.EventBus.Unsubscribe<CombatStarted>(OnCombatStarted);
        combatManager.CombatSystem.EventBus.Unsubscribe<CombatEnded>(OnCombatEnded);
        combatManager.CombatSystem.EventBus.Unsubscribe<DeathDeclared>(OnDeathDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<AttackDeclared>(OnAttackDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<BlockDeclared>(OnBlockDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<HpChanged>(OnHpChanged);
        combatManager.CombatSystem.EventBus.Unsubscribe<BlockChanged>(OnBlockChanged);
        combatManager.CombatSystem.EventBus.Unsubscribe<BuffChanged>(OnBuffChanged);
    }
    public void OnCombatStarted(CombatStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        animationSystem.Register(
            priority: AnimationPriority.UIWindow, 
            command: () => PlayAnimatorBoolCor(AnimationKeys.PLAYER_ENCOUNTER, false)
        );
        animationSystem.Register(
            priority: AnimationPriority.Entity, 
            command: () => ShowStatusCor()
        );
    }
    public void OnCombatEnded(CombatEnded e)
    {
        if (e.Result != CombatState.Victory)
        {
            return;
        }

        animationSystem.Register(
            priority: AnimationPriority.Entity, 
            command: () => PlayAnimatorTriggerCor(AnimationKeys.PLAYER_VICTORY, 1f)
        );
    }
    public void OnDeathDeclared(DeathDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Source.Id != instance.Id)
        {
            return;
        }

        animationSystem.Register(
            priority: AnimationPriority.Target,
            command: () => HideStatusCor()
        );
        animationSystem.Register(
            priority: AnimationPriority.Target, 
            command: () => PlayAnimatorTriggerCor(AnimationKeys.PLAYER_DIE, 1f)
        );
    }
    public void OnAttackDeclared(AttackDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Source != instance)
        {
            return;
        }

        animationSystem.Register(
            priority: AnimationPriority.Actor,
            command: () => PlayAnimatorTriggerCor(AnimationKeys.PLAYER_ATTACK)
        );
    }
    public void OnBlockDeclared(BlockDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Target != instance)
        {
            return;
        }

        animationSystem.Register(
            priority: AnimationPriority.Actor, 
            command: () => PlayAnimatorTriggerCor(AnimationKeys.PLAYER_SKILL, 1f)
        );
    }
    public void OnHpChanged(HpChanged e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Target.Id != instance.Id)
        {
            return;
        }

        if (e.EndAmount < e.StartAmount)
        {
            animationSystem.Register(
                priority: AnimationPriority.Target,
                command: () => ShowDamageTextCor(e.StartAmount - e.EndAmount)
            );

            if (e.EndAmount > 0)
            {
                animationSystem.Register(
                    priority: AnimationPriority.Target, 
                    command: () => PlayAnimatorTriggerCor(AnimationKeys.PLAYER_HIT, 1f)
                );
            }
        }

        animationSystem.Register(
            priority: AnimationPriority.Entity,
            command: () => ChangeHpCor(instance.CurrentHp, instance.MaxHp)
        );
    }
    public void OnBlockChanged(BlockChanged e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Target.Id != instance.Id)
        {
            return;
        }

        if (e.EndAmount > e.StartAmount)
        {
            animationSystem.Register(
                priority: AnimationPriority.Actor,
                command: () => ShowBlockCor(instance.Block)
            );
        }
        else
        {
            animationSystem.Register(
                priority: AnimationPriority.Target,
                command: () => ChangeBlockCor(instance.Block)
            );

            if (instance.Block <= 0)
            {
                animationSystem.Register(
                    priority: AnimationPriority.Target,
                    command: () => HideBlockCor()
                );
            }
        }
    }
    public void OnBuffChanged(BuffChanged e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Target.Id != instance.Id)
        {
            return;
        }

        if (buffViewDictionary.TryGetValue(e.Type, out EntityBuffView value))
        {
            if (e.EndAmount > 0)
            {
                value.SetText(e.EndAmount.ToString());
            }
            else
            {
                buffViewPool.Push(value);
                buffViewDictionary.Remove(e.Type);
            }
        }
        else
        {
            if (e.EndAmount > 0)
            {
                EntityBuffView view = buffViewPool.Pop();

                for (int i = 0; i < buffPresets.Length; i++)
                {
                    if (buffPresets[i].Type == e.Type)
                    {
                        view.Init(
                            preset: buffPresets[i], 
                            text: e.EndAmount.ToString(),
                            parent: buffParent
                        );

                        buffViewDictionary.Add(view.Type, view);
                        break;
                    }
                }
            }
        }
    }
}

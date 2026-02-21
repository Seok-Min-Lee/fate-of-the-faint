using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : EntityView, ITargetable
{
    private CombatManager combatManager;
    public Transform AimPoint => aimPoint;
    public EntityInstance Instance => instance;
    public void Init(
        PlayerInstance instance, 
        CombatManager combatManager, 
        Vector3 position, 
        EntityBuffViewPool buffViewPool,
        DamageTextPool damageTextPool
    )
    {
        this.combatManager = combatManager;
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

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Start,
            command: () => PlayAnimatorBoolCor(AnimationKeys.PLAYER_ENCOUNTER, false),
            source: this
        ));

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Entity,
            command: () => ShowStatusCor(),
            source: this
        ));
    }
    public void OnCombatEnded(CombatEnded e)
    {
        if (e.Result != CombatState.Victory)
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Entity,
            command: () => PlayAnimatorTriggerCor(AnimationKeys.PLAYER_VICTORY),
            source: this
        ));
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

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Target,
            command: () => HideStatusCor(),
            source: this
        ));
        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Target,
            command: () => PlayAnimatorTriggerCor(AnimationKeys.PLAYER_DIE),
            source: this
        ));
    }
    public void OnAttackDeclared(AttackDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Source != instance)
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Actor,
            command: () => PlayAnimatorTriggerCor(AnimationKeys.PLAYER_ATTACK),
            source: this
        ));
    }
    public void OnBlockDeclared(BlockDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Target != instance)
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Actor,
            command: () => PlayAnimatorTriggerCor(AnimationKeys.PLAYER_SKILL),
            source: this
        ));
    }
}

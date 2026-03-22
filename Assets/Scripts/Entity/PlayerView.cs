using DG.Tweening;
using System.Collections;
using UnityEngine;

public class PlayerView : EntityView, ITargetable
{
    private EventBus eventBus;
    public Transform AimPoint => aimPoint;
    public EntityInstance Instance => instance;

    public void Init(
        EventBus eventBus,
        PlayerInstance instance, 
        Vector3 position, 
        EntityBuffViewPool buffViewPool,
        DamageTextPool damageTextPool
    )
    {
        this.eventBus = eventBus;
        this.instance = instance;
        base.buffViewPool = buffViewPool;
        base.damageTextPool = damageTextPool;

        transform.position = position;

        eventBus.Subscribe<CombatStarted>(OnCombatStarted);
        eventBus.Subscribe<CombatEnded>(OnCombatEnded);
        eventBus.Subscribe<DeathDeclared>(OnDeathDeclared);
        eventBus.Subscribe<AttackPlayed>(OnAttackPlayed);
        eventBus.Subscribe<SkillPlayed>(OnSkillPlayed);
        eventBus.Subscribe<PowerPlayed>(OnPowerPlayed);
        eventBus.Subscribe<HpChanged>(OnHpChanged);
        eventBus.Subscribe<BlockChanged>(OnBlockChanged);
        eventBus.Subscribe<BuffChanged>(OnBuffChanged);

        animator.SetBool(AnimationKeys.PLAYER_ENCOUNTER, true);
        statusCG.alpha = 0f;
        hpView.Init(instance.CurrentHp, instance.MaxHp);
        blockView.Init(instance.Block);
    }
    private void OnDisable()
    {
        eventBus.Unsubscribe<CombatStarted>(OnCombatStarted);
        eventBus.Unsubscribe<CombatEnded>(OnCombatEnded);
        eventBus.Unsubscribe<DeathDeclared>(OnDeathDeclared);
        eventBus.Unsubscribe<AttackPlayed>(OnAttackPlayed);
        eventBus.Unsubscribe<SkillPlayed>(OnSkillPlayed);
        eventBus.Unsubscribe<PowerPlayed>(OnPowerPlayed);
        eventBus.Unsubscribe<HpChanged>(OnHpChanged);
        eventBus.Unsubscribe<BlockChanged>(OnBlockChanged);
        eventBus.Unsubscribe<BuffChanged>(OnBuffChanged);
    }
    public void OnCombatStarted(CombatStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        animator.SetBool(AnimationKeys.PLAYER_ENCOUNTER, false);

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
            priority: MotionPriority.Entity,
            command: () => DeathCor(AnimationKeys.PLAYER_DIE),
            source: this
        ));
    }
    public void OnAttackPlayed(AttackPlayed e)
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
    public void OnSkillPlayed(SkillPlayed e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Source != instance)
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Actor,
            command: () => PlayAnimatorTriggerCor(AnimationKeys.PLAYER_SKILL),
            source: this
        ));
    }
    public void OnPowerPlayed(PowerPlayed e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Source != instance)
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Actor,
            command: () => PlayPowerCor(AnimationKeys.PLAYER_POWER),
            source: this
        ));
    }
    IEnumerator PlayPowerCor(string key)
    {
        yield return PlayAnimatorTriggerCor(key);

        particle.Play(EntityParticleKey.Power);
        yield break;
    }
    public Tween Move(Vector3 target)
    {
        return transform.DOMove(target, 1f).SetEase(Ease.Linear);
    }
}

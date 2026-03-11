using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
public class EnemyView : EntityView, ITargetable
{
    [SerializeField] private IntentView intentView;
    public Transform AimPoint => aimPoint;
    public EntityInstance Instance => instance;

    private EventBus eventBus;

    public void Init(
        EnemyInstance instance,
        EventBus eventBus, 
        Vector3 position, 
        EntityBuffViewPool buffViewPool,
        DamageTextPool damageTextPool
    )
    {
        this.instance = instance;
        this.eventBus = eventBus;
        base.buffViewPool = buffViewPool;
        base.damageTextPool = damageTextPool;

        transform.position = position;

        eventBus.Subscribe<CombatStarted>(OnCombatStarted);
        eventBus.Subscribe<CombatEnded>(OnCombatEnded);
        eventBus.Subscribe<DeathDeclared>(OnDeathDeclared);
        eventBus.Subscribe<AttackPlayed>(OnAttackPlayed);
        eventBus.Subscribe<SkillPlayed>(OnSkillPlayed);
        eventBus.Subscribe<EnemyIntentDecided>(OnEnemyIntentDecided);
        eventBus.Subscribe<HpChanged>(OnHpChanged);
        eventBus.Subscribe<BlockChanged>(OnBlockChanged);
        eventBus.Subscribe<BuffChanged>(OnBuffChanged);

        statusCG.alpha = 0f;
        hpView.Init(instance.CurrentHp, instance.MaxHp);
        blockView.Init(instance.Block);
        intentView.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        eventBus.Unsubscribe<CombatStarted>(OnCombatStarted);
        eventBus.Unsubscribe<CombatEnded>(OnCombatEnded);
        eventBus.Unsubscribe<DeathDeclared>(OnDeathDeclared);
        eventBus.Subscribe<AttackPlayed>(OnAttackPlayed);
        eventBus.Subscribe<SkillPlayed>(OnSkillPlayed);
        eventBus.Unsubscribe<EnemyIntentDecided>(OnEnemyIntentDecided);
        eventBus.Unsubscribe<HpChanged>(OnHpChanged);
        eventBus.Unsubscribe<BlockChanged>(OnBlockChanged);
        eventBus.Unsubscribe<BuffChanged>(OnBuffChanged);
    }
    public void OnCombatStarted(CombatStarted e)
    {
        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Entity,
            command: () => ShowStatusCor(),
            source: this
        ));
    }
    public void OnCombatEnded(CombatEnded e)
    {
        if (e.Result != CombatState.Defeat)
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Entity,
            command: () => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_VICTORY),
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
            command: () => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_DIE),
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
        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Actor,
            command: () => HideIntentCor(),
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
        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Actor,
            command: () => HideIntentCor(),
            source: this
        ));
    }
    public void OnEnemyIntentDecided(EnemyIntentDecided e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Source.Id != instance.Id)
        {
            return;
        }

        if (instance is not EnemyInstance enemy)
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Actor,
            command: () => ShowIntentCor(enemy.NextAction),
            source: this
        ));
    }
    private IEnumerator ShowIntentCor(EnemyActionSO intent)
    {
        intentView.Show(intent);
        yield break;
    }
    private IEnumerator HideIntentCor()
    {
        intentView.Hide();
        yield break;
    }
}

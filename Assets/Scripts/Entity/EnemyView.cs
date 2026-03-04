using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
public class EnemyView : EntityView, ITargetable
{
    [SerializeField] private IntentView intentView;
    public Transform AimPoint => aimPoint;
    public EntityInstance Instance => instance;

    private CombatManager combatManager;

    public void Init(
        EnemyInstance instance,
        CombatManager combatManager, 
        Vector3 position, 
        EntityBuffViewPool buffViewPool,
        DamageTextPool damageTextPool
    )
    {
        this.instance = instance;
        this.combatManager = combatManager;
        base.buffViewPool = buffViewPool;
        base.damageTextPool = damageTextPool;

        transform.position = position;

        combatManager.CombatSystem.EventBus.Subscribe<CombatStarted>(OnCombatStarted);
        combatManager.CombatSystem.EventBus.Subscribe<CombatEnded>(OnCombatEnded);
        combatManager.CombatSystem.EventBus.Subscribe<DeathDeclared>(OnDeathDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<AttackPlayed>(OnAttackPlayed);
        combatManager.CombatSystem.EventBus.Subscribe<SkillPlayed>(OnSkillPlayed);
        combatManager.CombatSystem.EventBus.Subscribe<EnemyIntentDecided>(OnEnemyIntentDecided);
        combatManager.CombatSystem.EventBus.Subscribe<HpChanged>(OnHpChanged);
        combatManager.CombatSystem.EventBus.Subscribe<BlockChanged>(OnBlockChanged);
        combatManager.CombatSystem.EventBus.Subscribe<BuffChanged>(OnBuffChanged);

        statusCG.alpha = 0f;
        hpView.Init(instance.CurrentHp, instance.MaxHp);
        blockView.Init(instance.Block);
        intentView.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        combatManager.CombatSystem.EventBus.Unsubscribe<CombatStarted>(OnCombatStarted);
        combatManager.CombatSystem.EventBus.Unsubscribe<CombatEnded>(OnCombatEnded);
        combatManager.CombatSystem.EventBus.Unsubscribe<DeathDeclared>(OnDeathDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<AttackPlayed>(OnAttackPlayed);
        combatManager.CombatSystem.EventBus.Subscribe<SkillPlayed>(OnSkillPlayed);
        combatManager.CombatSystem.EventBus.Unsubscribe<EnemyIntentDecided>(OnEnemyIntentDecided);
        combatManager.CombatSystem.EventBus.Unsubscribe<HpChanged>(OnHpChanged);
        combatManager.CombatSystem.EventBus.Unsubscribe<BlockChanged>(OnBlockChanged);
        combatManager.CombatSystem.EventBus.Unsubscribe<BuffChanged>(OnBuffChanged);
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

        EnemyInstance enemy = instance as EnemyInstance;
        Sprite icon = enemy.NextAction.IntentIcon;
        string text = enemy.NextAction.Effects[0].Value.ToString();

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Actor,
            command: () => ShowIntentCor(sprite: icon, text: text),
            source: this
        ));
    }
    private IEnumerator ShowIntentCor(Sprite sprite, string text)
    {
        intentView.Show(sprite, text);
        yield break;
    }
    private IEnumerator HideIntentCor()
    {
        intentView.Hide();
        yield break;
    }
}

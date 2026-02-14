using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
public class EnemyView : EntityView, ITargetable
{
    public Transform AimPoint => aimPoint;
    public EntityInstance Instance => instance;

    [SerializeField] private SpriteRenderer intentRenderer;
    [SerializeField] private TextMeshPro intentText;

    private EnemyInstance instance;
    private CombatManager combatManager;
    private AnimationMonoSystem animationSystem;

    private void OnDisable()
    {
        combatManager.CombatSystem.EventBus.Unsubscribe<CombatStarted>(OnCombatStarted);
        combatManager.CombatSystem.EventBus.Unsubscribe<CombatEnded>(OnCombatEnded);
        combatManager.CombatSystem.EventBus.Unsubscribe<DeathDeclared>(OnDeathDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<AttackDeclared>(OnAttackDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<EnemyIntentDecided>(OnEnemyIntentDecided);
        combatManager.CombatSystem.EventBus.Unsubscribe<HpChanged>(OnHpChanged);
    }
    public void Init(EnemyInstance instance, CombatManager combatManager, AnimationMonoSystem animationSystem, Vector3 position)
    {
        this.instance = instance;
        this.combatManager = combatManager;
        this.animationSystem = animationSystem;
        transform.position = position;

        this.combatManager.CombatSystem.EventBus.Subscribe<CombatStarted>(OnCombatStarted);
        this.combatManager.CombatSystem.EventBus.Subscribe<CombatEnded>(OnCombatEnded);
        this.combatManager.CombatSystem.EventBus.Subscribe<DeathDeclared>(OnDeathDeclared);
        this.combatManager.CombatSystem.EventBus.Subscribe<AttackDeclared>(OnAttackDeclared);
        this.combatManager.CombatSystem.EventBus.Subscribe<EnemyIntentDecided>(OnEnemyIntentDecided);
        this.combatManager.CombatSystem.EventBus.Subscribe<HpChanged>(OnHpChanged);
    }
    public void OnCombatEnded(CombatEnded e)
    {
        if (e.Result != CombatState.Defeat)
        {
            return;
        }

        animationSystem.Enqueue(() => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_VICTORY, 0f));
    }
    public void OnCombatStarted(CombatStarted e)
    {
        hpText.text = instance.CurrentHp.ToString();
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

        animationSystem.Enqueue(() => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_DIE, 1f));
    }
    public void OnAttackDeclared(AttackDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Source != instance)
        {
            return;
        }
        intentRenderer.gameObject.SetActive(false);
        intentText.gameObject.SetActive(false);
        animationSystem.Enqueue(() => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_ATTACK, 1f));
    }
    public void OnEnemyIntentDecided(EnemyIntentDecided e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        intentRenderer.sprite = instance.NextAction.IntentIcon;
        intentText.text = instance.NextAction.Effects[0].value.ToString();

        intentRenderer.gameObject.SetActive(true);
        intentText.gameObject.SetActive(true);
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

        if (e.EndAmount > 0 && e.EndAmount < e.StartAmount)
        {
            animationSystem.Enqueue(() => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_HIT, 1f));
        }

        hpText.text = e.EndAmount.ToString();
    }
    private IEnumerator PlayAnimatorTriggerCor(string key, float duration)
    {
        animator.SetTrigger(key);
        yield return new WaitForSeconds(duration);
    }
}

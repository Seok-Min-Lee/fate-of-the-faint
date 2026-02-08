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

    private void OnDisable()
    {
        combatManager.CombatSystem.EventBus.Unsubscribe<CombatStarted>(OnCombatStarted);
        combatManager.CombatSystem.EventBus.Unsubscribe<CombatEnded>(OnCombatEnded);
        combatManager.CombatSystem.EventBus.Unsubscribe<DeathDeclared>(OnDeathDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<AttackDeclared>(OnAttackDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<EnemyIntentDecided>(OnEnemyIntentDecided);
        combatManager.CombatSystem.EventBus.Unsubscribe<HpChanged>(OnHpChanged);
    }
    public void Init(EnemyInstance instance, Vector3 position, CombatManager combat)
    {
        this.instance = instance;
        transform.position = position;
        combatManager = combat;

        combatManager.CombatSystem.EventBus.Subscribe<CombatStarted>(OnCombatStarted);
        combatManager.CombatSystem.EventBus.Subscribe<CombatEnded>(OnCombatEnded);
        combatManager.CombatSystem.EventBus.Subscribe<DeathDeclared>(OnDeathDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<AttackDeclared>(OnAttackDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<EnemyIntentDecided>(OnEnemyIntentDecided);
        combatManager.CombatSystem.EventBus.Subscribe<HpChanged>(OnHpChanged);
    }
    public void OnCombatEnded(CombatEnded e)
    {
        if (e.Result != CombatState.Defeat)
        {
            return;
        }

        animator.SetTrigger(AnimationKeys.ENEMY_VICTORY);
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

        animator.SetTrigger(AnimationKeys.ENEMY_DIE);
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
        animator.SetTrigger(AnimationKeys.ENEMY_ATTACK);
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

        if (e.EndAmount < e.StartAmount)
        {
            animator.SetTrigger(AnimationKeys.ENEMY_HIT);
        }

        hpText.text = e.EndAmount.ToString();
    }
}

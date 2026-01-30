using Unity.VisualScripting;
using UnityEngine;

public class Enemy : Entity
{
    [SerializeField] private CombatManager combatManager;
    public int hp;
    public int strength;
    public int shield;
    public EnemyIntent intent;

    public bool IsDeath => hp <= 0;

    private void OnEnable()
    {
        combatManager.EventBus.Subscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        combatManager.EventBus.Subscribe<EnemyActionStartRequested>(OnEnemyActionStartRequested);
        combatManager.EventBus.Subscribe<DamageRequested>(OnDamageRequested);
        combatManager.EventBus.Subscribe<DamageResolved>(OnDamageResolved);

    }
    private void OnDisable()
    {
        combatManager.EventBus.Unsubscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        combatManager.EventBus.Unsubscribe<EnemyActionStartRequested>(OnEnemyActionStartRequested);
        combatManager.EventBus.Unsubscribe<DamageRequested>(OnDamageRequested);
        combatManager.EventBus.Unsubscribe<DamageResolved>(OnDamageResolved);
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        intent = EnemyIntent.Attack; // Simplified for example
    }
    public void OnEnemyActionStartRequested(EnemyActionStartRequested e)
    {
        if (e.Enemy != this)
        {
            e.Request.isResult = false;
            return;
        }
        e.Request.isResult = true;

        ActionContext actionContext = new ActionContext(
            source: this,
            type: ActionType.EnemyAct
        );
        combatManager.ExcuteAction(actionContext, (eventContext) => 
        {
            // Simplified action execution based on intent
            switch (intent)
            {
                case EnemyIntent.Attack:
                    break;
                default:
                    break;
            }
        });
    }
    private void OnDamageRequested(DamageRequested e)
    {
        if (e.Damage.Source == this)
        {
            e.Damage.Add(strength, this);
        }
        else if (e.Damage.Target == this as ITargetable)
        {
            e.Damage.Subtract(shield, this);
        }
    }
    private void OnDamageResolved(DamageResolved e)
    {
        if (e.Target == this as ITargetable)
        {
            int startAmount = hp;
            hp -= e.Amount;

            EventContext eventContext = new EventContext(
                source: this, 
                action: e.Context.Action,
                turn: combatManager.TurnSystem.TurnContext,
                combat: combatManager.CombatContext
            );

            combatManager.EventBus.Publish<HpChanged>(new HpChanged(
                context: eventContext,
                target: this,
                startAmount: startAmount,
                endAmount: hp
            ));

            if (hp <= 0)
            {
                hp = 0;

                eventContext = new EventContext(
                    source: this,
                    action: e.Context.Action,
                    turn: combatManager.TurnSystem.TurnContext,
                    combat: combatManager.CombatContext
                );

                combatManager.EventBus.Publish<DeathDeclared>(new DeathDeclared(
                    context: eventContext,
                    target: this
                ));
            }
        }
    }
}
public enum EnemyIntent
{
    Attack,
    Defend,
    Buff,
    Debuff,
    None
}

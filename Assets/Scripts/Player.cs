using UnityEngine;

public class Player : Entity
{
    [SerializeField] private CombatManager combatManager;
    public int hp;
    public int strength;
    public int shield;

    private void OnEnable()
    {
        combatManager.EventBus.Subscribe<PlayerTurnEnded>(OnPlayerTurnEnded);
        combatManager.EventBus.Subscribe<DamageRequested>(OnDamageRequested);
        combatManager.EventBus.Subscribe<DamageResolved>(OnDamageResolved);
    }
    private void OnDisable()
    {
        combatManager.EventBus.Subscribe<PlayerTurnEnded>(OnPlayerTurnEnded);
        combatManager.EventBus.Unsubscribe<DamageRequested>(OnDamageRequested);
        combatManager.EventBus.Unsubscribe<DamageResolved>(OnDamageResolved);
    }
    public void OnPlayerTurnEnded(PlayerTurnEnded e)
    {
    }
    private void OnDamageRequested(DamageRequested e)
    {
        if (e.Damage.Source == this)
        {
            e.Damage.Add(strength, this);
        }
        else if (e.Damage.Target == this)
        {
            e.Damage.Subtract(shield, this);
        }
    }
    private void OnDamageResolved(DamageResolved e)
    {
        if (e.Target == this)
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

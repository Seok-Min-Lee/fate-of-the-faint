using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;

    private void OnEnable()
    {
        combatManager.EventBus.Subscribe<AttackDeclared>(OnAttackDeclared);
    }
    private void OnDisable()
    {
        combatManager.EventBus.Unsubscribe<AttackDeclared>(OnAttackDeclared);
    }
    private void OnAttackDeclared(AttackDeclared e)
    {
        DamageContext damage = new DamageContext(
            amount: e.Amount,
            source: e.Source,
            target: e.Target
        );

        EventContext eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );
        combatManager.EventBus.Publish<DamageRequested>(new DamageRequested(
            context: eventContext,
            damage: damage
        ));

        eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );
        combatManager.EventBus.Publish<DamageResolved>(new DamageResolved(
            context: eventContext,
            damage.Source,
            damage.Target,
            damage.Amount
        ));
    }
}
public class DamageContext
{
    public DamageContext(int amount, Entity source, Entity target)
    {
        Amount = amount;
        Source = source;
        Target = target;
        Modifiers = new List<object>();
    }
    public int Amount { get; private set; }
    public Entity Source { get; private set; }
    public Entity Target { get; private set; }
    public List<object> Modifiers { get; private set; }
    public void Add(int value, object source)
    {
        Amount += value;
        Modifiers.Add(source);
    }
    public void Subtract(int value, object source)
    {
        Amount -= value;
        Modifiers.Add(source);
    }
    public void Multiply(float value, object source)
    {
        Amount = (int)(Amount * value);
        Modifiers.Add(source);
    }
}
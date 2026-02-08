using System.Collections.Generic;
using UnityEngine;

public class DamageSystem
{
    private readonly EventBus eventBus;
    public DamageSystem(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }
    public void OnAttackDeclared(AttackDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        DamageContext damage = new DamageContext(
            amount: e.Amount,
            source: e.Source,
            target: e.Target
        );

        EventContext eventContext = CreateContext(e.Context);
        eventBus.Publish<DamageRequested>(new DamageRequested(
            context: eventContext,
            damage: damage
        ));

        eventContext = CreateContext(e.Context);
        eventBus.Publish<DamageResolved>(new DamageResolved(
            context: eventContext,
            source: damage.Source,
            target: damage.Target,
            amount: Mathf.Max(0, damage.Amount)
        ));
    }
    private EventContext CreateContext(EventContext context)
    {
        return new EventContext(
            source: this,
            action: context.Action,
            turn: context.Turn,
            combat: context.Combat
        );
    }
}
public class DamageContext
{
    public DamageContext(int amount, EntityInstance source, EntityInstance target)
    {
        Amount = amount;
        Source = source;
        Target = target;
        Modifiers = new List<object>();
    }
    public int Amount { get; private set; }
    public EntityInstance Source { get; private set; }
    public EntityInstance Target { get; private set; }
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
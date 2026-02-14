using System.Collections.Generic;
using UnityEngine;

public class BuffSystem : BaseSystem
{
    private readonly EventBus eventBus;
    public BuffSystem(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }
    public void OnBuffDeclared(BuffDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        BuffContext buff = new BuffContext(
            type: e.Type,
            amount: e.Amount,
            source: e.Source,
            target: e.Target
        );

        EventContext eventContext = CreateContext(e.Context);
        eventBus.Publish<BuffRequested>(new BuffRequested(
            context: eventContext,
            buff: buff
        ));

        eventContext = CreateContext(e.Context);
        eventBus.Publish<BuffResolved>(new BuffResolved(
            context: eventContext,
            source: buff.Source,
            target: buff.Target,
            type: buff.Type,
            amount: Mathf.Max(0, buff.Amount)
        ));
    }
}
public class BuffContext
{
    public BuffContext(BuffType type, int amount, EntityInstance source, EntityInstance target)
    {
        Type = type;
        Amount = amount;
        Source = source;
        Target = target;
        Modifiers = new List<object>();
    }
    public BuffType Type { get; private set; }
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
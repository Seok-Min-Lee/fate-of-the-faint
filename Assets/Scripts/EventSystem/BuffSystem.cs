using System.Collections.Generic;
using System.Linq;
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

        eventBus.Publish<BuffRequested>(new BuffRequested(
            context: e.Context.RewriteNew(this),
            buff: buff
        ));

        eventBus.Publish<BuffResolved>(new BuffResolved(
            context: e.Context.RewriteNew(this),
            motion: e.Motion,
            source: buff.Source,
            target: buff.Target,
            type: buff.Type,
            amount: buff.Calculate()
        ));
    }
}
public class BuffContext
{
    public BuffContext(BuffType type, int amount, object source, object target)
    {
        Type = type;
        Amount = amount;
        Source = source;
        Target = target;
        Modifications = new List<BuffModification>();
    }
    public BuffType Type { get; private set; }
    public int Amount { get; private set; }
    public object Source { get; private set; }
    public object Target { get; private set; }
    private List<BuffModification> Modifications;
    public void Add(int value, object source)
    {
        Modifications.Add(new BuffModification(
            type: BuffModificationType.Add,
            value: value,
            source: source
        ));
    }
    public void Subtract(int value, object source)
    {
        Modifications.Add(new BuffModification(
            type: BuffModificationType.Subtract,
            value: value,
            source: source
        ));
    }
    public void Multiply(float value, object source)
    {
        Modifications.Add(new BuffModification(
            type: BuffModificationType.Multiply,
            value: value,
            source: source
        ));
    }
    public int Calculate()
    {
        int sum = Amount;

        List<BuffModification> ordered = Modifications.OrderBy(m => m.Type).ToList();
        foreach (BuffModification dm in ordered)
        {
            switch (dm.Type)
            {
                case BuffModificationType.Add:
                    sum += (int)dm.Value;
                    break;
                case BuffModificationType.Subtract:
                    sum -= (int)dm.Value;
                    break;
                case BuffModificationType.Multiply:
                    sum = (int)(sum * dm.Value);
                    break;
            }
        }

        return sum;
    }
}
public struct BuffModification
{
    public BuffModification(BuffModificationType type, float value, object source)
    {
        Type = type;
        Value = value;
        Source = source;
    }
    public BuffModificationType Type;
    public float Value;
    public object Source;
}
public enum BuffModificationType
{
    Add = 1,
    Subtract = 3,
    Multiply = 2,
}
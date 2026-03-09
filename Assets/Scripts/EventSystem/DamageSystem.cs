using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DamageSystem : BaseSystem
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

        eventBus.Publish<DamageRequested>(new DamageRequested(
            context: e.Context.RewriteNew(this),
            motion: e.Motion,
            damage: damage
        ));

        int sum = damage.Calculate();

        eventBus.Publish<DamageResolved>(new DamageResolved(
            context: e.Context.RewriteNew(this),
            motion: e.Motion,
            source: damage.Source,
            target: damage.Target,
            amount: Mathf.Max(0, sum),
            repeat: e.Repeat
        ));
    }
}
public class DamageContext
{
    public DamageContext(int amount, object source, object target)
    {
        Amount = amount;
        Source = source;
        Target = target;
        Modifications = new List<DamageModification>();
    }
    private int Amount;
    public object Source { get; private set; }
    public object Target { get; private set; }

    private List<DamageModification> Modifications;
    public void Add(int value, object source)
    {
        Modifications.Add(new DamageModification(
            type: DamageModificationType.Add, 
            value: value, 
            source: source
        ));
    }
    public void Subtract(int value, object source)
    {
        Modifications.Add(new DamageModification(
            type: DamageModificationType.Subtract,
            value: value,
            source: source
        ));
    }
    public void Multiply(float value, object source)
    {
        Modifications.Add(new DamageModification(
            type: DamageModificationType.Multiply,
            value: value,
            source: source
        ));
    }
    public int Calculate()
    {
        int sum = Amount;

        List<DamageModification> ordered = Modifications.OrderBy(m => m.Type).ToList();
        foreach (DamageModification dm in ordered)
        {
            switch (dm.Type)
            {
                case DamageModificationType.Add:
                    sum += (int)dm.Value;
                    break;
                case DamageModificationType.Subtract:
                    sum -= (int)dm.Value;
                    break;
                case DamageModificationType.Multiply:
                    sum = (int)(sum * dm.Value);
                    break;
            }
        }

        return sum;
    }
}
public struct DamageModification
{
    public DamageModification(DamageModificationType type, float value, object source)
    {
        Type = type;
        Value = value;
        Source = source;
    }
    public DamageModificationType Type;
    public float Value;
    public object Source;
}
public enum DamageModificationType
{
    Add = 1,
    Subtract = 3,
    Multiply = 2,
}
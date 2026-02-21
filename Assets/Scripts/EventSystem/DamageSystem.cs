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

        EventContext eventContext = base.CreateContext(e.Context);
        eventBus.Publish<DamageRequested>(new DamageRequested(
            context: eventContext,
            damage: damage
        ));

        int sum = damage.Calculate();

        eventContext = base.CreateContext(e.Context);
        eventBus.Publish<DamageResolved>(new DamageResolved(
            context: eventContext,
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
    public DamageContext(int amount, EntityInstance source, EntityInstance target)
    {
        Amount = amount;
        Source = source;
        Target = target;
        Modifications = new List<DamageModification>();
    }
    private int Amount;
    public EntityInstance Source { get; private set; }
    public EntityInstance Target { get; private set; }

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
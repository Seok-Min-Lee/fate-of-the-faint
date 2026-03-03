using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnergySystem : BaseSystem
{
    private readonly EventBus eventBus;
    public int MaxEnergy;
    public int Energy;
    public EnergySystem(EventBus eventBus, int max)
    {
        this.eventBus = eventBus;

        MaxEnergy = max;
        Energy = MaxEnergy;
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EnergyContext energyContext = new EnergyContext(amount: MaxEnergy - Energy, source: this);

        eventBus.Publish<EnergyChargeRequested>(new EnergyChargeRequested(
            context: e.Context.RewriteNew(this),
            motion: e.Motion,
            energy: energyContext
        ));

        int sum = energyContext.Calculate();

        EnergyChanged(
            amount: sum,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnPlayerTurnEnded(PlayerTurnEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        Energy = 0;
    }
    public void OnEnergyChangeRequested(EnergyChangeRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Amount > 0 || Mathf.Abs(e.Amount) > Energy)
        {
            return;
        }

        e.Request.isResult = true;

        EnergyChanged(
            amount: e.Amount,
            context: e.Context,
            motion: e.Motion
        );

        eventBus.Publish(new EnergyResolved(
            context: e.Context.RewriteNew(this), 
            motion: e.Motion
        ));
    }
    public void OnGainEnergyDeclared(GainEnergyDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EnergyChanged(
            amount: e.Amount,
            context: e.Context,
            motion: e.Motion
        );
    }
    private void EnergyChanged(int amount, EventContext context, MotionContext motion)
    {
        int startAmount = Energy;
        Energy += amount;

        eventBus.Publish<EnergyChanged>(new EnergyChanged(
            context: context.RewriteNew(this),
            motion: motion,
            startAmount: startAmount, 
            endAmount: Energy,
            maxAmount: MaxEnergy
        ));
    }
}
public class EnergyContext
{
    public EnergyContext(int amount, object source)
    {
        Amount = amount;
        Source = source;
        Modifications = new List<EnergyModification>();
    }
    public object Source { get; private set; }
    private int Amount;
    private List<EnergyModification> Modifications;
    public void Add(int value, object source)
    {
        Modifications.Add(new EnergyModification(
            type: EnergyModificationType.Add,
            value: value,
            source: source
        ));
    }
    public void Subtract(int value, object source)
    {
        Modifications.Add(new EnergyModification(
            type: EnergyModificationType.Subtract,
            value: value,
            source: source
        ));
    }
    public void Multiply(float value, object source)
    {
        Modifications.Add(new EnergyModification(
            type: EnergyModificationType.Multiply,
            value: value,
            source: source
        ));
    }
    public int Calculate()
    {
        int sum = Amount;

        List<EnergyModification> ordered = Modifications.OrderBy(m => m.Type).ToList();
        foreach (EnergyModification dm in ordered)
        {
            switch (dm.Type)
            {
                case EnergyModificationType.Add:
                    sum += (int)dm.Value;
                    break;
                case EnergyModificationType.Subtract:
                    sum -= (int)dm.Value;
                    break;
                case EnergyModificationType.Multiply:
                    sum = (int)(sum * dm.Value);
                    break;
            }
        }

        return sum;
    }
}
public struct EnergyModification
{
    public EnergyModification(EnergyModificationType type, float value, object source)
    {
        Type = type;
        Value = value;
        Source = source;
    }
    public EnergyModificationType Type;
    public float Value;
    public object Source;
}
public enum EnergyModificationType
{
    Add = 1,
    Subtract = 3,
    Multiply = 2,
}

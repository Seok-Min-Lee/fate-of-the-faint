using UnityEngine;

public class EnergySystem
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

        EnergyChanged(MaxEnergy - Energy, e.Context);
    }
    public void OnEnergyChangeRequested(EnergyChangeRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        bool result = false;

        if (e.Amount == 0) // 비용 0
        {
            result = true;
        }
        else if (e.Amount > 0) // 충전
        {
            result = true;
            EnergyChanged(e.Amount, e.Context);
        }
        else // 사용
        {
            if (Energy >= Mathf.Abs(e.Amount))
            {
                result = true;
                EnergyChanged(e.Amount, e.Context);
            }
        }

        e.Request.isResult = result;

        if (!result)
        {
            return;
        }

        EventContext eventContext = CreateContext(e.Context);
        eventBus.Publish(new EnergyResolved(context: eventContext, result: result));
    }
    private void EnergyChanged(int amount, EventContext context)
    {
        int startAmount = Energy;
        Energy += amount;

        EventContext eventContext = CreateContext(context);

        eventBus.Publish<EnergyChanged>(new EnergyChanged(
            context: eventContext,
            startAmount: startAmount, 
            endAmount: Energy
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

using UnityEngine;

public class EnergySystem
{
    private readonly EventBus eventBus;
    public int MaxEnergy;
    public int Energy;
    public EnergySystem(EventBus eventBus)
    {
        this.eventBus = eventBus;

        MaxEnergy = 3;
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

        EventContext eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );
        eventBus.Publish(new EnergyResolved(context: e.Context, result: result));
    }
    private void EnergyChanged(int amount, EventContext context)
    {
        int startAmount = Energy;
        Energy += amount;

        EventContext eventContext = new EventContext(
            source: this,
            action: context.Action,
            turn: context.Turn,
            combat: context.Combat
        );

        eventBus.Publish<EnergyChanged>(new EnergyChanged(
            context: eventContext,
            startAmount: startAmount, 
            endAmount: Energy
        ));
    }
}

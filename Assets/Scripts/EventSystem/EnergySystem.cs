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

        EnergyChanged(
            amount: MaxEnergy - Energy, 
            context: e.Context,
            motion: e.Motion
        );
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
            EnergyChanged(
                amount: e.Amount, 
                context: e.Context,
                motion: e.Motion
            );
        }
        else // 사용
        {
            if (Energy >= Mathf.Abs(e.Amount))
            {
                result = true;
                EnergyChanged(
                    amount: e.Amount,
                    context: e.Context,
                    motion: e.Motion
                );
            }
        }

        e.Request.isResult = result;

        if (!result)
        {
            return;
        }

        eventBus.Publish(new EnergyResolved(
            context: CreateContext(e.Context), 
            motion: e.Motion,
            result: result
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
            context: CreateContext(context),
            motion: motion,
            startAmount: startAmount, 
            endAmount: Energy,
            maxAmount: MaxEnergy
        ));
    }
}

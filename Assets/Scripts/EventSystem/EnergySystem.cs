using UnityEngine;

public class EnergySystem : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;
    public int MaxEnergy;
    public int Energy;
    private void OnEnable()
    {
        combatManager.EventBus.Subscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        combatManager.EventBus.Subscribe<EnergyChangeRequested>(OnEnergyChangeRequested);
    }
    private void OnDisable()
    {
        combatManager.EventBus.Unsubscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        combatManager.EventBus.Unsubscribe<EnergyChangeRequested>(OnEnergyChangeRequested);
    }
    private void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        EnergyChanged(MaxEnergy - Energy, e.Context);
    }
    private void OnEnergyChangeRequested(EnergyChangeRequested e)
    {
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
        combatManager.EventBus.Publish(new EnergyResolved(context: e.Context, result: result));
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

        combatManager.EventBus.Publish<EnergyChanged>(new EnergyChanged(
            context: eventContext,
            startAmount: startAmount, 
            endAmount: Energy
        ));
    }
}

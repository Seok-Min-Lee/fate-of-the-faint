using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Turn Counting Gain Energy Relic ", menuName = "Scriptable Objects/Relic/Turn Counting Gain Energy Relic ")]
public class TurnCountingGainEnergyRelicSO : RelicSO
{
    [SerializeField] private int timing;
    [SerializeField] private int amount;
    public int Timing => timing;
    public int Amount => amount;
    public override RelicInstance CreateInstance(EventBus eventBus)
    {
        return new TurnCountingGainEnergyRelicInstance(eventBus: eventBus, origin: this);
    }
}
public class TurnCountingGainEnergyRelicInstance : RelicInstance, IPlayerTurnStarted, IEnergyChargeRequested
{
    private int timing = 0;
    private int amount = 0;
    private int count = 0;

    public TurnCountingGainEnergyRelicInstance(EventBus eventBus, TurnCountingGainEnergyRelicSO origin) : base(eventBus, origin)
    {
        timing = origin.Timing;
        amount = origin.Amount;
    }
    public override void Register()
    {
        EventBus.Subscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        EventBus.Subscribe<EnergyChargeRequested>(OnEnergyChargeRequested);
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        count++;
    }

    public void OnEnergyChargeRequested(EnergyChargeRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (count == timing)
        {
            Activate(e.Context, e.Motion, () =>
            {
                EventBus.Publish<GainEnergyDeclared>(new GainEnergyDeclared(
                    context: e.Context,
                    motion: e.Motion,
                    amount: amount
                ));

                count = 0;
            });
        }
    }
}
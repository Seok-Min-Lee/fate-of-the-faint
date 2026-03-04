using UnityEngine;

[CreateAssetMenu(fileName = "No Attack Card Played Last Turn Gain Energy Relic ", menuName = "Scriptable Objects/Relic/No Attack Card Played Last Turn Gain Energy Relic")] 
public class NoAttackCardPlayedLastTurnGainEnergyRelicSO : RelicSO
{
    [SerializeField] private int amount;
    public int Amount => amount;
    public override RelicInstance CreateInstance(EventBus eventBus)
    {
        return new NoAttackCardPlayedLastTurnGainEnergyRelicInstance(eventBus: eventBus, origin: this);
    }
}
public class NoAttackCardPlayedLastTurnGainEnergyRelicInstance : RelicInstance, IEnergyChargeRequested, ICardPlayDeclared
{
    private int amount;
    private bool usedCard = true;
    public NoAttackCardPlayedLastTurnGainEnergyRelicInstance(EventBus eventBus, NoAttackCardPlayedLastTurnGainEnergyRelicSO origin) : base(eventBus, origin)
    {
        amount = origin.Amount;
        usedCard = true;
    }
    public override void Register()
    {
        EventBus.Subscribe<EnergyChargeRequested>(OnEnergyChargeRequested);
        EventBus.Subscribe<CardPlayDeclared>(OnCardPlayDeclared);
    }
    public void OnEnergyChargeRequested(EnergyChargeRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (!usedCard)
        {
            Activate(e.Context, e.Motion, () =>
            {
                e.Energy.Add(amount, this);
            });
        }

        usedCard = false;
    }

    public void OnCardPlayDeclared(CardPlayDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.CardView.CardInstance.Origin is not AttackCardSO)
        {
            return;
        }

        usedCard = true;
    }

}

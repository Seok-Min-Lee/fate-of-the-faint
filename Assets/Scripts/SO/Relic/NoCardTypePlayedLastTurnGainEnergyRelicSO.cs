using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "No Card Type Played Last Turn Gain Energy Relic ", menuName = "Scriptable Objects/Relic/No Card Type Played Last Turn Gain Energy Relic")] 
public class NoCardTypePlayedLastTurnGainEnergyRelicSO : RelicSO
{
    [SerializeField] private CardType cardType;
    [SerializeField] private int amount;
    public CardType CardType => cardType;
    public int Amount => amount;
    public override RelicInstance CreateInstance(EventBus eventBus)
    {
        return new NoCardTypePlayedLastTurnGainEnergyRelicInstance(eventBus: eventBus, origin: this);
    }
}
public class NoCardTypePlayedLastTurnGainEnergyRelicInstance : RelicInstance, IEnergyChargeRequested, ICardPlayDeclared
{
    private CardType cardType;
    private int amount;
    private bool usedCard = true;
    public NoCardTypePlayedLastTurnGainEnergyRelicInstance(EventBus eventBus, NoCardTypePlayedLastTurnGainEnergyRelicSO origin) : base(eventBus, origin)
    {
        cardType = origin.CardType;
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

        if (e.CardView.CardInstance.Origin.Type != cardType)
        {
            return;
        }

        usedCard = true;
    }

}

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "No Card Type Played Last Turn Gain Energy Relic ", menuName = "Scriptable Objects/Relic/No Card Type Played Last Turn Gain Energy Relic")] 
public class NoCardTypePlayedLastTurnGainEnergyRelicSO : RelicSO
{
    [SerializeField] private CardView.ViewType cardType;
    [SerializeField] private int amount;
    public CardView.ViewType CardType => cardType;
    public int Amount => amount;
    public override RelicInstance CreateInstance(EventBus eventBus)
    {
        return new NoCardTypePlayedLastTurnGainEnergyRelicInstance(eventBus: eventBus, origin: this);
    }
}
public class NoCardTypePlayedLastTurnGainEnergyRelicInstance : RelicInstance, IPlayerTurnStarted, ICardPlayDeclared
{
    private CardView.ViewType cardType;
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
        EventBus.Subscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        EventBus.Subscribe<CardPlayDeclared>(OnCardPlayDeclared);
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (!usedCard)
        {
            Activate(e.Context, e.Motion, () =>
            {
                EventBus.Publish<GainEnergyDeclared>(new GainEnergyDeclared(
                    context: e.Context,
                    motion: e.Motion,
                    amount: amount
                ));

                usedCard = false;
            });
        }
    }

    public void OnCardPlayDeclared(CardPlayDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.CardView.Type != cardType)
        {
            return;
        }

        usedCard = true;
    }

}

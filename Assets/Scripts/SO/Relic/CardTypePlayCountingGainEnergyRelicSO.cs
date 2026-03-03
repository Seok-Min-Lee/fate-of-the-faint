using UnityEngine;

[CreateAssetMenu(fileName = "Card Type Play Counting Gain Energy Relic ", menuName = "Scriptable Objects/Relic/Card Type Play Counting Gain Energy Relic")]
public class CardTypePlayCountingGainEnergyRelicSO : RelicSO
{
    [SerializeField] private CardType cardType;
    [SerializeField] private int timing;
    [SerializeField] private int amount;
    public CardType CardType => cardType;
    public int Timing => timing;
    public int Amount => amount;
    public override RelicInstance CreateInstance(EventBus eventBus)
    {
        return new CardPlayCountTriggerRelicInstance(eventBus: eventBus, origin: this);
    }
}
public class CardPlayCountTriggerRelicInstance : RelicInstance, ICardPlayDeclared
{
    private CardType cardType;
    private int timing = 0;
    private int amount = 0;
    private int count = 0;

    public CardPlayCountTriggerRelicInstance(EventBus eventBus, CardTypePlayCountingGainEnergyRelicSO origin) : base(eventBus, origin)
    {
        cardType = origin.CardType;
        timing = origin.Timing;
        amount = origin.Amount;
    }
    public override void Register()
    {
        EventBus.Subscribe<CardPlayDeclared>(OnCardPlayDeclared);
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

        if (++count == timing)
        {
            Activate(e.Context, e.Motion, () =>
            {
                EventBus.Publish<EnergyChangeRequested>(new EnergyChangeRequested(
                    context: e.Context.RewriteNew(this),
                    request: new RequestContext(this),
                    motion: e.Motion,
                    amount: amount
                ));

                count = 0;
            });
        }
    }
}
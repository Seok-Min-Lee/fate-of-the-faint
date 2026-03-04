using UnityEngine;

[CreateAssetMenu(fileName = "Attack Card Play Counting Gain Energy Relic ", menuName = "Scriptable Objects/Relic/Attack Card Play Counting Gain Energy Relic")]
public class AttackCardPlayCountingGainEnergyRelicSO : RelicSO
{
    [SerializeField] private int timing;
    [SerializeField] private int amount;
    public int Timing => timing;
    public int Amount => amount;
    public override RelicInstance CreateInstance(EventBus eventBus)
    {
        return new AttackCardPlayCountTriggerRelicInstance(eventBus: eventBus, origin: this);
    }
}
public class AttackCardPlayCountTriggerRelicInstance : RelicInstance, ICardPlayDeclared
{
    private int timing = 0;
    private int amount = 0;
    private int count = 0;

    public AttackCardPlayCountTriggerRelicInstance(EventBus eventBus, AttackCardPlayCountingGainEnergyRelicSO origin) : base(eventBus, origin)
    {
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

        if (e.CardView.CardInstance.Origin is not AttackCardSO)
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
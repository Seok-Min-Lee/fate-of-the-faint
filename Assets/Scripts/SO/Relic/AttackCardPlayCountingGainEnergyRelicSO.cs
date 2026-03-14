using UnityEngine;

[CreateAssetMenu(fileName = "Attack Card Play Counting Gain Energy Relic ", menuName = "Scriptable Objects/Relic/Attack Card Play Counting Gain Energy Relic")]
public class AttackCardPlayCountingGainEnergyRelicSO : RelicSO
{
    [SerializeField] private int timing;
    [SerializeField] private int amount;
    public int Timing => timing;
    public int Amount => amount;
    public override RelicInstance CreateInstance()
    {
        return new AttackCardPlayCountTriggerRelicInstance(this);
    }
}
public class AttackCardPlayCountTriggerRelicInstance : RelicInstance, ICardPlayDeclared
{
    private int timing = 0;
    private int amount = 0;
    private int count = 0;

    public AttackCardPlayCountTriggerRelicInstance(AttackCardPlayCountingGainEnergyRelicSO origin) : base(origin)
    {
        timing = origin.Timing;
        amount = origin.Amount;
    }
    public override void Register(EventBus eventBus)
    {
        EventBus = eventBus;
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
                EventBus.Publish<GainEnergyDeclared>(new GainEnergyDeclared(
                    context: e.Context.RewriteNew(this),
                    motion: e.Motion,
                    amount: amount
                ));

                count = 0;
            });
        }
    }
}
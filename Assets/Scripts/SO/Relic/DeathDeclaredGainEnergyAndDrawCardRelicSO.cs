using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "Death Declared Gain Energy And Draw Card Relic ", menuName = "Scriptable Objects/Relic/Death Declared Gain Energy And Draw Card Relic ")]
public class DeathDeclaredGainEnergyAndDrawCardRelicSO : RelicSO
{
    [SerializeField] private int energyAmount;
    [SerializeField] private int cardAmount;
    public int EnergyAmount => energyAmount;
    public int CardAmount => cardAmount;
    public override RelicInstance CreateInstance(EventBus eventBus)
    {
        return new DeathDeclaredGainEnergyAndDrawCardRelicInstance(eventBus: eventBus, origin: this);
    }
}
public class DeathDeclaredGainEnergyAndDrawCardRelicInstance : RelicInstance, IDeathDeclared
{
    private int energyAmount = 0;
    private int cardAmount = 0;

    public DeathDeclaredGainEnergyAndDrawCardRelicInstance(EventBus eventBus, DeathDeclaredGainEnergyAndDrawCardRelicSO origin) : base(eventBus, origin)
    {
        energyAmount = origin.EnergyAmount;
        cardAmount = origin.CardAmount;
    }
    public override void Register()
    {
        EventBus.Subscribe<DeathDeclared>(OnDeathDeclared);
    }
    public void OnDeathDeclared(DeathDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }
        if (e.Source == e.Context.Combat.Player)
        {
            return;
        }

        Activate(e.Context, e.Motion, () =>
        {
            EventBus.Publish<DrawCardDeclared>(new DrawCardDeclared(
                context: e.Context.RewriteNew(this),
                motion: e.Motion,
                amount: cardAmount
            ));

            EventBus.Publish<EnergyChangeRequested>(new EnergyChangeRequested(
                context: e.Context.RewriteNew(this),
                request: new RequestContext(this),
                motion: e.Motion,
                amount: energyAmount
            ));
        });
    }
}
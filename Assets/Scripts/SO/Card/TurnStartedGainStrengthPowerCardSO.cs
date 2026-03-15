using UnityEngine;

[CreateAssetMenu(fileName = "pwr_turnStartedGainStrength_", menuName = "Scriptable Objects/Card/Turn Started Gain Strength Power Card")]
public class TurnStartedGainStrengthPowerCardSO : PowerCardSO
{
    [SerializeField] private int amount;
    public int Amount => amount;
    public override PowerInstance CreateInstance(EventBus eventBus)
    {
        return new TurnStartedGainStrengthPowerInstance(eventBus, this);
    }
    protected override string GetDescription()
    {
        return string.Format(description, amount)
                     .Replace("[", "<color=#00FF40>")
                     .Replace("]", "</color>"); ;
    }
}
public class TurnStartedGainStrengthPowerInstance : PowerInstance, IPlayerTurnStarted
{
    private int amount;
    public TurnStartedGainStrengthPowerInstance(EventBus eventBus, TurnStartedGainStrengthPowerCardSO origin) : base(eventBus, origin)
    {
        amount = origin.Amount;
    }
    public override void Register()
    {
        EventBus.Subscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
    }
    public override void Unregister()
    {
        EventBus.Unsubscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        Activate(e.Context, e.Motion, () =>
        {
            EventBus.Publish<BuffDeclared>(new BuffDeclared(
                context: e.Context.RewriteNew(this),
                motion: e.Motion,
                source: this,
                target: e.Context.Combat.Player,
                type: BuffType.Strength,
                amount: amount
            ));
        });
    }
}

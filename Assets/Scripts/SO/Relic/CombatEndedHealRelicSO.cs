using UnityEngine;

[CreateAssetMenu(fileName = "Combat Ended Heal Relic ", menuName = "Scriptable Objects/Relic/Combat Ended Heal Relic ")]
public class CombatEndedHealRelicSO : RelicSO
{
    [SerializeField] private int amount;
    public int Amount => amount;
    public override RelicInstance CreateInstance()
    {
        return new CombatEndedHealRelicInstance(this);
    }
}
public class CombatEndedHealRelicInstance : RelicInstance, ICombatEnded
{
    private int amount;
    public CombatEndedHealRelicInstance(CombatEndedHealRelicSO origin) : base(origin)
    {
        amount = origin.Amount;
    }
    public override void Register(EventBus eventBus)
    {
        EventBus = eventBus;
        EventBus.Subscribe<CombatEnded>(OnCombatEnded);
    }
    public void OnCombatEnded(CombatEnded e)
    {
        if (e.Context.Combat.state != CombatState.Victory)
        {
            return;
        }

        Activate(e.Context, e.Motion, () =>
        {
            if (e.Context.Combat.Player is not PlayerInstance player)
            {
                return;
            }

            player.ChangeHp(
                eventBus: EventBus,
                context: e.Context,
                motion: e.Motion,
                amount: amount
            );
        });
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "First Attack Card Played Bonus Damage Relic ", menuName = "Scriptable Objects/Relic/First Attack Card Played Bonus Damage Relic ")]
public class FirstAttackCardPlayedBonusDamageRelicSO : RelicSO
{
    [SerializeField] private int amount;
    public int Amount => amount;

    public override RelicInstance CreateInstance()
    {
        return new FirstAtackCardPlayedBonusDamageRelicInstance(this);
    }
}
public class FirstAtackCardPlayedBonusDamageRelicInstance : RelicInstance, ICombatStarted, ICardPlayDeclared, IDamageRequested
{
    private int amount;
    private bool usedThisCombat = false;
    private bool isPrepared = false;
    public FirstAtackCardPlayedBonusDamageRelicInstance(FirstAttackCardPlayedBonusDamageRelicSO origin) : base(origin)
    {
        amount = origin.Amount;
    }

    public override void Register(EventBus eventBus)
    {
        EventBus = eventBus;
        EventBus.Subscribe<CombatStarted>(OnCombatStarted);
        EventBus.Subscribe<CardPlayDeclared>(OnCardPlayDeclared);
        EventBus.Subscribe<DamageRequested>(OnDamageRequested);
    }
    public void OnCombatStarted(CombatStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        usedThisCombat = false;
        isPrepared = false;
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
        if (usedThisCombat || isPrepared)
        {
            return;
        }

        Activate(e.Context, e.Motion, () =>
        {
            isPrepared = true;
        });
    }

    public void OnDamageRequested(DamageRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }
        if (e.Damage.Source != e.Context.Combat.Player)
        {
            return;
        }
        if (usedThisCombat || !isPrepared)
        {
            return;
        }

        e.Damage.Add(amount, this);
        usedThisCombat = true;
    }

}

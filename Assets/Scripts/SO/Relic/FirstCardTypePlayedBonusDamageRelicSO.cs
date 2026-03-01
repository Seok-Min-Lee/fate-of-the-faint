using UnityEngine;

[CreateAssetMenu(fileName = "First Card Type Played Bonus Damage Relic ", menuName = "Scriptable Objects/Relic/First Card Type Played Bonus Damage Relic ")]
public class FirstCardTypePlayedBonusDamageRelicSO : RelicSO
{
    [SerializeField] private CardView.ViewType cardType;
    [SerializeField] private int amount;
    public CardView.ViewType CardType => cardType;
    public int Amount => amount;

    public override RelicInstance CreateInstance(EventBus eventBus)
    {
        return new FirstCardTypePlayedBonusDamageRelicInstance(eventBus: eventBus, origin: this);
    }
}
public class FirstCardTypePlayedBonusDamageRelicInstance : RelicInstance, ICombatStarted, ICardPlayDeclared, IDamageRequested
{
    private CardView.ViewType cardType;
    private int amount;
    private bool usedThisCombat = false;
    private bool isPrepared = false;
    public FirstCardTypePlayedBonusDamageRelicInstance(EventBus eventBus, FirstCardTypePlayedBonusDamageRelicSO origin) : base(eventBus, origin)
    {
        cardType = origin.CardType;
        amount = origin.Amount;
    }

    public override void Register()
    {
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
        if (e.CardView.Type != cardType)
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

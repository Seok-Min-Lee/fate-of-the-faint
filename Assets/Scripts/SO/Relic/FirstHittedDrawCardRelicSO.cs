using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "First Hitted Draw Card Relic ", menuName = "Scriptable Objects/Relic/First Hitted Draw Card Relic")]
public class FirstHittedDrawCardRelicSO : RelicSO
{
    [SerializeField] private int value;
    public int Value => value;
    public override RelicInstance CreateInstance(EventBus eventBus)
    {
        return new FirstHittedDrawCardRelicInstance(eventBus: eventBus, origin: this);
    }
}
public class FirstHittedDrawCardRelicInstance : RelicInstance, ICombatStarted, IHpChanged
{
    public bool usedThisCombat { get; private set; } = false;
    public int value;
    public FirstHittedDrawCardRelicInstance(EventBus eventBus, FirstHittedDrawCardRelicSO origin) : base(eventBus, origin)
    {
        value = origin.Value;
    }
    public override void Register()
    {
        EventBus.Subscribe<CombatStarted>(OnCombatStarted);
        EventBus.Subscribe<HpChanged>(OnHpChanged);
    }
    public void OnCombatStarted(CombatStarted e)
    {
        usedThisCombat = false;
    }
    public void OnHpChanged(HpChanged e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }
        if (e.Target != e.Context.Combat.Player)
        {
            return;
        }
        if (e.StartAmount <= e.EndAmount)
        {
            return;
        }
        if (usedThisCombat)
        {
            return;
        }

        Activate(e.Context, e.Motion, () =>
        {
            EventBus.Publish<DrawCardDeclared>(new DrawCardDeclared(
                context: e.Context.OverwriteNew(this),
                motion: e.Motion,
                amount: value
            ));

            usedThisCombat = true;
        });
    }
}
public static class EventMetas
{
    public static readonly EventMeta CombatStarted = new EventMeta("CombatStarted", EventCategory.Combat);
    public static readonly EventMeta CombatEnded = new EventMeta("CombatEnded", EventCategory.Combat);
    public static readonly EventMeta PlayerTurnStartRequested = new EventMeta("PlayerTurnStartRequested", EventCategory.Turn);
    public static readonly EventMeta PlayerTurnStarted = new EventMeta("PlayerTurnStarted", EventCategory.Turn);
    public static readonly EventMeta PlayerTurnEndRequested = new EventMeta("PlayerTurnEndRequested", EventCategory.Turn);
    public static readonly EventMeta PlayerTurnEnded = new EventMeta("PlayerTurnEnded", EventCategory.Turn);
    public static readonly EventMeta EnemyTurnStarted = new EventMeta("EnemyTurnStarted", EventCategory.Turn);
    public static readonly EventMeta EnemyTurnEnded = new EventMeta("EnemyTurnEnded", EventCategory.Turn);
    public static readonly EventMeta EnemyActionStartRequested = new EventMeta("EnemyActionStartRequested", EventCategory.Action);
    public static readonly EventMeta ActionStarted = new EventMeta("ActionStarted", EventCategory.Action);
    public static readonly EventMeta ActionEnded = new EventMeta("ActionEnded", EventCategory.Action);
    public static readonly EventMeta EnergyChangeRequested = new EventMeta("EnergyChangeRequested", EventCategory.Energy);
    public static readonly EventMeta EnergyChargeRequested = new EventMeta("EnergyChargeRequested", EventCategory.Energy);
    public static readonly EventMeta EnergyResolved = new EventMeta("EnergyResolved", EventCategory.Energy);
    public static readonly EventMeta EnergyChanged = new EventMeta("EnergyChanged", EventCategory.Energy);
    public static readonly EventMeta CardDrawed = new EventMeta("CardDrawed", EventCategory.Card);
    public static readonly EventMeta CardDiscarded = new EventMeta("CardDiscarded", EventCategory.Card);
    public static readonly EventMeta CardExhausted = new EventMeta("CardExhausted", EventCategory.Card);
    public static readonly EventMeta CardCharged = new EventMeta("CardCharged", EventCategory.Card);
    public static readonly EventMeta CardPlayDeclared = new EventMeta("CardPlayDeclared", EventCategory.Card);
    public static readonly EventMeta AttackPlayed = new EventMeta("AttackPlayed", EventCategory.Card);
    public static readonly EventMeta SkillPlayed = new EventMeta("SkillPlayed", EventCategory.Card);
    public static readonly EventMeta PowerPlayed = new EventMeta("PowerPlayed", EventCategory.Card);
    public static readonly EventMeta AttackDeclared = new EventMeta("AttackDeclared", EventCategory.Card);
    public static readonly EventMeta BlockDeclared = new EventMeta("BlockDeclared", EventCategory.Card);
    public static readonly EventMeta DrawCardDeclared = new EventMeta("DrawCardDeclared", EventCategory.Card);
    public static readonly EventMeta GainEnergyDeclared = new EventMeta("GainEnergyDeclared", EventCategory.Card);
    public static readonly EventMeta ModifyCostDeclared = new EventMeta("ModifyCostDeclared", EventCategory.Card);
    public static readonly EventMeta BuffDeclared = new EventMeta("BuffDeclared", EventCategory.Status);
    public static readonly EventMeta DamageRequested = new EventMeta("DamageRequested", EventCategory.Damage);
    public static readonly EventMeta DamageResolved = new EventMeta("DamageResolved", EventCategory.Damage);
    public static readonly EventMeta BuffRequested = new EventMeta("BuffRequested", EventCategory.Status);
    public static readonly EventMeta BuffResolved = new EventMeta("BuffResolved", EventCategory.Status);
    public static readonly EventMeta HpChanged = new EventMeta("HpChanged", EventCategory.Status);
    public static readonly EventMeta BlockChanged = new EventMeta("BlockChanged", EventCategory.Status);
    public static readonly EventMeta BuffChanged = new EventMeta("BuffChanged", EventCategory.Status);
    public static readonly EventMeta DeathDeclared = new EventMeta("DeathDeclared", EventCategory.Status);
    public static readonly EventMeta AnimationStarted = new EventMeta("AnimationStarted", EventCategory.UI);
    public static readonly EventMeta AnimationEnded = new EventMeta("AnimationEnded", EventCategory.UI); 
    public static readonly EventMeta EnemyIntentDecided = new EventMeta("EnemyIntentDecided", EventCategory.Status);
    public static readonly EventMeta RelicActivated = new EventMeta("RelicActivated", EventCategory.Status);
    public static readonly EventMeta RelicAdded = new EventMeta("RelicAdded", EventCategory.Status);
    public static readonly EventMeta CardAdded = new EventMeta("CardAdded", EventCategory.Status);
    public static readonly EventMeta GoldChanged = new EventMeta("GoldChanged", EventCategory.Status);
}
public readonly struct EventMeta
{
    public readonly string Name;
    public readonly EventCategory Category;

    public EventMeta(string name, EventCategory category)
    {
        Name = name;
        Category = category;
    }
}
public enum EventCategory
{
    Combat,
    Turn,
    Action,
    Energy,
    Damage,
    Card,
    Status,
    UI,
}


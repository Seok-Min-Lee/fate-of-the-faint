public static class EventMetas
{
    public static readonly EventMeta CombatStarted = new("CombatStarted", EventCategory.Combat);
    public static readonly EventMeta CombatEndRequested = new("CombatEndRequested", EventCategory.Combat);
    public static readonly EventMeta CombatEnded = new("CombatEnded", EventCategory.Combat);
    public static readonly EventMeta PlayerTurnStartRequested = new("PlayerTurnStartRequested", EventCategory.Turn);
    public static readonly EventMeta PlayerTurnStarted = new("PlayerTurnStarted", EventCategory.Turn);
    public static readonly EventMeta PlayerTurnEndRequested = new("PlayerTurnEndRequested", EventCategory.Turn);
    public static readonly EventMeta PlayerTurnEnded = new("PlayerTurnEnded", EventCategory.Turn);
    public static readonly EventMeta EnemyTurnStartRequested = new("EnemyTurnStartRequested", EventCategory.Turn);
    public static readonly EventMeta EnemyTurnStarted = new("EnemyTurnStarted", EventCategory.Turn);
    public static readonly EventMeta EnemyTurnEndRequested = new("EnemyTurnEndRequested", EventCategory.Turn);
    public static readonly EventMeta EnemyTurnEnded = new("EnemyTurnEnded", EventCategory.Turn);
    public static readonly EventMeta EnemyActionStartRequested = new("EnemyActionStartRequested", EventCategory.Action);
    public static readonly EventMeta ActionStarted = new("ActionStarted", EventCategory.Action);
    public static readonly EventMeta ActionEnded = new("ActionEnded", EventCategory.Action);
    public static readonly EventMeta EnergyChangeRequested = new("EnergyChangeRequested", EventCategory.Energy);
    public static readonly EventMeta EnergyResolved = new("EnergyResolved", EventCategory.Energy);
    public static readonly EventMeta EnergyChanged = new("EnergyChanged", EventCategory.Energy);
    public static readonly EventMeta CardDrawed = new("CardDrawed", EventCategory.Card);
    public static readonly EventMeta CardDiscarded = new("CardDiscarded", EventCategory.Card);
    public static readonly EventMeta CardCharged = new("CardCharged", EventCategory.Card);
    public static readonly EventMeta CardPlayDeclared = new("CardPlayDeclared", EventCategory.Card);
    public static readonly EventMeta AttackDeclared = new("AttackDeclared", EventCategory.Card);
    public static readonly EventMeta ShieldDeclared = new("ShieldDeclared", EventCategory.Card);
    public static readonly EventMeta DrawCardDeclared = new("DrawCardDeclared", EventCategory.Card);
    public static readonly EventMeta GainEnergyDeclared = new("GainEnergyDeclared", EventCategory.Card);
    public static readonly EventMeta ModifyCostDeclared = new("ModifyCostDeclared", EventCategory.Card);
    public static readonly EventMeta StrengthenDeclared = new("StrengthenDeclared", EventCategory.Card);
    public static readonly EventMeta WeakenDeclared = new("WeakenDeclared", EventCategory.Card);
    public static readonly EventMeta VulnerableDeclared = new("VulnerableDeclared", EventCategory.Card);
    public static readonly EventMeta DamageRequested = new("DamageRequested", EventCategory.Damage);
    public static readonly EventMeta DamageResolved = new("DamageResolved", EventCategory.Damage);
    public static readonly EventMeta HpChanged = new("HpChanged", EventCategory.Status);
    public static readonly EventMeta DeathDeclared = new("DeathDeclared", EventCategory.Status);
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
    Status
}


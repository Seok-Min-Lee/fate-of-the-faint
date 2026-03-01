using UnityEngine;
public interface INormalEffect
{
    public RelicTriggerEvent TriggerEvent { get; }
    public RelicTarget Target { get; }
    public RelicEffect Effect { get; }
    public int Value { get; }
}
[CreateAssetMenu(fileName = "Relic_", menuName = "Scriptable Objects/Relic")]
public abstract class RelicSO : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private string description;
    [SerializeField] private string flaverText;
    [SerializeField] private RelicRarity rarity;
    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public string Description => description;
    public string FlaverText => flaverText;
    public RelicRarity Rarity => rarity;
    public abstract RelicInstance CreateInstance(EventBus eventBus);
}
public enum RelicRarity
{
    Start,
    Normal,
    Rare,
    Special,
    Boss,
    Shop,
    Event
}
public enum RelicTriggerEvent
{
    CombatStarted,
    CombatEnded,
    PlayerTurnStartRequested,
    PlayerTurnStarted,
    PlayerTurnEndRequested,
    PlayerTurnEnded,
    EnemyTurnStarted,
    EnemyTurnEnded,
    EnemyActionStartRequested,
    ActionStarted,
    ActionEnded,
    EnergyChangeRequested,
    EnergyResolved,
    EnergyChanged,
    CardDrawed,
    CardDiscarded,
    CardExhausted,
    CardCharged,
    CardPlayDeclared,
    AttackPlayed,
    SkillPlayed,
    PowerPlayed,
    AttackDeclared,
    BlockDeclared,
    DrawCardDeclared,
    GainEnergyDeclared,
    ModifyCostDeclared,
    BuffDeclared,
    DamageRequested,
    DamageResolved,
    BuffRequested,
    BuffResolved,
    HpChanged,
    BlockChanged,
    BuffChanged,
    DeathDeclared,
    AnimationStarted,
    AnimationEnded,
    EnemyIntentDecided,
    RelicActivated,
}
public enum RelicTarget
{
    None,
    Player,
    EnemyAll,
    EnemyRandom,
}
public enum RelicEffect
{
    Hp,
    HpMax,
    Block,

    Strength,
    Weak,
    Vulnable,

    DrawCard,
    GainEnergy,
}
public enum UtilEffect
{
    DrawCard,
    GainEnergy,
}
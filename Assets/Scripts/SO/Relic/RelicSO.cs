using UnityEngine;

[CreateAssetMenu(fileName = "Relic_", menuName = "Scriptable Objects/Relic")]
public class RelicSO : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private string description;
    [SerializeField] private string flaverText;
    [SerializeField] private Sprite icon;
    [SerializeField] private RelicRarity rarity;

    [SerializeField] private RelicTrigger trigger;
    [SerializeField] private RelicTarget target;
    [SerializeField] private RelicEffect effect;
    [SerializeField] private int value;
    [SerializeField] private float ratio;
    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public string FlaverText => flaverText;
    public Sprite Icon => icon;
    public RelicRarity Rarity => rarity;
    public RelicTrigger Trigger => trigger;
    public RelicTarget Target => target;
    public RelicEffect Effect => effect;
    public int Value => value;
    public float Ratio => ratio;
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
public enum RelicTrigger
{
    CombatStarted, 
    CombatEnded,
    PlayerTurnStarted,
    PlayerTurnEnded,
    EnemyTurnStarted,
    EnemyTurnEnded,
    ActionStarted, 
    ActionEnded,
    AttackPlayed,
    SkillPlayed,
    PowerPlayed,

    CardDrawed,
    CardDiscarded,
    CardExhausted,
    CardCharged,

    DeathDeclared,
    DamageRequested,
    HpChanged
}
public enum RelicTarget
{
    None,
    Player,
    Enemy
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
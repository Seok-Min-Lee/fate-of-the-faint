using UnityEngine;

[CreateAssetMenu(fileName = "Card_", menuName = "Scriptable Objects/_base/Card")]
public abstract class CardSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] protected string id;
    [SerializeField] protected string name;
    [SerializeField] protected string description;
    [SerializeField] protected Sprite image;

    [SerializeField] protected bool isExhausted;
    [SerializeField] protected bool existTarget;
    [Header("Classification")]
    [SerializeField] protected CardRarity rarity;

    [Header("Cost")]
    [SerializeField] protected int cost;

    [Header("Upgrade")]
    [SerializeField] protected bool isUpgraded;
    [SerializeField] protected CardSO upgradeCard;

    public string Id => id;
    public string Name => isUpgraded ? $"<color=#00FF40>{name}</color>" : name;
    public string Description => GetDescription();
    public Sprite Image => image;
    public bool IsExhausted => isExhausted;
    public bool ExistTarget => existTarget;
    public CardRarity Rarity => rarity;
    public int Cost => cost;
    public bool IsUpgraded => isUpgraded;
    public CardSO UpgradeCard => upgradeCard;
    protected abstract string GetDescription();
}
public enum CardRarity
{
    Common,
    Uncommon,
    Rare
}
public enum TargetType
{
    Player,
    EnemySingle,
    EnemyAll,
    None
}
public enum EffectType
{
    Attack,
    Block,

    DrawCard,
    GainEnergy,
    ModifyCost,

    Strengthen,
    Weaken,
    Vulnerable,
}
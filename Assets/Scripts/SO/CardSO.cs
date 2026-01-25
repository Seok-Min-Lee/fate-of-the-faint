using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Objects/CardSO")]
public class CardSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id;
    [SerializeField] private string name;
    [SerializeField] private string description;
    [SerializeField] private Sprite image;

    [Header("Classification")]
    [SerializeField] private CardType type;
    [SerializeField] private CardRarity rarity;
    [SerializeField] private TargetType target;

    [Header("Cost")]
    [SerializeField] private int cost; 

    [Header("Effects")]
    [SerializeField] private CardEffect[] effects;

    [Header("Upgrade")]
    [SerializeField] private CardSO upgradeCard;

    #region ##### getter #####
    public string Id => id;
    public string Name => name;
    public string Description => description;
    public Sprite Image => image;
    public CardType Type => type;
    public CardRarity Rarity => rarity;
    public TargetType Target => target;
    public int Cost => cost;
    public CardEffect[] Effects => effects;
    public CardSO UpgradeCard => upgradeCard;
    #endregion
}
[Serializable]
public struct CardEffect
{
    public TargetType target;
    public EffectType effectType;
    public int repeat;
    public int value;
}
public enum CardType
{
    Attack,
    Skill,
    Power
}
public enum CardRarity
{
    Common,
    Uncommon,
    Rare
}
public enum TargetType
{
    Self,
    EnemySingle,
    EnemyAll,
    None
}
public enum EffectType
{
    Attack,
    Shield,

    DrawCard,
    GainEnergy,
    ModifyCost,

    Strengthen,
    Weaken,
    Vulnerable,
}
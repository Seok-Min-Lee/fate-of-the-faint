using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card_", menuName = "Scriptable Objects/CardSO")]
public abstract class CardSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id;
    [SerializeField] private string name;
    [SerializeField] private string description;
    [SerializeField] private Sprite image;

    [SerializeField] private bool isExhausted;
    [SerializeField] private bool existTarget;
    [Header("Classification")]
    [SerializeField] private CardRarity rarity;

    [Header("Cost")]
    [SerializeField] private int cost; 

    [Header("Upgrade")]
    [SerializeField] private CardSO upgradeCard;

    public string Id => id;
    public string Name => name;
    public string Description => description;
    public Sprite Image => image;
    public bool IsExhausted => isExhausted;
    public bool ExistTarget => existTarget;
    public CardRarity Rarity => rarity;
    public int Cost => cost;
    public CardSO UpgradeCard => upgradeCard;
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
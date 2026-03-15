using UnityEngine;

[CreateAssetMenu(fileName = "Relic_", menuName = "Scriptable Objects/_base/Relic")]
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
    public abstract RelicInstance CreateInstance();
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
public enum RelicTarget
{
    None,
    Player,
    EnemyAll,
    EnemyRandom,
}
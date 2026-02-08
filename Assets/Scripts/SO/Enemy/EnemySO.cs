using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy_", menuName = "Scriptable Objects/Enemy/EnemySO")]
public sealed class EnemySO : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;

    [Tooltip("전투에서 생성할 프리팹(EnemyView가 붙어있는 오브젝트)")]
    public GameObject prefab;

    public Sprite portrait;

    [Header("Base Stats")]
    public IntRange maxHpRange;

    [Min(0)] public int baseBlock;
    public int baseStrength;

    [Header("AI")]
    public EnemyAIPolicySO aiPolicy;

    [Header("Rewards (Optional)")]
    public IntRange goldReward;
    [Range(0f, 1f)] public float rareRelicChance;
}
public enum IntentType
{
    Attack,
    AttackBlock,
    Block,
    Buff,
    Debuff,
    Special
}

[Serializable]
public struct IntRange
{
    [SerializeField] private int min;
    [SerializeField] private int max;

    public int Roll()
    {
        return UnityEngine.Random.Range(min, max + 1);
    }
}
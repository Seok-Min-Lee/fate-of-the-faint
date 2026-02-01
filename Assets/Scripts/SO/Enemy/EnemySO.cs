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

    private void OnValidate()
    {
        if (id != null)
        {
            id = id.Trim();
        }

        if (displayName != null)
        {
            displayName = displayName.Trim();
        }

        if (maxHpRange.max < maxHpRange.min)
        {
            maxHpRange.max = maxHpRange.min;
        }
    }
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

public enum StatusType
{
    Strength,
    Weak,
    Vulnerable,
    Frail,
    Artifact
}

[Serializable]
public struct IntRange
{
    [Min(0)] public int min;
    [Min(0)] public int max;

    public int ClampMinMax(int value)
    {
        int lo = min;
        int hi = max;

        if (hi < lo)
        {
            hi = lo;
        }

        if (value < lo)
        {
            return lo;
        }

        if (value > hi)
        {
            return hi;
        }

        return value;
    }
}
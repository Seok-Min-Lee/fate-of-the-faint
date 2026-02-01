using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAction_", menuName = "Scriptable Objects/Enemy/EnemyActionSO")]
public sealed class EnemyActionSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("내부 식별 키 (예: bash, defend). 중복되지 않게 관리.")]
    public string key;

    [Tooltip("UI에 표시할 액션 이름(선택). 비워도 됨.")]
    public string displayName;

    [Header("Intent")]
    public IntentType intentType;

    [Tooltip("Intent 아이콘(선택).")]
    public Sprite intentIcon;

    [TextArea(1, 3)]
    public string intentDescription;

    [Header("Numbers")]
    [Min(0)] public int damage;
    [Min(1)] public int hits = 1;
    [Min(0)] public int block;

    [Header("Status Effects")]
    public List<StatusApplication> statuses = new List<StatusApplication>();

    [Header("AI Rules")]
    [Min(0)] public int weight = 10;

    [Tooltip("사용 후 n턴 동안 재사용 불가")]
    [Min(0)] public int cooldownTurns;

    [Tooltip("같은 액션 연속 사용 최대 횟수 (0이면 제한 없음)")]
    [Min(0)] public int maxConsecutive;

    private void OnValidate()
    {
        if (hits < 1)
        {
            hits = 1;
        }

        if (key != null)
        {
            key = key.Trim();
        }
    }
}

[Serializable]
public struct StatusApplication
{
    public StatusType type;

    [Tooltip("양수면 부여, 음수면 제거. (예: Strength +2)")]
    public int amount;

    [Tooltip("대상: false=플레이어, true=자기 자신(적)")]
    public bool targetSelf;
}

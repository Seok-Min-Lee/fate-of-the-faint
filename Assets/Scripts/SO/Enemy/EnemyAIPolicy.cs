using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAIPolicy_", menuName = "Scriptable Objects/Enemy/EnemyAIPolicySO")]
public sealed class EnemyAIPolicySO : ScriptableObject
{
    [Header("Actions")]
    public List<EnemyActionSO> actions = new List<EnemyActionSO>();

    [Header("Global Rules")]
    [Tooltip("같은 액션 연속 사용 최대 횟수(0이면 제한 없음). 액션별 maxConsecutive와 함께 적용됨.")]
    [Min(0)] public int globalMaxRepeat = 1;

    [Tooltip("후보가 0개가 될 때의 처리. true면 쿨다운/반복 제한 무시하고 actions에서 다시 뽑음.")]
    public bool allowFallbackPick = true;

    private void OnValidate()
    {
        if (actions == null)
        {
            actions = new List<EnemyActionSO>();
        }
    }
}

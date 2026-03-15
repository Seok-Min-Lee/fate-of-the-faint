using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAction_", menuName = "Scriptable Objects/Enemy/Enemy Action")]
public sealed class EnemyActionSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("내부 식별 키 (예: bash, defend). 중복되지 않게 관리.")]
    [SerializeField] private string key;

    [Tooltip("UI에 표시할 액션 이름(선택). 비워도 됨.")]
    [SerializeField] private string displayName;

    [Header("Intent")]
    [SerializeField] private IntentType intentType;

    [Tooltip("Intent 아이콘(선택).")]
    [SerializeField] private Sprite intentIcon;

    [TextArea(1, 3)]
    [SerializeField] private string intentDescription;

    [Header("Effects")]
    [SerializeField] private EnemyEffectSO[] effects;

    [Header("AI Rules")]
    [Min(0)][SerializeField] private int weight = 10;

    [Tooltip("사용 후 n턴 동안 재사용 불가")]
    [Min(0)][SerializeField] private int cooldownTurns;

    [Tooltip("같은 액션 연속 사용 최대 횟수 (0이면 제한 없음)")]
    [Min(0)][SerializeField] private int maxConsecutive;

    public string Key => key;
    public string DisplayName => displayName;
    public IntentType IntentType => intentType;
    public Sprite IntentIcon => intentIcon;
    public string IntentDescription => intentDescription;
    public EnemyEffectSO[] Effects => effects;
    public int Weight => weight;
    public int CooldownTurns => cooldownTurns;
    public int MaxConsecutive => maxConsecutive;
    private void OnValidate()
    {
        if (key != null)
        {
            key = key.Trim();
        }
    }
}
public enum IntentTarget
{
    Self,
    Player,
    Member,
    MemberAll
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

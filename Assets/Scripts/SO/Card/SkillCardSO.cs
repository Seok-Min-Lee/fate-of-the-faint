using UnityEngine;

[CreateAssetMenu(fileName = "skl_", menuName = "Scriptable Objects/CardSO/Skill Card")]
public class SkillCardSO : CardSO
{
    [Header("Effects")]
    [SerializeField] private EffectSO[] effects;
    public EffectSO[] Effects => effects;
}

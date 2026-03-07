using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "skl_", menuName = "Scriptable Objects/CardSO/Skill Card")]
public class SkillCardSO : CardSO
{
    [Header("Effects")]
    [SerializeField] private EffectSO[] effects;
    public EffectSO[] Effects => effects;
    protected override string GetDescription()
    {
        return string.Format(description, effects.Select(e => (object)e.Value).ToArray())
                     .Replace("[", "<color=#00FF40>")
                     .Replace("]", "</color>");
    }
}

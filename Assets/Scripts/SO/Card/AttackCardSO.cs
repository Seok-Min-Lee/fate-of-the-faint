using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "atk_", menuName = "Scriptable Objects/Card/Attack Card")]
public class AttackCardSO : CardSO
{
    [Header("Effects")]
    [SerializeField] private EffectSO[] effects;
    public EffectSO[] Effects => effects;
    protected override string GetDescription()
    {
        List<object> values = new List<object>();
        for (int i = 0; i < effects.Length; i++)
        {
            values.Add(effects[i].Value);

            if (effects[i] is AttackEffectSO attack && attack.Repeat > 1)
            {
                values.Add(attack.Repeat);
            }
        }

        return string.Format(description, values.ToArray())
                     .Replace("[", "<color=#00FF40>")
                     .Replace("]", "</color>");
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Gain Energy Effect ", menuName = "Scriptable Objects/EffectSO/Gain Energy Effect ")]
public class GainEnergyEffectSO : EffectSO
{
    [SerializeField] private int value;
    public int Value => value;
    public override Action Apply(
        EventBus eventBus,
        EventContext context,
        MotionContext motion,
        EntityInstance source = null,
        IEnumerable<EntityInstance> targets = null
    )
    {
        if (targets != null && targets.Count() != 0)
        {
            return null;
        }

        return () =>
        {
            eventBus.Publish<GainEnergyDeclared>(new GainEnergyDeclared(
                context: context,
                motion: motion,
                amount: value
            ));
        };
    }
}

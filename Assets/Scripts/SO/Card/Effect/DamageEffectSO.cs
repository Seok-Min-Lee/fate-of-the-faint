using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Damage ", menuName = "Scriptable Objects/Card Effect/Damage ")]
public class DamageEffectSO : EffectSO
{
    public override Action Apply(
        EventBus eventBus,
        EventContext context,
        MotionContext motion,
        EntityInstance source = null,
        IEnumerable<EntityInstance> targets = null
    )
    {
        if (targets?.Count() == 0)
        {
            return null;
        }

        return () =>
        {
            foreach (EntityInstance target in targets)
            {
                target.Damage(
                    eventBus: eventBus, 
                    context: context, 
                    motion: motion,
                    amount: value
                );
            }
        };
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Heal ", menuName = "Scriptable Objects/Card Effect/Heal ")]
public class HealEffectSO : EffectSO
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
                target.ChangeHp(
                    eventBus: eventBus, 
                    context: context, 
                    motion: motion, 
                    amount: value
                );
            }
        };
    }
}

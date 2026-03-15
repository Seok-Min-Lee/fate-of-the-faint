using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack ", menuName = "Scriptable Objects/Card Effect/Attack ")]
public class AttackEffectSO : EffectSO
{
    [SerializeField] private int repeat;
    public int Repeat => repeat;
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
                eventBus.Publish<AttackDeclared>(new AttackDeclared(
                    context: context,
                    motion: motion,
                    source: source,
                    target: target,
                    amount: value,
                    repeat: repeat
                ));
            }
        }; 
    }
}


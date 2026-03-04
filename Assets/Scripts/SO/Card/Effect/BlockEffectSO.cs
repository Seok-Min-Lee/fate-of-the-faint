using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Block Effect ", menuName = "Scriptable Objects/EffectSO/Block Effect ")]
public class BlockEffectSO : EffectSO
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
        if (targets?.Count() == 0)
        {
            return null;
        }

        return () =>
        {
            foreach (EntityInstance target in targets)
            {
                eventBus.Publish<BlockDeclared>(new BlockDeclared(
                    context: context,
                    motion: motion,
                    source: source,
                    target: target,
                    amount: value
                ));
            }
        };
    }
}

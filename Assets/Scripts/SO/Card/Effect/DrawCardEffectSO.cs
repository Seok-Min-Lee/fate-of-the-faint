using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Draw Card Effect ", menuName = "Scriptable Objects/EffectSO/Draw Card Effect ")]
public class DrawCardEffectSO : EffectSO
{
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
            eventBus.Publish<DrawCardDeclared>(new DrawCardDeclared(
                context: context,
                motion: motion,
                amount: value
            ));
        };
    }
}

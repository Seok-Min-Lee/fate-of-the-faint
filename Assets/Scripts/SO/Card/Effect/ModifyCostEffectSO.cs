using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Modify Cost ", menuName = "Scriptable Objects/EffectSO/Modify Cost Effect ")]
public class ModifyCostEffectSO : EffectSO
{
    [SerializeField] private CostModificationScope range;
    public CostModificationScope Range => range;
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
            eventBus.Publish<ModifyCostDeclared>(new ModifyCostDeclared(
                context: context,
                scope: range,
                amount: value
            ));
        };
    }
}

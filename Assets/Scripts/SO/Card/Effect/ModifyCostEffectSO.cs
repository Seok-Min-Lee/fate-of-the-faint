using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Modify Cost Effect ", menuName = "Scriptable Objects/EffectSO/Modify Cost Effect ")]
public class ModifyCostEffectSO : EffectSO
{
    [SerializeField] private CostModificationScope range;
    [SerializeField] private int value;
    public CostModificationScope Range => range;
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
            eventBus.Publish<ModifyCostDeclared>(new ModifyCostDeclared(
                context: context,
                scope: range,
                amount: value
            ));
        };
    }
}

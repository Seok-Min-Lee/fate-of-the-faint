using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(fileName = "Buff ", menuName = "Scriptable Objects/Enemy Effect/Buff")]
public class EnemyBuffSO : EnemyEffectSO
{
    [SerializeField] private BuffType buffType;
    public BuffType BuffType => buffType;
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
                eventBus.Publish<BuffDeclared>(new BuffDeclared(
                    context: context,
                    motion: motion,
                    source: source,
                    target: target,
                    type: buffType,
                    amount: value
                ));
            }
        };
    }
}

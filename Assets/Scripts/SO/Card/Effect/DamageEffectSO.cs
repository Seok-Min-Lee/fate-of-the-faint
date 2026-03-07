using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Damage ", menuName = "Scriptable Objects/EffectSO/Damage Effect ")]
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
            EntityInstance player = context.Combat.Player;
            int startAmount = player.CurrentHp;
            player.SetCurrentHp(startAmount - value);

            eventBus.Publish<HpChanged>(new HpChanged(
                context: context.RewriteNew(this),
                motion: motion,
                target: player,
                startAmount: startAmount,
                endAmount: player.CurrentHp
            ));
        };
    }
}


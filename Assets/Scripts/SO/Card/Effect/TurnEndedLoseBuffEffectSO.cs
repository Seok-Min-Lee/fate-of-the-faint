using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Lose Buff ", menuName = "Scriptable Objects/Card Effect/Lose Buff Effect ")]
public class TurnEndedLoseBuffEffectSO : EffectSO
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
            new TurnEndedLoseEffectReservation(
                eventBus: eventBus, 
                origin: this,
                targets: targets
            );
        };
    }
}

public class TurnEndedLoseEffectReservation : IPlayerTurnEnded
{
    private readonly EventBus eventBus;
    private readonly TurnEndedLoseBuffEffectSO origin;
    private IEnumerable<EntityInstance> targets;
    private BuffType buffType;
    private int amount;
    public TurnEndedLoseEffectReservation(
        EventBus eventBus, 
        TurnEndedLoseBuffEffectSO origin,
        IEnumerable<EntityInstance> targets
    ) 
    {
        this.eventBus = eventBus;
        this.origin = origin;
        this.targets = targets;

        this.buffType = origin.BuffType;
        this.amount = origin.Value;

        if (eventBus != null)
        {
            eventBus.Subscribe<PlayerTurnEnded>(OnPlayerTurnEnded);
        }
    }
    public void OnPlayerTurnEnded(PlayerTurnEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        foreach (EntityInstance target in targets)
        {
            eventBus.Publish<BuffDeclared>(new BuffDeclared(
                context: e.Context.RewriteNew(this),
                motion: e.Motion,
                source: this,
                target: target,
                type: buffType,
                amount: -amount
            ));
        }

        eventBus.Unsubscribe<PlayerTurnEnded>(OnPlayerTurnEnded);
    }
}

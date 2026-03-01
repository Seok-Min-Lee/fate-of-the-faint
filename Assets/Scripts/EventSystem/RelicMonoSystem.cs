using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class RelicMonoSystem : BaseMonoSystem
{
    [SerializeField] private RelicViewPool pool;
    private List<RelicInstance> relicAll;
    private List<RegularRelicInstance> regularRelics = new List<RegularRelicInstance>();

    private EventBus eventBus;
    private PlayerInstance player;
    private List<EnemyInstance> enemies = new List<EnemyInstance>();
    public void Init(EventBus eventBus, PlayerInstance player, IEnumerable<EnemyInstance> enemies, IEnumerable<RelicSO> relics)
    {
        this.eventBus = eventBus;
        this.player = player;
        this.enemies = new List<EnemyInstance>(enemies);

        relicAll = new List<RelicInstance>();
        foreach(RelicSO so in relics)
        {
            RelicInstance instance = so.CreateInstance(eventBus);

            if (instance is RegularRelicInstance)
            {
                regularRelics.Add(instance as RegularRelicInstance);
            }
            else
            {
                instance.Register();
            }

            relicAll.Add(instance);
        }

        pool.CreateViews(relicAll);

        foreach (RelicView view in pool.Actives)
        {
            eventBus.Subscribe<RelicActivated>(view.OnRelicActivated);
        }
    }
    public void OnCombatStarted(CombatStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.CombatStarted,
            context: e.Context,
            motion: e.Motion
        );
    }

    public void OnCombatEnded(CombatEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.CombatEnded,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.PlayerTurnStarted,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnPlayerTurnEnded(PlayerTurnEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.PlayerTurnEnded,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnEnemyTurnStarted(EnemyTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.EnemyTurnStarted,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnEnemyTurnEnded(EnemyTurnEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.EnemyTurnEnded,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnActionStarted(ActionStarted e) 
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.ActionStarted,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnActionEnded(ActionEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.ActionEnded,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnAttackPlayed(AttackPlayed e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.AttackPlayed,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnSkillPlayed(SkillPlayed e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.SkillPlayed,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnPowerPlayed(PowerPlayed e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.PowerPlayed,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnCardDrawed(CardDrawed e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.CardDrawed,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnCardDiscarded(CardDiscarded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.CardDiscarded,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnCardExhausted(CardExhausted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.CardDiscarded,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnCardCharged(CardCharged e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.CardCharged,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnDeathDeclared(DeathDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.DeathDeclared,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnDamageRequested(DamageRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.DamageRequested,
            context: e.Context,
            motion: e.Motion
        );
    }
    public void OnHpChanged(HpChanged e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTriggerEvent.HpChanged,
            context: e.Context,
            motion: e.Motion
        );
    }
    private void ActivateRelic(RelicTriggerEvent trigger, EventContext context, MotionContext motion)
    {
        //for (int i = 0; i < regularRelics.Count; i++)
        //{
        //    RegularRelicSO origin = regularRelics[i].Origin as RegularRelicSO;

        //    if (origin.TriggerEvent == trigger)
        //    {
        //        List<EntityInstance> targets = new List<EntityInstance>();
        //        switch (origin.Target)
        //        {
        //            case RelicTarget.Player:
        //                targets.Add(player);
        //                break;
        //            case RelicTarget.EnemyAll:
        //                targets.AddRange(enemies);
        //                break;
        //            default:
        //                break;
        //        }

        //        regularRelics[i].Activate(
        //            context: context,
        //            motion: motion,
        //            targets: targets
        //        );
        //    }
        //}
    }
}

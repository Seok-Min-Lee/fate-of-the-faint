using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RelicMonoSystem : BaseMonoSystem
{
    [SerializeField] private RelicViewPool pool;
    private List<RelicEntry> relics;

    private EventBus eventBus;
    private PlayerInstance player;
    private List<EnemyInstance> enemies = new List<EnemyInstance>();
    public void Init(EventBus eventBus, PlayerInstance player, IEnumerable<EnemyInstance> enemies, IEnumerable<RelicEntry> relics)
    {
        this.eventBus = eventBus;
        this.player = player;
        this.enemies = new List<EnemyInstance>(enemies);

        this.relics = new List<RelicEntry>(relics);
        pool.CreateViews(relics);

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
            trigger: RelicTrigger.CombatStarted, 
            triggerValue: eventBus.PublishCounter[typeof(CombatStarted)],
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
            trigger: RelicTrigger.CombatEnded,
            triggerValue: eventBus.PublishCounter[typeof(CombatEnded)],
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
            trigger: RelicTrigger.PlayerTurnStarted,
            triggerValue: eventBus.PublishCounter[typeof(PlayerTurnStarted)],
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
            trigger: RelicTrigger.PlayerTurnEnded,
            triggerValue: eventBus.PublishCounter[typeof(PlayerTurnEnded)],
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
            trigger: RelicTrigger.EnemyTurnStarted,
            triggerValue: eventBus.PublishCounter[typeof(EnemyTurnStarted)],
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
            trigger: RelicTrigger.EnemyTurnEnded,
            triggerValue: eventBus.PublishCounter[typeof(EnemyTurnEnded)],
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
            trigger: RelicTrigger.ActionStarted,
            triggerValue: eventBus.PublishCounter[typeof(ActionStarted)],
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
            trigger: RelicTrigger.ActionEnded,
            triggerValue: eventBus.PublishCounter[typeof(ActionEnded)],
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
            trigger: RelicTrigger.AttackPlayed,
            triggerValue: eventBus.PublishCounter[typeof(AttackPlayed)],
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
            trigger: RelicTrigger.SkillPlayed,
            triggerValue: eventBus.PublishCounter[typeof(SkillPlayed)],
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
            trigger: RelicTrigger.PowerPlayed,
            triggerValue: eventBus.PublishCounter[typeof(PowerPlayed)],
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
            trigger: RelicTrigger.CardDrawed,
            triggerValue: eventBus.PublishCounter[typeof(CardDrawed)],
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
            trigger: RelicTrigger.CardDiscarded,
            triggerValue: eventBus.PublishCounter[typeof(CardDiscarded)],
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
            trigger: RelicTrigger.CardDiscarded,
            triggerValue: eventBus.PublishCounter[typeof(CardDiscarded)],
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
            trigger: RelicTrigger.CardCharged,
            triggerValue: eventBus.PublishCounter[typeof(CardCharged)],
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
            trigger: RelicTrigger.DeathDeclared,
            triggerValue: eventBus.PublishCounter[typeof(DeathDeclared)],
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
            trigger: RelicTrigger.DamageRequested,
            triggerValue: eventBus.PublishCounter[typeof(DamageRequested)],
            context: e.Context,
            motion: e.Motion
        );
        //if (e.Damage.Source == player)
        //{
        //    e.Damage.Add(relic.strength, relic);
        //}
    }
    public void OnHpChanged(HpChanged e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ActivateRelic(
            trigger: RelicTrigger.HpChanged,
            triggerValue: eventBus.PublishCounter[typeof(HpChanged)],
            context: e.Context,
            motion: e.Motion
        );
    }
    private void ActivateRelic(RelicTrigger trigger, int triggerValue, EventContext context, MotionContext motion)
    {
        for (int i = 0; i < relics.Count; i++)
        {
            RelicSO origin = relics[i].Origin;

            if (origin.Trigger == trigger)
            {
                eventBus.Publish<RelicActivated>(new RelicActivated(
                    context: CreateContext(context),
                    motion: motion,
                    source: relics[i]
                ));
                Debug.Log(origin.Effect.ToString());

                //
                List<EntityInstance> targets = new List<EntityInstance>();
                if (origin.Target == RelicTarget.Enemy)
                {
                    targets.AddRange(enemies);
                }
                else
                {
                    targets.Add(player);
                }

                //
                for (int j = 0; j < targets.Count; j++)
                {
                    int startAmount;
                    switch (origin.Effect)
                    {
                        case RelicEffect.Hp:
                            startAmount = targets[j].CurrentHp;
                            targets[j].SetCurrentHp(startAmount + origin.Value);

                            eventBus.Publish<HpChanged>(new HpChanged(
                                context: CreateContext(context),
                                motion: motion,
                                target: targets[j],
                                startAmount: startAmount,
                                endAmount: targets[j].CurrentHp
                            ));
                            break;
                        case RelicEffect.Block:
                            startAmount = targets[j].Block;
                            targets[j].SetBlock(startAmount + origin.Value);

                            eventBus.Publish<BlockChanged>(new BlockChanged(
                                context: CreateContext(context),
                                motion: motion,
                                target: targets[j],
                                startAmount: startAmount,
                                endAmount: targets[j].Block
                            ));
                            break;
                        case RelicEffect.Strength:
                            startAmount = targets[j].Getbuff(BuffType.Strength);
                            targets[j].ApplyBuff(BuffType.Strength, origin.Value);

                            eventBus.Publish<BuffChanged>(new BuffChanged(
                                context: CreateContext(context),
                                motion: motion,
                                target: targets[j],
                                type: BuffType.Strength,
                                startAmount: startAmount,
                                endAmount: targets[j].Getbuff(BuffType.Strength)
                            ));
                            break;
                        case RelicEffect.Weak:
                            startAmount = targets[j].Getbuff(BuffType.Weak);
                            targets[j].ApplyBuff(BuffType.Weak, origin.Value);

                            eventBus.Publish<BuffChanged>(new BuffChanged(
                                context: CreateContext(context),
                                motion: motion,
                                target: targets[j],
                                type: BuffType.Weak,
                                startAmount: startAmount,
                                endAmount: targets[j].Getbuff(BuffType.Weak)
                            ));
                            break;
                        case RelicEffect.Vulnable:
                            startAmount = targets[j].Getbuff(BuffType.Vulnerable);
                            targets[j].ApplyBuff(BuffType.Vulnerable, origin.Value);

                            eventBus.Publish<BuffChanged>(new BuffChanged(
                                context: CreateContext(context),
                                motion: motion,
                                target: targets[j],
                                type: BuffType.Vulnerable,
                                startAmount: startAmount,
                                endAmount: targets[j].Getbuff(BuffType.Vulnerable)
                            ));
                            break;
                        case RelicEffect.DrawCard:
                            eventBus.Publish<DrawCardDeclared>(new DrawCardDeclared(
                                context: context,
                                motion: motion,
                                amount: origin.Value
                            ));
                            break;
                        case RelicEffect.GainEnergy:
                            eventBus.Publish<GainEnergyDeclared>(new GainEnergyDeclared(
                                context: context,
                                motion: motion,
                                amount: origin.Value
                            ));
                            break;
                    }
                }
            }
        }
    }
}

using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardMonoSystem : BaseMonoSystem
{
    private EventBus eventBus;
    private CombatSystem combatSystem;
    private ActionSystem actionSystem;
    private EntityInstance player;

    [SerializeField] private CardContainer cardHand;
    [SerializeField] private CardViewPool cardViewPool;

    [SerializeField] private TextMeshProUGUI drawPileText;
    [SerializeField] private TextMeshProUGUI discardPileText;

    private List<CardInstance> cardInstanceAll = new List<CardInstance>();
    private List<CardInstance> drawPile = new List<CardInstance>();
    private List<CardInstance> hand = new List<CardInstance>();
    private List<CardInstance> discardPile = new List<CardInstance>();
    private List<CardInstance> exhaustPile = new List<CardInstance>();

    private CardInstance cardInstance;
    private ITargetable target;
    public void Init(
        EventBus eventBus,
        CombatSystem combatSystem,
        ActionSystem actionSystem,
        IEnumerable<CardInstance> cardInstances,
        EntityInstance player
    )
    {
        this.eventBus = eventBus;
        this.combatSystem = combatSystem;
        this.actionSystem = actionSystem;
        this.player = player;

        cardInstanceAll.AddRange(cardInstances);
        drawPile.AddRange(Utils.Shuffle(cardInstances));

        UpdateUI();
    }
    public void PlayCardStart(CardView cardView, ITargetable target)
    {
        this.cardInstance = cardView.CardInstance;
        this.target = target;

        ActionContext actionContext = new ActionContext(source: this, type: ActionType.PlayerCardPlay);

        actionSystem.ExcuteAction(actionContext, (eventContext, animationContext) =>
        {
            bool existModifier = cardInstanceAll.Any(c => c.ExistModifier);

            eventBus.Publish<CardPlayDeclared>(new CardPlayDeclared(
                context: eventContext,
                cardView: cardView
            ));

            RequestContext requestContext = new RequestContext(source: this);

            eventBus.Publish<EnergyChangeRequested>(new EnergyChangeRequested(
                context: eventContext,
                motion: animationContext,
                request: requestContext,
                amount: -cardInstance.Cost
            ));

            // 
            if (requestContext.isResult)
            {
                DiscardCard(
                    cardView: cardView, 
                    context: eventContext, 
                    motion: animationContext
                );
            }
            else
            {
                cardView = null;
                cardInstance = null;
                target = null;
            }

            // Cost Update
            if (existModifier)
            {
                RemoveModificationsByScope(CostModificationScope.Action);
            }
        });
    }
    public void OnCombatEnded(CombatEnded e)
    {
        if (e.Result != CombatState.Victory)
        {
            return;
        }

        RemoveModificationsByScope(CostModificationScope.Combat);
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e) 
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        for (int i = 0; i < 5; i++)
        {
            DrawCard(context: e.Context, motion: e.Motion);
        }
    }
    public void OnPlayerTurnEnded(PlayerTurnEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ClearCardHand(
            context: CreateContext(e.Context), 
            motion: e.Motion
        );

        RemoveModificationsByScope(CostModificationScope.Turn);
    }
    public void OnEnergyResolved(EnergyResolved e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        foreach (CardEffect ce in cardInstance.Origin.Effects)
        {
            List<EntityInstance> targets = new List<EntityInstance>();
            switch (ce.targetType)
            {
                case TargetType.Self:
                    targets.Add(player);
                    break;
                case TargetType.EnemySingle:
                    targets.Add(target.Instance);
                    break;
                case TargetType.EnemyAll:
                    targets.AddRange(combatSystem.liveEnemies);
                    break;
            }


            EventContext context = CreateContext(e.Context);

            if (ce.effectType == EffectType.DrawCard)
            {
                eventBus.Publish<DrawCardDeclared>(new DrawCardDeclared(
                    context: context, 
                    motion: e.Motion,
                    amount: ce.value
                ));
            }
            else if (ce.effectType == EffectType.GainEnergy)
            {
                // 여기서 EnergyChangeRequest 사용하면 StackOverflow 발생
                eventBus.Publish<GainEnergyDeclared>(new GainEnergyDeclared(
                    context: context,
                    motion: e.Motion,
                    amount: ce.value
                ));
            }
            else if (ce.effectType == EffectType.ModifyCost)
            {
                eventBus.Publish<ModifyCostDeclared>(new ModifyCostDeclared(
                    context: context, 
                    scope: CostModificationScope.Action, 
                    amount: ce.value
                ));
            }
            else
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    EntityInstance target = targets[i];

                    context = CreateContext(e.Context);

                    if (ce.effectType == EffectType.Attack)
                    {
                        eventBus.Publish<AttackDeclared>(new AttackDeclared(
                            context: context,
                            motion: e.Motion,
                            source: player,
                            target: target,
                            amount: ce.value,
                            repeat: ce.repeat
                        ));
                    }
                    else if (ce.effectType == EffectType.Block)
                    {
                        eventBus.Publish<BlockDeclared>(new BlockDeclared(
                            context: context,
                            motion: e.Motion,
                            source: player,
                            target: player,
                            amount: ce.value
                        ));
                    }
                    else if (ce.effectType == EffectType.Strengthen)
                    {
                        eventBus.Publish<BuffDeclared>(new BuffDeclared(
                            context: context,
                            motion: e.Motion,
                            source: player,
                            target: target,
                            type: BuffType.Strength,
                            amount: ce.value
                        ));
                    }
                    else if (ce.effectType == EffectType.Weaken)
                    {
                        eventBus.Publish<BuffDeclared>(new BuffDeclared(
                            context: context,
                            motion: e.Motion,
                            source: player,
                            target: target,
                            type: BuffType.Weak,
                            amount: ce.value
                        ));
                    }
                    else if (ce.effectType == EffectType.Vulnerable)
                    {
                        eventBus.Publish<BuffDeclared>(new BuffDeclared(
                            context: context,
                            motion: e.Motion,
                            source: player,
                            target: target,
                            type: BuffType.Vulnerable,
                            amount: ce.value
                        ));
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
    }
    public void OnDrawCardDeclared(DrawCardDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EventContext context = CreateContext(e.Context);

        for (int i = 0; i < e.Amount; i++)
        {
            DrawCard(context: context, motion: e.Motion);
        }
    }
    public void OnModifyCostDeclared(ModifyCostDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        foreach (CardInstance instance in cardInstanceAll)
        {
            instance.AddModification(new CostModification(
                scope: e.Scope,
                amount: -e.Amount,
                source: this
            ));
        }

        foreach (CardView view in cardViewPool.Actives)
        {
            view.ModifiyCost();
        }
    }
    private void DrawCard(EventContext context, MotionContext motion)
    {
        if (hand.Count == 10)
        {
            return;
        }

        ChargeCard(context: context);

        // Instance 
        CardInstance cardInstance = drawPile.FirstOrDefault();
        drawPile.Remove(cardInstance);
        hand.Add(cardInstance);

        // Animation
        motion.AddTask(new MotionTask(
            priority: MotionPriority.Card,
            command: () => DrawCardCor(cardInstance),
            source: this
        ));

        // Event
        EventContext _context = CreateContext(context);
        eventBus.Publish<CardDrawed>(new CardDrawed(context: _context));
    }
    private void DiscardCard(CardView cardView, EventContext context, MotionContext motion)
    {
        // Instance
        hand.Remove(cardView.CardInstance);
        discardPile.Add(cardView.CardInstance);

        // Animation
        motion.AddTask(new MotionTask(
            priority: MotionPriority.Card,
            command: () => DiscardCardCor(cardView),
            source: this
        ));

        // Event
        EventContext _context = CreateContext(context);
        eventBus.Publish<CardDiscarded>(new CardDiscarded(context: _context));
    }
    private void DiscardCards(IEnumerable<CardView> views, EventContext context, MotionContext motion)
	{
        // Instance
        foreach (CardView view in views)
		{
			hand.Remove(view.CardInstance);
			discardPile.Add(view.CardInstance);
		}

		// Animation
		motion.AddTask(new MotionTask(
			priority: MotionPriority.Start,
			command: () => DiscardCardsCor(views),
			source: this
		));

		// Event
		EventContext _context = CreateContext(context);
		eventBus.Publish<CardDiscarded>(new CardDiscarded(context: _context));
	}
    private void ChargeCard(EventContext context)
    {
        if (drawPile.Count > 0)
        {
            return;
        }

        drawPile.AddRange(Utils.Shuffle(discardPile));
        discardPile.Clear();

        UpdateUI();

        EventContext _context = CreateContext(context);
        eventBus.Publish<CardCharged>(new CardCharged(context: _context));
    }

    private void ClearCardHand(EventContext context, MotionContext motion)
    {
		DiscardCards(
			views: cardHand.Cards.Reverse(),
			context: context,
			motion: motion
		);
	}
    IEnumerator DrawCardCor(CardInstance cardInstance)
    {
        CardView cardView = cardViewPool.Pop(false);
        cardView.Init(
            cardInstance: cardInstance,
            cardSystem: this,
            cardContainer: cardHand
        );

        cardView.Draw();
        yield return new WaitForSeconds(0.1f);
        UpdateUI();
    }
    IEnumerator DiscardCardCor(CardView cardView)
    {
        cardHand.DestroyCard(cardView);

        yield return cardView.Discard().WaitForCompletion();

		cardViewPool.Push(cardView);
		UpdateUI();
    }
    IEnumerator DiscardCardsCor(IEnumerable<CardView> views)
    {
        List<CardView> copies = new List<CardView>(views);

        for (int i = 0; i < copies.Count; i++)
        {
            CardView view = copies.ElementAt(i);
			cardHand.DestroyCard(view);

            if (i < copies.Count - 1)
            {
                view.Discard();
                yield return new WaitForSeconds(0.05f);
			}
            else
            {
                yield return view.Discard().WaitForCompletion();
			}

			cardViewPool.Push(view);
		    UpdateUI();
		}
	}
    private void RemoveModificationsByScope(CostModificationScope scope)
    {
        foreach (CardInstance c in cardInstanceAll)
        {
            c.RemoveModifications(scope);
        }

        foreach (CardView view in cardViewPool.Actives)
        {
            view.ModifiyCost();
        }
    }
    public void UpdateUI()
    {
        drawPileText.text = drawPile.Count.ToString();
        discardPileText.text = discardPile.Count.ToString();
    }
}

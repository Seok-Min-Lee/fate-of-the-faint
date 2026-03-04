using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardMonoSystem : BaseMonoSystem
{
    private EventBus eventBus;
    private ActionSystem actionSystem;
    private PowerSystem powerSystem;

    [SerializeField] private CardContainer cardHand;
    [SerializeField] private CardViewPool cardViewPool;

    [SerializeField] private TextMeshProUGUI drawPileText;
    [SerializeField] private TextMeshProUGUI discardPileText;
    [SerializeField] private TextMeshProUGUI exhaustPileText;

    private List<CardInstance> cardInstanceAll = new List<CardInstance>();
    private List<CardInstance> drawPile = new List<CardInstance>();
    private List<CardInstance> hand = new List<CardInstance>();
    private List<CardInstance> discardPile = new List<CardInstance>();
    private List<CardInstance> exhaustPile = new List<CardInstance>();

    private CardInstance cardInstance;
    private ITargetable target;
    public void Init(
        EventBus eventBus,
        ActionSystem actionSystem,
        PowerSystem powerSystem,
        IEnumerable<CardInstance> cardInstances
    )
    {
        this.eventBus = eventBus;
        this.actionSystem = actionSystem;
        this.powerSystem = powerSystem;

        cardInstanceAll.AddRange(cardInstances);
        drawPile.AddRange(Utils.Shuffle(cardInstances));

        UpdateUI();
    }
    public void PlayCardStart(CardView cardView, ITargetable target)
    {
        this.cardInstance = cardView.CardInstance;
        this.target = target;

        actionSystem.ExcuteAction(source: this, type: ActionType.PlayerCardPlay, (eventContext, motionContext) =>
        {
            bool existModifier = cardInstanceAll.Any(c => c.ExistModifier);

            eventBus.Publish<CardPlayDeclared>(new CardPlayDeclared(
                context: eventContext,
                motion: motionContext,
                cardView: cardView
            ));

            RequestContext requestContext = new RequestContext(source: this);

            eventBus.Publish<EnergyChangeRequested>(new EnergyChangeRequested(
                context: eventContext,
                motion: motionContext,
                request: requestContext,
                amount: -cardInstance.Cost
            ));

            // 
            if (requestContext.isResult)
            {
                if (cardView.CardInstance.Origin.IsExhausted)
                {
                    ExhaustCard(
                        cardView: cardView,
                        context: eventContext,
                        motion: motionContext
                    );
                }
                else
                {
                    DiscardCard(
                        cardView: cardView,
                        context: eventContext,
                        motion: motionContext
                    );
                }
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
            context: e.Context.RewriteNew(this), 
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

        EventContext context = e.Context.RewriteNew(this);

        // Motion Play
        if (cardInstance.Origin is AttackCardSO)
        {
            eventBus.Publish<AttackPlayed>(new AttackPlayed(
                context: context,
                motion: e.Motion,
                source: context.Combat.Player
            ));

            AttackCardSO cardSO = cardInstance.Origin as AttackCardSO;

            ApplyEffects(
                context: e.Context,
                motion: e.Motion,
                effects: cardSO.Effects
            );
        }
        else if (cardInstance.Origin is SkillCardSO)
        {
            eventBus.Publish<SkillPlayed>(new SkillPlayed(
                context: context,
                motion: e.Motion,
                source: context.Combat.Player
            ));

            SkillCardSO cardSO = cardInstance.Origin as SkillCardSO;

            ApplyEffects(
                context: e.Context,
                motion: e.Motion,
                effects: cardSO.Effects
            );
        }
        else
        {
            eventBus.Publish<PowerPlayed>(new PowerPlayed(
                context: context,
                motion: e.Motion,
                source: context.Combat.Player
            ));

            PowerCardSO cardSO = cardInstance.Origin as PowerCardSO;

            powerSystem.AddPower(cardSO);
        }
    }
    public void OnDrawCardDeclared(DrawCardDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EventContext context = e.Context.RewriteNew(this);

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
    private void ApplyEffects(IEnumerable<EffectSO> effects, EventContext context, MotionContext motion)
    {
        List<Action> applies = new List<Action>();

        foreach (EffectSO effect in effects)
        {
            // 타겟 설정
            List<EntityInstance> targets = GetTargets(
                type: effect.TargetType, 
                context: context
            );

            // 효과 로직 가져오기
            Action apply = effect.Apply(
                eventBus: eventBus, 
                context: context,
                motion: motion, 
                source: context.Combat.Player, 
                targets: targets
            );

            // 오류 체크
            if (apply == null)
            {
                throw new InvalidOperationException("Effect Apply returned null Action");
            }

            applies.Add(apply);
        }

        // 효과 로직 실행
        foreach (Action apply in applies)
        {
            apply.Invoke();
        }
    }
    private List<EntityInstance> GetTargets(TargetType type, EventContext context)
    {
        List<EntityInstance> targets = new List<EntityInstance>();

        switch (type)
        {
            case TargetType.Player:
                targets.Add(context.Combat.Player);
                break;
            case TargetType.EnemySingle:
                targets.Add(target.Instance);
                break;
            case TargetType.EnemyAll:
                targets.AddRange(context.Combat.Enemies);
                break;
        }

        return targets;
    }
    private void DrawCard(EventContext context, MotionContext motion)
    {
        if (hand.Count == 10)
        {
            return;
        }

        ChargeCard(context: context, motion: motion);

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
        eventBus.Publish<CardDrawed>(new CardDrawed(
            context: context.RewriteNew(this),
            motion: motion
        ));
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
        eventBus.Publish<CardDiscarded>(new CardDiscarded(
            context: context.RewriteNew(this),
            motion: motion
        ));
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
		eventBus.Publish<CardDiscarded>(new CardDiscarded(
            context: context.RewriteNew(this),
            motion: motion
        ));
    }
    private void ExhaustCard(CardView cardView, EventContext context, MotionContext motion)
    {
        // Instance
        hand.Remove(cardView.CardInstance);
        exhaustPile.Add(cardView.CardInstance);
        // Animation
        motion.AddTask(new MotionTask(
            priority: MotionPriority.Card,
            command: () => ExhaustCardCor(cardView),
            source: this
        ));
        // Event
        eventBus.Publish<CardExhausted>(new CardExhausted(
            context: context.RewriteNew(this),
            motion: motion
        ));
    }
    private void ChargeCard(EventContext context, MotionContext motion)
    {
        if (drawPile.Count > 0)
        {
            return;
        }

        drawPile.AddRange(Utils.Shuffle(discardPile));
        discardPile.Clear();

        UpdateUI();

        eventBus.Publish<CardCharged>(new CardCharged(
            context: context.RewriteNew(this),
            motion: motion
        ));
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

    IEnumerator ExhaustCardCor(CardView cardView)
    {
        cardHand.DestroyCard(cardView);

        yield return cardView.Exhaust().WaitForCompletion();

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
        exhaustPileText.text = exhaustPile.Count.ToString();
    }
    public void AddCard(CardSO card)
    {
        PlayManager.Instance.CurrentData.AddCard(card);

        //eventBus.Publish<CardAdded>(new CardAdded(
        //    context: new EventContext(this, null, null, null), 
        //    motion: null,
        //    source: card
        //));
    }
}

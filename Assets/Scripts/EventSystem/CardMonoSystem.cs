using DG.Tweening;
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
    private AnimationMonoSystem animationSystem;
    private EntityInstance player;

    [SerializeField] private CardContainer cardHand;
    [SerializeField] private CardViewPool cardViewPool;

    [SerializeField] private TextMeshProUGUI drawPileText;
    [SerializeField] private TextMeshProUGUI discardPileText;

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
        AnimationMonoSystem animationSystem,
        IEnumerable<CardInstance> cardInstances,
        EntityInstance player
    )
    {
        this.eventBus = eventBus;
        this.combatSystem = combatSystem;
        this.actionSystem = actionSystem;
        this.animationSystem = animationSystem;
        this.player = player;

        drawPile.AddRange(Utils.Shuffle(cardInstances));

        UpdateUI();
    }
    public void PlayCardStart(CardView cardView, ITargetable target)
    {
        this.cardInstance = cardView.CardInstance;
        this.target = target;

        ActionContext actionContext = new ActionContext(source: this, type: ActionType.PlayerCardPlay);

        actionSystem.ExcuteAction(actionContext, (eventContext) =>
        {
            eventBus.Publish<CardPlayDeclared>(new CardPlayDeclared(
                context: eventContext,
                cardView: cardView
            ));

            RequestContext requestContext = new RequestContext(source: this);

            eventBus.Publish<EnergyChangeRequested>(new EnergyChangeRequested(
                context: eventContext,
                request: requestContext,
                amount: -cardInstance.CostForTurn
            ));

            if (requestContext.isResult)
            {
                DiscardCard(cardView, eventContext);
            }
            else
            {
                cardView = null;
                cardInstance = null;
                target = null;
            }
        });
    }
    public void OnCombatEnded(CombatEnded e)
    {
        if (e.Result != CombatState.Victory)
        {
            return;
        }

        //EventContext eventContext = CreateContext(e.Context);
        //ClearCardHand(eventContext);
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e) 
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        for (int i = 0; i < 5; i++)
        {
            DrawCard(e.Context);
        }
    }
    public void OnPlayerTurnEnded(PlayerTurnEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EventContext eventContext = CreateContext(e.Context);

        ClearCardHand(eventContext);
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

            for (int i = 0; i < targets.Count; i++)
            {
                for (int j = 0; j < ce.repeat; j++)
                {
                    EntityInstance target = targets[i];

                    EventContext context = CreateContext(e.Context);

                    if (ce.effectType == EffectType.Attack)
                    {
                        eventBus.Publish<AttackDeclared>(new AttackDeclared(
                            context: context,
                            source: player,
                            target: target,
                            amount: ce.value
                        ));
                    }
                    else if (ce.effectType == EffectType.Block)
                    {
                        eventBus.Publish<BlockDeclared>(new BlockDeclared(
                            context: context,
                            source: player,
                            target: player,
                            amount: ce.value
                        ));
                    }
                    else if (ce.effectType == EffectType.DrawCard)
                    {
                        eventBus.Publish<DrawCardDeclared>(new DrawCardDeclared(context: context));
                    }
                    else if (ce.effectType == EffectType.GainEnergy)
                    {
                        eventBus.Publish<GainEnergyDeclared>(new GainEnergyDeclared(context: context));
                    }
                    else if (ce.effectType == EffectType.ModifyCost)
                    {
                        eventBus.Publish<ModifyCostDeclared>(new ModifyCostDeclared(context: context));
                    }
                    else if (ce.effectType == EffectType.Strengthen)
                    {
                        eventBus.Publish<BuffDeclared>(new BuffDeclared(
                            context: context,
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
    private void DrawCard(EventContext context)
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
        animationSystem.Enqueue(() => DrawCardCor(cardInstance));

        // Event
        EventContext _context = CreateContext(context);
        eventBus.Publish<CardDrawed>(new CardDrawed(context: _context));
    }
    private void DiscardCard(CardView cardView, EventContext context)
    {
        // Instance
        hand.Remove(cardView.CardInstance);
        discardPile.Add(cardView.CardInstance);

        // CardView
        cardHand.DestroyCard(cardView);
        cardViewPool.Push(cardView);

        // Animation
        cardView.transform.parent = cardViewPool.transform;
        cardView.Discard();

        UpdateUI();

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

    private void ClearCardHand(EventContext context)
    {
        int count = cardViewPool.Actives.Count;
        List<CardView> views = cardViewPool.Actives.ToList();
        for (int i = 0; i < count; i++)
        {
            DiscardCard(views[i], context);
        }
    }
    IEnumerator DrawCardCor(CardInstance cardInstance)
    {
        CardView cardView = cardViewPool.Pop();
        cardView.Init(
            cardInstance: cardInstance,
            cardSystem: this,
            pool: cardViewPool,
            cardContainer: cardHand
        );

        //yield return cardView.Draw().WaitForCompletion();
        cardView.Draw();
        yield return new WaitForSeconds(0.1f);
        UpdateUI();
    }
    public void UpdateUI()
    {
        drawPileText.text = drawPile.Count.ToString();
        discardPileText.text = discardPile.Count.ToString();
    }
}

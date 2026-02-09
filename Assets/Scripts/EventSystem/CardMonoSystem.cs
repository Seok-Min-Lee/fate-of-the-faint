using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardMonoSystem : MonoBehaviour
{
    private EventBus eventBus;
    private CombatSystem combatSystem;
    private ActionSystem actionSystem;
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
        IEnumerable<CardInstance> cardInstances,
        EntityInstance player
    )
    {
        this.eventBus = eventBus;
        this.combatSystem = combatSystem;
        this.actionSystem = actionSystem;
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

        EventContext eventContext = CreateContext(e.Context);
        ClearCardHand(eventContext);
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e) 
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < 5; i++)
        {
            sequence.AppendCallback(() => DrawCard(e.Context));
            sequence.AppendInterval(0.1f);
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
                            amount: -1
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
                        eventBus.Publish<StrengthenDeclared>(new StrengthenDeclared(context: context));
                    }
                    else if (ce.effectType == EffectType.Weaken)
                    {
                        eventBus.Publish<WeakenDeclared>(new WeakenDeclared(context: context));
                    }
                    else if (ce.effectType == EffectType.Vulnerable)
                    {
                        eventBus.Publish<VulnerableDeclared>(new VulnerableDeclared(context: context));
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
    }
    public void OnDamageResolved(DamageResolved e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

    }
    private void DrawCard(EventContext context)
    {
        if (hand.Count == 10)
        {
            return;
        }

        ChargeCard(context: context);

        CardInstance cardInstance = drawPile.FirstOrDefault();
        drawPile.Remove(cardInstance);
        hand.Add(cardInstance);

        CardView cardView = cardViewPool.Pop(cardInstance.Origin.Type);
        cardView.Init(cardInstance, this, cardViewPool);
        cardView.transform.parent = cardHand.transform;

        UpdateUI();

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

        cardView.transform.parent = cardViewPool.transform;
        cardView.Discard();

        UpdateUI();

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
    public void UpdateUI()
    {
        drawPileText.text = drawPile.Count.ToString();
        discardPileText.text = discardPile.Count.ToString();
    }
    private EventContext CreateContext(EventContext context)
    {
        return new EventContext(
            source: this,
            action: context.Action,
            turn: context.Turn,
            combat: context.Combat
        );
    }
}

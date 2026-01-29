using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardSystem : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private Player player;
    [SerializeField] private Enemy tempTarget;

    [SerializeField] private CardContainer cardHand;
    [SerializeField] private CardViewPool cardViewPool;

    [SerializeField] private TextMeshProUGUI drawPileText;
    [SerializeField] private TextMeshProUGUI discardPileText;

    [SerializeField] CardSO[] cardSOs;

    public List<CardInstance> drawPile = new List<CardInstance>();
    public List<CardInstance> hand = new List<CardInstance>();
    public List<CardInstance> discardPile = new List<CardInstance>();
    public List<CardInstance> exhaustPile = new List<CardInstance>();

    private CardView cardView;
    private CardInstance cardInstance;
    private void Start()
    {
        foreach (CardSO so in Utils.Shuffle(cardSOs))
        {
            drawPile.Add(new CardInstance(
                instanceId: "", 
                baseDef: so, 
                costForTurn: so.Cost, 
                costForCombat: so.Cost
            ));
        }

        UpdateUI();
    }
    private void OnEnable()
    {
        combatManager.EventBus.Subscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        combatManager.EventBus.Subscribe<EnergyResolved>(OnEnergyResolved);
        combatManager.EventBus.Subscribe<DamageResolved>(OnDamageResolved);
        combatManager.EventBus.Subscribe<PlayerTurnEnded>(OnPlayerTurnEnded);
    }
    private void OnDisable()
    {
        combatManager.EventBus.Unsubscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        combatManager.EventBus.Unsubscribe<EnergyResolved>(OnEnergyResolved);
        combatManager.EventBus.Unsubscribe<DamageResolved>(OnDamageResolved);
        combatManager.EventBus.Unsubscribe<PlayerTurnEnded>(OnPlayerTurnEnded);
    }
    public void PlayCardStart(CardView cardView)
    {
        this.cardView = cardView;
        this.cardInstance = cardView.CardInstance;

        ActionContext actionContext = new ActionContext(source: player, type: ActionType.PlayerCardPlay);

        combatManager.ExcuteAction(actionContext, (eventContext) =>
        {
            combatManager.EventBus.Publish<CardPlayDeclared>(new CardPlayDeclared(
                context: eventContext,
                cardView: cardView
            ));

            RequestContext requestContext = new RequestContext(source: this);

            combatManager.EventBus.Publish<EnergyChangeRequested>(new EnergyChangeRequested(
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
            }
        });
    }
    private void OnPlayerTurnStarted(PlayerTurnStarted e) 
    {
        DG.Tweening.Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < 5; i++)
        {
            sequence.AppendCallback(() => DrawCard(e.Context));
            sequence.AppendInterval(0.1f);
        }
    }
    private void OnPlayerTurnEnded(PlayerTurnEnded e)
    {
        int count = cardViewPool.Actives.Count;
        List<CardView> views = cardViewPool.Actives.ToList();
        for (int i = 0; i < count; i++)
        {
            DiscardCard(views[i], e.Context);
        }
    }
    private void OnEnergyResolved(EnergyResolved e)
    {
        foreach (CardEffect ce in cardInstance.BaseDef.Effects)
        {
            EventContext context = new EventContext(
                source: this,
                action: e.Context.Action,
                turn: e.Context.Turn,
                combat: e.Context.Combat
            );

            if (ce.effectType == EffectType.Attack)
            {
                combatManager.EventBus.Publish<AttackDeclared>(new AttackDeclared(
                    context: context,
                    source: player,
                    target: tempTarget,
                    amount: ce.value
                ));
            }
            else if (ce.effectType == EffectType.Shield)
            {
                combatManager.EventBus.Publish<ShieldDeclared>(new ShieldDeclared(context: context));
            }
            else if (ce.effectType == EffectType.DrawCard)
            {
                combatManager.EventBus.Publish<DrawCardDeclared>(new DrawCardDeclared(context: context));
            }
            else if (ce.effectType == EffectType.GainEnergy)
            {
                combatManager.EventBus.Publish<GainEnergyDeclared>(new GainEnergyDeclared(context: context));
            }
            else if (ce.effectType == EffectType.ModifyCost)
            {
                combatManager.EventBus.Publish<ModifyCostDeclared>(new ModifyCostDeclared(context: context));
            }
            else if (ce.effectType == EffectType.Strengthen)
            {
                combatManager.EventBus.Publish<StrengthenDeclared>(new StrengthenDeclared(context: context));
            }
            else if (ce.effectType == EffectType.Weaken)
            {
                combatManager.EventBus.Publish<WeakenDeclared>(new WeakenDeclared(context: context));
            }
            else if (ce.effectType == EffectType.Vulnerable)
            {
                combatManager.EventBus.Publish<VulnerableDeclared>(new VulnerableDeclared(context: context));
            }
            else
            {
                return;
            }
        }
    }
    private void OnDamageResolved(DamageResolved e)
    {
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

        CardView cardView = cardViewPool.Pop(cardInstance.BaseDef.Type);
        cardView.Init(cardInstance, this, cardViewPool);
        cardView.transform.parent = cardHand.transform;

        UpdateUI();

        EventContext _context = new EventContext(
            source: this,
            action: context.Action,
            turn: context.Turn,
            combat: context.Combat
        );
        combatManager.EventBus.Publish<CardDrawed>(new CardDrawed(context: _context));
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
        cardView.gameObject.SetActive(false);

        UpdateUI();

        EventContext _context = new EventContext(
            source: this,
            action: context.Action,
            turn: context.Turn,
            combat: context.Combat
        );
        combatManager.EventBus.Publish<CardDiscarded>(new CardDiscarded(context: _context));
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

        EventContext _context = new EventContext(
            source: this,
            action: context.Action,
            turn: context.Turn,
            combat: context.Combat
        );
        combatManager.EventBus.Publish<CardCharged>(new CardCharged(context: _context));
    }

    public void UpdateUI()
    {
        drawPileText.text = drawPile.Count.ToString();
        discardPileText.text = discardPile.Count.ToString();
    }
}

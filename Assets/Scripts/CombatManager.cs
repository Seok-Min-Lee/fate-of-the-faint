using JetBrains.Annotations;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public DamageSystem DamageSystem { get; private set; }
    public EnergySystem EnergySystem { get; private set; }
    public TurnSystem TurnSystem { get; private set; }
    public CardSystem CardSystem { get; private set; }
    public UISystem UISystem { get; private set; }

    public EventBus EventBus = new EventBus();

    public CombatContext CombatContext { get; private set; }

    private void Awake()
    {
        DamageSystem = GetComponent<DamageSystem>();
        EnergySystem = GetComponent<EnergySystem>();
        TurnSystem = GetComponent<TurnSystem>();
        CardSystem = GetComponent<CardSystem>();
        UISystem = GetComponent<UISystem>();
    }
    private void Start()
    {
        CombatStart();
        TryPlayerTurnStart();
    }
    private void OnEnable()
    {
        EventBus.Subscribe<CombatEndRequested>(OnCombatEndRequested);
        EventBus.Subscribe<EnemyTurnEnded>(OnEnemyTurnEnded);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<CombatEndRequested>(OnCombatEndRequested);
        EventBus.Unsubscribe<EnemyTurnEnded>(OnEnemyTurnEnded);
    }
    private void OnCombatEndRequested(CombatEndRequested e)
    {
        if (CombatContext.CombatId != e.Context.Combat.CombatId)
        {
            e.Request.isResult = false;
            return;
        }
        e.Request.isResult = true;

        CombatContext = null;

        EventContext eventContext = new EventContext(
            source: this, 
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );
        EventBus.Publish<CombatEnded>(new CombatEnded(eventContext));
    }
    private void OnEnemyTurnEnded(EnemyTurnEnded e)
    {
        TryPlayerTurnStart();
    }
    private void TryPlayerTurnStart()
    {
        EventContext eventContext = new EventContext(
            source: this,
            action: null,
            turn: TurnSystem.TurnContext,
            combat: CombatContext
        );
        RequestContext requestContext = new RequestContext(source: this);

        EventBus.Publish<PlayerTurnStartRequested>(new PlayerTurnStartRequested(
            context: eventContext,
            request: requestContext
        ));
    }
    private void CombatStart()
    {
        CombatContext = new CombatContext(combatId: 0, source: this);

        EventContext eventContext = new EventContext(
            source: this, 
            action: null,
            turn: null,
            combat: CombatContext
        );
        EventBus.Publish<CombatStarted>(new CombatStarted(eventContext));
    }
    public void ExcuteAction(ActionContext action, System.Action<EventContext> declared)
    {
        EventContext eventContext = new EventContext(
            source: action.Source,
            action: action,
            turn: TurnSystem.TurnContext,
            combat: CombatContext
        );

        EventBus.Publish<ActionStarted>(new ActionStarted(eventContext));

        declared?.Invoke(eventContext);

        EventBus.Publish<ActionEnded>(new ActionEnded(eventContext));
    }
}
public class CombatContext
{
    public CombatContext(int combatId, object source)
    {
        CombatId = combatId;
        Source = source;
    }
    public int CombatId { get; set; }
    public object Source { get; private set; }
}

using System;
using System.Collections.Generic;
using System.Linq;

public class TurnSystem
{
    private readonly EventBus eventBus;
    public TurnSystem(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    public TurnContext TurnContext { get; private set; }
    private int turnId = 0;
    private List<EnemyInstance> enemies;
    private Queue<Action> eventQueue = new Queue<Action>();
    public void Init(IEnumerable<EnemyInstance> enemies)
    {
        this.enemies = new List<EnemyInstance>(enemies);
    }
    private int unit = 0;
    public void UpdateTick()
    {
        if (++unit < 30)
        {
            return;
        }

        if (eventQueue.Count > 0)
        {
            eventQueue.Dequeue().Invoke();
        }

        unit = 0;
    }
    public void OnCombatStarted(CombatStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        PublishPlayerTurnStarted(e);
    }
    public void OnActionEnded(ActionEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (TurnContext.Phase == TurnPhase.Player)
        {
            if (e.Context.Action.Type == ActionType.PlayerTurnEnd)
            {
                eventQueue.Enqueue(() => PublishEnemyTurnStarted(e));
            }
            else if (e.Context.Action.Type == ActionType.PlayerCardPlay)
            {

            }
        }
        else
        {
            if (e.Context.Action.Type == ActionType.EnemyAct)
            {
                if (TurnContext.EnemyQueue.Count > 0)
                {
                    eventQueue.Enqueue(() => PublishEnemyActionStartRequested(e));
                }
                else
                {
                    eventQueue.Enqueue(() => PublishEnemyTurnEnded(e));
                }
            }
        }
    }
    public void OnPlayerTurnEndRequested(PlayerTurnEndRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        e.Request.isResult = true;

        EventContext eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );
        eventBus.Publish<PlayerTurnEnded>(new PlayerTurnEnded(context: eventContext));
    }
    public void OnEnemyTurnStarted(EnemyTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            if (!enemies[i].IsDead)
            {
                TurnContext.EnemyQueue.Enqueue(enemies[i]);
            }
        }

        EventContext eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );

        RequestContext request = new RequestContext(source: this);

        eventBus.Publish<EnemyActionStartRequested>(new EnemyActionStartRequested(
            context: eventContext,
            request: request,
            enemy: TurnContext.EnemyQueue.Dequeue()
        ));
    }
    private void PublishPlayerTurnStarted(ICombatEvent e)
    {
        TurnContext = new TurnContext(
            turnId: ++turnId,
            phase: TurnPhase.Player,
            source: this
        );

        EventContext eventContext = new EventContext(
            source: this,
            action: null,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );
        eventBus.Publish<PlayerTurnStarted>(new PlayerTurnStarted(context: eventContext));
    }
    private void PublishEnemyTurnStarted(ICombatEvent e)
    {
        TurnContext = new TurnContext(
            turnId: ++turnId,
            phase: TurnPhase.Enemy,
            source: this
        );

        EventContext eventContext = new EventContext(
            source: this,
            action: null,
            turn: TurnContext,
            combat: e.Context.Combat
        );
        eventBus.Publish<EnemyTurnStarted>(new EnemyTurnStarted(context: eventContext));
    }
    private void PublishEnemyTurnEnded(ICombatEvent e)
    {
        EventContext eventContext = new EventContext(
            source: this,
            action: null,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );

        eventBus.Publish<EnemyTurnEnded>(new EnemyTurnEnded(context: eventContext));

        eventQueue.Enqueue(() => PublishPlayerTurnStarted(e));
    }
    private void PublishEnemyActionStartRequested(ICombatEvent e)
    {
        EventContext eventContext = new EventContext(
            source: this,
            action: null,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );

        RequestContext request = new RequestContext(source: this);

        eventBus.Publish<EnemyActionStartRequested>(new EnemyActionStartRequested(
            context: eventContext,
            request: request,
            enemy: TurnContext.EnemyQueue.Dequeue()
        ));
    }
}
public class TurnContext
{
    public TurnContext(int turnId, TurnPhase phase, object source)
    {
        TurnId = turnId;
        Phase = phase;
        Source = source;
    }
    public int TurnId { get; private set; }
    public TurnPhase Phase { get; private set; }
    public object Source { get; private set; }

    public Queue<EnemyInstance> EnemyQueue { get; private set; } = new Queue<EnemyInstance>();
}
public enum TurnPhase
{
    Player,
    Enemy
}

using System.Collections.Generic;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private Enemy[] enemies;

    public TurnContext TurnContext { get; private set; }
    private int turnId = 0;

    private void OnEnable()
    {
        combatManager.EventBus.Subscribe<ActionEnded>(OnActionEnded);
        combatManager.EventBus.Subscribe<PlayerTurnStartRequested>(OnPlayerTurnStartRequested);
        combatManager.EventBus.Subscribe<PlayerTurnEndRequested>(OnPlayerTurnEndRequested);
        combatManager.EventBus.Subscribe<EnemyTurnStartRequested>(OnEnemyTurnStartRequested);
        combatManager.EventBus.Subscribe<EnemyTurnStarted>(OnEnemyTurnStarted);
        combatManager.EventBus.Subscribe<EnemyTurnEndRequested>(OnEnemyTurnEndRequested);
    }
    private void OnDisable()
    {
        combatManager.EventBus.Unsubscribe<ActionEnded>(OnActionEnded);
        combatManager.EventBus.Unsubscribe<PlayerTurnStartRequested>(OnPlayerTurnStartRequested);
        combatManager.EventBus.Unsubscribe<PlayerTurnEndRequested>(OnPlayerTurnEndRequested);
        combatManager.EventBus.Unsubscribe<EnemyTurnStartRequested>(OnEnemyTurnStartRequested);
        combatManager.EventBus.Unsubscribe<EnemyTurnStarted>(OnEnemyTurnStarted);
        combatManager.EventBus.Unsubscribe<EnemyTurnEndRequested>(OnEnemyTurnEndRequested);
    }
    private void OnActionEnded(ActionEnded e)
    {
        if (TurnContext.Phase == TurnPhase.Player)
        {
            if (e.Context.Action.Type == ActionType.PlayerTurnEnd)
            {
                // enemy turn start
                RequestContext request = new RequestContext(source: this);
                combatManager.EventBus.Publish<EnemyTurnStartRequested>(new EnemyTurnStartRequested(e.Context, request));
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
                    EventContext eventContext = new EventContext(
                        source: this,
                        action: e.Context.Action,
                        turn: e.Context.Turn,
                        combat: e.Context.Combat
                    );

                    RequestContext request = new RequestContext(source: this);

                    combatManager.EventBus.Publish<EnemyActionStartRequested>(new EnemyActionStartRequested(
                        context: eventContext,
                        request: request,
                        enemy: TurnContext.EnemyQueue.Dequeue()
                    ));
                }
                else
                {
                    RequestContext request = new RequestContext(source: this);
                    combatManager.EventBus.Publish<EnemyTurnEndRequested>(new EnemyTurnEndRequested(e.Context, request));
                }
            }
        }
    }
    private void OnPlayerTurnStartRequested(PlayerTurnStartRequested e)
    {
        e.Request.isResult = true;

        TurnContext = new TurnContext(
            turnId: ++turnId,
            phase: TurnPhase.Player,
            source: this
        );

        EventContext eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );
        combatManager.EventBus.Publish<PlayerTurnStarted>(new PlayerTurnStarted(context: eventContext));
    }
    private void OnPlayerTurnEndRequested(PlayerTurnEndRequested e)
    {
        e.Request.isResult = true;

        EventContext eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );
        combatManager.EventBus.Publish<PlayerTurnEnded>(new PlayerTurnEnded(context: eventContext));
    }
    public void OnEnemyTurnStartRequested(EnemyTurnStartRequested e)
    {
        e.Request.isResult = true;

        TurnContext = new TurnContext(
            turnId: ++turnId,
            phase: TurnPhase.Enemy,
            source: this
        );

        EventContext eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );
        combatManager.EventBus.Publish<EnemyTurnStarted>(new EnemyTurnStarted(context: eventContext));
    }

    private void OnEnemyTurnEndRequested(EnemyTurnEndRequested e)
    {
        e.Request.isResult = true;

        EventContext eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );
        combatManager.EventBus.Publish<EnemyTurnEnded>(new EnemyTurnEnded(context: eventContext));
    }
    private void OnEnemyTurnStarted(EnemyTurnStarted e)
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (!enemies[i].IsDeath)
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

        combatManager.EventBus.Publish<EnemyActionStartRequested>(new EnemyActionStartRequested(
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

    public Queue<Enemy> EnemyQueue { get; private set; } = new Queue<Enemy>();
}
public enum TurnPhase
{
    Player,
    Enemy
}

using System;

public class ActionSystem : BaseSystem
{
    private readonly CombatSystem combatSystem;
    private readonly EventBus eventBus;

    private int actionNum = 0;
    public ActionSystem(EventBus eventBus, CombatSystem combatSystem) 
    {
        this.eventBus = eventBus;
        this.combatSystem = combatSystem;
    }
    public void ExcuteAction(object source, ActionType type, Action<EventContext, MotionContext> declared)
    {
        ActionContext action = new ActionContext(
            type: type, 
            source: source
        );

        EventContext eventContext = new EventContext(
            source: source,
            action: action,
            turn: combatSystem.TurnSystem.TurnContext,
            combat: combatSystem.CombatContext
        );

        MotionContext motionContext = new MotionContext(this);

        eventBus.Publish<ActionStarted>(new ActionStarted(
            context: eventContext.RewriteNew(this),
            motion: motionContext
        ));

        declared?.Invoke(
            arg1: eventContext.RewriteNew(source),
            arg2: motionContext
        );

        eventBus.Publish<ActionEnded>(new ActionEnded(
            context: eventContext.RewriteNew(this),
            motion: motionContext
        ));

        combatSystem.AnimationSystem.Play(
            context: eventContext.RewriteNew(this),
            motion: motionContext
        );
    }
}
public class ActionContext
{
    public ActionContext(ActionType type, object source)
    {
        ActionId = Guid.NewGuid();
        Type = type;
        Source = source;
    }
    public Guid ActionId { get; private set; }
    public object Source { get; private set; }   // Card, Monster, Relic 등
    public ActionType Type { get; private set; }
    public bool isCancelled = false;
}
public enum ActionType
{
    PlayerCardPlay,
    PlayerTurnEnd,
    EnemyAct,
}

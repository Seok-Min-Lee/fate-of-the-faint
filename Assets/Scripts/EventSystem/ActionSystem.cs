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
        ActionContext action = new ActionContext(type: type, source: source);

        MotionContext motionContext = new MotionContext(this);

        eventBus.Publish<ActionStarted>(new ActionStarted(
            context:CreateContext(action),
            motion: motionContext
        ));

        declared?.Invoke(CreateContext(action), motionContext);

        eventBus.Publish<ActionEnded>(new ActionEnded(
            context: CreateContext(action),
            motion: motionContext
        ));

        combatSystem.AnimationSystem.Play(
            context: CreateContext(action),
            motion: motionContext
        );
    }
    private EventContext CreateContext(ActionContext action)
    {
        return new EventContext(
            source: action.Source,
            action: action,
            turn: combatSystem.TurnSystem.TurnContext,
            combat: combatSystem.CombatContext
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

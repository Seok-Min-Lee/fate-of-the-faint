using System;

public class ActionSystem : BaseSystem
{
    private readonly CombatSystem combatSystem;
    private readonly EventBus eventBus;
    public ActionSystem(EventBus eventBus, CombatSystem combatSystem) 
    {
        this.eventBus = eventBus;
        this.combatSystem = combatSystem;
    }
    public void ExcuteAction(ActionContext action, Action<EventContext> declared)
    {
        eventBus.Publish<ActionStarted>(new ActionStarted(CreateContext(action)));

        declared?.Invoke(CreateContext(action));

        eventBus.Publish<ActionEnded>(new ActionEnded(CreateContext(action)));

        combatSystem.AnimationSystem.PlayQueue(CreateContext(action));
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
    public ActionContext(object source, ActionType type)
    {
        ActionId = Guid.NewGuid();
        Source = source;
        Type = type;
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

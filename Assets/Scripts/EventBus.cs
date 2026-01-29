using System;
using System.Collections.Generic;

public interface ICombatEvent 
{
    EventContext Context { get; }
    EventMeta Meta { get; }
}
public class EventBus
{
    public event Action<ICombatEvent> OnPublished;

    private readonly Dictionary<Type, Delegate> _handlers = new();

    public void Subscribe<T>(Action<T> handler) where T : ICombatEvent
    {
        var t = typeof(T);
        _handlers.TryGetValue(t, out var existing);
        _handlers[t] = (Action<T>)existing + handler;
    }

    public void Unsubscribe<T>(Action<T> handler) where T : ICombatEvent
    {
        var t = typeof(T);
        if (_handlers.TryGetValue(t, out var existing))
        {
            _handlers[t] = (Action<T>)existing - handler;
        }
    }

    public void Publish<T>(T evt) where T : ICombatEvent
    {
        OnPublished?.Invoke(evt);

        if (_handlers.TryGetValue(typeof(T), out var del))
        {
            ((Action<T>)del)?.Invoke(evt);
        }
    }
}
public struct CombatStarted : ICombatEvent
{
    public CombatStarted(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.CombatStarted;
}
public struct CombatEndRequested : ICombatEvent
{
    public CombatEndRequested(EventContext context, RequestContext request)
    {
        Context = context;
        Request = request;
    }
    public EventContext Context { get; private set; }
    public RequestContext Request { get; private set; }
    public EventMeta Meta => EventMetas.CombatEndRequested;
}
public struct CombatEnded : ICombatEvent
{
    public CombatEnded(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.CombatEnded;
}
public struct PlayerTurnStartRequested : ICombatEvent
{
    public PlayerTurnStartRequested(EventContext context, RequestContext request)
    {
        Context = context;
        Request = request;
    }
    public EventContext Context { get; private set; }
    public RequestContext Request { get; private set; }
    public EventMeta Meta => EventMetas.PlayerTurnStartRequested;
}
public struct PlayerTurnStarted : ICombatEvent
{
    public PlayerTurnStarted(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.PlayerTurnStarted;
}
public struct PlayerTurnEndRequested : ICombatEvent
{
    public PlayerTurnEndRequested(EventContext context, RequestContext request)
    {
        Context = context;
        Request = request;
    }
    public EventContext Context { get; private set; }
    public RequestContext Request { get; private set; }
    public EventMeta Meta => EventMetas.PlayerTurnEndRequested;
}
public struct PlayerTurnEnded : ICombatEvent
{
    public PlayerTurnEnded(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.PlayerTurnEnded;
}
public struct EnemyTurnStartRequested : ICombatEvent
{
    public EnemyTurnStartRequested(EventContext context, RequestContext request)
    {
        Context = context;
        Request = request;
    }
    public EventContext Context { get; private set; }
    public RequestContext Request { get; private set; }
    public EventMeta Meta => EventMetas.EnemyTurnStartRequested;
}
public struct EnemyTurnStarted : ICombatEvent
{
    public EnemyTurnStarted(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.EnemyTurnStarted;
}
public struct EnemyTurnEndRequested : ICombatEvent
{
    public EnemyTurnEndRequested(EventContext context, RequestContext request)
    {
        Context = context;
        Request = request;
    }
    public EventContext Context { get; private set; }
    public RequestContext Request { get; private set; }
    public EventMeta Meta => EventMetas.EnemyTurnEndRequested;
}
public struct EnemyTurnEnded : ICombatEvent
{
    public EnemyTurnEnded(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.EnemyTurnEnded;
}
public struct EnemyActionStartRequested : ICombatEvent
{
    public EnemyActionStartRequested(EventContext context, RequestContext request, Enemy enemy)
    {
        Context = context;
        Request = request;
        Enemy = enemy;
    }
    public EventContext Context { get; private set; }
    public RequestContext Request { get; private set; }
    public Enemy Enemy { get; private set; }
    public EventMeta Meta => EventMetas.EnemyActionStartRequested;
}
public struct ActionStarted : ICombatEvent
{
    public ActionStarted(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.ActionStarted;
}
public struct ActionEnded : ICombatEvent
{
    public ActionEnded(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.ActionEnded;
}
public struct EnergyChangeRequested : ICombatEvent
{
    public EnergyChangeRequested(EventContext context, RequestContext request, int amount)
    {
        Context = context;
        Request = request;
        Amount = amount;
    }
    public EventContext Context { get; private set; }
    public RequestContext Request { get; private set; }
    public int Amount { get; private set; }
    public EventMeta Meta => EventMetas.EnergyChangeRequested;
}
public struct EnergyResolved : ICombatEvent
{
    public EnergyResolved(EventContext context, bool result)
    {
        Context = context;
        Result = result;
    }
    public EventContext Context { get; private set; }
    public bool Result { get; private set; }
    public EventMeta Meta => EventMetas.EnergyResolved;
}
public struct EnergyChanged : ICombatEvent
{
    public EnergyChanged(EventContext context, int startAmount, int endAmount)
    {
        Context = context;
        StartAmount = startAmount;
        EndAmount = endAmount;
    }
    public EventContext Context { get; private set; }
    public int StartAmount { get; private set; }
    public int EndAmount { get; private set; }
    public EventMeta Meta => EventMetas.EnergyChanged;
}
public struct CardDrawed : ICombatEvent
{
    public CardDrawed(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.CardDrawed;
}
public struct CardDiscarded : ICombatEvent
{
    public CardDiscarded(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.CardDiscarded;
}
public struct CardCharged : ICombatEvent
{
    public CardCharged(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.CardCharged;
}
public struct CardPlayDeclared : ICombatEvent
{
    public CardPlayDeclared(EventContext context, CardView cardView)
    {
        Context = context;
        CardView = cardView;
    }
    public EventContext Context { get; private set; }
    public CardView CardView { get; private set; }
    public EventMeta Meta => EventMetas.CardPlayDeclared;
}
public struct AttackDeclared : ICombatEvent
{
    public AttackDeclared(EventContext context, Entity source, Entity target, int amount)
    {
        Context = context;
        Source = source;
        Target = target;
        Amount = amount;
    }
    public EventContext Context { get; private set; }
    public Entity Source { get; private set; }
    public Entity Target { get; private set; }
    public int Amount { get; private set; }
    public EventMeta Meta => EventMetas.AttackDeclared;
}
public struct ShieldDeclared : ICombatEvent
{
    public ShieldDeclared(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.ShieldDeclared;
}
public struct DrawCardDeclared : ICombatEvent
{
    public DrawCardDeclared(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.DrawCardDeclared;
}
public struct GainEnergyDeclared : ICombatEvent
{
    public GainEnergyDeclared(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.GainEnergyDeclared;
}
public struct ModifyCostDeclared : ICombatEvent
{
    public ModifyCostDeclared(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.ModifyCostDeclared;
}
public struct StrengthenDeclared : ICombatEvent
{
    public StrengthenDeclared(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.StrengthenDeclared;
}
public struct WeakenDeclared : ICombatEvent
{
    public WeakenDeclared(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.WeakenDeclared;
}
public struct VulnerableDeclared : ICombatEvent
{
    public VulnerableDeclared(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.VulnerableDeclared;
}
public struct DamageRequested : ICombatEvent
{
    public DamageRequested(EventContext context, DamageContext damage)
    {
        Context = context;
        Damage = damage;
    }
    public EventContext Context { get; private set; }
    public DamageContext Damage { get; private set; }
    public EventMeta Meta => EventMetas.DamageRequested;
}
public struct DamageResolved : ICombatEvent
{
    public DamageResolved(EventContext context, Entity source, Entity target, int amount)
    {
        Context = context;
        Source = source;
        Target = target;
        Amount = amount;
    }
    public EventContext Context { get; private set; }
    public Entity Source { get; private set; }
    public Entity Target { get; private set; }
    public int Amount { get; private set; }
    public EventMeta Meta => EventMetas.DamageResolved;
}
public struct HpChanged : ICombatEvent
{
    public HpChanged(EventContext context, Entity target, int startAmount, int endAmount)
    {
        Context = context;
        Target = target;
        StartAmount = startAmount;
        EndAmount = endAmount;
    }
    public EventContext Context { get; private set; }
    public Entity Target { get; private set; }
    public int StartAmount { get; private set; }
    public int EndAmount { get; private set; }
    public EventMeta Meta => EventMetas.HpChanged;
}
public struct DeathDeclared : ICombatEvent
{
    public DeathDeclared(EventContext context, Entity target)
    {
        Context = context;
        Target = target;
    }
    public EventContext Context { get; private set; }
    public Entity Target { get; private set; }
    public EventMeta Meta => EventMetas.DeathDeclared;
}
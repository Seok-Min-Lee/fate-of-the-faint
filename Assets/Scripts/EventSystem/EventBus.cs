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
public struct CombatEnded : ICombatEvent
{
    public CombatEnded(EventContext context, CombatState result)
    {
        Context = context;
        Result = result;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.CombatEnded;
    public CombatState Result;
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
public struct EnemyTurnStarted : ICombatEvent
{
    public EnemyTurnStarted(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.EnemyTurnStarted;
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
    public EnemyActionStartRequested(EventContext context, RequestContext request, EnemyInstance enemy)
    {
        Context = context;
        Request = request;
        Enemy = enemy;
    }
    public EventContext Context { get; private set; }
    public RequestContext Request { get; private set; }
    public EnemyInstance Enemy { get; private set; }
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
    public EnergyChanged(EventContext context, int startAmount, int endAmount, int maxAmount)
    {
        Context = context;
        StartAmount = startAmount;
        EndAmount = endAmount;
        MaxAmount = maxAmount;
    }
    public EventContext Context { get; private set; }
    public int StartAmount { get; private set; }
    public int EndAmount { get; private set; }
    public int MaxAmount { get; private set; }
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
    public AttackDeclared(EventContext context, EntityInstance source, EntityInstance target, int amount, int repeat)
    {
        Context = context;
        Source = source;
        Target = target;
        Amount = amount;
        Repeat = repeat;
    }
    public EventContext Context { get; private set; }
    public EntityInstance Source { get; private set; }
    public EntityInstance Target { get; private set; }
    public int Amount { get; private set; }
    public int Repeat { get; private set; }
    public EventMeta Meta => EventMetas.AttackDeclared;
}
public struct BlockDeclared : ICombatEvent
{
    public BlockDeclared(EventContext context, EntityInstance source, EntityInstance target, int amount)
    {
        Context = context;
        Source = source;
        Target = target;
        Amount = amount;
    }
    public EventContext Context { get; private set; }
    public EntityInstance Source { get; private set; }
    public EntityInstance Target { get; private set; }
    public int Amount { get; private set; }
    public EventMeta Meta => EventMetas.BlockDeclared;
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
    public GainEnergyDeclared(EventContext context, int amount)
    {
        Context = context;
        Amount = amount;
    }
    public EventContext Context { get; private set; }
    public int Amount { get; private set; }
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
public struct BuffDeclared : ICombatEvent
{
    public BuffDeclared(EventContext context, EntityInstance source, EntityInstance target, BuffType type, int amount)
    {
        Context = context;
        Source = source;
        Target = target;
        Type = type;
        Amount = amount;
    }
    public EventContext Context { get; private set; }
    public EntityInstance Source { get; private set; }
    public EntityInstance Target { get; private set; }
    public BuffType Type { get; private set; }
    public int Amount { get ; private set; }
    public EventMeta Meta => EventMetas.BuffDeclared;
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
    public DamageResolved(EventContext context, EntityInstance source, EntityInstance target, int amount, int repeat)
    {
        Context = context;
        Source = source;
        Target = target;
        Amount = amount;
        Repeat = repeat;
    }
    public EventContext Context { get; private set; }
    public EntityInstance Source { get; private set; }
    public EntityInstance Target { get; private set; }
    public int Amount { get; private set; }
    public int Repeat { get; private set; }
    public EventMeta Meta => EventMetas.DamageResolved;
}
public struct BuffRequested : ICombatEvent
{
    public BuffRequested(EventContext context, BuffContext buff)
    {
        Context = context;
        Buff = buff;
    }
    public EventContext Context { get; private set; }
    public BuffContext Buff { get; private set; }
    public EventMeta Meta => EventMetas.BuffRequested;
}
public struct BuffResolved : ICombatEvent
{
    public BuffResolved(EventContext context, EntityInstance source, EntityInstance target, BuffType type, int amount)
    {
        Context = context;
        Source = source;
        Target = target;
        Type = type;
        Amount = amount;
    }
    public EventContext Context { get; private set; }
    public EntityInstance Source { get; private set; }
    public EntityInstance Target { get; private set; }
    public BuffType Type { get; private set; }
    public int Amount { get; private set; }
    public EventMeta Meta => EventMetas.BuffResolved;
}
public struct HpChanged : ICombatEvent
{
    public HpChanged(EventContext context, EntityInstance target, int startAmount, int endAmount)
    {
        Context = context;
        Target = target;
        StartAmount = startAmount;
        EndAmount = endAmount;
    }
    public EventContext Context { get; private set; }
    public EntityInstance Target { get; private set; }
    public int StartAmount { get; private set; }
    public int EndAmount { get; private set; }
    public EventMeta Meta => EventMetas.HpChanged;
}
public struct BlockChanged : ICombatEvent
{
    public BlockChanged(EventContext context, EntityInstance target, int startAmount, int endAmount)
    {
        Context = context;
        Target = target;
        StartAmount = startAmount;
        EndAmount = endAmount;
    }
    public EventContext Context { get; private set; }
    public EntityInstance Target { get; private set; }
    public int StartAmount { get; private set; }
    public int EndAmount { get; private set; }
    public EventMeta Meta => EventMetas.BlockChanged;
}
public struct BuffChanged : ICombatEvent
{
    public BuffChanged(EventContext context, EntityInstance target, BuffType type, int startAmount, int endAmount)
    {
        Context = context;
        Target = target;
        Type = type;
        StartAmount = startAmount;
        EndAmount = endAmount;
    }
    public EventContext Context { get; private set; }
    public EntityInstance Target { get; private set; }
    public BuffType Type { get; private set; }
    public int StartAmount { get; private set; }
    public int EndAmount { get; private set; }
    public EventMeta Meta => EventMetas.BuffChanged;
}
public struct DeathDeclared : ICombatEvent
{
    public DeathDeclared(EventContext context, EntityInstance target)
    {
        Context = context;
        Source = target;
    }
    public EventContext Context { get; private set; }
    public EntityInstance Source { get; private set; }
    public EventMeta Meta => EventMetas.DeathDeclared;
}
public struct AnimationStarted : ICombatEvent
{
    public AnimationStarted(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.AnimationStarted;
}
public struct AnimationEnded : ICombatEvent
{
    public AnimationEnded(EventContext context)
    {
        Context = context;
    }
    public EventContext Context { get; private set; }
    public EventMeta Meta => EventMetas.AnimationEnded;
}
public struct EnemyIntentDecided : ICombatEvent
{
    public EnemyIntentDecided(EventContext context, EnemyInstance source)
    {
        Context = context;
        Source = source;
    }
    public EventContext Context { get; private set; }
    public EnemyInstance Source { get; private set; }
    public EventMeta Meta => EventMetas.EnemyIntentDecided;
}
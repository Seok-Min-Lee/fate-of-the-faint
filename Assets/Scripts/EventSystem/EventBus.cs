using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;

public interface ICombatEvent 
{
    EventContext Context { get; }
    EventMeta Meta { get; }
}
public class EventBus
{
    public event Action<ICombatEvent> OnPublished;
    public IReadOnlyDictionary<Type, int> PublishCounter => _publishCounter;

    private readonly Dictionary<Type, Delegate> _handlers = new();
    private readonly Dictionary<Type, int> _publishCounter = new();

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

        var t = typeof(T);
        if (_handlers.TryGetValue(t, out var del))
        {
            if (!_publishCounter.TryGetValue(t, out int value))
            {
                _publishCounter.Add(t, 0);
            }
            _publishCounter[t]++;

            ((Action<T>)del)?.Invoke(evt);
        }
    }
}
public struct CombatStarted : ICombatEvent
{
    public CombatStarted(EventContext context, MotionContext motion)
    {
        Context = context;
        Motion = motion;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EventMeta Meta => EventMetas.CombatStarted;
}
public struct CombatEnded : ICombatEvent
{
    public CombatEnded(EventContext context, MotionContext motion, CombatState result)
    {
        Context = context;
        Motion = motion;
        Result = result;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
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
    public PlayerTurnStarted(EventContext context, MotionContext motion)
    {
        Context = context;
        Motion = motion;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EventMeta Meta => EventMetas.PlayerTurnStarted;
}
public struct PlayerTurnEndRequested : ICombatEvent
{
    public PlayerTurnEndRequested(EventContext context, MotionContext motion, RequestContext request)
    {
        Context = context;
        Motion = motion;
        Request = request;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public RequestContext Request { get; private set; }
    public EventMeta Meta => EventMetas.PlayerTurnEndRequested;
}
public struct PlayerTurnEnded : ICombatEvent
{
    public PlayerTurnEnded(EventContext context, MotionContext motion)
    {
        Context = context;
        Motion = motion;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EventMeta Meta => EventMetas.PlayerTurnEnded;
}
public struct EnemyTurnStarted : ICombatEvent
{
    public EnemyTurnStarted(EventContext context, MotionContext motion)
    {
        Context = context;
        Motion = motion;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EventMeta Meta => EventMetas.EnemyTurnStarted;
}
public struct EnemyTurnEnded : ICombatEvent
{
    public EnemyTurnEnded(EventContext context, MotionContext motion)
    {
        Context = context;
        Motion = motion;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
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
    public ActionStarted(EventContext context, MotionContext motion)
    {
        Context = context;
        Motion = motion;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EventMeta Meta => EventMetas.ActionStarted;
}
public struct ActionEnded : ICombatEvent
{
    public ActionEnded(EventContext context, MotionContext motion)
    {
        Context = context;
        Motion = motion;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EventMeta Meta => EventMetas.ActionEnded;
}
public struct EnergyChangeRequested : ICombatEvent
{
    public EnergyChangeRequested(EventContext context, RequestContext request, MotionContext motion, int amount)
    {
        Context = context;
        Request = request;
        Motion = motion;
        Amount = amount;
    }
    public EventContext Context { get; private set; }
    public RequestContext Request { get; private set; }
    public MotionContext Motion { get; private set; }
    public int Amount { get; private set; }
    public EventMeta Meta => EventMetas.EnergyChangeRequested;
}
public struct EnergyChargeRequested : ICombatEvent
{
    public EnergyChargeRequested(EventContext context, MotionContext motion, EnergyContext energy)
    {
        Context = context;
        Motion = motion;
        Energy = energy;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EnergyContext Energy { get; private set; }
    public EventMeta Meta => EventMetas.EnergyChargeRequested;
}
public struct EnergyResolved : ICombatEvent
{
    public EnergyResolved(EventContext context, MotionContext motion)
    {
        Context = context;
        Motion = motion;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EventMeta Meta => EventMetas.EnergyResolved;
}
public struct EnergyChanged : ICombatEvent
{
    public EnergyChanged(EventContext context, MotionContext motion, int startAmount, int endAmount, int maxAmount)
    {
        Context = context;
        Motion = motion;
        StartAmount = startAmount;
        EndAmount = endAmount;
        MaxAmount = maxAmount;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public int StartAmount { get; private set; }
    public int EndAmount { get; private set; }
    public int MaxAmount { get; private set; }
    public EventMeta Meta => EventMetas.EnergyChanged;
}
public struct CardDrawed : ICombatEvent
{
    public CardDrawed(EventContext context, MotionContext motion)
    {
        Context = context;
        Motion = motion;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EventMeta Meta => EventMetas.CardDrawed;
}
public struct CardDiscarded : ICombatEvent
{
    public CardDiscarded(EventContext context, MotionContext motion)
    {
        Context = context;
        Motion = motion;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EventMeta Meta => EventMetas.CardDiscarded;
}
public struct CardExhausted : ICombatEvent
{
    public CardExhausted(EventContext context, MotionContext motion)
    {
        Context = context;
        Motion = motion;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EventMeta Meta => EventMetas.CardExhausted;
}
public struct CardCharged : ICombatEvent
{
    public CardCharged(EventContext context, MotionContext motion)
    {
        Context = context;
        Motion = motion;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EventMeta Meta => EventMetas.CardCharged;
}
public struct CardPlayDeclared : ICombatEvent
{
    public CardPlayDeclared(EventContext context, MotionContext motion, CardView cardView)
    {
        Context = context;
        Motion = motion;
        CardView = cardView;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public CardView CardView { get; private set; }
    public EventMeta Meta => EventMetas.CardPlayDeclared;
}
public struct AttackPlayed : ICombatEvent
{
    public AttackPlayed(EventContext context, MotionContext motion, EntityInstance source)
    {
        Context = context;
        Motion = motion;
        Source = source;
    }

    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EntityInstance Source { get; private set; }
    public EventMeta Meta => EventMetas.AttackPlayed;
}
public struct SkillPlayed : ICombatEvent
{
    public SkillPlayed(EventContext context, MotionContext motion, EntityInstance source)
    {
        Context = context;
        Motion = motion;
        Source = source;
    }

    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EntityInstance Source { get; private set; }
    public EventMeta Meta => EventMetas.SkillPlayed;
}
public struct PowerPlayed : ICombatEvent
{
    public PowerPlayed(EventContext context, MotionContext motion, EntityInstance source)
    {
        Context = context;
        Motion = motion;
        Source = source;
    }

    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EntityInstance Source { get; private set; }
    public EventMeta Meta => EventMetas.PowerPlayed;
}
public struct AttackDeclared : ICombatEvent
{
    public AttackDeclared(EventContext context, MotionContext motion, EntityInstance source, EntityInstance target, int amount, int repeat)
    {
        Context = context;
        Motion = motion;
        Source = source;
        Target = target;
        Amount = amount;
        Repeat = repeat;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EntityInstance Source { get; private set; }
    public EntityInstance Target { get; private set; }
    public int Amount { get; private set; }
    public int Repeat { get; private set; }
    public EventMeta Meta => EventMetas.AttackDeclared;
}
public struct BlockDeclared : ICombatEvent
{
    public BlockDeclared(EventContext context, MotionContext motion, EntityInstance source, EntityInstance target, int amount)
    {
        Context = context;
        Motion = motion;
        Source = source;
        Target = target;
        Amount = amount;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EntityInstance Source { get; private set; }
    public EntityInstance Target { get; private set; }
    public int Amount { get; private set; }
    public EventMeta Meta => EventMetas.BlockDeclared;
}
public struct DrawCardDeclared : ICombatEvent
{
    public DrawCardDeclared(EventContext context, MotionContext motion, int amount)
    {
        Context = context;
        Motion = motion;
        Amount = amount;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public int Amount { get; private set; }
    public EventMeta Meta => EventMetas.DrawCardDeclared;
}
public struct GainEnergyDeclared : ICombatEvent
{
    public GainEnergyDeclared(EventContext context, MotionContext motion, int amount)
    {
        Context = context;
        Motion = motion;
        Amount = amount;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public int Amount { get; private set; }
    public EventMeta Meta => EventMetas.GainEnergyDeclared;
}
public struct ModifyCostDeclared : ICombatEvent
{
    public ModifyCostDeclared(EventContext context, CostModificationScope scope, int amount)
    {
        Context = context;
        Scope = scope;
        Amount = amount;
    }
    public EventContext Context { get; private set; }
    public CostModificationScope Scope { get; private set; }
    public int Amount { get; private set; }
    public EventMeta Meta => EventMetas.ModifyCostDeclared;
}
public struct BuffDeclared : ICombatEvent
{
    public BuffDeclared(EventContext context, MotionContext motion, EntityInstance source, EntityInstance target, BuffType type, int amount)
    {
        Context = context;
        Motion = motion;
        Source = source;
        Target = target;
        Type = type;
        Amount = amount;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EntityInstance Source { get; private set; }
    public EntityInstance Target { get; private set; }
    public BuffType Type { get; private set; }
    public int Amount { get ; private set; }
    public EventMeta Meta => EventMetas.BuffDeclared;
}
public struct DamageRequested : ICombatEvent
{
    public DamageRequested(EventContext context, MotionContext motion, DamageContext damage)
    {
        Context = context;
        Motion = motion;
        Damage = damage;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public DamageContext Damage { get; private set; }
    public EventMeta Meta => EventMetas.DamageRequested;
}
public struct DamageResolved : ICombatEvent
{
    public DamageResolved(EventContext context, MotionContext motion, EntityInstance source, EntityInstance target, int amount, int repeat)
    {
        Context = context;
        Motion = motion;
        Source = source;
        Target = target;
        Amount = amount;
        Repeat = repeat;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
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
    public BuffResolved(EventContext context, MotionContext motion, EntityInstance source, EntityInstance target, BuffType type, int amount)
    {
        Context = context;
        Motion = motion;
        Source = source;
        Target = target;
        Type = type;
        Amount = amount;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EntityInstance Source { get; private set; }
    public EntityInstance Target { get; private set; }
    public BuffType Type { get; private set; }
    public int Amount { get; private set; }
    public EventMeta Meta => EventMetas.BuffResolved;
}
public struct HpChanged : ICombatEvent
{
    public HpChanged(EventContext context, MotionContext motion, EntityInstance target, int startAmount, int endAmount)
    {
        Context = context;
        Motion = motion;
        Target = target;
        StartAmount = startAmount;
        EndAmount = endAmount;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EntityInstance Target { get; private set; }
    public int StartAmount { get; private set; }
    public int EndAmount { get; private set; }
    public EventMeta Meta => EventMetas.HpChanged;
}
public struct BlockChanged : ICombatEvent
{
    public BlockChanged(EventContext context, MotionContext motion, EntityInstance target, int startAmount, int endAmount)
    {
        Context = context;
        Motion = motion;
        Target = target;
        StartAmount = startAmount;
        EndAmount = endAmount;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EntityInstance Target { get; private set; }
    public int StartAmount { get; private set; }
    public int EndAmount { get; private set; }
    public EventMeta Meta => EventMetas.BlockChanged;
}
public struct BuffChanged : ICombatEvent
{
    public BuffChanged(EventContext context, MotionContext motion, EntityInstance target, BuffType type, int startAmount, int endAmount)
    {
        Context = context;
        Motion = motion;
        Target = target;
        Type = type;
        StartAmount = startAmount;
        EndAmount = endAmount;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EntityInstance Target { get; private set; }
    public BuffType Type { get; private set; }
    public int StartAmount { get; private set; }
    public int EndAmount { get; private set; }
    public EventMeta Meta => EventMetas.BuffChanged;
}
public struct DeathDeclared : ICombatEvent
{
    public DeathDeclared(EventContext context, MotionContext motion, EntityInstance target)
    {
        Context = context;
        Motion = motion;
        Source = target;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
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
    public EnemyIntentDecided(EventContext context, MotionContext motion, EnemyInstance source)
    {
        Context = context;
        Motion = motion;
        Source = source;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public EnemyInstance Source { get; private set; }
    public EventMeta Meta => EventMetas.EnemyIntentDecided;
}
public struct RelicActivated : ICombatEvent
{
    public RelicActivated(EventContext context, MotionContext motion, RelicInstance source)
    {
        Context = context;
        Motion = motion;
        Source = source;
    }
    public EventContext Context { get; private set; }
    public MotionContext Motion { get; private set; }
    public RelicInstance Source { get; private set; }
    public EventMeta Meta => EventMetas.RelicActivated;
}
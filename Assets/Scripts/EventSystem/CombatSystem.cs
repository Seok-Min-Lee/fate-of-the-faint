using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class CombatSystem
{
    public CombatSystem()
    {
        CombatContext = new CombatContext(combatId: 0, source: this);

        TurnSystem = new TurnSystem(EventBus);
        ActionSystem = new ActionSystem(eventBus: EventBus, combatSystem: this);
    }
    public EventBus EventBus { get; } = new EventBus();
    public TurnSystem TurnSystem { get; }
    public ActionSystem ActionSystem { get; }
    public CombatContext CombatContext { get; private set; }
    public AnimationMonoSystem AnimationSystem { get; private set; }

    private DamageSystem damageSystem;
    private BuffSystem buffSystem;
    private EnergySystem energySystem;
    private CardMonoSystem cardSystem;
    private UIMonoSystem uiSystem;
    private RelicMonoSystem relicSystem;

    private PlayerInstance player;
    private Dictionary<Guid, EnemyInstance> enemies;
    public IReadOnlyList<EnemyInstance> liveEnemies => enemies.Values.Where(e => !e.IsDead).ToList();

    private Queue<Action> actionRequestQueue = new Queue<Action>();
    public void Init(
        DamageSystem damageSystem,
        BuffSystem buffSystem,
        EnergySystem energySystem, 
        CardMonoSystem cardSystem, 
        UIMonoSystem uiSystem,
        AnimationMonoSystem animationSystem,
        RelicMonoSystem relicSystem,
        PlayerInstance player,
        IEnumerable<EnemyInstance> enemies
    )
    {
        this.player = player;
        this.enemies = enemies.ToDictionary(key => key.Id, value => value);

        this.damageSystem = damageSystem;
        this.buffSystem = buffSystem;
        this.energySystem = energySystem;
        this.cardSystem = cardSystem;
        this.uiSystem = uiSystem;
        this.relicSystem = relicSystem;
        this.AnimationSystem = animationSystem;

        TurnSystem.Init(enemies);
    }
    public void UpdateTick()
    {
        TurnSystem.UpdateTick();

        if (AnimationSystem.IsPlaying)
        {
            return;
        }

        if (actionRequestQueue.Count > 0)
        {
            actionRequestQueue.Dequeue().Invoke();
        }
    }
    public void OnEnable()
    {
        EventBus.Subscribe<EnemyActionStartRequested>(OnEnemyActionStartRequested);
        EventBus.Subscribe<DeathDeclared>(OnDeathDeclared);
        EventBus.Subscribe<DamageRequested>(OnDamageRequested);
        EventBus.Subscribe<DamageResolved>(OnDamageResolved);
        EventBus.Subscribe<BuffResolved>(OnBuffResolved);
        EventBus.Subscribe<BlockDeclared>(OnBlockDeclared);

        EventBus.Subscribe<PlayerTurnStartRequested>(TurnSystem.OnPlayerTurnStartRequested);
        EventBus.Subscribe<PlayerTurnEndRequested>(TurnSystem.OnPlayerTurnEndRequested);
        EventBus.Subscribe<ActionEnded>(TurnSystem.OnActionEnded);

        EventBus.Subscribe<AttackDeclared>(damageSystem.OnAttackDeclared);

        EventBus.Subscribe<BuffDeclared>(buffSystem.OnBuffDeclared);

        EventBus.Subscribe<PlayerTurnStarted>(energySystem.OnPlayerTurnStarted);
        EventBus.Subscribe<EnergyChangeRequested>(energySystem.OnEnergyChangeRequested);

        EventBus.Subscribe<CombatStarted>(uiSystem.OnCombatStarted);
        EventBus.Subscribe<CombatEnded>(uiSystem.OnCombatEnded);
        EventBus.Subscribe<PlayerTurnStarted>(uiSystem.OnPlayerTurnStarted);
        EventBus.Subscribe<PlayerTurnEnded>(uiSystem.OnPlayerTurnEnded);
        EventBus.Subscribe<EnemyTurnStarted>(uiSystem.OnEnemyTurnStarted);
        EventBus.Subscribe<EnergyChanged>(uiSystem.OnEnergyChanged);

        EventBus.Subscribe<CombatEnded>(cardSystem.OnCombatEnded);
        EventBus.Subscribe<PlayerTurnStarted>(cardSystem.OnPlayerTurnStarted);
        EventBus.Subscribe<PlayerTurnEnded>(cardSystem.OnPlayerTurnEnded);
        EventBus.Subscribe<EnergyResolved>(cardSystem.OnEnergyResolved);

        EventBus.Subscribe<DamageRequested>(relicSystem.OnDamageRequested);

        EventBus.Subscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        EventBus.Subscribe<EnemyTurnStarted>(OnEnemyTurnStarted);
    }
    public void OnDisable()
    {
        EventBus.Unsubscribe<EnemyActionStartRequested>(OnEnemyActionStartRequested);
        EventBus.Unsubscribe<DeathDeclared>(OnDeathDeclared);
        EventBus.Unsubscribe<DamageRequested>(OnDamageRequested);
        EventBus.Unsubscribe<DamageResolved>(OnDamageResolved);
        EventBus.Unsubscribe<BuffResolved>(OnBuffResolved);
        EventBus.Unsubscribe<BlockDeclared>(OnBlockDeclared);

        EventBus.Unsubscribe<PlayerTurnStartRequested>(TurnSystem.OnPlayerTurnStartRequested);
        EventBus.Unsubscribe<PlayerTurnEndRequested>(TurnSystem.OnPlayerTurnEndRequested);
        EventBus.Unsubscribe<ActionEnded>(TurnSystem.OnActionEnded);

        EventBus.Unsubscribe<AttackDeclared>(damageSystem.OnAttackDeclared);

        EventBus.Unsubscribe<BuffDeclared>(buffSystem.OnBuffDeclared);

        EventBus.Unsubscribe<PlayerTurnStarted>(energySystem.OnPlayerTurnStarted);
        EventBus.Unsubscribe<EnergyChangeRequested>(energySystem.OnEnergyChangeRequested);

        EventBus.Unsubscribe<CombatStarted>(uiSystem.OnCombatStarted);
        EventBus.Unsubscribe<CombatEnded>(uiSystem.OnCombatEnded);
        EventBus.Unsubscribe<PlayerTurnStarted>(uiSystem.OnPlayerTurnStarted);
        EventBus.Unsubscribe<PlayerTurnEnded>(uiSystem.OnPlayerTurnEnded);
        EventBus.Unsubscribe<EnemyTurnStarted>(uiSystem.OnEnemyTurnStarted);
        EventBus.Unsubscribe<EnergyChanged>(uiSystem.OnEnergyChanged);

        EventBus.Unsubscribe<CombatEnded>(cardSystem.OnCombatEnded);
        EventBus.Unsubscribe<PlayerTurnStarted>(cardSystem.OnPlayerTurnStarted);
        EventBus.Unsubscribe<PlayerTurnEnded>(cardSystem.OnPlayerTurnEnded);
        EventBus.Unsubscribe<EnergyResolved>(cardSystem.OnEnergyResolved);

        EventBus.Unsubscribe<DamageRequested>(relicSystem.OnDamageRequested);

        EventBus.Unsubscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        EventBus.Unsubscribe<EnemyTurnStarted>(OnEnemyTurnStarted);
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        List<BuffType> types = new List<BuffType>(player.Buffs.Keys);
        foreach (BuffType type in types)
        {
            int startAmount = player.Getbuff(type);
            player.ApplyBuff(type, -1);
            EventBus.Publish<BuffChanged>(new BuffChanged(
                context: CreateContext(e.Context),
                target: player,
                type: type,
                startAmount: startAmount,
                endAmount: startAmount - 1
            ));
        }

        if (player.Block > 0)
        {
            int beforeBlock = player.Block;
            player.SetBlock(0);

            EventBus.Publish<BlockChanged>(new BlockChanged(
                context: CreateContext(e.Context),
                target: player,
                startAmount: beforeBlock,
                endAmount: player.Block
            ));
        }

        foreach (EnemyInstance ei in enemies.Values.Where(enemy => !enemy.IsDead))
        {
            ei.DecideNextAction(new System.Random());
        }

        AnimationSystem.PlayQueue(e.Context);
        actionRequestQueue.Enqueue(() =>
        {
            EventBus.Publish<EnemyIntentDecided>(new EnemyIntentDecided(CreateContext(e.Context)));
        });
    }
    public void OnEnemyTurnStarted(EnemyTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        foreach (EnemyInstance enemy in enemies.Values.Where(enemy => !enemy.IsDead))
        {
            List<BuffType> types = new List<BuffType>(enemy.Buffs.Keys);
            foreach (BuffType type in types)
            {
                int startAmount = enemy.Getbuff(type);
                enemy.ApplyBuff(type, -1);
                EventBus.Publish<BuffChanged>(new BuffChanged(
                    context: CreateContext(e.Context),
                    target: enemy,
                    type: type,
                    startAmount: startAmount,
                    endAmount: startAmount - 1
                ));
            }
        }

        AnimationSystem.PlayQueue(e.Context);
        actionRequestQueue.Enqueue(() => EnemyActionStart(e.Context.Turn.EnemyQueue.Dequeue().Id));
    }
    private void EnemyActionStart(Guid enemyId)
    {
        if (CombatContext.state != CombatState.Combat)
        {
            return;
        }
        if (!enemies.TryGetValue(key: enemyId, value: out EnemyInstance enemy))
        {
            return;
        }

        ActionContext actionContext = new ActionContext(
            source: enemy,
            type: ActionType.EnemyAct
        );

        ActionSystem.ExcuteAction(actionContext, (eventContext) =>
        {
            foreach (IntentEffect effect in enemy.NextAction.Effects)
            {
                List<EntityInstance> targets = new List<EntityInstance>();
                switch (effect.targetType)
                {
                    case IntentTarget.Player:
                        targets.Add(player);
                        break;
                    case IntentTarget.Self:
                        targets.Add(enemy);
                        break;
                    case IntentTarget.Member:
                        //targets.Add(target);
                        break;
                    case IntentTarget.MemberAll:
                        //targets.AddRange(combatSystem.liveEnemies);
                        break;
                }

                for (int i = 0; i < targets.Count; i++)
                {
                    for (int j = 0; j < effect.repeat; j++)
                    {
                        switch (effect.effectType)
                        {
                            case EffectType.Attack:
                                EventBus.Publish<AttackDeclared>(new AttackDeclared(
                                    context: eventContext,
                                    source: enemy,
                                    target: targets[i],
                                    amount: effect.value
                                ));
                                break;
                            case EffectType.Block:
                                EventBus.Publish<BlockDeclared>(new BlockDeclared(
                                    context: eventContext,
                                    source: enemy,
                                    target: targets[i],
                                    amount: effect.value
                                ));
                                break;
                            case EffectType.Strengthen:
                                break;
                            case EffectType.Weaken:
                                break;
                            case EffectType.Vulnerable:
                                break;
                            default:
                                return;
                        }
                    }
                }
            }
        });
    }
    public void OnEnemyActionStartRequested(EnemyActionStartRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        AnimationSystem.PlayQueue(e.Context);
        actionRequestQueue.Enqueue(() => EnemyActionStart(e.Enemy.Id));
    }
    public void OnDeathDeclared(DeathDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EventContext eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );

        if (e.Source == player)
        {
            CombatContext.state = CombatState.Defeat;

            EventBus.Publish<CombatEnded>(new CombatEnded(
                context: eventContext, 
                result: CombatState.Defeat
            ));
            AnimationSystem.PlayQueue(e.Context);
        }
        else
        {
            if (!enemies.Values.Any(enemy => !enemy.IsDead))
            {
                CombatContext.state = CombatState.Victory;

                EventBus.Publish<CombatEnded>(new CombatEnded(
                    context: eventContext,
                    result: CombatState.Victory
                ));
                AnimationSystem.PlayQueue(e.Context);
            }
        }
    }
    private void OnDamageRequested(DamageRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EnemyInstance enemy;
        if (e.Damage.Source.Id == player.Id)
        {
            if (player.Buffs.TryGetValue(key: BuffType.Strength, value: out int strength))
            {
                e.Damage.Add(strength, player);
            }
            if (player.Buffs.ContainsKey(BuffType.Weak))
            {
                e.Damage.Multiply(0.5f, player);
            }
        }
        else if (enemies.TryGetValue(key: e.Damage.Source.Id, value: out enemy))
        {
            if (enemy.Buffs.TryGetValue(key: BuffType.Strength, value: out int strength))
            {
                e.Damage.Add(strength, enemy);
            }
            if (enemy.Buffs.ContainsKey(BuffType.Weak))
            {
                e.Damage.Multiply(0.5f, enemy);
            }
        }
        else if (e.Damage.Target.Id == player.Id)
        {
            if (player.Buffs.ContainsKey(BuffType.Vulnerable))
            {
                e.Damage.Multiply(1.5f, player);
            }
        }
        else if (enemies.TryGetValue(key: e.Damage.Target.Id, value: out enemy))
        {
            if (enemy.Buffs.ContainsKey(BuffType.Vulnerable))
            {
                e.Damage.Multiply(1.5f, player);
            }
        }
    }
    private void OnDamageResolved(DamageResolved e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EntityInstance instance = null;
        if (e.Target == player)
        {
            instance = player;
        }
        else if (enemies.ContainsKey(e.Target.Id))
        {
            instance = enemies[e.Target.Id];
        }
        else
        {
            return;
        }

        int damage = e.Amount;

        if (instance.Block > 0)
        {
            damage -= instance.Block;

            int beforeBlock = instance.Block;
            instance.SetBlock(Mathf.Max(0, instance.Block - e.Amount));

            EventBus.Publish<BlockChanged>(new BlockChanged(
                context: CreateContext(e.Context),
                target: instance,
                startAmount: beforeBlock,
                endAmount: instance.Block
            ));
        }

        if (damage > 0)
        {
            int beforeHp = instance.CurrentHp;
            instance.SetCurrentHp(Mathf.Max(0, instance.CurrentHp - damage));

            EventBus.Publish<HpChanged>(new HpChanged(
                context: CreateContext(e.Context),
                target: instance,
                startAmount: beforeHp,
                endAmount: instance.CurrentHp
            ));
        }

        if (instance.CurrentHp <= 0)
        {
            EventBus.Publish<DeathDeclared>(new DeathDeclared(
                context: CreateContext(e.Context),
                target: instance
            ));
        }
    }

    private void OnBuffResolved(BuffResolved e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EntityInstance instance = null;
        if (e.Target == player)
        {
            instance = player;
        }
        else if (enemies.ContainsKey(e.Target.Id))
        {
            instance = enemies[e.Target.Id];
        }
        else
        {
            return;
        }

        int startAmount = instance.Buffs.TryGetValue(e.Type, out int value) ? value : 0;
        int endAmount = Mathf.Max(0, startAmount + e.Amount);

        instance.ApplyBuff(type: e.Type, delta: e.Amount);

        EventContext eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );

        EventBus.Publish<BuffChanged>(new BuffChanged(
            context: eventContext,
            target: instance,
            type: e.Type,
            startAmount: startAmount,
            endAmount: endAmount
        ));
    }
    private void OnBlockDeclared(BlockDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat) 
        {
            return;
        }

        EntityInstance instance;
        if (e.Source == player)
        {
            instance = player;
        }
        else if (enemies.TryGetValue(e.Source.Id, out EnemyInstance enemy) && !enemy.IsDead)
        {
            instance = enemy;
        }
        else
        {
            return;
        }

        int beforeBlock = instance.Block;
        instance.AddBlock(e.Amount);

        EventBus.Publish<BlockChanged>(new BlockChanged(
            context: CreateContext(e.Context),
            target: instance,
            startAmount: beforeBlock,
            endAmount: instance.Block
        ));
    }
    public void CombatStart()
    {
        CombatContext = new CombatContext(combatId: 0, source: this);

        EventContext eventContext = new EventContext(
            source: this,
            action: null,
            turn: null,
            combat: CombatContext
        );

        CombatContext.state = CombatState.Combat;
        EventBus.Publish<CombatStarted>(new CombatStarted(eventContext));

        AnimationSystem.PlayQueue(eventContext);
        actionRequestQueue.Enqueue(() =>
        {
            RequestContext requestContext = new RequestContext(this);

            eventContext = new EventContext(
                source: this,
                action: null,
                turn: null,
                combat: CombatContext
            );

            EventBus.Publish<PlayerTurnStartRequested>(new PlayerTurnStartRequested(
                context: eventContext,
                request: requestContext
            ));
        });
    }
    private EventContext CreateContext(EventContext context)
    {
        return new EventContext(
            source: this,
            action: context.Action,
            turn: context.Turn,
            combat: context.Combat
        );
    }
}

public class CombatContext
{
    public CombatContext(int combatId, object source)
    {
        CombatId = combatId;
        Source = source;

        state = CombatState.Wait;
    }
    public int CombatId { get; set; }
    public object Source { get; private set; }
    public CombatState state;
}
public enum CombatState
{
    Wait,
    Combat,
    Victory,
    Defeat
}
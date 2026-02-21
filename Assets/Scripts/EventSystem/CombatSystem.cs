using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CombatSystem : BaseSystem
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
    public MotionMonoSystem AnimationSystem { get; private set; }

    private DamageSystem damageSystem;
    private BuffSystem buffSystem;
    private EnergySystem energySystem;
    private CardMonoSystem cardSystem;
    private UIMonoSystem uiSystem;
    private CameraMonoSystem cameraSystem;
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
        CameraMonoSystem cameraSystem,
        MotionMonoSystem animationSystem,
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
        this.cameraSystem = cameraSystem;
        this.AnimationSystem = animationSystem;
        this.relicSystem = relicSystem;

        TurnSystem.Init(enemies, animationSystem);
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
        EventBus.Subscribe<GainEnergyDeclared>(energySystem.OnGainEnergyDeclared);

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
        EventBus.Subscribe<DrawCardDeclared>(cardSystem.OnDrawCardDeclared);
        EventBus.Subscribe<ModifyCostDeclared>(cardSystem.OnModifyCostDeclared);

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
        EventBus.Unsubscribe<GainEnergyDeclared>(energySystem.OnGainEnergyDeclared);

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
        EventBus.Unsubscribe<DrawCardDeclared>(cardSystem.OnDrawCardDeclared);
        EventBus.Unsubscribe<ModifyCostDeclared>(cardSystem.OnModifyCostDeclared);

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

        // Block Clear
        if (player.Block > 0)
        {
            int beforeBlock = player.Block;
            player.SetBlock(0);

            EventBus.Publish<BlockChanged>(new BlockChanged(
                context: CreateContext(e.Context),
                motion: e.Motion,
                target: player,
                startAmount: beforeBlock,
                endAmount: player.Block
            ));
        }

        // Buff Update
        List<BuffType> types = new List<BuffType>(player.Buffs.Keys);
        foreach (BuffType type in types)
        {
            int startAmount = player.Getbuff(type);
            player.ApplyBuff(type, -1);
            EventBus.Publish<BuffChanged>(new BuffChanged(
                context: CreateContext(e.Context),
                motion: e.Motion,
                target: player,
                type: type,
                startAmount: startAmount,
                endAmount: startAmount - 1
            ));
        }

        // Enemy Intent Update
        foreach (EnemyInstance enemy in enemies.Values.Where(enemy => !enemy.IsDead))
        {
            enemy.DecideNextAction(new System.Random());

            EventBus.Publish<EnemyIntentDecided>(new EnemyIntentDecided(
                context: CreateContext(e.Context),
                motion: e.Motion,
                source: enemy
            ));
        }
    }
    public void OnEnemyTurnStarted(EnemyTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        foreach (EnemyInstance enemy in enemies.Values.Where(enemy => !enemy.IsDead))
        {
            // Block Clear
            if (enemy.Block > 0)
            {
                int beforeBlock = enemy.Block;
                enemy.SetBlock(0);

                EventBus.Publish<BlockChanged>(new BlockChanged(
                    context: CreateContext(e.Context),
                    motion: e.Motion,
                    target: enemy,
                    startAmount: beforeBlock,
                    endAmount: enemy.Block
                ));
            }

            // Buff Update
            List<BuffType> types = new List<BuffType>(enemy.Buffs.Keys);
            foreach (BuffType type in types)
            {
                int startAmount = enemy.Getbuff(type);
                enemy.ApplyBuff(type, -1);
                EventBus.Publish<BuffChanged>(new BuffChanged(
                    context: CreateContext(e.Context),
                    motion: e.Motion,
                    target: enemy,
                    type: type,
                    startAmount: startAmount,
                    endAmount: startAmount - 1
                ));
            }
        }

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

        ActionSystem.ExcuteAction(actionContext, (eventContext, animationContext) =>
        {
            IntentEffect[] effects = enemy.NextAction.Effects;

            // Motion Play
            switch (effects[0].effectType)
            {
                case EffectType.Attack:
                    EventBus.Publish<AttackPlayed>(new AttackPlayed(
                        context: eventContext,
                        motion: animationContext,
                        source: enemy
                    )); 
                    break;
                default:
                    EventBus.Publish<SkillPlayed>(new SkillPlayed(
                        context: eventContext,
                        motion: animationContext,
                        source: enemy
                    ));
                    break;
            }

            // Effect Process
            foreach (IntentEffect effect in effects)
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
                    switch (effect.effectType)
                    {
                        case EffectType.Attack:
                            EventBus.Publish<AttackDeclared>(new AttackDeclared(
                                context: eventContext,
                                motion: animationContext,
                                source: enemy,
                                target: targets[i],
                                amount: effect.value,
                                repeat: effect.repeat
                            ));
                            break;
                        case EffectType.Block:
                            EventBus.Publish<BlockDeclared>(new BlockDeclared(
                                context: eventContext,
                                motion: animationContext,
                                source: enemy,
                                target: targets[i],
                                amount: effect.value
                            ));
                            break;
                        case EffectType.Strengthen:
                            EventBus.Publish<BuffDeclared>(new BuffDeclared(
                                context: eventContext,
                                motion: animationContext,
                                source: enemy,
                                target: targets[i],
                                type: BuffType.Strength,
                                amount: effect.value
                            ));
                            break;
                        case EffectType.Weaken:
                            EventBus.Publish<BuffDeclared>(new BuffDeclared(
                                context: eventContext,
                                motion: animationContext,
                                source: enemy,
                                target: targets[i],
                                type: BuffType.Weak,
                                amount: effect.value
                            ));
                            break;
                        case EffectType.Vulnerable:
                            EventBus.Publish<BuffDeclared>(new BuffDeclared(
                                context: eventContext,
                                motion: animationContext,
                                source: enemy,
                                target: targets[i],
                                type: BuffType.Vulnerable,
                                amount: effect.value
                            ));
                            break;
                        default:
                            return;
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

            actionRequestQueue.Enqueue(() =>
            {
                MotionContext motionContext = new MotionContext(this);

                EventBus.Publish<CombatEnded>(new CombatEnded(
                    context: eventContext,
                    motion: motionContext,
                    result: CombatState.Defeat
                ));

                AnimationSystem.Play(
                    context: eventContext, 
                    motion: motionContext
                );
            });
        }
        else
        {
            if (!enemies.Values.Any(enemy => !enemy.IsDead))
            {
                CombatContext.state = CombatState.Victory;

                actionRequestQueue.Enqueue(() =>
                {
                    MotionContext motionContext = new MotionContext(this);

                    EventBus.Publish<CombatEnded>(new CombatEnded(
                        context: eventContext,
                        motion: motionContext,
                        result: CombatState.Victory
                    ));

                    AnimationSystem.Play(
                        context: eventContext,
                        motion: motionContext
                    );
                });
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

        for (int i = 0; i < e.Repeat; i++)
        {
            if (instance.IsDead)
            {
                break;
            }

            int damage = e.Amount;

            if (instance.Block > 0)
            {
                damage -= instance.Block;

                int beforeBlock = instance.Block;
                instance.SetBlock(Mathf.Max(0, instance.Block - e.Amount));

                EventBus.Publish<BlockChanged>(new BlockChanged(
                    context: CreateContext(e.Context),
                    motion: e.Motion,
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
                    motion: e.Motion,
                    target: instance,
                    startAmount: beforeHp,
                    endAmount: instance.CurrentHp
                ));
            }

            if (instance.CurrentHp <= 0)
            {
                EventBus.Publish<DeathDeclared>(new DeathDeclared(
                    context: CreateContext(e.Context),
                    motion: e.Motion,
                    target: instance
                ));
            }
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
            motion: e.Motion,
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
            motion: e.Motion,
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

        MotionContext motionContext = new MotionContext(this);

        CombatContext.state = CombatState.Combat;

        EventBus.Publish<CombatStarted>(new CombatStarted(
            context: eventContext, 
            motion: motionContext
        ));

        AnimationSystem.Play(
            context: eventContext, 
            motion: motionContext
        );

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
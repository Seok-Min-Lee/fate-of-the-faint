using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class CombatSystem
{
    public EventBus EventBus { get; } = new EventBus();
    public CombatContext CombatContext { get; private set; }
    public TurnSystem TurnSystem { get; private set; }
    public ActionSystem ActionSystem { get; private set; }
    private DamageSystem damageSystem;
    private EnergySystem energySystem;
    private CardSystem cardSystem;
    private UISystem uiSystem;
    private RelicSystem relicSystem;

    private PlayerInstance player;
    private Dictionary<Guid, EnemyInstance> enemies;
    public IReadOnlyList<EnemyInstance> liveEnemies => enemies.Values.Where(e => !e.IsDead).ToList();
    public CombatSystem()
    {
        CombatContext = new CombatContext(combatId: 0, source: this);

        TurnSystem = new TurnSystem(EventBus);
        ActionSystem = new ActionSystem(eventBus: EventBus, combatSystem: this);
    }
    public void Init(
        DamageSystem damageSystem, 
        EnergySystem energySystem, 
        CardSystem cardSystem, 
        UISystem uiSystem, 
        RelicSystem relicSystem,
        PlayerInstance player,
        IEnumerable<EnemyInstance> enemies
    )
    {
        this.player = player;
        this.enemies = enemies.ToDictionary(key => key.Id, value => value);

        this.damageSystem = damageSystem;
        this.energySystem = energySystem;
        this.cardSystem = cardSystem;
        this.uiSystem = uiSystem;
        this.relicSystem = relicSystem;

        TurnSystem.Init(enemies);
    }
    public void OnEnable()
    {
        EventBus.Subscribe<DeathDeclared>(OnDeathDeclared);
        EventBus.Subscribe<DamageRequested>(OnDamageRequested);
        EventBus.Subscribe<DamageResolved>(OnDamageResolved);
        EventBus.Subscribe<EnemyActionStartRequested>(OnEnemyActionStartRequested);
        EventBus.Subscribe<PlayerTurnStarted>(OnPlayerTurnStarted);

        EventBus.Subscribe<CombatStarted>(TurnSystem.OnCombatStarted);
        EventBus.Subscribe<ActionEnded>(TurnSystem.OnActionEnded);
        EventBus.Subscribe<PlayerTurnEndRequested>(TurnSystem.OnPlayerTurnEndRequested);
        EventBus.Subscribe<EnemyTurnStarted>(TurnSystem.OnEnemyTurnStarted);

        EventBus.Subscribe<AttackDeclared>(damageSystem.OnAttackDeclared);

        EventBus.Subscribe<PlayerTurnStarted>(energySystem.OnPlayerTurnStarted);
        EventBus.Subscribe<EnergyChangeRequested>(energySystem.OnEnergyChangeRequested);

        EventBus.Subscribe<PlayerTurnStarted>(cardSystem.OnPlayerTurnStarted);
        EventBus.Subscribe<EnergyResolved>(cardSystem.OnEnergyResolved);
        EventBus.Subscribe<DamageResolved>(cardSystem.OnDamageResolved);
        EventBus.Subscribe<PlayerTurnEnded>(cardSystem.OnPlayerTurnEnded);
        EventBus.Subscribe<CombatEnded>(cardSystem.OnCombatEnded);

        EventBus.Subscribe<CombatStarted>(uiSystem.OnCombatStarted);
        EventBus.Subscribe<CombatEnded>(uiSystem.OnCombatEnded);
        EventBus.Subscribe<PlayerTurnStarted>(uiSystem.OnPlayerTurnStarted);
        EventBus.Subscribe<PlayerTurnEnded>(uiSystem.OnPlayerTurnEnded);
        EventBus.Subscribe<EnergyChanged>(uiSystem.OnEnergyChanged);

        EventBus.Subscribe<DamageRequested>(relicSystem.OnDamageRequested);
    }
    public void OnDisable()
    {
        EventBus.Unsubscribe<DeathDeclared>(OnDeathDeclared);
        EventBus.Unsubscribe<DamageRequested>(OnDamageRequested);
        EventBus.Unsubscribe<DamageResolved>(OnDamageResolved);
        EventBus.Unsubscribe<EnemyActionStartRequested>(OnEnemyActionStartRequested);
        EventBus.Unsubscribe<PlayerTurnStarted>(OnPlayerTurnStarted);

        EventBus.Unsubscribe<CombatStarted>(TurnSystem.OnCombatStarted);
        EventBus.Unsubscribe<ActionEnded>(TurnSystem.OnActionEnded);
        EventBus.Unsubscribe<PlayerTurnEndRequested>(TurnSystem.OnPlayerTurnEndRequested);
        EventBus.Unsubscribe<EnemyTurnStarted>(TurnSystem.OnEnemyTurnStarted);

        EventBus.Unsubscribe<AttackDeclared>(damageSystem.OnAttackDeclared);

        EventBus.Unsubscribe<PlayerTurnStarted>(energySystem.OnPlayerTurnStarted);
        EventBus.Unsubscribe<EnergyChangeRequested>(energySystem.OnEnergyChangeRequested);

        EventBus.Unsubscribe<PlayerTurnStarted>(cardSystem.OnPlayerTurnStarted);
        EventBus.Unsubscribe<EnergyResolved>(cardSystem.OnEnergyResolved);
        EventBus.Unsubscribe<DamageResolved>(cardSystem.OnDamageResolved);
        EventBus.Unsubscribe<PlayerTurnEnded>(cardSystem.OnPlayerTurnEnded);
        EventBus.Unsubscribe<CombatEnded>(cardSystem.OnCombatEnded);

        EventBus.Unsubscribe<PlayerTurnStarted>(uiSystem.OnPlayerTurnStarted);
        EventBus.Unsubscribe<PlayerTurnEnded>(uiSystem.OnPlayerTurnEnded);
        EventBus.Unsubscribe<EnergyChanged>(uiSystem.OnEnergyChanged);

        EventBus.Unsubscribe<DamageRequested>(relicSystem.OnDamageRequested);
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        foreach (EnemyInstance ei in enemies.Values)
        {
            if (!ei.IsDead)
            {
                ei.DecideNextAction(new System.Random());
            }
        }

        EventContext eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );
        EventBus.Publish<EnemyIntentDecided>(new EnemyIntentDecided(eventContext));
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
            }
        }
    }
    private void OnDamageRequested(DamageRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Damage.Source.Id == player.Id)
        {
            if (player.Buffs.TryGetValue(key: BuffType.Strength, value: out int strength))
            {
                e.Damage.Add(strength, player);
            }
        }
        else if (e.Damage.Target.Id == player.Id)
        {
            if (player.Block > 0)
            {
                e.Damage.Subtract(player.Block, player);
            }
        }
        else if (enemies.TryGetValue(key: e.Damage.Target.Id, value: out EnemyInstance enemy))
        {
            if (enemy.Block > 0)
            {
                e.Damage.Subtract(enemy.Block, enemy);
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
        int startAmount = 0;
        int endAmount = 0;
        if (e.Target == player)
        {
            instance = player;
            startAmount = player.CurrentHp;
            endAmount = Mathf.Max(0, startAmount - e.Amount);
        }
        else if (enemies.ContainsKey(e.Target.Id))
        {
            instance = enemies[e.Target.Id];
            startAmount = enemies[e.Target.Id].CurrentHp;
            endAmount = Mathf.Max(0, startAmount - e.Amount);
        }
        else
        {
            return;
        }

        Process(instance, startAmount, endAmount);

        void Process(EntityInstance instance, int startAmount, int endAmount)
        {
            instance.SetCurrentHp(endAmount);

            EventContext eventContext = new EventContext(
                source: this,
                action: e.Context.Action,
                turn: e.Context.Turn,
                combat: e.Context.Combat
            );

            EventBus.Publish<HpChanged>(new HpChanged(
                context: eventContext,
                target: instance,
                startAmount: startAmount,
                endAmount: endAmount
            )); 

            if (instance.CurrentHp <= 0)
            {
                EventBus.Publish<DeathDeclared>(new DeathDeclared(
                    context: eventContext,
                    target: instance
                ));
            }
        }
    }
    public void OnEnemyActionStartRequested(EnemyActionStartRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (!enemies.TryGetValue(key: e.Enemy.Id, value: out EnemyInstance enemy))
        {
            return;
        }

        e.Request.isResult = true;

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
    public void CombatStart()
    {
        CombatContext = new CombatContext(combatId: 999, source: this);

        EventContext eventContext = new EventContext(
            source: this,
            action: null,
            turn: null,
            combat: CombatContext
        );

        CombatContext.state = CombatState.Combat;
        EventBus.Publish<CombatStarted>(new CombatStarted(eventContext));
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
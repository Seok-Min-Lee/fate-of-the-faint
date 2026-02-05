using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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

    private PlayerView player;
    private List<EnemyView> enemies;
    public IReadOnlyList<EnemyView> liveEnemies => enemies.Where(e => !e.IsDeath).ToList();
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
        PlayerView player,
        IEnumerable<EnemyView> enemies
    )
    {
        this.player = player;
        this.enemies = new List<EnemyView>(enemies);

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

        EventBus.Unsubscribe<PlayerTurnStarted>(uiSystem.OnPlayerTurnStarted);
        EventBus.Unsubscribe<PlayerTurnEnded>(uiSystem.OnPlayerTurnEnded);
        EventBus.Unsubscribe<EnergyChanged>(uiSystem.OnEnergyChanged);

        EventBus.Unsubscribe<DamageRequested>(relicSystem.OnDamageRequested);
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
            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemies[i].IsDeath)
                {
                    return;
                }
            }

            CombatContext.state = CombatState.Victory;

            EventBus.Publish<CombatEnded>(new CombatEnded(
                context: eventContext,
                result: CombatState.Victory
            ));
        }
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
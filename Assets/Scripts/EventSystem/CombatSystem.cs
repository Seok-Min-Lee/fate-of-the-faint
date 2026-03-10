using System;
using System.Collections.Generic;
using System.Linq;

public class CombatSystem : BaseSystem
{
    public CombatSystem()
    {
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
    private PowerSystem powerSystem;
    private CameraMonoSystem cameraSystem;
    private RelicMonoSystem relicSystem;
    private GoldMonoSystem goldSystem;
    private HpMonoSystem hpSystem;

    private PlayerInstance player;
    private Dictionary<Guid, EnemyInstance> enemies;
    public IReadOnlyList<EnemyInstance> liveEnemies => enemies.Values.Where(e => !e.IsDead).ToList();
    private Queue<Action> actionRequestQueue = new Queue<Action>();
    public void Init(
        DamageSystem damageSystem,
        BuffSystem buffSystem,
        EnergySystem energySystem, 
        PowerSystem powerSystem,
        CardMonoSystem cardSystem, 
        UIMonoSystem uiSystem,
        CameraMonoSystem cameraSystem,
        MotionMonoSystem animationSystem,
        RelicMonoSystem relicSystem,
        GoldMonoSystem goldSystem,
        HpMonoSystem hpSystem,
        PlayerInstance player,
        IEnumerable<EnemyInstance> enemies
    )
    {
        this.player = player;
        this.enemies = enemies.ToDictionary(key => key.Id, value => value);

        this.damageSystem = damageSystem;
        this.buffSystem = buffSystem;
        this.energySystem = energySystem;
        this.powerSystem = powerSystem;
        this.cardSystem = cardSystem;
        this.uiSystem = uiSystem;
        this.cameraSystem = cameraSystem;
        this.AnimationSystem = animationSystem;
        this.relicSystem = relicSystem;
        this.goldSystem = goldSystem;
        this.hpSystem = hpSystem;

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

        EventBus.Subscribe<CombatEnded>(powerSystem.OnCombatEnded);

        EventBus.Subscribe<CombatStarted>(uiSystem.OnCombatStarted);
        EventBus.Subscribe<CombatEnded>(uiSystem.OnCombatEnded);
        EventBus.Subscribe<PlayerTurnStarted>(uiSystem.OnPlayerTurnStarted);
        EventBus.Subscribe<PlayerTurnEnded>(uiSystem.OnPlayerTurnEnded);
        EventBus.Subscribe<EnemyTurnStarted>(uiSystem.OnEnemyTurnStarted);
        EventBus.Subscribe<EnergyChanged>(uiSystem.OnEnergyChanged);

        EventBus.Subscribe<ActionEnded>(hpSystem.OnActionEnded);

        EventBus.Subscribe<CombatEnded>(cardSystem.OnCombatEnded);
        EventBus.Subscribe<PlayerTurnStarted>(cardSystem.OnPlayerTurnStarted);
        EventBus.Subscribe<PlayerTurnEnded>(cardSystem.OnPlayerTurnEnded);
        EventBus.Subscribe<EnergyResolved>(cardSystem.OnEnergyResolved);
        EventBus.Subscribe<DrawCardDeclared>(cardSystem.OnDrawCardDeclared);
        EventBus.Subscribe<ModifyCostDeclared>(cardSystem.OnModifyCostDeclared);

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

        EventBus.Unsubscribe<CombatEnded>(powerSystem.OnCombatEnded);

        EventBus.Unsubscribe<CombatStarted>(uiSystem.OnCombatStarted);
        EventBus.Unsubscribe<CombatEnded>(uiSystem.OnCombatEnded);
        EventBus.Unsubscribe<PlayerTurnStarted>(uiSystem.OnPlayerTurnStarted);
        EventBus.Unsubscribe<PlayerTurnEnded>(uiSystem.OnPlayerTurnEnded);
        EventBus.Unsubscribe<EnemyTurnStarted>(uiSystem.OnEnemyTurnStarted);
        EventBus.Unsubscribe<EnergyChanged>(uiSystem.OnEnergyChanged);

        EventBus.Unsubscribe<ActionEnded>(hpSystem.OnActionEnded);

        EventBus.Unsubscribe<CombatEnded>(cardSystem.OnCombatEnded);
        EventBus.Unsubscribe<PlayerTurnStarted>(cardSystem.OnPlayerTurnStarted);
        EventBus.Unsubscribe<PlayerTurnEnded>(cardSystem.OnPlayerTurnEnded);
        EventBus.Unsubscribe<EnergyResolved>(cardSystem.OnEnergyResolved);
        EventBus.Unsubscribe<DrawCardDeclared>(cardSystem.OnDrawCardDeclared);
        EventBus.Unsubscribe<ModifyCostDeclared>(cardSystem.OnModifyCostDeclared);

        EventBus.Unsubscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        EventBus.Unsubscribe<EnemyTurnStarted>(OnEnemyTurnStarted);
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // Enemy Intent Update
        foreach (EnemyInstance enemy in e.Context.Combat.Enemies)
        {
            enemy.DecideIntent(
                eventBus: EventBus,
                context: e.Context,
                motion: e.Motion,
                rng: new System.Random()
            );
        }

        if (e.Context.Turn.TurnId < 1)
        {
            return;
        }

        // Clear Player Block
        if (!powerSystem.ExistPower<TurnStartedRemainBlockPowerInstance>())
        {
            player.ChangeBlock(
                eventBus: EventBus,
                context: e.Context,
                motion: e.Motion,
                amount: -player.Block
            );
        }

        // Update Player Buff & Debuff
        List<BuffType> buffTypes = new List<BuffType>(player.Buffs.Keys);
        foreach (BuffType type in buffTypes)
        {
            player.ApplyBuff(
                eventBus: EventBus,
                context: e.Context,
                motion: e.Motion,
                type: type,
                delta: -1
            );
        }

        // Update Enemy Buff & Debuff
        foreach (EnemyInstance enemy in e.Context.Combat.Enemies)
        {
            buffTypes.Clear();
            buffTypes.AddRange(enemy.Buffs.Keys);

            foreach (BuffType type in buffTypes)
            {
                enemy.ApplyBuff(
                    eventBus: EventBus,
                    context: e.Context,
                    motion: e.Motion,
                    type: type,
                    delta: -1
                );
            }
        }
    }
    public void OnEnemyTurnStarted(EnemyTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // Clear Enemy Block
        foreach (EnemyInstance enemy in enemies.Values.Where(enemy => !enemy.IsDead))
        {
            enemy.ChangeBlock(
                eventBus: EventBus,
                context: e.Context,
                motion: e.Motion,
                amount: -enemy.Block
            );
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

        ActionSystem.ExcuteAction(source: enemy, type: ActionType.EnemyAct, (eventContext, motionContext) =>
        {
            // 애니메이션 재생
            switch (enemy.NextAction.IntentType)
            {
                case IntentType.Attack:
                case IntentType.AttackBlock:
                    EventBus.Publish<AttackPlayed>(new AttackPlayed(
                        context: eventContext,
                        motion: motionContext,
                        source: enemy
                    ));
                    break;
                default:
                    EventBus.Publish<SkillPlayed>(new SkillPlayed(
                        context: eventContext,
                        motion: motionContext,
                        source: enemy
                    ));
                    break;
            }

            List<Action> applies = new List<Action>();
            foreach (EnemyEffectSO effect in enemy.NextAction.Effects)
            {
                // 타겟 설정
                List<EntityInstance> targets = new List<EntityInstance>();
                switch (effect.TargetType)
                {
                    case IntentTarget.Player:
                        targets.Add(eventContext.Combat.Player);
                        break;
                    case IntentTarget.Self:
                        targets.Add(enemy);
                        break;
                    case IntentTarget.MemberAll:
                        targets.AddRange(eventContext.Combat.Enemies);
                        break;
                }

                // 효과 처리 로직 가져오기
                Action apply = effect.Apply(
                    eventBus: EventBus, 
                    context: eventContext, 
                    motion: motionContext, 
                    source: enemy, 
                    targets: targets
                );

                // 오류 체크
                if (apply == null)
                {
                    throw new InvalidOperationException("Effect Apply returned null Action");
                }

                applies.Add(apply);
            }

            // 효과 로직 실행
            foreach (Action apply in applies)
            {
                apply.Invoke();
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

        if (e.Source == player)
        {
            CombatEnd(e.Context, CombatState.Defeat);
        }
        else
        {
            if (e.Context.Combat.Enemies.Count > 0)
            {
                return;
            }

            CombatEnd(e.Context, CombatState.Victory);
        }

        void CombatEnd(EventContext context, CombatState result)
        {
            actionRequestQueue.Enqueue(() =>
            {
                CombatContext.state = result;

                MotionContext motionContext = new MotionContext(this);

                EventBus.Publish<CombatEnded>(new CombatEnded(
                    context: e.Context.RewriteNew(this),
                    motion: motionContext,
                    result: result
                ));

                AnimationSystem.Play(
                    context: e.Context.RewriteNew(this),
                    motion: motionContext
                );
            });
        }
    }
    private void OnDamageRequested(DamageRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EntityInstance source = e.Damage.Source as EntityInstance;
        source.ModifyDamage(e.Damage);

        EntityInstance target = e.Damage.Target as EntityInstance;
        target.ModifyDamage(e.Damage);
    }
    private void OnDamageResolved(DamageResolved e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EntityInstance instance = e.Target switch
        {
            PlayerInstance player => player,
            EnemyInstance enemy => enemies[enemy.Id],
            _ => null
        };

        if (instance == null)
        {
            return;
        }

        for (int i = 0; i < e.Repeat; i++)
        {
            if (instance.IsDead)
            {
                break;
            }

            instance.Hit(
                eventBus: EventBus, 
                context: e.Context, 
                motion: e.Motion, 
                amount: e.Amount
            );
        }
    }

    private void OnBuffResolved(BuffResolved e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EntityInstance instance = e.Target switch
        {
            PlayerInstance player => player,
            EnemyInstance enemy => enemy,
            _ => null
        };

        if (instance == null)
        {
            return;
        }

        instance.ApplyBuff(
            eventBus: EventBus,
            context: e.Context,
            motion: e.Motion,
            type: e.Type,
            delta: e.Amount
        );
    }
    private void OnBlockDeclared(BlockDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat) 
        {
            return;
        }

        EntityInstance instance = e.Target switch
        {
            PlayerInstance player => player,
            EnemyInstance enemy => enemy,
            _ => null
        };

        if (instance == null)
        {
            return;
        }

        instance.ChangeBlock(
            eventBus: EventBus,
            context: e.Context,
            motion: e.Motion,
            amount: e.Amount
        );
    }
    public void CombatStart()
    {
        CombatContext = new CombatContext(
            combatId: 0, 
            source: this,
            player: player,
            enemies: enemies.Values
        );

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

            EventBus.Publish<PlayerTurnStartRequested>(new PlayerTurnStartRequested(
                context: eventContext.RewriteNew(this),
                request: requestContext
            ));
        });
    }
    public void Save()
    {
        PlayManager.Instance.CurrentData.SetHp(player.CurrentHp, player.MaxHp);
        PlayManager.Instance.SaveData();
    }
}

public class CombatContext
{
    public CombatContext(int combatId, object source, EntityInstance player, IEnumerable<EntityInstance> enemies)
    {
        CombatId = combatId;
        Source = source;
        Player = player;
        this.enemies = new List<EntityInstance>(enemies);

        GoldReward = 0;
        state = CombatState.Wait;
    }
    public void AddGold(int amount)
    {
        GoldReward += amount;
    }
    public int CombatId { get; set; }
    public object Source { get; private set; }
    public EntityInstance Player { get; private set; }
    public int GoldReward { get; private set; }

    public CombatState state;
    public IReadOnlyList<EntityInstance> Enemies => enemies.Where(e => !e.IsDead).ToList();
    private List<EntityInstance> enemies;
}
public enum CombatState
{
    Wait,
    Combat,
    Victory,
    Defeat
}
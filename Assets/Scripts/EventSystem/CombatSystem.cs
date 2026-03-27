using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// 전투 상태, 턴 흐름, 기저 시스템 상호작용 및 이벤트 구독/해제를 총괄하는 중앙 관리 시스템
/// </summary>
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
    public MotionMonoSystem MotionSystem { get; private set; }

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
    private RecordSystem recordSystem;

    private PlayerInstance player;
    private Dictionary<Guid, EnemyInstance> enemies;
    private Queue<Action> actionRequestQueue = new Queue<Action>(); // 행동 제어 이벤트 대기열

    /// <summary>
    /// 하위 시스템 및 데이터 초기화
    /// </summary>
    public void Init(
        DamageSystem damageSystem,
        BuffSystem buffSystem,
        EnergySystem energySystem, 
        PowerSystem powerSystem,
        CardMonoSystem cardSystem, 
        UIMonoSystem uiSystem,
        CameraMonoSystem cameraSystem,
        MotionMonoSystem motionSystem,
        RelicMonoSystem relicSystem,
        GoldMonoSystem goldSystem,
        HpMonoSystem hpSystem,
        RecordSystem recordSystem,
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
        this.MotionSystem = motionSystem;
        this.relicSystem = relicSystem;
        this.goldSystem = goldSystem;
        this.hpSystem = hpSystem;
        this.recordSystem = recordSystem;

        TurnSystem.Init(motionSystem);
    }
    /// <summary>
    /// 업데이트 수행 및 큐에 쌓인 행동 순차 실행
    /// </summary>
    public void UpdateTick()
    {
        TurnSystem.UpdateTick();

        if (MotionSystem.IsPlaying)
        {
            return;
        }

        if (actionRequestQueue.Count > 0)
        {
            actionRequestQueue.Dequeue().Invoke();
        }
    }

    /// <summary>
    /// 전투 진행에 필요한 각 시스템의 이벤트 구독 일괄 등록
    /// </summary>
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
        EventBus.Subscribe<EnemyTurnStarted>(uiSystem.OnEnemyTurnStarted);
        EventBus.Subscribe<EnergyChanged>(uiSystem.OnEnergyChanged);

        EventBus.Subscribe<ActionEnded>(hpSystem.OnActionEnded);

        EventBus.Subscribe<CombatEnded>(cardSystem.OnCombatEnded);
        EventBus.Subscribe<PlayerTurnStarted>(cardSystem.OnPlayerTurnStarted);
        EventBus.Subscribe<PlayerTurnEnded>(cardSystem.OnPlayerTurnEnded);
        EventBus.Subscribe<EnergyResolved>(cardSystem.OnEnergyResolved);
        EventBus.Subscribe<DrawCardDeclared>(cardSystem.OnDrawCardDeclared);
        EventBus.Subscribe<ModifyCostDeclared>(cardSystem.OnModifyCostDeclared);

        EventBus.Subscribe<PlayerTurnStarted>(recordSystem.OnPlayerTurnStarted);
        EventBus.Subscribe<CardPlayDeclared>(recordSystem.OnCardPlayDeclared);
        EventBus.Subscribe<DeathDeclared>(recordSystem.OnDeathDeclared);

        EventBus.Subscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        EventBus.Subscribe<EnemyTurnStarted>(OnEnemyTurnStarted);
    }

    /// <summary>
    /// 등록된 이벤트 일괄 구독 해제
    /// </summary>
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
        EventBus.Unsubscribe<EnemyTurnStarted>(uiSystem.OnEnemyTurnStarted);
        EventBus.Unsubscribe<EnergyChanged>(uiSystem.OnEnergyChanged);

        EventBus.Unsubscribe<ActionEnded>(hpSystem.OnActionEnded);

        EventBus.Unsubscribe<CombatEnded>(cardSystem.OnCombatEnded);
        EventBus.Unsubscribe<PlayerTurnStarted>(cardSystem.OnPlayerTurnStarted);
        EventBus.Unsubscribe<PlayerTurnEnded>(cardSystem.OnPlayerTurnEnded);
        EventBus.Unsubscribe<EnergyResolved>(cardSystem.OnEnergyResolved);
        EventBus.Unsubscribe<DrawCardDeclared>(cardSystem.OnDrawCardDeclared);
        EventBus.Unsubscribe<ModifyCostDeclared>(cardSystem.OnModifyCostDeclared);

        EventBus.Unsubscribe<PlayerTurnStarted>(recordSystem.OnPlayerTurnStarted);
        EventBus.Unsubscribe<CardPlayDeclared>(recordSystem.OnCardPlayDeclared);
        EventBus.Unsubscribe<DeathDeclared>(recordSystem.OnDeathDeclared);

        EventBus.Unsubscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        EventBus.Unsubscribe<EnemyTurnStarted>(OnEnemyTurnStarted);
    }

    /// <summary>
    /// 단위 턴 시작 처리 (플레이어)
    /// </summary>
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 1. 적 의도(Intent) 난수 기반 결정 갱신
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
            return; // 첫 턴인 경우 아래 페이즈 생략
        }

        // 2. 플레이어 방어도 잔류 파워 확인 및 수치 초기화
        if (!powerSystem.ExistPower<TurnStartedRemainBlockPowerInstance>())
        {
            player.ChangeBlock(
                eventBus: EventBus,
                context: e.Context,
                motion: e.Motion,
                amount: -player.Block
            );
        }

        // 3. 플레이어 보유 버프/디버프 갱신
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

        // 4. 각 적 개체별 버프/디버프 갱신
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

    /// <summary>
    /// 단위 턴 시작 처리 (적 전체 대기열)
    /// </summary>
    public void OnEnemyTurnStarted(EnemyTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 전체 적 개체의 전턴 잔여 방어도 초기화
        foreach (EnemyInstance enemy in enemies.Values.Where(enemy => !enemy.IsDead))
        {
            enemy.ChangeBlock(
                eventBus: EventBus,
                context: e.Context,
                motion: e.Motion,
                amount: -enemy.Block
            );
        }

        // 첫 번째 대기열의 적 개체 행동 순서 예약 등록
        actionRequestQueue.Enqueue(() => EnemyActionStart(e.Context.Turn.EnemyQueue.Dequeue().Id));
    }

    /// <summary>
    /// 개별 적 개체 행동 처리 진입
    /// </summary>
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
            // 의도에 따른 이벤트 발행
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
                // 주기에 연결된 ScriptableObject 효과별 실제 적용 대상 배열 구성
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

                // 타겟별 실제 수치 처리 및 작동 콜백 함수 생성
                Action apply = effect.Apply(
                    eventBus: EventBus, 
                    context: eventContext, 
                    motion: motionContext, 
                    source: enemy, 
                    targets: targets
                );

                if (apply == null)
                {
                    throw new InvalidOperationException("Effect Apply returned null Action");
                }

                applies.Add(apply);
            }

            // 확정 수집된 모든 시스템 단위 효과 연계 코드 전체 일괄 실행
            foreach (Action apply in applies)
            {
                apply.Invoke();
            }
        });
    }

    /// <summary>
    /// 개별 적 행동 개시 흐름 제어
    /// </summary>
    public void OnEnemyActionStartRequested(EnemyActionStartRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 해당 개체의 행동 처리 메서드를 대기열 큐에 등록
        actionRequestQueue.Enqueue(() => EnemyActionStart(e.Enemy.Id));
    }

    /// <summary>
    /// 유닛 사망 이벤트 발생 시 판별 후 전체 전투 승/패 상태 종료 선언
    /// </summary>
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

                MotionSystem.Play(
                    context: e.Context.RewriteNew(this),
                    motion: motionContext
                );
            });
        }
    }

    /// <summary>
    /// 데미지 계산 보정 과정 진입
    /// </summary>
    private void OnDamageRequested(DamageRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 공격 주체자가 가진 데미지 증폭이나 버프 효과 확인 수치 보정
        if (e.Damage.Source is EntityInstance source)
        {
            source.ModifyDamage(e.Damage);
        }

        // 타겟이 가진 데미지 감소나 취약 등의 효과 수치 보정
        if (e.Damage.Target is EntityInstance target)
        {
            target.ModifyDamage(e.Damage);
        }
    }

    /// <summary>
    /// 확정된 최종 데미지 반영
    /// </summary>
    private void OnDamageResolved(DamageResolved e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 이벤트 타겟 개체 분배 판별 매핑 처리
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

        // 반복 횟수(타수)만큼 피격(Hit) 로직 반복, 도작 중 사망 시 조기 이탈 보호
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

    /// <summary>
    /// 최종 버프 연산 확정 시 대상 개체에 버프 수치 적용
    /// </summary>
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

    /// <summary>
    /// 방어도 증감 선언 시 대상 개체의 방어도를 갱신
    /// </summary>
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

    /// <summary>
    /// 전투 세션 인스턴스 초기화 및 가동
    /// </summary>
    public void CombatStart()
    {
        // 최상위 전투 컨텍스트 구축 및 상태 변경
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

        // 시스템 전투 시작 이벤트 발행 및 연출 모션 확보
        EventBus.Publish<CombatStarted>(new CombatStarted(
            context: eventContext, 
            motion: motionContext
        ));

        MotionSystem.Play(
            context: eventContext, 
            motion: motionContext
        );

        // 첫 번째 턴 프레임(플레이어) 진행 단계 큐 예약
        actionRequestQueue.Enqueue(() =>
        {
            RequestContext requestContext = new RequestContext(this);

            EventBus.Publish<PlayerTurnStartRequested>(new PlayerTurnStartRequested(
                context: eventContext.RewriteNew(this),
                request: requestContext
            ));
        });
    }

    /// <summary>
    /// 영구 지속 세이브 데이터 전송
    /// </summary>
    public void Save()
    {
        // 상태값 런 매니저 등록 후 물리 데이터 구축
        RunManager.Instance.CurrentData.SetHp(player.CurrentHp, player.MaxHp);
        RunManager.Instance.SaveData();
    }
}

/// <summary>
/// 특정 전투 세션의 전역 상태 및 참여 개체 데이터 집합
/// </summary>
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

/// <summary>
/// 전투 시스템의 현행 상태 및 진행 단계 분류표
/// </summary>
public enum CombatState
{
    Wait,
    Combat,
    Victory,
    Defeat
}
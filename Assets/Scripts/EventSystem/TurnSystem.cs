using System;
using System.Collections.Generic;

/// <summary>
/// 턴 전환 및 진행 흐름 관리 시스템
/// </summary>
public class TurnSystem : BaseSystem
{
    private readonly EventBus eventBus;
    public TurnSystem(EventBus eventBus)
    {   
        this.eventBus = eventBus;
    }

    public TurnContext TurnContext { get; private set; }
    private MotionMonoSystem motionSystem;

    private int turnId = 0;
    private Queue<Action> eventQueue = new Queue<Action>(); // 이벤트 처리 대기열 큐
    
    /// <summary>
    /// TurnSystem 초기화
    /// </summary>
    public void Init(MotionMonoSystem motionSystem)
    {
        this.motionSystem = motionSystem;
    }

    /// <summary>
    /// 대기열에 쌓인 이벤트(턴 전환 등)를 순차적으로 실행
    /// </summary>
    public void UpdateTick()
    {
        if (eventQueue.Count > 0)
        {
            eventQueue.Dequeue().Invoke();
        }
    }

    /// <summary>
    /// 전투 중일 때 플레이어 턴 시작 요청 처리
    /// </summary>
    public void OnPlayerTurnStartRequested(PlayerTurnStartRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        PublishPlayerTurnStarted(e);
    }

    /// <summary>
    /// 액션 종료 시 다음 턴 전개(적 턴 예약, 개별 적 행동 등) 판단
    /// </summary>
    public void OnActionEnded(ActionEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 플레이어 턴인 경우
        if (TurnContext.Phase == TurnPhase.Player)
        {
            if (e.Context.Action.Type == ActionType.PlayerTurnEnd)
            {
                eventQueue.Enqueue(() => PublishEnemyTurnStarted(e));
            }
            else if (e.Context.Action.Type == ActionType.PlayerCardPlay)
            {
            }
        }
        else // 적 턴인 경우
        {
            if (e.Context.Action.Type == ActionType.EnemyAct)
            {
                // 행동하지 않은 적이 남아있으면 다음 행동 예약, 없으면 턴 종료
                if (TurnContext.EnemyQueue.Count > 0)
                {
                    eventQueue.Enqueue(() => PublishEnemyActionStartRequested(e));
                }
                else
                {
                    eventQueue.Enqueue(() => PublishEnemyTurnEnded(e));
                }
            }
        }
    }

    /// <summary>
    /// 플레이어 턴 종료 요청 승인 및 이벤트 발행
    /// </summary>
    public void OnPlayerTurnEndRequested(PlayerTurnEndRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        e.Request.isResult = true;

        eventBus.Publish<PlayerTurnEnded>(new PlayerTurnEnded(
            context: e.Context.RewriteNew(this),
            motion: e.Motion
        ));
    }

    /// <summary>
    /// 플레이어 턴 시작 컨텍스트 생성 및 연출 큐 등록
    /// </summary>
    private void PublishPlayerTurnStarted(ICombatEvent e)
    {
        // 새 턴 컨텍스트 생성
        TurnContext = new TurnContext(
            turnId: turnId++,
            phase: TurnPhase.Player,
            source: this
        );

        // 이벤트 전달 및 모션에 담을 데이터 구성
        EventContext eventContext = new EventContext(
            source: this,
            action: null,
            turn: TurnContext,
            combat: e.Context.Combat
        ); 

        MotionContext motionContext = new MotionContext(this);

        // 턴 시작 이벤트 발행
        eventBus.Publish<PlayerTurnStarted>(new PlayerTurnStarted(
            context: eventContext,
            motion: motionContext
        ));

        // 모션 큐에 누적된 연출들 재생
        motionSystem.Play(
            context: eventContext,
            motion: motionContext
        );
    }

    /// <summary>
    /// 적 턴 시작 컨텍스트 생성, 적 행동 대기열 스택 생성 및 연출 큐 등록
    /// </summary>
    private void PublishEnemyTurnStarted(ICombatEvent e)
    {
        if (e.Context.Combat.Enemies.Count == 0)
        {
            return;
        }

        // 새 턴 컨텍스트 생성
        TurnContext = new TurnContext(
            turnId: turnId++,
            phase: TurnPhase.Enemy,
            source: this
        );

        // 생존 중인 적들을 대기열에 추가
        for (int i = 0; i < e.Context.Combat.Enemies.Count; i++)
        {
            TurnContext.EnemyQueue.Enqueue(e.Context.Combat.Enemies[i] as EnemyInstance);
        }

        // 이벤트 전달 및 모션에 담을 데이터 구성
        EventContext eventContext = new EventContext(
            source: this,
            action: null,
            turn: TurnContext,
            combat: e.Context.Combat
        ); 

        MotionContext motionContext = new MotionContext(this);

        // 적 턴 시작 이벤트 발행
        eventBus.Publish<EnemyTurnStarted>(new EnemyTurnStarted(
            context: eventContext,
            motion: motionContext
        ));

        // 모션 큐 재생
        motionSystem.Play(
            context: eventContext,
            motion: motionContext
        );
    }

    /// <summary>
    /// 적 턴 종료 이벤트 발행 및 다시 플레이어 턴 시작 예약
    /// </summary>
    private void PublishEnemyTurnEnded(ICombatEvent e)
    {
        // 이벤트 전달용 데이터 구성
        EventContext eventContext = new EventContext(
            source: this,
            action: null,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );
        MotionContext motionContext = new MotionContext(this);

        // 적 턴 종료 이벤트 발행
        eventBus.Publish<EnemyTurnEnded>(new EnemyTurnEnded(
            context: eventContext,
            motion: motionContext
        ));

        // 다음으로 이어질 플레이어 턴 시작 예약
        eventQueue.Enqueue(() => PublishPlayerTurnStarted(e));
    }

    /// <summary>
    /// 개별 적 행동 시작 요청 및 대기열(EnemyQueue) 1명 소비
    /// </summary>
    private void PublishEnemyActionStartRequested(ICombatEvent e)
    {
        // 이벤트 전달용 데이터 구성
        EventContext eventContext = new EventContext(
            source: this,
            action: null,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );

        RequestContext request = new RequestContext(source: this);

        // 대기열에서 적 하나를 빼내어 개별 행동 시작 이벤트 발행
        eventBus.Publish<EnemyActionStartRequested>(new EnemyActionStartRequested(
            context: eventContext,
            request: request,
            enemy: TurnContext.EnemyQueue.Dequeue()
        ));
    }
}

/// <summary>
/// 현재 진행 중인 턴 단위의 상태 데이터 컨텍스트
/// </summary>
public class TurnContext
{
    public TurnContext(int turnId, TurnPhase phase, object source)
    {
        TurnId = turnId;
        Phase = phase;
        Source = source;
    }
    public int TurnId { get; private set; } // 누적 진행 턴 수
    public TurnPhase Phase { get; private set; } // 플레이어 또는 적 페이즈
    public object Source { get; private set; }

    public Queue<EnemyInstance> EnemyQueue { get; private set; } = new Queue<EnemyInstance>(); // 이번 턴에 행동할 적 대기열
}

/// <summary>
/// 턴 페이즈(종류) 정의
/// </summary>
public enum TurnPhase
{
    Player, // 플레이어 턴
    Enemy   // 적 턴
}

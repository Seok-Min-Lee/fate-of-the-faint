using System;

/// <summary>
/// 게임 내 액션(행동) 처리를 관리하는 시스템
/// </summary>
public class ActionSystem : BaseSystem
{
    private readonly CombatSystem combatSystem;
    private readonly EventBus eventBus;

    public ActionSystem(EventBus eventBus, CombatSystem combatSystem) 
    {
        this.eventBus = eventBus;
        this.combatSystem = combatSystem;
    }

    /// <summary>
    /// 실제 액션 로직 실행 및 이벤트 관리
    /// </summary>
    public void ExcuteAction(object source, ActionType type, Action<EventContext, MotionContext> declared)
    {
        // 액션 컨텍스트 생성
        ActionContext action = new ActionContext(
            type: type, 
            source: source
        );

        // 이벤트 컨텍스트 생성
        EventContext eventContext = new EventContext(
            source: source,
            action: action,
            turn: combatSystem.TurnSystem.TurnContext,
            combat: combatSystem.CombatContext
        );

        // 모션 컨텍스트 생성
        MotionContext motionContext = new MotionContext(this);

        // 액션 시작 이벤트 발행
        eventBus.Publish<ActionStarted>(new ActionStarted(
            context: eventContext.RewriteNew(this),
            motion: motionContext
        ));

        // 전달받은 실제 액션(카드 사용, 턴 종료 등) 처리
        declared?.Invoke(
            arg1: eventContext.RewriteNew(source),
            arg2: motionContext
        );

        // 액션 종료 이벤트 발행
        eventBus.Publish<ActionEnded>(new ActionEnded(
            context: eventContext.RewriteNew(this),
            motion: motionContext
        ));

        // 해당 액션에서 누적된 모션 큐 재생 요청
        combatSystem.MotionSystem.Play(
            context: eventContext.RewriteNew(this),
            motion: motionContext
        );
    }
}

/// <summary>
/// 진행 중인 액션의 정보(데이터)
/// </summary>
public class ActionContext
{
    public ActionContext(ActionType type, object source)
    {
        ActionId = Guid.NewGuid();
        Type = type;
        Source = source;
    }
    public Guid ActionId { get; private set; }
    public object Source { get; private set; }   // Card, Monster, Relic 등
    public ActionType Type { get; private set; }
    public bool isCancelled = false;
}

/// <summary>
/// 액션 종류 정의
/// </summary>
public enum ActionType
{
    PlayerCardPlay, // 플레이어 카드 사용
    PlayerTurnEnd,  // 플레이어 턴 종료
    EnemyAct,       // 적 행동
}

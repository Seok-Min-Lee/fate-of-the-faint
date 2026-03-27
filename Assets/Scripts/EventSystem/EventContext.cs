using System;

/// <summary>
/// 이벤트 발생 시 전반적인 환경 상황 정보를 캡슐화한 데이터 컨텍스트
/// </summary>
public class EventContext
{
    public EventContext(object source, ActionContext action, TurnContext turn, CombatContext combat)
    {
        EventId = Guid.NewGuid();
        Source = source;
        Time = UnityEngine.Time.time;
        Action = action;
        Turn = turn;
        Combat = combat;
    }

    /// <summary>
    /// 기존 컨텍스트를 유지하면서 새로운 전파 주체로 갱신하여 반환
    /// </summary>
    public EventContext RewriteNew(object source)
    {
        return new EventContext(source, this.Action, this.Turn, this.Combat);
    }
    
    // 이벤트 자체 고유 식별자
    public Guid EventId { get; private set; }

    // 메인 시스템 흐름 계층 식별 정보 (이벤트 간 상관관계 증명)
    public CombatContext Combat { get; private set; }
    public TurnContext Turn { get; private set; }
    public ActionContext Action { get; private set; }

    // 이벤트 발생 타임라인 시점 (디버그 및 로그 추적용)
    public float Time { get; private set; }

    // 이벤트 발생 주체
    public object Source { get; private set; }
}

/// <summary>
/// 승인 가부 판별이 필요한 시스템 트리거용 요청 전파 컨텍스트
/// </summary>
public class RequestContext
{
    public RequestContext(object source)
    {
        ActionId = Guid.NewGuid();
        Source = source;
        isResult = false;
    }
    
    // 요청 단위 고유 식별자
    public Guid ActionId { get; private set; }
    // 요청 발생 및 트리거 주체
    public object Source { get; private set; }
    // 시스템 요구 심사 결과(승인/반려) 판별 플래그
    public bool isResult;
}
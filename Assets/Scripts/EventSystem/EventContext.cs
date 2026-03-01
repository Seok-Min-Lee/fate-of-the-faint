using System;
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
    public EventContext OverwriteNew(object source)
    {
        return new EventContext(source, this.Action, this.Turn, this.Combat);
    }
    // 이벤트 자체 식별
    public Guid EventId { get; private set; }

    // 흐름 식별 (상관관계 핵심)
    public CombatContext Combat { get; private set; }
    public TurnContext Turn { get; private set; }
    public ActionContext Action { get; private set; }

    // 발생 시점 정보
    public float Time { get; private set; }

    // 발생 주체 (선택)
    public object Source { get; private set; }
}
public class RequestContext
{
    public RequestContext(object source)
    {
        ActionId = Guid.NewGuid();
        Source = source;
        isResult = false;
    }
    public Guid ActionId { get; private set; }
    public object Source { get; private set; }
    public bool isResult;
}
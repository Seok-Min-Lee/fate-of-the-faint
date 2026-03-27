/// <summary>
/// 인게임 플레이 누적 통계 데이터 기록 시스템
/// </summary>
public class RecordSystem : IPlayerTurnStarted, IDeathDeclared, ICardPlayDeclared
{
    private readonly EventBus eventBus;
    
    public RecordSystem(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    /// <summary>
    /// 카드 사용 통계 누적
    /// </summary>
    public void OnCardPlayDeclared(CardPlayDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 1. 런 매니저에 누적 카드 사용 횟수 1 증가 기록
        RunManager.Instance.CurrentData.AddRecord(PlayRecordKeys.CARD_PLAY_COUNT, 1);
    }

    /// <summary>
    /// 적 처치 통계 누적
    /// </summary>
    public void OnDeathDeclared(DeathDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 1. 사망 판정 주체가 적 개체가 아닐 경우 제외
        if (e.Source is not EnemyInstance)
        {
            return;
        }

        // 2. 런 매니저에 적 처치 횟수 1 증가 기록
        RunManager.Instance.CurrentData.AddRecord(PlayRecordKeys.ENEMY_KILL_COUNT, 1);
    }

    /// <summary>
    /// 단위 턴 진행 통계 누적
    /// </summary>
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 1. 런 매니저에 턴 소모 진행 횟수 1 증가 기록
        RunManager.Instance.CurrentData.AddRecord(PlayRecordKeys.TURN_COUNT, 1);
    }
}

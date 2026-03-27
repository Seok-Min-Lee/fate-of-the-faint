using System.Collections.Generic;

/// <summary>
/// 전투 중 적용된 파워(지속 효과) 관리 시스템
/// </summary>
public class PowerSystem : BaseSystem, ICombatEnded
{
    private readonly EventBus eventBus;
    private List<PowerInstance> powers = new List<PowerInstance>(); // 현재 목록

    public PowerSystem(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    /// <summary>
    /// 전투 종료 시 모든 파워 등록 해제 및 초기화
    /// </summary>
    public void OnCombatEnded(CombatEnded e)
    {
        for (int i = 0; i < powers.Count; i++)
        {
            powers[i].Unregister();
        }
        powers.Clear();
    }

    /// <summary>
    /// 신규 파워 추가 및 이벤트 발행
    /// </summary>
    public void AddPower(PowerCardSO power)
    {
        // 파워 인스턴스 생성 및 이벤트 구독 등록
        PowerInstance newInstance = power.CreateInstance(eventBus);
        newInstance.Register();

        // 파워 목록에 추가
        powers.Add(newInstance);

        // 파워 추가 이벤트 발행
        eventBus.Publish<PowerAdded>(new PowerAdded(
            context: new EventContext(this, null, null, null),
            motion: null,
            source: newInstance
        ));
    }

    /// <summary>
    /// 특정 파워(T) 존재 여부 확인
    /// </summary>
    public bool ExistPower<T>() where T : PowerInstance
    {
        for (int i = 0; i < powers.Count; i++)
        {
            if (powers[i] is T)
            {
                return true;
            }
        }

        return false;
    }
}

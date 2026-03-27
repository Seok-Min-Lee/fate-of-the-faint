using TMPro;
using UnityEngine;

/// <summary>
/// 플레이어 체력(HP) 텍스트 UI 관리 시스템
/// </summary>
public class HpMonoSystem : BaseMonoSystem
{
    [SerializeField] private TextMeshProUGUI text;

    private EventBus eventBus;

    public void Start()
    {
        Refresh();
    }

    /// <summary>
    /// HpMonoSystem 초기화
    /// </summary>
    public void Init(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    /// <summary>
    /// 데이터 기반 체력 UI 즉시 갱신
    /// </summary>
    public void Refresh()
    {
        SetHpText(RunManager.Instance.CurrentData.CurrentHp, RunManager.Instance.CurrentData.MaxHp);
    }

    /// <summary>
    /// 액션 종료 이벤트 후 전투 중인 플레이어 체력 반영
    /// </summary>
    public void OnActionEnded(ActionEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        SetHpText(e.Context.Combat.Player.CurrentHp, e.Context.Combat.Player.MaxHp);
    }

    /// <summary>
    /// 현재 / 최대 체력 텍스트 서식 적용
    /// </summary>
    private void SetHpText(int currentHp, int maxHp)
    {
        text.text = $"{currentHp.ToString()}/{maxHp.ToString()}";
    }
}

using TMPro;
using UnityEngine;

/// <summary>
/// 재화(골드)의 획득, 소비를 관리하고 UI를 갱신하는 클래스입니다.
/// </summary>
public class GoldMonoSystem : BaseMonoSystem
{
    [SerializeField] private TextMeshProUGUI goldText;

    private EventBus eventBus;

    public void Start()
    {
        Refresh();
    }

    /// <summary>
    /// GoldMonoSystem의 의존성을 주입하고 초기화합니다.
    /// </summary>
    public void Init(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    /// <summary>
    /// 골드 획득
    /// </summary>
    public void Add(int amount)
    {
        // 데이터 상에 골드 추가
        int startAmount = RunManager.Instance.CurrentData.Gold;
        RunManager.Instance.CurrentData.AddGold(amount);

        // 갱신된 골드량 UI 반영
        int endAmount = RunManager.Instance.CurrentData.Gold;
        goldText.text = endAmount.ToString();

        // 골드 변경 이벤트 발행
        eventBus.Publish<GoldChanged>(new GoldChanged(
            context: new EventContext(this, null, null, null),
            motion: null,
            startAmount: startAmount,
            endAmount: endAmount
        ));
    }

    /// <summary>
    /// 골드 소모
    /// </summary>
    public void Substract(int amount)
    {
        // 데이터 상에 골드 소모
        int startAmount = RunManager.Instance.CurrentData.Gold;
        RunManager.Instance.CurrentData.SubtractGold(amount);

        // 갱신된 골드량 UI 반영
        int endAmount = RunManager.Instance.CurrentData.Gold;
        goldText.text = endAmount.ToString();

        // 골드 변경 이벤트 발행
        eventBus.Publish<GoldChanged>(new GoldChanged(
            context: new EventContext(this, null, null, null),
            motion: null,
            startAmount: startAmount,
            endAmount: endAmount
        ));
    }

    /// <summary>
    /// 현재 골드값으로 UI 갱신
    /// </summary>
    public void Refresh()
    {
        goldText.text = RunManager.Instance.CurrentData.Gold.ToString();
    }
}
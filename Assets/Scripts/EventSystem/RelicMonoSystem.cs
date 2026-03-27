using System;
using UnityEngine;
/// <summary>
/// 유물(Relic) 획득, 조회 및 UI 표시 관리 시스템
/// </summary>
public class RelicMonoSystem : BaseMonoSystem
{
    [SerializeField] private RelicInstanceViewPool pool;       // 유물 아이콘 UI 풀
    [SerializeField] private UIWindowManager windowManager;    // UI 윈도우 관리자

    private EventBus eventBus;
    private Action<RelicInstanceView> onClick;

    private void Start()
    {
        CreateViews();
    }

    /// <summary>
    /// RelicMonoSystem 초기화 및 유물 이벤트 구독
    /// </summary>
    public void Init(EventBus eventBus)
    {
        this.eventBus = eventBus;

        // 보유 중인 유물 이벤트 등록
        foreach (RelicInstance instance in RunManager.Instance.CurrentData.Relics)
        {
            instance.Register(eventBus);
        }

        // 유물 UI 생성
        CreateViews();

        // 생성된 유물 UI에 발동 이벤트 구독 연결
        foreach (RelicInstanceView view in pool.Actives)
        {
            eventBus.Subscribe<RelicActivated>(view.OnRelicActivated);
        }
    }

    /// <summary>
    /// 신규 유물 추가 및 등록
    /// </summary>
    public void AddRelic(RelicSO relic)
    {
        // 유물 인스턴스 생성 및 보유 데이터 추가
        RelicInstance newInstance = relic.CreateInstance();
        RunManager.Instance.CurrentData.AddRelic(newInstance);

        // 유물 UI 뷰 생성
        RelicInstanceView newView = pool.CreateView(newInstance);
        newView.AddListener(onClick);

        if (eventBus == null)
        {
            return;
        }

        // 유물 이벤트 구독 등록 및 UI 연결
        newInstance.Register(eventBus);
        eventBus.Subscribe<RelicActivated>(newView.OnRelicActivated);
        
        // 유물 획득 이벤트 발행
        eventBus.Publish<RelicAdded>(new RelicAdded(
            context: new EventContext(this, null, null, null),
            motion: null,
            source: relic
        ));
    }

    /// <summary>
    /// 유물 UI 클릭 시 상세 윈도우 팝업
    /// </summary>
    public void OnClickRelic(RelicInstanceView relic)
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        if (!windowManager.TryGetWindow(WindowType.Relic, out UIWindow window) ||
            window is not RelicWindow relicWindow)
        {
            return;
        }

        // 선택한 유물 상세 정보 바인딩
        relicWindow.Bind(relic);

        // 상세 윈도우 활성화
        if (!window.gameObject.activeSelf)
        {
            windowManager.ActivateWindow(WindowType.Relic, WindowMode.Single);
        }
    }

    /// <summary>
    /// 보유 데이터 기준 유물 UI(아이콘) 생성
    /// </summary>
    private void CreateViews()
    {
        // 이미 생성되었거나 데이터가 없으면 리턴
        if (pool.Actives.Count > 0 || RunManager.Instance.CurrentData.Relics.Count <= 0)
        {
            return;
        }

        pool.CreateViews(
            samples: RunManager.Instance.CurrentData.Relics,
            onClick: (view) => OnClickRelic(view)
        );
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 및 시스템 이벤트의 발생 순서를 추적하여 인게임 디버그 패널로 시각화하는 시스템
/// </summary>
public class DebugSystem : BaseMonoSystem
{
    [SerializeField] private CombatCtrl combatManager;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject camera;
    
    private readonly List<EventTrace> traces = new(); // 추적된 이벤트 로그 목록

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TextMeshProUGUI debugTextUI;

    private Coroutine _scrollRoutine;
    private bool bRebuild = false; // 텍스트 재구축 대기 플래그

    private void Awake()
    {
        // 1. 에디터 환경에서만 디버그 UI 활성화 처리
#if UNITY_EDITOR
        gameObject.SetActive(true);
#else
        gameObject.SetActive(false);
#endif
    }

    /// <summary>
    /// 디버그 로그 수집을 위한 전역 이벤트 게시 및 주요 지점 구독
    /// </summary>
    private void OnEnable()
    {
        if (combatManager.CombatSystem == null)
        {
            return;
        }

        combatManager.CombatSystem.EventBus.OnPublished += Record;
        combatManager.CombatSystem.EventBus.Subscribe<ActionEnded>(OnActionEnded);
        combatManager.CombatSystem.EventBus.Subscribe<EnemyTurnEnded>(OnEnemyTurnEnded);
        combatManager.CombatSystem.EventBus.Subscribe<CombatEnded>(OnCombatEnded);
        combatManager.CombatSystem.EventBus.Subscribe<AnimationEnded>(OnAnimationEnded);
    }

    /// <summary>
    /// 등록된 이벤트 일괄 구독 해제
    /// </summary>
    private void OnDisable()
    {
        if (combatManager.CombatSystem == null)
        {
            return;
        }

        combatManager.CombatSystem.EventBus.OnPublished -= Record;
        combatManager.CombatSystem.EventBus.Unsubscribe<ActionEnded>(OnActionEnded);
        combatManager.CombatSystem.EventBus.Unsubscribe<EnemyTurnEnded>(OnEnemyTurnEnded);
        combatManager.CombatSystem.EventBus.Unsubscribe<CombatEnded>(OnCombatEnded);
        combatManager.CombatSystem.EventBus.Unsubscribe<AnimationEnded>(OnAnimationEnded);
    }

    /// <summary>
    /// 플래그 확인 후 주기 말(프레임 후반) 텍스트 재구축 처리
    /// </summary>
    private void LateUpdate()
    {
        if (!bRebuild)
        {
            return; // 변경 사항 미존재 시 대기
        }

        // 1. 디버그 텍스트 재구축 및 스크롤 위치 보정 수행
        RebuildDebugText();
        bRebuild = false;
    }

    private void OnAnimationEnded(AnimationEnded e)
    {
        bRebuild = true;
    }
    
    private void OnActionEnded(ActionEnded e)
    {
        bRebuild = true;
    }
    
    private void OnEnemyTurnEnded(EnemyTurnEnded e)
    {
        bRebuild = true;
    }
    
    private void OnCombatEnded(CombatEnded e)
    {
        bRebuild = true;
    }

    /// <summary>
    /// 발생한 이벤트를 추적 포맷으로 변환하여 리스트에 등록
    /// </summary>
    public void Record(ICombatEvent e)
    {
        EventContext ctx = e.Context;

        traces.Add(new EventTrace(
            eventId: ctx.EventId,
            actionId: ctx.Action?.ActionId,
            turnId: ctx.Turn?.TurnId ?? -1,
            combatId: ctx.Combat?.CombatId ?? -1,
            source: ctx.Source?.GetType().Name ?? null,
            time: e.Context.Time,
            meta: e.Meta
        ));
    }

    /// <summary>
    /// 추적된 로그 기반 텍스트 계층 병합 및 스크롤 하단 이동 제어
    /// </summary>
    public void RebuildDebugText()
    {
        // 1. 누적 로그 리스트를 트리 구조로 변환
        DebugTree tree = DebugGrouper.BuildTree(traces);

        // 2. 변환된 트리를 텍스트로 변환하여 UI에 반영
        debugTextUI.text = BuildText(tree);

        // 3. 스크롤 위치가 최하단 부근일 경우 신규 로그를 보기 위해 자동 하단 정렬 수행
        if (scrollRect.verticalNormalizedPosition <= 0.1f)
        {
            ScrollToBottom();
        }
    }

    /// <summary>
    /// 계층 구조 컴파일 데이터를 UI 전용 문자열로 빌드
    /// </summary>
    private string BuildText(DebugTree tree)
    {
        var sb = new System.Text.StringBuilder();

        // 1. 전투(Combat) 계층 순회
        foreach (var combat in tree.Combats)
        {
            sb.AppendLine($"=== Combat {combat.CombatId} ===");

            // 2. 턴(Turn) 계층 순회
            foreach (var turn in combat.Turns)
            {
                sb.AppendLine($"  <color=#FF8080>Turn {turn.TurnId}</color>");

                // 2-1. 턴 종속 시스템별 이벤트 순회
                foreach (var te in turn.TurnEvents)
                {
                    sb.AppendLine($"      {te.ToDebugString()}");
                }

                // 3. 행동(Action) 계층 및 하위 이벤트 순회
                foreach (var action in turn.Actions)
                {
                    sb.AppendLine($"    <color=#FFFF80>Action {action.ActionId}</color>");

                    foreach (var e in action.Events)
                    {
                        sb.AppendLine($"      {e.ToDebugString()}");
                    }
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 스크롤 하단 자동 정렬 코루틴 시작 제어
    /// </summary>
    private void ScrollToBottom()
    {
        if (scrollRect == null)
        {
            return;
        }

        if (_scrollRoutine != null) 
        {
            StopCoroutine(_scrollRoutine);
        }

        _scrollRoutine = StartCoroutine(CoScrollToBottom());
    }

    /// <summary>
    /// 레이아웃 업데이트 수명 주기에 맞춘 스크롤 뷰 위치 강제 초기화 코루틴
    /// </summary>
    private IEnumerator CoScrollToBottom()
    {
        // 1. TMP/레이아웃 갱신이 한 프레임 뒤에 적용되는 케이스가 많으므로 프레임 대기
        yield return null;

        // 2. 강제로 레이아웃 갱신
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        // 3. verticalNormalizedPosition: 1 = 위, 0 = 아래 (하단 이동 반영)
        scrollRect.verticalNormalizedPosition = 0f;

        // 4. 간헐적 UI 프레임 드랍 누락 방지용 2차 갱신 체킹
        yield return null;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;

        _scrollRoutine = null;
    }
}

/// <summary>
/// 개별 이벤트 발생 로그 기록 추적 데이터 포맷
/// </summary>
public struct EventTrace
{
    public EventTrace(Guid eventId, Guid? actionId, int turnId, int combatId, string source, float time, EventMeta meta)
    {
        EventId = eventId;
        ActionId = actionId;
        TurnId = turnId;
        CombatId = combatId;
        Source = source;
        Time = time;
        Meta = meta;
    }
    
    /// <summary>
    /// 디버그 UI 노출용 포맷팅 문자열 반환
    /// </summary>
    public string ToDebugString()
    {
        return $"{Time:0.000} | {Meta.Name} ({Meta.Category}) | Src:{Source}";
    }

    public Guid EventId { get; private set; }
    public Guid? ActionId { get; private set; }
    public int TurnId { get; private set; }
    public int CombatId { get; private set; }
    public string Source { get; private set; }
    public float Time { get; private set; }
    public EventMeta Meta { get; private set; }
}
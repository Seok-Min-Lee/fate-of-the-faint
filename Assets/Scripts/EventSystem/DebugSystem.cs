using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugSystem : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;
    private readonly List<EventTrace> traces = new();

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TextMeshProUGUI debugTextUI;

    private Coroutine _scrollRoutine;
    private bool bRebuild = false;
    private void OnEnable()
    {
        combatManager.EventBus.OnPublished += Record;
        combatManager.EventBus.Subscribe<ActionEnded>(OnActionEnded);
        combatManager.EventBus.Subscribe<EnemyTurnEnded>(OnEnemyTurnEnded);
    }
    private void OnDisable()
    {
        combatManager.EventBus.OnPublished -= Record;
        combatManager.EventBus.Unsubscribe<ActionEnded>(OnActionEnded);
        combatManager.EventBus.Unsubscribe<EnemyTurnEnded>(OnEnemyTurnEnded);
    }
    private void LateUpdate()
    {
        if (!bRebuild)
        {
            return;
        }

        RebuildDebugText();
        bRebuild = false;
    }
    private void OnActionEnded(ActionEnded e)
    {
        bRebuild = true;
    }
    private void OnEnemyTurnEnded(EnemyTurnEnded e)
    {
        bRebuild = true;
    }
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
    public void RebuildDebugText()
    {
        DebugTree tree = DebugGrouper.BuildTree(traces);
        debugTextUI.text = BuildText(tree);

        if (scrollRect.verticalNormalizedPosition <= 0.1f)
        {
            ScrollToBottom();
        }
    }
    private string BuildText(DebugTree tree)
    {
        var sb = new System.Text.StringBuilder();

        foreach (var combat in tree.Combats)
        {
            sb.AppendLine($"=== Combat {combat.CombatId} ===");

            foreach (var turn in combat.Turns)
            {
                sb.AppendLine($"  <color=#FF8080>Turn {turn.TurnId}</color>");

                // Turn-level events
                foreach (var te in turn.TurnEvents)
                {
                    sb.AppendLine($"    [Turn] {te.ToDebugString()}");
                }

                // Actions
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

    private IEnumerator CoScrollToBottom()
    {
        // TMP/레이아웃 갱신이 한 프레임 뒤에 적용되는 케이스가 많아서 기다림
        yield return null;

        // 강제로 레이아웃 갱신
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        // verticalNormalizedPosition: 1 = 위, 0 = 아래
        scrollRect.verticalNormalizedPosition = 0f;

        // 혹시 또 씹히는 UI 조합이 있어 한 번 더(안정성용)
        yield return null;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;

        _scrollRoutine = null;
    }
}
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
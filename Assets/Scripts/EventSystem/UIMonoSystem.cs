using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
/// <summary>
/// 게임 이벤트 기반 UI 연출 지시하는 시스템
/// </summary>
public class UIMonoSystem : BaseMonoSystem
{
    private EventBus eventBus;
    private ActionSystem actionSystem;

    [Header("Canvas")]
    [SerializeField] private UIWindowManager windowManager;
    [SerializeField] private EnergyView energy;

    /// <summary>
    /// UIMonoSystem 초기화
    /// </summary>
    public void Init(EventBus eventBus, ActionSystem actionSystem)
    {
        this.eventBus = eventBus;
        this.actionSystem = actionSystem;
    }
    /// <summary>
    /// 전투 시작 UI 연출 등록
    /// </summary>
    public void OnCombatStarted(CombatStarted e)
    {
        if (!windowManager.TryGetWindow(WindowType.Combat, out UIWindow window) ||
            window is not CombatWindow combatWindow)
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Window,
            command: combatWindow.GetMotion(MotionKey.CombatStarted),
            source: this
        ));
    }
    /// <summary>
    /// 전투 결과창 연출 등록
    /// </summary>
    public void OnCombatEnded(CombatEnded e)
    {
        UIWindow window;
        Func<IEnumerator> process;

        // 승리 결과창 표시
        if (e.Context.Combat.state == CombatState.Victory &&
            windowManager.TryGetWindow(WindowType.Victory, out window) &&
            window is VictoryWindow victoryWindow)
        {
            victoryWindow.Init(e.Context.Combat.GoldReward);

            process = victoryWindow.GetMotion(MotionKey.WindowShow);
        }
        // 패배 결과창 표시
        else if (e.Context.Combat.state == CombatState.Defeat &&
                 windowManager.TryGetWindow(WindowType.Defeat, out window) &&
                 window is DefeatWindow defeatWindow)
        {
            process = defeatWindow.GetMotion(MotionKey.WindowShow);
        }
        else
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Window,
            command: process,
            source: this
        ));
    }
    /// <summary>
    /// 플레이어 턴 시작 UI 연출 등록
    /// </summary>
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (!windowManager.TryGetWindow(WindowType.Combat, out UIWindow window) ||
            window is not CombatWindow combatWindow)
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Window,
            command: combatWindow.GetMotion(MotionKey.PlayerTurnStarted),
            source: this
        ));
    }
    /// <summary>
    /// 적 턴 시작 UI 연출 등록
    /// </summary>
    public void OnEnemyTurnStarted(EnemyTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (!windowManager.TryGetWindow(WindowType.Combat, out UIWindow window) ||
            window is not CombatWindow combatWindow)
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Window,
            command: combatWindow.GetMotion(MotionKey.EnemyTurnStarted),
            source: this
        ));
    }
    /// <summary>
    /// 에너지 UI 갱신 연출 재생
    /// </summary>
    public void OnEnergyChanged(EnergyChanged e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Actor,
            command: () => energy.ChangeCor(e.EndAmount, e.MaxAmount),
            source: this
        ));
    }
    /// <summary>
    /// 카드 더미 조회 UI 활성화
    /// </summary>
    public void OnClickCardDisplay()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        windowManager.ActivateWindow(WindowType.CardDisplay, WindowMode.Single);
    }

    /// <summary>
    /// 전체 지도 UI 활성화
    /// </summary>
    public void OnClickMap()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        windowManager.ActivateWindow(WindowType.Map, WindowMode.Single);
    }

    /// <summary>
    /// 게임 설정 UI 활성화
    /// </summary>
    public void OnClickSetting()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        windowManager.ActivateWindow(WindowType.Setting, WindowMode.Single);
    }

    /// <summary>
    /// 턴 종료 액션 요청
    /// </summary>
    public void OnClickReturn()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        actionSystem.ExcuteAction(source: this, type: ActionType.PlayerTurnEnd, (eventContext, motionContext) =>
        {
            RequestContext requestContext = new RequestContext(source: this);

            eventBus.Publish<PlayerTurnEndRequested>(new PlayerTurnEndRequested(
                context: eventContext,
                motion: motionContext,
                request: requestContext
            ));
        });
    }
}

using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
public class UIMonoSystem : BaseMonoSystem
{
    private EventBus eventBus;
    private ActionSystem actionSystem;

    [Header("Canvas")]
    [SerializeField] private UIWindowManager windowManager;
    [SerializeField] private EnergyView energy;

    public void Init(EventBus eventBus, ActionSystem actionSystem)
    {
        this.eventBus = eventBus;
        this.actionSystem = actionSystem;
    }
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
    public void OnCombatEnded(CombatEnded e)
    {
        UIWindow window;
        Func<IEnumerator> process;

        if (e.Context.Combat.state == CombatState.Victory &&
            windowManager.TryGetWindow(WindowType.Victory, out window) &&
            window is VictoryWindow victoryWindow)
        {
            victoryWindow.Init(e.Context.Combat.GoldReward);

            process = victoryWindow.GetMotion(MotionKey.WindowShow);
        }
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
    public void OnClickCardDisplay()
    {
        windowManager.ActivateWindow(WindowType.CardDisplay, WindowMode.Single);
    }
    public void OnClickMap()
    {
        windowManager.ActivateWindow(WindowType.Map, WindowMode.Single);
    }
    public void OnClickSetting()
    {
        windowManager.ActivateWindow(WindowType.Setting, WindowMode.Single);
    }
    public void OnClickReturn()
    {
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

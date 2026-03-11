using DG.Tweening;
using System.Collections;
using UnityEngine;
public class UIMonoSystem : BaseMonoSystem
{
    private EventBus eventBus;
    private ActionSystem actionSystem;

    [Header("Canvas")]
    [SerializeField] private UIWindowManager windowManager;
    [SerializeField] private EnergyView energy;

    private Sequence sequence;
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
            command: () => CombatStartedAnimationCor(combatWindow),
            source: this
        ));
    }
    public void OnCombatEnded(CombatEnded e)
    {
        UIWindow window;

        if (e.Context.Combat.state == CombatState.Victory &&
            windowManager.TryGetWindow(WindowType.Victory, out window) &&
            window is VictoryWindow victoryWindow)
        {
            victoryWindow.Init(e.Context.Combat.GoldReward);

            e.Motion.AddTask(new MotionTask(
                priority: MotionPriority.Window,
                command: () => CombatEndedVictoryAnimationCor(victoryWindow),
                source: this
            ));
        }
        else if (e.Context.Combat.state == CombatState.Defeat &&
                 windowManager.TryGetWindow(WindowType.Defeat, out window) &&
                 window is DefeatWindow defeatWindow)
        {
            e.Motion.AddTask(new MotionTask(
                priority: MotionPriority.Window,
                command: () => CombatEndedDefeatWindowAnimationCor(defeatWindow),
                source: this
            ));
        }
        else
        {
            return;
        }
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
            command: () => PlayerTurnStartedMotionCor(combatWindow),
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
            command: () => EnemyTurnStartedAnimationCor(combatWindow),
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
    IEnumerator CombatStartedAnimationCor(CombatWindow combatWindow)
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() => windowManager.ActivateWindow(WindowType.Combat, WindowMode.Single));
        sequence.Append(combatWindow.AnnounceCombat());

        yield return sequence.WaitForCompletion();
    }
    IEnumerator CombatEndedVictoryAnimationCor(VictoryWindow victoryWindow)
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.Append(victoryWindow.GetMotion(MotionKey.WindowShow));

        yield return sequence.WaitForCompletion();
    }
    IEnumerator CombatEndedDefeatWindowAnimationCor(DefeatWindow defeatWindow)
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.Append(defeatWindow.GetMotion(MotionKey.WindowShow));

        yield return sequence.WaitForCompletion();
    }
    IEnumerator PlayerTurnStartedMotionCor(CombatWindow combatWindow)
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.Append(combatWindow.PlayerTurnAnnounce());
        sequence.Append(combatWindow.FadeIn());

        yield return sequence.WaitForCompletion();
    }
    IEnumerator EnemyTurnStartedAnimationCor(CombatWindow combatWindow)
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.Append(combatWindow.FadeOut());
        sequence.Append(combatWindow.EnemyTurnAnnounce());

        yield return sequence.WaitForCompletion();
    }
}

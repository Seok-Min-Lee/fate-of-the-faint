using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class UIMonoSystem : BaseMonoSystem
{
    private EventBus eventBus;
    private ActionSystem actionSystem;

    [Header("Canvas")]
    [SerializeField] private Transform windowParent;

    [SerializeField] private EnergyView energy;
    [SerializeField] private TextMeshProUGUI drawPile;
    [SerializeField] private TextMeshProUGUI discardPile;

    private Dictionary<WindowType, UIWindow> windowDictionary = new Dictionary<WindowType, UIWindow>();
    private Stack<HashSet<WindowType>> windowSnapshot = new Stack<HashSet<WindowType>>();

    private Sequence sequence;
    private void Awake()
    {
        foreach (UIWindow window in windowParent.GetComponentsInChildren<UIWindow>(true))
        {
            if (!windowDictionary.ContainsKey(window.Type))
            {
                windowDictionary.Add(window.Type, window);
                window.ChangeWindow = ChangeWindow;
            }
        }
    }
    private void Start()
    {
        ChangeWindow(WindowType.Combat, WindowMode.Single);
    }
    private void ChangeWindow(WindowType source, WindowMode mode)
    {
        UIWindow window;
        HashSet<WindowType> snapshot;
        if (mode == WindowMode.Revert)
        {
            if (windowSnapshot.Count > 0)
            {
                snapshot = windowSnapshot.Pop();
                foreach (KeyValuePair<WindowType, UIWindow> kvp in windowDictionary)
                {
                    kvp.Value.gameObject.SetActive(snapshot.Contains(kvp.Key));
                }
            }
        }
        else
        {
            snapshot = new HashSet<WindowType>();
            foreach (KeyValuePair<WindowType, UIWindow> kvp in windowDictionary)
            {
                if (kvp.Value.gameObject.activeSelf)
                {
                    snapshot.Add(kvp.Key);
                }
            }
            windowSnapshot.Push(snapshot);

            if (windowDictionary.TryGetValue(source, out window))
            {
                if (mode == WindowMode.Single)
                {
                    foreach (UIWindow w in windowDictionary.Values)
                    {
                        w.gameObject.SetActive(false);
                    }
                }

                window.gameObject.SetActive(true);
            }
        }
    }
    public void Init(EventBus eventBus, ActionSystem actionSystem)
    {
        this.eventBus = eventBus;
        this.actionSystem = actionSystem;
    }
    public void OnCombatStarted(CombatStarted e)
    {
        if (!windowDictionary.TryGetValue(WindowType.Combat, out UIWindow window))
        { 
            return;
        }
        CombatWindow combatWindow = window as CombatWindow;

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
            windowDictionary.TryGetValue(WindowType.Victory, out window))
        {
            VictoryWindow victoryWindow = window as VictoryWindow;

            e.Motion.AddTask(new MotionTask(
                priority: MotionPriority.Window,
                command: () => CombatEndedVictoryAnimationCor(victoryWindow),
                source: this
            ));
        }
        else if (e.Context.Combat.state == CombatState.Defeat &&
                 windowDictionary.TryGetValue(WindowType.Defeat, out window))
        {
            DefeatWindow defeatWindow = window as DefeatWindow;

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

        if (!windowDictionary.TryGetValue(WindowType.Combat, out UIWindow window))
        {
            return;
        }
        CombatWindow combatWindow = window as CombatWindow;

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

        if (!windowDictionary.TryGetValue(WindowType.Combat, out UIWindow window))
        {
            return;
        }
        CombatWindow combatWindow = window as CombatWindow;

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Window,
            command: () => EnemyTurnStartedAnimationCor(combatWindow),
            source: this
        ));
    }
    public void OnPlayerTurnEnded(PlayerTurnEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }
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
        if (!windowDictionary.TryGetValue(WindowType.CardDisplay, out UIWindow window) ||
            window.gameObject.activeSelf)
        {
            return;
        }

        ChangeWindow(WindowType.CardDisplay, WindowMode.Single);
    }
    public void OnClickMap()
    {
        if (!windowDictionary.TryGetValue(WindowType.Map, out UIWindow window) ||
            window.gameObject.activeSelf)
        {
            return;
        }
        ChangeWindow(WindowType.Map, WindowMode.Single);
    }
    public void OnClickSetting()
    {
        if (!windowDictionary.TryGetValue(WindowType.Setting, out UIWindow window) ||
            window.gameObject.activeSelf)
        {
            return;
        }
        ChangeWindow(WindowType.Setting, WindowMode.Single);
    }
    public void OnClickReturn()
    {
        actionSystem.ExcuteAction(source: this, type: ActionType.PlayerTurnEnd, (eventContext, animationContext) =>
        {
            RequestContext requestContext = new RequestContext(source: this);

            eventBus.Publish<PlayerTurnEndRequested>(new PlayerTurnEndRequested(
                context: eventContext,
                motion: animationContext,
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

        sequence.AppendCallback(() => ChangeWindow(WindowType.Combat, WindowMode.Single));
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

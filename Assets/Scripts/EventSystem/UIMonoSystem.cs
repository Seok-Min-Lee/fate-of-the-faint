using DG.Tweening;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
public class UIMonoSystem : MonoBehaviour
{
    private ActionSystem actionSystem;
    private EventBus eventBus;

    [Header("Canvas")]
    [SerializeField] private Transform windowParent;
    //[SerializeField] private CombatWindow combatWindow;
    //[SerializeField] private DefeatWindow defeatWindow;
    //[SerializeField] private VictoryWindow victoryWindow;
    //[SerializeField] private CardRewardWindow cardRewardWindow;
    //[SerializeField] private CardDisplayWindow cardDisplayWindow;

    [SerializeField] private TextMeshProUGUI energy;
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

        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() => ChangeWindow(WindowType.Combat, WindowMode.Single));
        sequence.AppendCallback(() => eventBus.Publish(new AnimationStarted(CreateContext(e.Context))));
        sequence.Append(combatWindow.AnnounceCombat());
        sequence.AppendCallback(() => eventBus.Publish(new AnimationEnded(CreateContext(e.Context))));
    }
    public void OnCombatEnded(CombatEnded e)
    {
        UIWindow window;
        Sequence mainProcess = null;
        if (e.Context.Combat.state == CombatState.Victory && 
            windowDictionary.TryGetValue(WindowType.Victory, out window))
        {
            VictoryWindow victoryWindow = window as VictoryWindow;
            mainProcess = victoryWindow.GetMotion(MotionKey.WindowShow);
        }
        else if (e.Context.Combat.state == CombatState.Defeat &&
                 windowDictionary.TryGetValue(WindowType.Defeat, out window))
        {
            DefeatWindow defeatWindow = window as DefeatWindow;
            mainProcess = defeatWindow.GetMotion(MotionKey.WindowShow);
        }
        else
        {
            return;
        }


        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() => eventBus.Publish(new AnimationStarted(CreateContext(e.Context))));
        sequence.Append(mainProcess);
        sequence.AppendCallback(() => eventBus.Publish(new AnimationEnded(CreateContext(e.Context))));
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

        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() => eventBus.Publish(new AnimationStarted(CreateContext(e.Context))));
        sequence.Append(combatWindow.PlayerTurnAnnounce());
        sequence.AppendCallback(() => eventBus.Publish(new AnimationEnded(CreateContext(e.Context))));
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

        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() => eventBus.Publish(new AnimationStarted(CreateContext(e.Context))));
        sequence.Append(combatWindow.EnemyTurnAnnounce());
        sequence.AppendCallback(() => eventBus.Publish(new AnimationEnded(CreateContext(e.Context))));
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

        energy.text = e.EndAmount.ToString();
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
        ActionContext actionContext = new ActionContext(source: this, type: ActionType.PlayerTurnEnd);

        actionSystem.ExcuteAction(actionContext, (eventContext) =>
        {
            RequestContext requestContext = new RequestContext(source: this);

            eventBus.Publish<PlayerTurnEndRequested>(new PlayerTurnEndRequested(
                context: eventContext,
                request: requestContext
            ));
        });
    }
    private EventContext CreateContext(EventContext context)
    {
        return new EventContext(
            source: this,
            action: context.Action,
            turn: context.Turn,
            combat: context.Combat
        );
    }
}

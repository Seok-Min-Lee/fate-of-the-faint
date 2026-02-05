using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class UISystem : MonoBehaviour
{
    private ActionSystem actionSystem;
    private EventBus eventBus;

    [Header("World")]
    [SerializeField] private TextMeshPro playerHp;
    [SerializeField] private TextMeshPro enemyHp;

    [Header("Canvas")]
    [SerializeField] private CanvasGroup combatUICG;
    [SerializeField] private GameObject playerTurnAlarm;
    [SerializeField] private GameObject rewardWindow;
    [SerializeField] private GameObject defeatWindow;

    [SerializeField] private TextMeshProUGUI energy;
    [SerializeField] private TextMeshProUGUI drawPile;
    [SerializeField] private TextMeshProUGUI discardPile;

    private Sequence sequence;
    public void Init(EventBus eventBus, ActionSystem actionSystem)
    {
        this.eventBus = eventBus;
        this.actionSystem = actionSystem;
    }
    public void OnCombatStarted(CombatStarted e)
    {
        EventContext eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );

        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            eventBus.Publish<AnimationStarted>(new AnimationStarted(context: eventContext));
            combatUICG.alpha = 0f;
            combatUICG.blocksRaycasts = false;
            combatUICG.DOFade(1f, 1f);
        });
        sequence.AppendInterval(1f);
        sequence.AppendCallback(() =>
        {
            combatUICG.blocksRaycasts = true;
            eventBus.Publish<AnimationEnded>(new AnimationEnded(context: eventContext));
        });
    }
    public void OnCombatEnded(CombatEnded e)
    {
        Action<EventContext> preProcess, postProcess;
        float interval = 0;
        if (e.Context.Combat.state == CombatState.Victory)
        {
            preProcess = (eventContext) =>
            {
                eventBus.Publish<AnimationStarted>(new AnimationStarted(context: eventContext));
                rewardWindow.SetActive(true);
            };
            interval = 0f;
            postProcess = (eventContext) =>
            {
                eventBus.Publish<AnimationEnded>(new AnimationEnded(context: eventContext));
            };
        }
        else if (e.Context.Combat.state == CombatState.Defeat)
        {
            preProcess = (eventContext) =>
            {
                eventBus.Publish<AnimationStarted>(new AnimationStarted(context: eventContext));
                defeatWindow.SetActive(true);
            };
            interval = 0f;
            postProcess = (eventContext) =>
            {
                eventBus.Publish<AnimationEnded>(new AnimationEnded(context: eventContext));
            };
        }
        else
        {
            return;
        }

        EventContext eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );

        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() => preProcess(eventContext));
        sequence.AppendInterval(interval);
        sequence.AppendCallback(() => postProcess(eventContext));
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EventContext eventContext = new EventContext(
            source: this,
            action: e.Context.Action,
            turn: e.Context.Turn,
            combat: e.Context.Combat
        );

        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            eventBus.Publish<AnimationStarted>(new AnimationStarted(context: eventContext));
            playerTurnAlarm.SetActive(true);
            combatUICG.blocksRaycasts = false;
        });
        sequence.AppendInterval(0.5f);
        sequence.AppendCallback(() =>
        {
            playerTurnAlarm.SetActive(false);
            combatUICG.blocksRaycasts = true;
            eventBus.Publish<AnimationEnded>(new AnimationEnded(context: eventContext));
        });
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
    public void OnClickReturn()
    {
        TryPlayerTurnEnd();
    }
    public void OnClickGoldButton()
    {
        //PlayManager.Instance.CurrentData.AddGold();
    }
    public void OnClickRelicButton()
    {
        //PlayManager.Instance.CurrentData.AddRelic();
    }
    public void OnClickCardButton()
    {

    }
    public void OnClickRewardCard()
    {
        //PlayManager.Instance.CurrentData.AddCard();
    }
    public void OnClickRewardEnd()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MAP);
    }
    public void OnClickDefeatEnd()
    {
        PlayManager.Instance.ClearPlayData();
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.HOME);
    }
    private void TryPlayerTurnEnd()
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
}

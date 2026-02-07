using DG.Tweening;
using System;
using System.Runtime.CompilerServices;
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
    [SerializeField] private CombatWindow combatWindow;
    [SerializeField] private DefeatWindow defeatWindow;
    [SerializeField] private VictoryWindow victoryWindow;

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
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() => eventBus.Publish(new AnimationStarted(CreateContext(e.Context))));
        sequence.Append(combatWindow.AnnounceCombat());
        sequence.AppendCallback(() => eventBus.Publish(new AnimationEnded(CreateContext(e.Context))));
    }
    public void OnCombatEnded(CombatEnded e)
    {
        Sequence mainProcess = null;

        if (e.Context.Combat.state == CombatState.Victory)
        {
            mainProcess = victoryWindow.GetMotion(MotionKey.WindowShow);
        }
        else if (e.Context.Combat.state == CombatState.Defeat)
        {
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
    public void OnClickReturn()
    {
        TryPlayerTurnEnd();
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

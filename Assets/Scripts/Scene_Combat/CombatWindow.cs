using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class CombatWindow : UIMotionWindow
{
    [SerializeField] private CanvasGroup combatUICG;
    [SerializeField] private CombatAnnouncer combatAnnouncer;
    [SerializeField] private TurnAnnouncer turnAnnouncer;

    private Sequence sequence;
    protected override void Awake()
    {
        _handler.Add(MotionKey.CombatStarted, StartCombat());
        _handler.Add(MotionKey.PlayerTurnStarted, StartPlayerTurn());
        _handler.Add(MotionKey.EnemyTurnStarted, StartEnemyTurn());
    }
    private Func<IEnumerator> StartCombat()
    {
        return () => AnnounceCombatCor();
    }
    private Func<IEnumerator> StartPlayerTurn()
    {
        return () => StartPlayerTurnCor();
    }
    private Func<IEnumerator> StartEnemyTurn()
    {
        return () => StartEnemyTurnCor();
    }
    private IEnumerator AnnounceCombatCor()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() => ChangeWindow(WindowType.Combat, WindowMode.Single));
        sequence.Append(combatAnnouncer.Announce());

        yield return sequence.WaitForCompletion();
    }
    private IEnumerator StartPlayerTurnCor()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.Append(turnAnnouncer.PlayerTurnAnnounce());
        sequence.Append(FadeIn());

        yield return sequence.WaitForCompletion();
    }
    private IEnumerator StartEnemyTurnCor()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.Append(FadeOut());
        sequence.Append(turnAnnouncer.EnemyTurnAnnounce());

        yield return sequence.WaitForCompletion();
    }
    private Sequence FadeIn()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(combatUICG.DOFade(1f, 0.5f));
        sequence.AppendCallback(() => combatUICG.blocksRaycasts = true);

        return sequence;
    }
    private Sequence FadeOut()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(() => combatUICG.blocksRaycasts = false);
        sequence.Append(combatUICG.DOFade(0f, 0.5f));

        return sequence;
    }
}

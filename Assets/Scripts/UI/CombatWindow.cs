using DG.Tweening;
using UnityEngine;

public class CombatWindow : UIMotionWindow
{
    [SerializeField] private CanvasGroup combatUICG;
    [SerializeField] private CombatAnnouncer combatAnnouncer;
    [SerializeField] private TurnAnnouncer turnAnnouncer;

    private Sequence sequence;
    private void Awake()
    {
        _handler.Add(MotionKey.CombatAnnounce, AnnounceCombat);
        _handler.Add(MotionKey.PlayerTurnAnnounce, PlayerTurnAnnounce);
        _handler.Add(MotionKey.FadeIn, FadeIn);
        _handler.Add(MotionKey.FadeOut, FadeOut);
    }
    public Sequence AnnounceCombat()
    {
        return combatAnnouncer.Announce();
    }
    public Sequence PlayerTurnAnnounce()
    {
        return turnAnnouncer.PlayerTurnAnnounce();
    }
    public Sequence EnemyTurnAnnounce()
    {
        return turnAnnouncer.EnemyTurnAnnounce();
    }
    public Sequence FadeIn()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.Append(combatUICG.DOFade(1f, 0.5f));
        sequence.AppendCallback(() => combatUICG.blocksRaycasts = true);

        return sequence;
    }
    public Sequence FadeOut()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() => combatUICG.blocksRaycasts = false);
        sequence.Append(combatUICG.DOFade(0f, 0.5f));

        return sequence;
    }
}

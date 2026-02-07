using DG.Tweening;
using UnityEngine;

public class CombatWindow : UIWindow
{
    [SerializeField] private CombatAnnouncer combatAnnouncer;
    [SerializeField] private TurnAnnouncer turnAnnouncer;

    private void Awake()
    {
        _handler.Add(MotionKey.CombatAnnounce, AnnounceCombat);
        _handler.Add(MotionKey.PlayerTurnAnnounce, PlayerTurnAnnounce);
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
}

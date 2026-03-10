using UnityEngine;

public class RecordSystem : IPlayerTurnStarted, IDeathDeclared, ICardPlayDeclared
{
    private readonly EventBus eventBus;
    
    public RecordSystem(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    public void OnCardPlayDeclared(CardPlayDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        PlayManager.Instance.CurrentData.AddRecord(PlayRecordKeys.CARD_PLAY_COUNT, 1);
    }

    public void OnDeathDeclared(DeathDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Source is not EnemyInstance)
        {
            return;
        }

        PlayManager.Instance.CurrentData.AddRecord(PlayRecordKeys.ENEMY_KILL_COUNT, 1);
    }

    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        PlayManager.Instance.CurrentData.AddRecord(PlayRecordKeys.TURN_COUNT, 1);
    }
}

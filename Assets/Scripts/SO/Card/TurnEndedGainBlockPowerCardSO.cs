using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PWR_TurnEndedGainBlock_", menuName = "Scriptable Objects/CardSO/Turn Ended Gain Block Power Card")]
public class TurnEndedGainBlockPowerCardSO : PowerCardSO
{
    [SerializeField] private int amount;
    public int Amount => amount;
    public override PowerInstance CreateInstance(EventBus eventBus) 
    {
        return new TurnEndedGainBlockPowerCardInstance(eventBus, this);
    }
}

public class TurnEndedGainBlockPowerCardInstance : PowerInstance, IPlayerTurnEnded
{
    private readonly int amount;
    public TurnEndedGainBlockPowerCardInstance(EventBus eventBus, TurnEndedGainBlockPowerCardSO origin) : base(eventBus, origin)
    {
        amount = origin.Amount;
    }
    public override void Register()
    {
        EventBus.Subscribe<PlayerTurnEnded>(OnPlayerTurnEnded);
    }
    public override void Unregister()
    {
        EventBus.Unsubscribe<PlayerTurnEnded>(OnPlayerTurnEnded);
    }
    public void OnPlayerTurnEnded(PlayerTurnEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        Activate(e.Context, e.Motion, () =>
        {
            EntityInstance player = e.Context.Combat.Player;
            int startAmount = player.Block;
            player.AddBlock(amount);

            EventBus.Publish<BlockChanged>(new BlockChanged(
                context: e.Context.RewriteNew(this), 
                motion: e.Motion,
                target: e.Context.Combat.Player,
                startAmount: startAmount,
                endAmount: player.Block
            ));
        });
    }
}
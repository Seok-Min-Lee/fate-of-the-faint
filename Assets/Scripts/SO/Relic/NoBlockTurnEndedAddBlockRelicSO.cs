using UnityEngine;

[CreateAssetMenu(fileName = "No Block Turn Ended Add Block Relic ", menuName = "Scriptable Objects/Relic/No Block Turn Ended Add Block Relic ")]
public class NoBlockTurnEndedAddBlockRelicSO : RelicSO
{
    [SerializeField] private int amount;
    public int Amount => amount;
    public override RelicInstance CreateInstance(EventBus eventBus)
    {
        return new NoBlockThisTurnRelicInstance(eventBus: eventBus, origin: this);
    }
}
public class NoBlockThisTurnRelicInstance : RelicInstance, IPlayerTurnEnded
{
    private int amount;
    public NoBlockThisTurnRelicInstance(EventBus eventBus, NoBlockTurnEndedAddBlockRelicSO origin) : base(eventBus, origin)
    {
        amount = origin.Amount;
    }
    public override void Register()
    {
        EventBus.Subscribe<PlayerTurnEnded>(OnPlayerTurnEnded);
    }
    public void OnPlayerTurnEnded(PlayerTurnEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EntityInstance player = e.Context.Combat.Player;

        if (player.Block != 0)
        {
            return;
        }

        Activate(e.Context, e.Motion, () =>
        {
            int startAmount = player.Block;
            player.SetBlock(startAmount + amount);

            EventBus.Publish<BlockChanged>(new BlockChanged(
                context: e.Context.RewriteNew(this),
                motion: e.Motion,
                target: player,
                startAmount: startAmount,
                endAmount: player.Block
            ));
        });
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "pwr_turnEndedEntityDamage_", menuName = "Scriptable Objects/CardSO/Turn Ended Entity Damage Power Card")]
public class TurnEndedEntityDamagePowerCardSO : PowerCardSO
{
    [SerializeField] private int playerDamage;
    [SerializeField] private int enemyDamage;
    public int PlayerDamage => playerDamage;
    public int EnemyDamage => enemyDamage;
    public override PowerInstance CreateInstance(EventBus eventBus)
    {
        return new TurnEndedEntityDamagePowerInstance(eventBus, this);
    }
    protected override string GetDescription()
    {
        string[] values = new string[]
        {
            playerDamage.ToString(), 
            enemyDamage.ToString()
        };

        return string.Format(description, values)
                     .Replace("[", "<color=#00FF40>")
                     .Replace("]", "</color>");
    }
}
public class TurnEndedEntityDamagePowerInstance : PowerInstance, IPlayerTurnEnded
{
    private int playerDamage;
    private int enemyDamage;
    public TurnEndedEntityDamagePowerInstance(EventBus eventBus, TurnEndedEntityDamagePowerCardSO origin) : base(eventBus, origin)
    {
        playerDamage = origin.PlayerDamage;
        enemyDamage = origin.EnemyDamage;
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
            e.Context.Combat.Player.Damage(
                eventBus: EventBus,
                context: e.Context,
                motion: e.Motion,
                amount: playerDamage
            );

            foreach (EntityInstance enemy in e.Context.Combat.Enemies)
            {
                enemy.Hit(
                    eventBus: EventBus,
                    context: e.Context,
                    motion: e.Motion,
                    amount: enemyDamage
                );
            }
        });
    }
}

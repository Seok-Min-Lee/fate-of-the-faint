using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "pwr_blockAddedRandomEnemyDamage_", menuName = "Scriptable Objects/CardSO/Block Added Random Enemy Damage Power Card")]
public class BlockAddedRandomEnemyDamagePowerCardSO : PowerCardSO
{
    [SerializeField] private int amount;
    public int Amount => amount;
    public override PowerInstance CreateInstance(EventBus eventBus)
    {
        return new BlockAddedRandomEnemyDamagePowerInstance(eventBus, this);
    }
}
public class BlockAddedRandomEnemyDamagePowerInstance : PowerInstance, IBlockChanged
{
    private int amount;
    public BlockAddedRandomEnemyDamagePowerInstance(EventBus eventBus, BlockAddedRandomEnemyDamagePowerCardSO origin) : base(eventBus, origin)
    {
        amount = origin.Amount;
    }
    public override void Register()
    {
        EventBus.Subscribe<BlockChanged>(OnBlockChanged);
    }
    public override void Unregister()
    {
        EventBus.Unsubscribe<BlockChanged>(OnBlockChanged);
    }
    public void OnBlockChanged(BlockChanged e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }
        if (e.Target is not PlayerInstance)
        {
            return;
        }
        if (e.EndAmount <= e.StartAmount)
        {
            return;
        }

        Activate(e.Context, e.Motion, () =>
        {
            IReadOnlyList<EntityInstance> enemies = e.Context.Combat.Enemies;
            EntityInstance target = enemies[Random.Range(0, enemies.Count)];

            EventBus.Publish<AttackDeclared>(new AttackDeclared(
                context: e.Context.RewriteNew(this),
                motion: e.Motion,
                source: this,
                target: target,
                amount: amount,
                repeat: 1
            ));
        });
    }
}

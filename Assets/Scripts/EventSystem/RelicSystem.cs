using UnityEngine;
public class RelicSystem : MonoBehaviour
{
    public Relic relic;

    private EventBus eventBus;
    private PlayerView player;
    public void Init(EventBus eventBus, PlayerView player)
    {
        this.eventBus = eventBus;
        this.player = player;
    }
    public void OnDamageRequested(DamageRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Damage.Source == player)
        {
            e.Damage.Add(relic.strength, relic);
        }
    }
}

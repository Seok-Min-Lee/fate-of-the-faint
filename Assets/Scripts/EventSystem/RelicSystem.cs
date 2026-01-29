using UnityEngine;
public class RelicSystem : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private Player player;
    public Relic relic;
    private void OnEnable()
    {
        combatManager.EventBus.Subscribe<DamageRequested>(OnDamageRequested);
    }
    private void OnDisable()
    {
        combatManager.EventBus.Unsubscribe<DamageRequested>(OnDamageRequested);
    }

    private void OnDamageRequested(DamageRequested e)
    {
        if (e.Damage.Source == player)
        {
            e.Damage.Add(relic.strength, relic);
        }
    }
}

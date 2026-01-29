using TMPro;
using UnityEngine;

public class UISystem : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private Player player;

    [Header("World")]
    [SerializeField] private TextMeshPro playerHp;
    [SerializeField] private TextMeshPro enemyHp;

    [Header("Canvas")]
    [SerializeField] private TextMeshProUGUI energy;
    [SerializeField] private TextMeshProUGUI drawPile;
    [SerializeField] private TextMeshProUGUI discardPile;
    private void OnEnable()
    {
        combatManager.EventBus.Subscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        combatManager.EventBus.Subscribe<PlayerTurnEnded>(OnPlayerTurnEnded);
        combatManager.EventBus.Subscribe<EnergyChanged>(OnEnergyChanged);
    }
    private void OnDisable()
    {
        combatManager.EventBus.Unsubscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        combatManager.EventBus.Subscribe<PlayerTurnEnded>(OnPlayerTurnEnded);
        combatManager.EventBus.Unsubscribe<EnergyChanged>(OnEnergyChanged);
    }
    private void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        // draw card
    }
    private void OnPlayerTurnEnded(PlayerTurnEnded e)
    {
    }
    private void OnEnergyChanged(EnergyChanged e)
    {
        energy.text = e.EndAmount.ToString();
    }
    public void OnClickReturn()
    {
        TryPlayerTurnEnd();
    }
    private void TryPlayerTurnEnd()
    {
        ActionContext actionContext = new ActionContext(source: player, type: ActionType.PlayerTurnEnd);

        combatManager.ExcuteAction(actionContext, (eventContext) =>
        {
            RequestContext requestContext = new RequestContext(source: player);

            combatManager.EventBus.Publish<PlayerTurnEndRequested>(new PlayerTurnEndRequested(
                context: eventContext,
                request: requestContext
            ));
        });
    }
}

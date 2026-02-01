using TMPro;
using UnityEngine;

public class EnemyView : Entity
{
    [SerializeField] private SpriteRenderer intentRenderer;
    [SerializeField] private TextMeshPro intentText;
    [SerializeField] private TextMeshPro hpText;

    public EnemyInstance Instance { get; private set; }
    private CombatManager combatManager;
    private ITargetable player;

    public int hp;
    public int strength;
    public int shield;
    public EnemyIntent intent;
    public int intentValue;

    public bool IsDeath => hp <= 0;

    private void OnEnable()
    {
        combatManager.EventBus.Subscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        combatManager.EventBus.Subscribe<EnemyActionStartRequested>(OnEnemyActionStartRequested);
        combatManager.EventBus.Subscribe<DamageRequested>(OnDamageRequested);
        combatManager.EventBus.Subscribe<DamageResolved>(OnDamageResolved);
    }
    private void OnDisable()
    {
        combatManager.EventBus.Unsubscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        combatManager.EventBus.Unsubscribe<EnemyActionStartRequested>(OnEnemyActionStartRequested);
        combatManager.EventBus.Unsubscribe<DamageRequested>(OnDamageRequested);
        combatManager.EventBus.Unsubscribe<DamageResolved>(OnDamageResolved);
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        shield = 0;

        if (Random.Range(0, 2) == 0)
        {
            intent = EnemyIntent.Attack;
            //intentValue = attackPower;

            //intentRenderer.sprite = attackIcon;
            intentText.text = intentValue.ToString();
        }
        else
        {
            intent = EnemyIntent.Defend;
            //intentValue = shieldPower;

            //intentRenderer.sprite = shieldIcon;
            intentText.text = intentValue.ToString();
        }

        intentRenderer.gameObject.SetActive(true);
        intentText.gameObject.SetActive(true);
    }
    public void OnEnemyActionStartRequested(EnemyActionStartRequested e)
    {
        if (e.Enemy != this)
        {
            return;
        }
        e.Request.isResult = true;

        ActionContext actionContext = new ActionContext(
            source: this,
            type: ActionType.EnemyAct
        );
        combatManager.ExcuteAction(actionContext, (eventContext) =>
        {
            EventContext context = new EventContext(
                source: this,
                action: e.Context.Action,
                turn: e.Context.Turn,
                combat: e.Context.Combat
            );

            switch (intent)
            {
                case EnemyIntent.Attack:
                    combatManager.EventBus.Publish<AttackDeclared>(new AttackDeclared(
                        context: context,
                        source: this,
                        target: player,
                        amount: intentValue
                    ));
                    intentRenderer.gameObject.SetActive(false);
                    intentText.gameObject.SetActive(false);
                    break;

                case EnemyIntent.Defend:
                    combatManager.EventBus.Publish<ShieldDeclared>(new ShieldDeclared(
                        context: context,
                        source: this,
                        target: this,
                        amount: intentValue
                    ));
                    intentRenderer.gameObject.SetActive(false);
                    intentText.gameObject.SetActive(false);

                    shield += intentValue;
                    break;

                default:
                    break;
            }
        });
    }
    private void OnDamageRequested(DamageRequested e)
    {
        if (e.Damage.Target == this as ITargetable)
        {
            e.Damage.Subtract(shield, this);
        }
    }
    private void OnDamageResolved(DamageResolved e)
    {
        if (e.Target == this as ITargetable)
        {
            int startAmount = hp;
            hp -= e.Amount;

            hpText.text = hp.ToString();

            EventContext eventContext = new EventContext(
                source: this, 
                action: e.Context.Action,
                turn: combatManager.TurnSystem.TurnContext,
                combat: combatManager.CombatContext
            );

            combatManager.EventBus.Publish<HpChanged>(new HpChanged(
                context: eventContext,
                target: this,
                startAmount: startAmount,
                endAmount: hp
            ));

            if (hp <= 0)
            {
                hp = 0;

                eventContext = new EventContext(
                    source: this,
                    action: e.Context.Action,
                    turn: combatManager.TurnSystem.TurnContext,
                    combat: combatManager.CombatContext
                );

                combatManager.EventBus.Publish<DeathDeclared>(new DeathDeclared(
                    context: eventContext,
                    target: this
                ));

                Destroy(gameObject);
            }
        }
    }
    public void Init(EnemyInstance instance, ITargetable player, Vector3 position, CombatManager combat)
    {
        Instance = instance;
        this.player = player;
        transform.position = position;
        combatManager = combat;
    }
}
public enum EnemyIntent
{
    Attack,
    Defend,
    Buff,
    Debuff,
    None
}

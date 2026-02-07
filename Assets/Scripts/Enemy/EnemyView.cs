using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
public class EnemyView : Entity
{
    [SerializeField] private SpriteRenderer intentRenderer;
    [SerializeField] private TextMeshPro intentText;
    [SerializeField] private TextMeshPro hpText;

    [SerializeField] private Animator animator;

    public EnemyInstance Instance { get; private set; }
    private CombatManager combatManager;
    private ITargetable player;

    private EnemyActionSO nextAction;

    public int hp = 100;
    public int strength;
    public int block;
    public IntentType intent;
    public int intentValue;

    public bool IsDeath => hp <= 0;
    private void OnDisable()
    {
        combatManager.CombatSystem.EventBus.Unsubscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        combatManager.CombatSystem.EventBus.Unsubscribe<EnemyActionStartRequested>(OnEnemyActionStartRequested);
        combatManager.CombatSystem.EventBus.Unsubscribe<DamageRequested>(OnDamageRequested);
        combatManager.CombatSystem.EventBus.Unsubscribe<DamageResolved>(OnDamageResolved);
    }
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        nextAction = Instance.Data.aiPolicy.actions[Random.Range(0, Instance.Data.aiPolicy.actions.Count)];
        intent = nextAction.IntentType;
        intentRenderer.sprite = nextAction.IntentIcon;
        intentValue = nextAction.Effects[0].value;
        intentText.text = intentValue.ToString();

        intentRenderer.gameObject.SetActive(true);
        intentText.gameObject.SetActive(true);
    }
    public void OnEnemyActionStartRequested(EnemyActionStartRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Enemy != this)
        {
            return;
        }
        e.Request.isResult = true;

        ActionContext actionContext = new ActionContext(
            source: this,
            type: ActionType.EnemyAct
        );

        combatManager.CombatSystem.ActionSystem.ExcuteAction(actionContext, (eventContext) =>
        {
            foreach (IntentEffect effect in nextAction.Effects)
            {
                List<ITargetable> targets = new List<ITargetable>();
                switch (effect.targetType)
                {
                    case IntentTarget.Player:
                        targets.Add(player);
                        break;
                    case IntentTarget.Self:
                        targets.Add(this);
                        break;
                    case IntentTarget.Member:
                        //targets.Add(target);
                        break;
                    case IntentTarget.MemberAll:
                        //targets.AddRange(combatSystem.liveEnemies);
                        break;
                }

                switch (effect.effectType)
                {
                    case EffectType.Attack:
                        combatManager.CombatSystem.EventBus.Publish<AttackDeclared>(new AttackDeclared(
                            context: eventContext,
                            source: this,
                            target: player,
                            amount: intentValue
                        ));
                        intentRenderer.gameObject.SetActive(false);
                        intentText.gameObject.SetActive(false);
                        animator.SetTrigger(AnimatorTriggers.ENEMY_ATTACK);
                        break;
                    case EffectType.Block:
                        combatManager.CombatSystem.EventBus.Publish<BlockDeclared>(new BlockDeclared(
                            context: eventContext,
                            source: this,
                            target: this,
                            amount: intentValue
                        ));
                        intentRenderer.gameObject.SetActive(false);
                        intentText.gameObject.SetActive(false);
                        animator.SetTrigger(AnimatorTriggers.ENEMY_SKILL);
                        break;
                    case EffectType.Strengthen:
                        break;
                    case EffectType.Weaken:
                        break;
                    case EffectType.Vulnerable:
                        break;
                    default:
                        return;
                }
            }
        });
    }
    private void OnDamageRequested(DamageRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Damage.Target == this as ITargetable)
        {
            e.Damage.Subtract(block, this);
        }
    }
    private void OnDamageResolved(DamageResolved e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Target == this as ITargetable)
        {
            int startAmount = hp;
            hp -= e.Amount;

            hpText.text = hp.ToString();

            EventContext eventContext = new EventContext(
                source: this, 
                action: e.Context.Action,
                turn: e.Context.Turn,
                combat: e.Context.Combat
            );

            combatManager.CombatSystem.EventBus.Publish<HpChanged>(new HpChanged(
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
                    turn: e.Context.Turn,
                    combat: e.Context.Combat
                );

                combatManager.CombatSystem.EventBus.Publish<DeathDeclared>(new DeathDeclared(
                    context: eventContext,
                    target: this
                ));

                Destroy(gameObject);
            }
            else
            {
                animator.SetTrigger(AnimatorTriggers.ENEMY_HIT);
            }
        }
    }
    public void Init(EnemyInstance instance, ITargetable player, Vector3 position, CombatManager combat)
    {
        Instance = instance;
        this.player = player;
        transform.position = position;
        combatManager = combat;

        combatManager.CombatSystem.EventBus.Subscribe<PlayerTurnStarted>(OnPlayerTurnStarted);
        combatManager.CombatSystem.EventBus.Subscribe<EnemyActionStartRequested>(OnEnemyActionStartRequested);
        combatManager.CombatSystem.EventBus.Subscribe<DamageRequested>(OnDamageRequested);
        combatManager.CombatSystem.EventBus.Subscribe<DamageResolved>(OnDamageResolved);

        hp = Instance.MaxHp;
        hpText.text = hp.ToString();
    }
}

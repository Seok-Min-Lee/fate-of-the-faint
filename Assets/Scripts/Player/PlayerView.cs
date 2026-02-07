using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerView : Entity
{
    [SerializeField] private TextMeshPro hpText;
    [SerializeField] private Animator animator;

    public int hp;
    public int strength;
    public int block;

    private CombatManager combatManager;
    private PlayerInstance instance;
    private void OnDisable()
    {
        combatManager.CombatSystem.EventBus.Unsubscribe<PlayerTurnEnded>(OnPlayerTurnEnded);
        combatManager.CombatSystem.EventBus.Unsubscribe<AttackDeclared>(OnAttackDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<BlockDeclared>(OnBlockDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<DamageRequested>(OnDamageRequested);
        combatManager.CombatSystem.EventBus.Unsubscribe<DamageResolved>(OnDamageResolved);
    }
    public void Init(PlayerInstance instance, CombatManager combatManager, Vector3 position)
    {
        this.combatManager = combatManager;
        this.instance = instance;

        transform.position = position;

        combatManager.CombatSystem.EventBus.Subscribe<PlayerTurnEnded>(OnPlayerTurnEnded);
        combatManager.CombatSystem.EventBus.Subscribe<AttackDeclared>(OnAttackDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<BlockDeclared>(OnBlockDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<DamageRequested>(OnDamageRequested);
        combatManager.CombatSystem.EventBus.Subscribe<DamageResolved>(OnDamageResolved);

        //hp = instance.CurrentHp;
        hp = 100;
        hpText.text = hp.ToString();
    }
    public void OnPlayerTurnEnded(PlayerTurnEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }
    }
    public void OnAttackDeclared(AttackDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Source != this)
        {
            return;
        }
        animator.SetTrigger(AnimatorTriggers.PLAYER_ATTACK);
    }
    public void OnBlockDeclared(BlockDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Target != this)
        {
            return;
        }
    }
    private void OnDamageRequested(DamageRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Damage.Source == this)
        {
            e.Damage.Add(strength, this);
        }
        else if (e.Damage.Target == this as ITargetable)
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
            hp = Mathf.Max(0, hp - e.Amount);

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
                animator.SetTrigger(AnimatorTriggers.PLAYER_DIE);
            }
            else
            {
                animator.SetTrigger(AnimatorTriggers.PLAYER_HIT);
            }
        }
    }
}

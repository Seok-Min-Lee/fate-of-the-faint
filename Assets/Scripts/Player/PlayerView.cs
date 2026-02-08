using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour, ITargetable
{
    [SerializeField] private Transform aimPoint;

    [SerializeField] private TextMeshPro hpText;
    [SerializeField] private Animator animator;

    private CombatManager combatManager;
    private PlayerInstance instance;
    public Transform AimPoint => aimPoint;
    public EntityInstance Instance => instance;
    private void OnDisable()
    {
        combatManager.CombatSystem.EventBus.Unsubscribe<AttackDeclared>(OnAttackDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<BlockDeclared>(OnBlockDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<HpChanged>(OnHpChanged);
        combatManager.CombatSystem.EventBus.Unsubscribe<DeathDeclared>(OnDeathDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<CombatStarted>(OnCombatStarted);
        combatManager.CombatSystem.EventBus.Unsubscribe<CombatEnded>(OnCombatEnded);
    }
    public void Init(PlayerInstance instance, CombatManager combatManager, Vector3 position)
    {
        this.combatManager = combatManager;
        this.instance = instance;

        transform.position = position;

        combatManager.CombatSystem.EventBus.Subscribe<AttackDeclared>(OnAttackDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<BlockDeclared>(OnBlockDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<HpChanged>(OnHpChanged);
        combatManager.CombatSystem.EventBus.Subscribe<DeathDeclared>(OnDeathDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<CombatStarted>(OnCombatStarted);
        combatManager.CombatSystem.EventBus.Subscribe<CombatEnded>(OnCombatEnded);

        hpText.text = instance.CurrentHp.ToString();
        animator.SetBool(AnimationKeys.PLAYER_ENCOUNTER, true);
    }
    public void OnAttackDeclared(AttackDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Source != instance)
        {
            return;
        }
        animator.SetTrigger(AnimationKeys.PLAYER_ATTACK);
    }
    public void OnBlockDeclared(BlockDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Target != instance)
        {
            return;
        }
        animator.SetTrigger(AnimationKeys.PLAYER_SKILL);
    }
    public void OnHpChanged(HpChanged e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Target.Id != instance.Id)
        {
            return;
        }

        if (e.EndAmount < e.StartAmount)
        {
            animator.SetTrigger(AnimationKeys.PLAYER_HIT);
        }

        hpText.text = e.EndAmount.ToString();
    }
    public void OnDeathDeclared(DeathDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Source.Id != instance.Id)
        {
            return;
        }

        animator.SetTrigger(AnimationKeys.PLAYER_DIE);
    }
    public void OnCombatStarted(CombatStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        animator.SetBool(AnimationKeys.PLAYER_ENCOUNTER, false);
    }
    public void OnCombatEnded(CombatEnded e)
    {
        if (e.Result != CombatState.Victory)
        {
            return;
        }

        animator.SetTrigger(AnimationKeys.PLAYER_VICTORY);
    }
}

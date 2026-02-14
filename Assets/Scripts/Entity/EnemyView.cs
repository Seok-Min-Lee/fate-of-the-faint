using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
public class EnemyView : EntityView, ITargetable
{
    public Transform AimPoint => aimPoint;
    public EntityInstance Instance => instance;

    [SerializeField] private SpriteRenderer intentRenderer;
    [SerializeField] private TextMeshPro intentText;

    private EnemyInstance instance;
    private CombatManager combatManager;
    private AnimationMonoSystem animationSystem;

    public void Init(EnemyInstance instance, CombatManager combatManager, AnimationMonoSystem animationSystem, Vector3 position, EntityBuffViewPool buffViewPool)
    {
        this.instance = instance;
        this.combatManager = combatManager;
        this.animationSystem = animationSystem;
        base.buffViewPool = buffViewPool;

        transform.position = position;

        combatManager.CombatSystem.EventBus.Subscribe<CombatStarted>(OnCombatStarted);
        combatManager.CombatSystem.EventBus.Subscribe<CombatEnded>(OnCombatEnded);
        combatManager.CombatSystem.EventBus.Subscribe<DeathDeclared>(OnDeathDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<AttackDeclared>(OnAttackDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<BlockDeclared>(OnBlockDeclared);
        combatManager.CombatSystem.EventBus.Subscribe<EnemyIntentDecided>(OnEnemyIntentDecided);
        combatManager.CombatSystem.EventBus.Subscribe<HpChanged>(OnHpChanged);
        combatManager.CombatSystem.EventBus.Subscribe<BlockChanged>(OnBlockChanged);
        combatManager.CombatSystem.EventBus.Subscribe<BuffChanged>(OnBuffChanged);
    }
    private void OnDisable()
    {
        combatManager.CombatSystem.EventBus.Unsubscribe<CombatStarted>(OnCombatStarted);
        combatManager.CombatSystem.EventBus.Unsubscribe<CombatEnded>(OnCombatEnded);
        combatManager.CombatSystem.EventBus.Unsubscribe<DeathDeclared>(OnDeathDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<AttackDeclared>(OnAttackDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<BlockDeclared>(OnBlockDeclared);
        combatManager.CombatSystem.EventBus.Unsubscribe<EnemyIntentDecided>(OnEnemyIntentDecided);
        combatManager.CombatSystem.EventBus.Unsubscribe<HpChanged>(OnHpChanged);
        combatManager.CombatSystem.EventBus.Unsubscribe<BlockChanged>(OnBlockChanged);
        combatManager.CombatSystem.EventBus.Unsubscribe<BuffChanged>(OnBuffChanged);
    }
    public void OnCombatStarted(CombatStarted e)
    {
        hpText.text = instance.CurrentHp.ToString();
    }
    public void OnCombatEnded(CombatEnded e)
    {
        if (e.Result != CombatState.Defeat)
        {
            return;
        }

        animationSystem.Enqueue(() => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_VICTORY, 0f));
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

        animationSystem.Enqueue(() => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_DIE, 1f));
    }
    public void OnAttackDeclared(AttackDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Source != instance)
        {
            return;
        }
        intentRenderer.gameObject.SetActive(false);
        intentText.gameObject.SetActive(false);
        animationSystem.Enqueue(() => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_ATTACK, 1f));
    }
    public void OnBlockDeclared(BlockDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Target != instance)
        {
            return;
        }

        animationSystem.Enqueue(() => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_SKILL, 1f));
    }
    public void OnEnemyIntentDecided(EnemyIntentDecided e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        intentRenderer.sprite = instance.NextAction.IntentIcon;
        intentText.text = instance.NextAction.Effects[0].value.ToString();

        intentRenderer.gameObject.SetActive(true);
        intentText.gameObject.SetActive(true);
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

        if (e.EndAmount > 0 && e.EndAmount < e.StartAmount)
        {
            animationSystem.Enqueue(() => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_HIT, 1f));
        }

        hpText.text = e.EndAmount.ToString();
    }
    public void OnBlockChanged(BlockChanged e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Target.Id != instance.Id)
        {
            return;
        }

        if (e.EndAmount > e.StartAmount)
        {
            animationSystem.Enqueue(() => ShowBlockCor());
        }
        else
        {
            animationSystem.Enqueue(() => ChagneBlockCor());

            if (instance.Block <= 0)
            {
                animationSystem.Enqueue(() => HideBlockCor());
            }
        }
    }
    public void OnBuffChanged(BuffChanged e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Target.Id != instance.Id)
        {
            return;
        }

        if (buffViewDictionary.TryGetValue(e.Type, out EntityBuffView value))
        {
            if (e.EndAmount > 0)
            {
                value.SetText(e.EndAmount.ToString());
            }
            else
            {
                buffViewPool.Push(value);
                buffViewDictionary.Remove(e.Type);
            }
        }
        else
        {
            if (e.EndAmount > 0)
            {
                EntityBuffView view = buffViewPool.Pop();

                for (int i = 0; i < buffPresets.Length; i++)
                {
                    if (buffPresets[i].Type == e.Type)
                    {
                        view.Init(
                            preset: buffPresets[i],
                            text: e.EndAmount.ToString(),
                            parent: buffParent
                        );

                        buffViewDictionary.Add(view.Type, view);
                        break;
                    }
                }
            }
        }
    }
    private IEnumerator PlayAnimatorTriggerCor(string key, float duration)
    {
        animator.SetTrigger(key);
        yield return new WaitForSeconds(duration);
    }
    private IEnumerator ShowBlockCor()
    {
        yield return blockView.Show(instance.Block.ToString()).WaitForCompletion();
    }
    private IEnumerator HideBlockCor()
    {
        yield return blockView.Hide().WaitForCompletion();
    }
    private IEnumerator ChagneBlockCor()
    {
        yield return blockView.Change(instance.Block.ToString()).WaitForCompletion();
    }
}

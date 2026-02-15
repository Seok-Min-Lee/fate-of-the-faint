using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
public class EnemyView : EntityView, ITargetable
{
    [SerializeField] private IntentView intentView;
    public Transform AimPoint => aimPoint;
    public EntityInstance Instance => instance;

    private EnemyInstance instance;
    private CombatManager combatManager;
    private AnimationMonoSystem animationSystem;

    public void Init(
        EnemyInstance instance,
        CombatManager combatManager, 
        AnimationMonoSystem animationSystem, 
        Vector3 position, 
        EntityBuffViewPool buffViewPool,
        DamageTextPool damageTextPool
    )
    {
        this.instance = instance;
        this.combatManager = combatManager;
        this.animationSystem = animationSystem;
        base.buffViewPool = buffViewPool;
        base.damageTextPool = damageTextPool;

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

        statusCG.alpha = 0f;
        hpView.Init(instance.CurrentHp, instance.MaxHp);
        blockView.Init(instance.Block);
        intentView.gameObject.SetActive(false);
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
        animationSystem.Enqueue(() => ShowStatusCor());
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
        animationSystem.Enqueue(() => HideStatusCor());
    }
    public void OnAttackDeclared(AttackDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Source != instance)
        {
            return;
        }

        animationSystem.Enqueue(() => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_ATTACK, 1f));
        animationSystem.Enqueue(() => HideIntentCor());
    }
    public void OnBlockDeclared(BlockDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat ||
            e.Target != instance)
        {
            return;
        }

        animationSystem.Enqueue(() => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_SKILL, 1f));
        animationSystem.Enqueue(() => HideIntentCor());
    }
    public void OnEnemyIntentDecided(EnemyIntentDecided e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        if (e.Source.Id != instance.Id)
        {
            return;
        }

        animationSystem.Enqueue(() => ShowIntentCor(
            sprite: instance.NextAction.IntentIcon,
            text: instance.NextAction.Effects[0].value.ToString()
        ));
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
            if (e.EndAmount > 0)
            {
                animationSystem.Enqueue(() => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_HIT, 1f));
            }

            DamageText damageText = damageTextPool.Pop();
            damageText.Spawn(
                text: (e.StartAmount - e.EndAmount).ToString(),
                parent: statusCG.transform,
                pool: damageTextPool
            );
        }

        animationSystem.Enqueue(() => ChangeHpCor(instance.CurrentHp, instance.MaxHp));
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
            animationSystem.Enqueue(() => ShowBlockCor(instance.Block));
        }
        else
        {
            animationSystem.Enqueue(() => ChangeBlockCor(instance.Block));

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
    private IEnumerator ShowIntentCor(Sprite sprite, string text)
    {
        yield return intentView.Show(sprite, text).WaitForCompletion();
    }
    private IEnumerator HideIntentCor()
    {
        yield return intentView.Hide().WaitForCompletion();
    }
}

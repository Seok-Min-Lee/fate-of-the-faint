using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityView : MonoBehaviour
{
    [SerializeField] protected Transform aimPoint;
    [SerializeField] protected Animator animator;
    [SerializeField] protected EntityParticleManager particle;

    [SerializeField] protected CanvasGroup statusCG;
    [SerializeField] protected EntityBlockView blockView;
    [SerializeField] protected EntityHpView hpView;

    [SerializeField] protected EntityBuffPreset[] buffPresets;
    [SerializeField] protected Transform buffParent;

    protected Dictionary<BuffType, EntityBuffView> buffViewDictionary = new Dictionary<BuffType, EntityBuffView>();
    protected EntityBuffViewPool buffViewPool;
    protected DamageTextPool damageTextPool;

    protected EntityInstance instance;
    public void OnHpChanged(HpChanged e)
    {
        // 전투 종료 후 유물이나 파워에 의해 회복할 수 있음
        //if (e.Context.Combat.state != CombatState.Combat)
        //{
        //    return;
        //}

        if (e.Target.Id != instance.Id)
        {
            return;
        }

        if (e.StartAmount == e.EndAmount)
        {
            return;
        }

        int currentHp = instance.CurrentHp;
        int maxHp = instance.MaxHp;

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Entity,
            command: () => e.StartAmount > e.EndAmount ? 
                           HitCor(e.StartAmount, e.EndAmount, currentHp, maxHp) : 
                           HealCor(currentHp, maxHp),
            source: this
        ));
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

        if (e.StartAmount == e.EndAmount)
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Actor,
            command: () => e.EndAmount > e.StartAmount ? 
                           AddBlockCor(e.EndAmount) : 
                           SubstractBlockCor(e.StartAmount, e.EndAmount),
            source: this
        ));

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

        Func<IEnumerator> process;
        if (!buffViewDictionary.TryGetValue(e.Type, out EntityBuffView view))
        {
            if (e.EndAmount <= 0)
            {
                return;
            }

            process = () => ShowBuffCor(
                buffType: e.Type,
                amount: e.EndAmount
            );
        }
        else
        {
            if (e.EndAmount > 0)
            {
                process = () => ChangeBuffCor(
                    view: view, 
                    buffType: e.Type, 
                    startAmount: e.StartAmount, 
                    endAmount: e.EndAmount
                );
            }
            else
            {
                process = () => HideBuffCor(
                    view: view,
                    buffType: e.Type
                );
            }
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Actor,
            command: process,
            source: this
        ));

    }
    protected IEnumerator ShowStatusCor()
    {
        statusCG.gameObject.SetActive(true);
        statusCG.DOFade(1f, 1f);
        yield return null;
    }

    IEnumerator HealCor(int currentHp, int maxHp)
    {
        // 파티클
        particle.Play(EntityParticleKey.Heal);
        yield return null;

        // UI
        hpView.Change(currentHp, maxHp);
        yield return null;
    }
    IEnumerator HitCor(int startAmount, int endAmount, int currentHp, int maxHp)
    {
        // 데미지 표시
        DamageText damageText = damageTextPool.Pop();

        damageText.Spawn(
            text: (startAmount - endAmount).ToString(),
            parent: statusCG.transform,
            pool: damageTextPool
        );
        yield return null;

        // 피격 모션
        if (endAmount > 0)
        {
            yield return PlayAnimatorTriggerCor(AnimationKeys.ENEMY_HIT);
        }

        // UI
        hpView.Change(currentHp, maxHp);
        yield return null;
    }
    IEnumerator AddBlockCor(int amount)
    {
        // 파티클
        particle.Play(EntityParticleKey.Block);
        yield return null;

        // UI
        blockView.Show(amount);
        yield return null;
    }

    IEnumerator SubstractBlockCor(int startAmount, int endAmount)
    {
        // UI
        blockView.Change(endAmount);
        yield return null;

        // 남은 경우 UI 유지
        if (endAmount > 0)
        {
            yield break;
        }

        // 소진한 경우 UI 숨기기
        blockView.Hide();
        yield return null;
    }
    IEnumerator ShowBuffCor(BuffType buffType, int amount)
    {
        // 파티클
        EntityParticleKey key = buffType switch
        {
            BuffType.Strength => EntityParticleKey.Buff,
            BuffType.Weak or BuffType.Vulnerable => EntityParticleKey.Debuff,
            _ => EntityParticleKey.None
        };
        particle.Play(key);
        yield return null;

        // UI
        EntityBuffView view = buffViewPool.Pop();

        for (int i = 0; i < buffPresets.Length; i++)
        {
            if (buffPresets[i].Type == buffType)
            {
                view.Init(
                    preset: buffPresets[i],
                    text: amount.ToString(),
                    parent: buffParent
                );

                buffViewDictionary.Add(view.Type, view);
                break;
            }
        }

        view.Show();
        yield return null;
    }

    IEnumerator HideBuffCor(EntityBuffView view, BuffType buffType)
    {
        buffViewPool.Push(view);
        buffViewDictionary.Remove(buffType);
        yield return null;
    }

    IEnumerator ChangeBuffCor(EntityBuffView view, BuffType buffType, int startAmount, int endAmount)
    {
        // 파티클
        if (endAmount > startAmount)
        {
            EntityParticleKey key = buffType switch
            {
                BuffType.Strength => EntityParticleKey.Buff,
                BuffType.Weak or BuffType.Vulnerable => EntityParticleKey.Debuff,
                _ => EntityParticleKey.None
            };
            particle.Play(key);
            yield return null;
        }

        // UI
        view.Change(endAmount.ToString());
        yield return null;
    }
    protected IEnumerator DeathCor(string key)
    {
        // 사망 모션
        yield return PlayAnimatorTriggerCor(key);

        // UI
        statusCG.DOFade(0f, 1f).OnComplete(() => statusCG.gameObject.SetActive(false));
        yield return null;
    }
    protected IEnumerator PlayAnimatorTriggerCor(string key, float duration = 0.25f)
    {
        animator.SetTrigger(key);
        yield return new WaitForSeconds(duration);
    }
}

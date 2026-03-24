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
            priority: MotionPriority.Target,
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
            priority: MotionPriority.Target,
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
            if (e.EndAmount <= 0 || !TryGetBuffPreset(e.Type, out EntityBuffPreset preset))
            {
                return;
            }

            // UI 생성
            view = buffViewPool.Pop();
            view.Init(
                preset: preset,
                text: e.EndAmount.ToString(),
                parent: buffParent
            );
            buffViewDictionary.Add(e.Type, view);

            // 모션
            process = () => ShowBuffCor(
                particleKey: GetParticleKey(e.Type),
                view: view
            );
        }
        else
        {
            if (e.EndAmount > 0)
            {
                process = () => ChangeBuffCor(
                    particleKey: GetParticleKey(e.Type),
                    view: view,
                    startAmount: e.StartAmount,
                    endAmount: e.EndAmount
                );
            }
            else
            {
                process = () => HideBuffCor(view);
            }
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Target,
            command: process,
            source: this
        ));
    }
    protected bool TryGetBuffPreset(BuffType key, out EntityBuffPreset preset)
    {
        for (int i = 0; i < buffPresets.Length; i++)
        {
            if (buffPresets[i].Type == key)
            {
                preset = buffPresets[i];
                return true;
            }
        }

        preset = default;
        return false;
    }
    protected EntityParticleKey GetParticleKey(BuffType buff)
    {
        EntityParticleKey key;

        switch (buff)
        {
            case BuffType.Strength:
                key = EntityParticleKey.Buff;
                break;

            case BuffType.Weak:
            case BuffType.Vulnerable:
                key = EntityParticleKey.Debuff;
                break;

            default:
                key = EntityParticleKey.None;
                break;
        }

        return key;
    }
    protected IEnumerator ShowStatusCor()
    {
        statusCG.gameObject.SetActive(true);
        statusCG.DOFade(1f, 1f);
        yield return null;
    }

    protected IEnumerator HealCor(int currentHp, int maxHp)
    {
        // 파티클
        particle.Play(EntityParticleKey.Heal);
        yield return null;

        // UI
        hpView.Change(currentHp, maxHp);
        yield return null;
    }
    protected IEnumerator HitCor(int startAmount, int endAmount, int currentHp, int maxHp)
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
    protected IEnumerator AddBlockCor(int amount)
    {
        // 파티클
        particle.Play(EntityParticleKey.Block);
        yield return null;

        // UI
        blockView.Show(amount);
        yield return null;
    }

    protected IEnumerator SubstractBlockCor(int startAmount, int endAmount)
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
    protected IEnumerator ShowBuffCor(EntityParticleKey particleKey, EntityBuffView view)
    {
        // 파티클
        particle.Play(particleKey);
        yield return null;

        // UI
        view.Show();
        yield return null;
    }

    protected IEnumerator HideBuffCor(EntityBuffView view)
    {
        buffViewPool.Push(view);
        buffViewDictionary.Remove(view.Type);
        yield return null;
    }

    protected IEnumerator ChangeBuffCor(EntityParticleKey particleKey, EntityBuffView view, int startAmount, int endAmount)
    {
        // 파티클
        if (endAmount > startAmount)
        {
            particle.Play(particleKey);
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
        statusCG.DOFade(0f, 1f)
                .OnComplete(() => statusCG.gameObject.SetActive(false));
        yield return null;
    }
    protected IEnumerator PlayAnimatorTriggerCor(string key, float duration = 0.25f)
    {
        animator.SetTrigger(key);
        yield return new WaitForSeconds(duration);
    }
}

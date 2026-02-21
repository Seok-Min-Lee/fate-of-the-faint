using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityView : MonoBehaviour
{
    [SerializeField] protected Transform aimPoint;
    [SerializeField] protected Animator animator;

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
            e.Motion.AddTask(new MotionTask(
                priority: MotionPriority.Target,
                command: () => ShowDamageTextCor(e.StartAmount - e.EndAmount),
                source: this
            ));

            if (e.EndAmount > 0)
            {
                e.Motion.AddTask(new MotionTask(
                    priority: MotionPriority.Target,
                    command: () => PlayAnimatorTriggerCor(AnimationKeys.ENEMY_HIT),
                    source: this
                ));
            }
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Entity,
            command: () => ChangeHpCor(instance.CurrentHp, instance.MaxHp),
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

        if (e.EndAmount > e.StartAmount)
        {
            e.Motion.AddTask(new MotionTask(
                priority: MotionPriority.Actor,
                command: () => ShowBlockCor(e.EndAmount),
                source: this
            ));
        }
        else
        {
            e.Motion.AddTask(new MotionTask(
                priority: MotionPriority.Target,
                command: () => ChangeBlockCor(e.EndAmount),
                source: this
            ));

            if (instance.Block <= 0)
            {
                e.Motion.AddTask(new MotionTask(
                    priority: MotionPriority.Target,
                    command: () => HideBlockCor(),
                    source: this
                ));
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
    protected IEnumerator ShowStatusCor()
    {
        statusCG.DOFade(1f, 1f);
        yield break;
    }
    protected IEnumerator HideStatusCor()
    {
        statusCG.DOFade(0f, 1f);
        yield break;
    }
    protected IEnumerator PlayAnimatorTriggerCor(string key, float duration = 0)
    {
        animator.SetTrigger(key);

        if (duration > 0)
        {
            yield return new WaitForSeconds(duration);
        }
        else
        {
            yield break;
        }
    }
    protected IEnumerator PlayAnimatorBoolCor(string key, bool value)
    {
        animator.SetBool(AnimationKeys.PLAYER_ENCOUNTER, value);
        yield break;
    }
    protected IEnumerator ShowBlockCor(int value)
    {
        blockView.Show(value);
        yield break;
    }
    protected IEnumerator HideBlockCor()
    {
        blockView.Hide();
        yield break;
    }
    protected IEnumerator ChangeBlockCor(int value)
    {
        blockView.Change(value);
        yield break;
    }
    protected IEnumerator ChangeHpCor(int currentHp, int maxHp)
    {
        hpView.Change(currentHp, maxHp);
        yield break;
    }
    protected IEnumerator ShowDamageTextCor(int value)
    {
        DamageText damageText = damageTextPool.Pop();
        damageText.Spawn(
            text: value.ToString(),
            parent: statusCG.transform,
            pool: damageTextPool
        );
        yield break;
    }
}

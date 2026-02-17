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

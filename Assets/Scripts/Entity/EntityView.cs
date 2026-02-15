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
        yield return statusCG.DOFade(1f, 1f).WaitForCompletion();
    }
    protected IEnumerator HideStatusCor()
    {
        yield return statusCG.DOFade(0f, 1f).WaitForCompletion();
    }
    protected IEnumerator PlayAnimatorTriggerCor(string key, float duration)
    {
        animator.SetTrigger(key);
        yield return new WaitForSeconds(duration);
    }
    protected IEnumerator PlayAnimatorBoolCor(string key, bool value)
    {
        animator.SetBool(AnimationKeys.PLAYER_ENCOUNTER, value);
        yield return null;
    }
    protected IEnumerator ShowBlockCor(int value)
    {
        yield return blockView.Show(value).WaitForCompletion();
    }
    protected IEnumerator HideBlockCor()
    {
        yield return blockView.Hide().WaitForCompletion();
    }
    protected IEnumerator ChangeBlockCor(int value)
    {
        yield return blockView.Change(value).WaitForCompletion();
    }
    protected IEnumerator ChangeHpCor(int currentHp, int maxHp)
    {
        yield return hpView.Change(currentHp, maxHp).WaitForCompletion();
    }
}

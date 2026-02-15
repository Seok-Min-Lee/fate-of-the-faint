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

    protected IEnumerator ShowStatusCor()
    {
        yield return statusCG.DOFade(1f, 1f).WaitForCompletion();
    }
}

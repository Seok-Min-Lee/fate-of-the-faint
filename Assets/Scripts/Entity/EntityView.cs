using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EntityView : MonoBehaviour
{
    [SerializeField] protected Transform aimPoint;
    [SerializeField] protected TextMeshProUGUI hpText;
    [SerializeField] protected Animator animator;

    [SerializeField] protected EntityBuffPreset[] buffPresets;
    [SerializeField] protected Transform buffParent;

    protected Dictionary<BuffType, EntityBuffView> buffViewDictionary = new Dictionary<BuffType, EntityBuffView>();
    protected EntityBuffViewPool buffViewPool;
}

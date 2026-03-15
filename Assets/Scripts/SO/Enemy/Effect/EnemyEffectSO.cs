using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Buff ", menuName = "Scriptable Objects/_base/Enemy Effect")]
public abstract class EnemyEffectSO : ScriptableObject
{
    [SerializeField] protected IntentTarget targetType;
    [SerializeField] protected int value;
    public IntentTarget TargetType => targetType;
    public int Value => value;
    public abstract Action Apply(
        EventBus eventBus,
        EventContext context,
        MotionContext motion,
        EntityInstance source = null,
        IEnumerable<EntityInstance> targets = null
    );
}

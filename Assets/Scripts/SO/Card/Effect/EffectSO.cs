using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectSO", menuName = "Scriptable Objects/EffectSO")]
public abstract class EffectSO : ScriptableObject
{
    [SerializeField] protected TargetType targetType;
    public TargetType TargetType => targetType;
    public abstract Action Apply(
        EventBus eventBus, 
        EventContext context, 
        MotionContext motion, 
        EntityInstance source = null, 
        IEnumerable<EntityInstance> targets = null
    );
}

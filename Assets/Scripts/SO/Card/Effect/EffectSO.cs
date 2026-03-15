using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectSO", menuName = "Scriptable Objects/_base/Effect")]
public abstract class EffectSO : ScriptableObject
{
    [SerializeField] protected TargetType targetType;
    [SerializeField] protected int value;
    public TargetType TargetType => targetType;
    public int Value => value;
    public abstract Action Apply(
        EventBus eventBus, 
        EventContext context, 
        MotionContext motion, 
        EntityInstance source = null, 
        IEnumerable<EntityInstance> targets = null
    );
}

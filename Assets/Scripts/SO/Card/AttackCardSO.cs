using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "atk_", menuName = "Scriptable Objects/CardSO/Attack Card")]
public class AttackCardSO : CardSO
{
    [Header("Effects")]
    [SerializeField] private EffectSO[] effects;
    public EffectSO[] Effects => effects;
}

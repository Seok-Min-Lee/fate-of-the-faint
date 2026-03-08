using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Spawn Plan ", menuName = "Scriptable Objects/Enemy/Enemy Spawn Plan")]
public sealed class EnemySpawnPlanSO : ScriptableObject
{
    [SerializeField] private EnemySO[] enemies;
    public EnemySO[] Enemies => enemies;
}
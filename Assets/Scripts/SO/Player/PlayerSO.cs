using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player_", menuName = "Scriptable Objects/PlayerSO")]
public class PlayerSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id;
    [SerializeField] private string displayName;

    [Header("Base Stats")]
    [SerializeField] private int maxHp;
    [SerializeField] private int baseEnergy;

    [Header("Start")]
    [SerializeField] private List<CardSO> startingCards;
    [SerializeField] private List<RelicSO> startingRelics;
    public string Id => id;
    public string DisplayName => displayName;
    public int MaxHp => maxHp;
    public int BaseEnergy => baseEnergy;
    public IReadOnlyList<CardSO> StartingCards => startingCards;
    public IReadOnlyList<RelicSO> StartingRelics => startingRelics;
}
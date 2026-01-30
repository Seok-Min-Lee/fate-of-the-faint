using UnityEngine;

public class Entity : MonoBehaviour, ITargetable
{
    [SerializeField] private Transform aimPoint;
    public Transform AimPoint => aimPoint;
}
public interface ITargetable
{
    Transform AimPoint { get; }
}
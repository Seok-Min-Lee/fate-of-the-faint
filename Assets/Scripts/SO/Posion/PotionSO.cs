using UnityEngine;

[CreateAssetMenu(fileName = "Posion_", menuName = "Scriptable Objects/PosionSO")]
public class PotionSO : ScriptableObject
{
    [SerializeField] private string id;
    public string Id => id;
}

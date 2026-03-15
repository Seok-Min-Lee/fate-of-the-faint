using UnityEngine;

[CreateAssetMenu(fileName = "Potion_", menuName = "Scriptable Objects/_base/Potion")]
public class PotionSO : ScriptableObject
{
    [SerializeField] private string id;
    public string Id => id;
}

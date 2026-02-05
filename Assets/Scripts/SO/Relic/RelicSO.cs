using UnityEngine;

[CreateAssetMenu(fileName = "Relic_", menuName = "Scriptable Objects/Relic")]
public class RelicSO : ScriptableObject
{
    [SerializeField] private string id;
    public string Id => id;
}

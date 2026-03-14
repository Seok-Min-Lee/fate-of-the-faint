using TMPro;
using UnityEngine;

public class FloorView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private void Start()
    {
        text.text = RunManager.Instance.MapGraph.LatestNode.Floor.ToString();
    }
}

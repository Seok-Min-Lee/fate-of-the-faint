using TMPro;
using UnityEngine;

public class RecordView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    public void Init(string str)
    {
        text.text = str;
    }
}

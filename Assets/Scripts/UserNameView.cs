using System;
using TMPro;
using UnityEngine;

public class UserNameView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text; 
    private void Start()
    {
        text.text = Environment.MachineName;
    }
}

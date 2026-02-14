using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityBuffView : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;

    public BuffType Type { get; private set; }
    public void Init(EntityBuffPreset preset, string text, Transform parent)
    {
        Type = preset.Type;
        image.sprite = preset.Symbol;
        this.text.text = text;

        transform.parent = parent;
        GetComponent<RectTransform>().localPosition = Vector3.zero;
    }
    public void SetText(string text)
    {
        this.text.text = text;
    }
}
[Serializable]
public struct EntityBuffPreset
{
    public BuffType Type;
    public Sprite Symbol;
}
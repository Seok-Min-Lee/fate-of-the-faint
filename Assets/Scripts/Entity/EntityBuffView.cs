using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityBuffView : MonoBehaviour, ITooltip
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

    public void GetTooltip(out string name, out string description)
    {
        switch (Type)
        {
            case BuffType.Strength:
                name = "강화";
                description = "수치만큼 더 강한 피해를 입힌다.";
                break;
            case BuffType.Weak:
                name = "약화";
                description = "가하는 데미지가 50% 감소한다.";
                break;
            case BuffType.Vulnerable:
                name = "취약";
                description = "받는 데미지가 50% 증가한다.";
                break;
            default:
                name = string.Empty;
                description = string.Empty;
                break;
        }
    }
}
[Serializable]
public struct EntityBuffPreset
{
    public BuffType Type;
    public Sprite Symbol;
}
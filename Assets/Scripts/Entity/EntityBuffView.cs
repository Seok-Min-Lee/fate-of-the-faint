using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityBuffView : MonoBehaviour, ITooltip
{
    [SerializeField] private Image image;
    [SerializeField] private Transform textPivot;
    [SerializeField] private TextMeshProUGUI text;

    public BuffType Type { get; private set; }
    private Sequence sequence;
    public void Init(EntityBuffPreset preset, string text, Transform parent)
    {
        Type = preset.Type;
        image.sprite = preset.Symbol;
        this.text.text = text;

        transform.parent = parent;
        GetComponent<RectTransform>().localPosition = Vector3.zero;

        image.transform.localScale = Vector3.zero;
    }
    public void SetText(string text)
    {
        this.text.text = text;
    }
    public Sequence Show()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            image.transform.localScale = Vector3.one * 1.25f;
            
        });
        sequence.Append(image.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));

        return sequence;
    }
    public Sequence Change(string str)
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            text.text = str;
            textPivot.localScale = Vector3.one * 1.25f;
        });
        sequence.Append(textPivot.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));

        return sequence;
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
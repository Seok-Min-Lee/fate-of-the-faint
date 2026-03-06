using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDisplayView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI cost;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI desc;
    [SerializeField] private CardArt[] arts;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color upgradedColor;
    private bool IsHold = false;
    public RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            return _rectTransform;
        }
    }
    private RectTransform _rectTransform;
    public Button Button 
    {
        get
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            return _button;
        }
    }
    private Button _button;
    public int Id { get; private set; }
    public CardEntry Entry { get; private set; }
    public CardSO Origin { get; private set; }
    private float hoverScale;
    public void Init(int id, float hoverScale, CardSO origin, Transform parent, CardEntry entry = null, UnityAction onClick = null)
    {
        Id = id;
        Origin = origin;
        Entry = entry;
        this.hoverScale = hoverScale;
        transform.parent = parent;

        cost.text = origin.Cost.ToString();
        name.text = origin.Name;
        desc.text = origin.Description;

        name.color = origin.IsUpgraded ? upgradedColor : defaultColor;

        int typeIndex = origin switch
        {
            AttackCardSO => 0,
            SkillCardSO => 1,
            _ => 2
        };

        for (int i = 0; i < arts.Length; i++)
        {
            if (i == typeIndex)
            {
                arts[i].Activate(origin.Image);
            }
            else
            {
                arts[i].Deactivate();
            }
        }

        AddOnClickListener(onClick);

        transform.localScale = Vector3.one;
        IsHold = false;
    }
    public void AddOnClickListener(UnityAction onClick)
    {
        if (onClick == null)
        {
            Button.enabled = false;
            return;
        }

        Button.enabled = true;
        Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(onClick);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsHold)
        {
            return;
        }
        Hover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsHold)
        {
            return;
        }
        HoverCancel();
    }
    public void Hover()
    {
        transform.localScale = Vector3.one * hoverScale;
    }
    public void HoverCancel()
    {
        transform.localScale = Vector3.one;
    }
    public void Hold()
    {
        IsHold = true;
    }
    public void HoldCancel()
    {
        IsHold = false;
    }
    public Sequence Select(Vector3 dir)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(transform.DOMove(dir, 0.5f).SetEase(Ease.OutSine));
        sequence.Join(transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InSine));
        sequence.AppendCallback(() =>
        {
            Button.onClick.RemoveAllListeners();
        });

        return sequence;
    }
}

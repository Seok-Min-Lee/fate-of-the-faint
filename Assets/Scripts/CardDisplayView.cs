using DG.Tweening;
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
    private bool IsHold = false;
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
    public CardSO Origin { get; private set; }
    private float hoverScale;
    public void Init(int id, float hoverScale, CardSO origin, Transform parent, bool isButton = false, UnityAction onClick = null)
    {
        Id = id;
        Origin = origin;
        this.hoverScale = hoverScale;
        transform.parent = parent;

        cost.text = origin.Cost.ToString();
        name.text = origin.Name;
        desc.text = origin.Description;

        int typeIndex;
        if (origin is AttackCardSO)
        {
            typeIndex = 0;
        }
        else if(origin is SkillCardSO)
        {
            typeIndex = 1;
        }
        else
        {
            typeIndex = 2;
        }

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

        Button.enabled = isButton; 
        Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(onClick);

        transform.localScale = Vector3.one;
        IsHold = false;
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

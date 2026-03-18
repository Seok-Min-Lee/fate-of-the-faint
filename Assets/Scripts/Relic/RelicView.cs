using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RelicView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image image;
    public RelicInstance Instance { get; private set; }
    public TooltipView Tooltip { get; private set; }
    private Sequence sequence;
    private Action<RelicView> onClick;
    public void Init(RelicInstance instance, TooltipView tooltip)
    {
        Instance = instance;
        Tooltip = tooltip;
        image.sprite = instance.Origin.Icon;

    }
    public void AddListener(Action<RelicView> onClick)
    {
        this.onClick = onClick;
    }

    public void OnRelicActivated(RelicActivated e)
    {
        if (e.Source.Id != Instance.Id)
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Entity,
            command: () => ActivateCor(),
            source: this
        ));
    }
    private IEnumerator ActivateCor()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.Append(image.transform.DOScale(Vector3.one * 1.5f, 0.25f).SetLoops(4, LoopType.Yoyo));
        yield return null;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Tooltip.Bind(
            name: Instance.Origin.DisplayName, 
            description: Instance.Origin.Description
        );

        Tooltip.transform.position = transform.position;
        Tooltip.transform.parent = transform.parent.parent;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltip.Clear();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(this);
    }
}

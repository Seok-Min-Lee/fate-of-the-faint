using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class RelicInstanceView : RelicDefaultView, IPointerClickHandler
{
    public RelicInstance Instance { get; private set; }
    private Sequence sequence;
    private Action<RelicInstanceView> onClick;
    public void Init(RelicInstance instance, TooltipView tooltip)
    {
        base.Init(instance.Origin, tooltip);
        Instance = instance;
    }
    public void AddListener(Action<RelicInstanceView> onClick)
    {
        this.onClick = onClick;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(this);
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
}

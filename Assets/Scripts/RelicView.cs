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
    public RelicSimplePopup SimplePopup { get; private set; }
    private Sequence sequence;
    private Action<RelicSO> onClick;
    public void Init(RelicInstance instance, RelicSimplePopup simplePopup)
    {
        Instance = instance;
        SimplePopup = simplePopup;
        image.sprite = instance.Origin.Icon;

    }
    public void AddListener(Action<RelicSO> onClick)
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
        Activate();
        yield return null;
    }
    private void Activate()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.Append(image.transform.DOScale(Vector3.one * 1.5f, 0.25f).SetLoops(4, LoopType.Yoyo));
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        SimplePopup.Bind(Instance.Origin.DisplayName, Instance.Origin.Description, transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SimplePopup.Clear();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(Instance.Origin);
    }
}

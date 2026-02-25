using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RelicView : MonoBehaviour
{
    [SerializeField] private Image image;
    public RelicEntry Instance { get; private set; }
    private Sequence sequence;
    public void Init(RelicEntry instance)
    {
        Instance = instance;
        image.sprite = instance.Origin.Icon;
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
}

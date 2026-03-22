using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreasurePopup : MonoBehaviour
{
    [SerializeField] private CanvasGroup dimmedCG;
    [SerializeField] private Transform content;

    [SerializeField] private RelicMonoSystem relicSystem;
    [SerializeField] private RelicInstanceViewPool samplePool;
    [SerializeField] private TreasureCtrl ctrl;

    [SerializeField] private int radius;

    private List<RelicInstanceView> sampleViews;

    private Sequence sequence;
    public void Start()
    {
        dimmedCG.alpha = 0f;
        content.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
    public void Init()
    {
        Show();
    }
    private void Show()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            GetViews(3);
            gameObject.SetActive(true);
        });
        sequence.Append(dimmedCG.DOFade(1f, 0.5f));
        sequence.Join(content.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));

        sequence.Append(DOTween.To(() => content.localEulerAngles,
           x => content.localEulerAngles = new Vector3(content.localEulerAngles.x, content.localEulerAngles.y, x.z),
           new Vector3(0, 0, 15), 5f)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo));


    }
    private void Hide(RelicInstanceView view)
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            view.gameObject.SetActive(false);

            samplePool.Tooltip.gameObject.SetActive(false);
        });
        sequence.Append(dimmedCG.DOFade(0f, 0.5f));
        sequence.Join(content.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBack));
        sequence.AppendCallback(() => 
        {
            gameObject.SetActive(false);

            relicSystem.AddRelic(view.Instance.Origin);
            ctrl.ShowNext();
        });
    }
    private void SelectView(RelicInstanceView view)
    {
        Hide(view);
    }
    private void GetViews(int count)
    {
        List<RelicSO> candidates = RunManager.Instance.GetUnacquiredRelics(count);

        List<RelicInstance> instances = new List<RelicInstance>();
        for (int i = 0; i < candidates.Count; i++)
        {
            instances.Add(candidates[i].CreateInstance());
        }

        sampleViews = samplePool.CreateViews(
            samples: instances, 
            onClick: (view) => SelectView(view)
        );

        List<Vector3> positions = Utils.GetCircleAlignedPositions(count, radius);

        for (int i = 0; i < sampleViews.Count; i++)
        {
            RelicInstanceView view = sampleViews[i];

            view.transform.parent = content;
            view.transform.localScale = Vector3.one;
            //view.AddListener((v) => SelectView(v));

            RectTransform rt = view.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = positions[i];
        }
    }
}

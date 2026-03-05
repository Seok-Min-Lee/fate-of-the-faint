using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreasurePopup : MonoBehaviour
{
    [SerializeField] private CanvasGroup dimmedCG;
    [SerializeField] private Transform content;

    [SerializeField] private RelicMonoSystem relicSystem;
    [SerializeField] private RelicViewPool samplePool;
    [SerializeField] private TreasureCtrl ctrl;

    [SerializeField] private int radius;

    private List<RelicView> sampleViews;

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
    }
    private void Hide(RelicView view)
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            view.gameObject.SetActive(false);

            samplePool.SimplePopup.gameObject.SetActive(false);
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
    private void SelectView(RelicView view)
    {
        Hide(view);
    }
    private void GetViews(int count)
    {
        HashSet<RelicSO> hashset = PlayManager.Instance.CurrentData.Relics.Select(x => x.Origin).ToHashSet();

        List<RelicSO> candidates = Utils.PickRandom<RelicSO>(
            source: PlayManager.Instance.Catalog.RelicList.Where(candidate => !hashset.Contains(candidate)),
            count: count
        );

        List<RelicInstance> instances = new List<RelicInstance>();
        for (int i = 0; i < candidates.Count; i++)
        {
            instances.Add(candidates[i].CreateInstance());
        }

        sampleViews = samplePool.CreateViews(instances);
        List<Vector3> positions = Utils.GetCircleAlignedPositions(count, radius);

        for (int i = 0; i < sampleViews.Count; i++)
        {
            RelicView view = sampleViews[i];

            view.transform.parent = content;
            view.transform.localScale = Vector3.one;
            view.AddListener((v) => SelectView(v));

            RectTransform rt = view.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = positions[i];
        }
    }
}

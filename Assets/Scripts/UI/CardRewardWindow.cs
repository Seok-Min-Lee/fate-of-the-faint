using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardRewardWindow : UIWindow
{
    [SerializeField] private CardDisplayViewPool pool;
    [SerializeField] private HorizontalLayoutGroup rewardGroupLayout;
    [SerializeField] private RectTransform selectButton;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject skipButton;
    [SerializeField] private Transform icon;

    private List<CardDisplayView> views = new List<CardDisplayView>();
    private List<CardSO> candidates = new List<CardSO>();
    private CardDisplayView selectedView;
    public bool IsSelected { get; private set; } = true;

    public void Init()
    {
        if (!IsSelected)
        {
            return;
        }
        IsSelected = false;

        int count = PlayManager.Instance.CurrentData.RewardCardOptionCount;
        candidates = Utils.PickRandom<CardSO>(PlayManager.Instance.Catalog.CardList, count);

        for (int i = 0; i < candidates.Count; i++)
        {
            CardDisplayView view = pool.Pop();

            view.Init(
                id: i,
                hoverScale: 1.25f,
                origin: candidates[i],
                parent: rewardGroupLayout.transform,
                isButton: true
            );

            view.Button.onClick.AddListener(() => OnCardSelected(view));

            views.Add(view);
        }
        rewardGroupLayout.enabled = true;

        nextButton.SetActive(false);
        skipButton.SetActive(true);
        selectButton.gameObject.SetActive(true);
        selectButton.anchoredPosition = new Vector2(450, 150);
    }
    public void OnClickSelect()
    {
        PlayManager.Instance.CurrentData.AddCard(selectedView.Origin);

        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            rewardGroupLayout.enabled = false;
            skipButton.SetActive(false);
            selectButton.gameObject.SetActive(false);
        });
        sequence.Append(selectedView.transform.DOMove(icon.transform.position, 0.5f).SetEase(Ease.OutSine));
        sequence.Join(selectedView.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InSine));
        sequence.AppendCallback(() =>
        {
            pool.Push(selectedView);
            selectedView.Button.onClick.RemoveAllListeners();

            candidates.Remove(selectedView.Origin);
            views.Remove(selectedView);
            selectedView = null;

            nextButton.SetActive(true);
        });
    }
    public void OnClickReset()
    {
        for (int i = 0; i < views.Count; i++)
        {
            views[i].transform.localScale = Vector3.one;
        }

        if (selectedView != null)
        {
            selectedView.IsIgnorePointer = false;
        }
        selectedView = null;

        selectButton.DOAnchorPos(new Vector2(450, 150), 0.5f);
    }
    public void OnClickSkip()
    {
        ChangeWindow?.Invoke(WindowType.CardRewards, WindowMode.Revert);
    }
    public void OnClickNext()
    {
        IsSelected = true;

        for (int i = 0; i < views.Count; i++)
        {
            pool.Push(views[i]);
            views[i].Button.onClick.RemoveAllListeners();
        }
        views.Clear();

        selectedView = null;

        ChangeWindow?.Invoke(WindowType.Victory, WindowMode.Single);
    }
    private void OnCardSelected(CardDisplayView view)
    {
        for (int i = 0; i < views.Count; i++)
        {
            if (view.Id != views[i].Id)
            {
                views[i].transform.localScale = Vector3.one;
            }
        }

        if (selectedView != null)
        {
            selectedView.IsIgnorePointer = false;
        }

        selectedView = view;
        selectedView.IsIgnorePointer = true;

        selectButton.DOAnchorPos(new Vector2(150, 150), 0.5f);
    }
}

using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardRewardWindow : UIWindow
{
    [SerializeField] private CardMonoSystem cardSystem;

    [SerializeField] private CardDisplayViewPool pool;
    [SerializeField] private HorizontalLayoutGroup rewardGroupLayout;
    [SerializeField] private SideButton selectButton;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject skipButton;
    [SerializeField] private Transform icon;

    private CardDisplayView selectedView;

    private Action endProcess;
    protected override void OnEnable()
    {
        return;
    }
    public void Init(IEnumerable<CardSO> candidates,  Action endProcess)
    {
        this.endProcess = endProcess;

        int count = candidates.Count();
        for (int i = 0; i < count; i++)
        {
            CardDisplayView view = pool.Pop();

            view.Init(
                index: i,
                hoverScale: 1.25f,
                origin: candidates.ElementAt(i)
            );

            view.transform.localScale = Vector3.one;
            view.transform.parent = rewardGroupLayout.transform;

            view.BindOnClickListener(() => OnCardSelected(view));
        }
        rewardGroupLayout.enabled = true;

        nextButton.SetActive(false);
        skipButton.SetActive(true);
        selectButton.Hide();
    }
    public void OnClickSubmit()
    {
        Vector3 startPos = selectedView.transform.position;
        Vector3 endPos = icon.transform.position;

        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            rewardGroupLayout.enabled = false;
            skipButton.SetActive(false);
            selectButton.gameObject.SetActive(false);
        });

        sequence.Append(selectedView.transform.DOScale(Vector3.one * 0.05f, 0.5f));
        sequence.Append(DOVirtual.Float(0, 1, 0.5f, t =>
        {
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            currentPos.x -= Mathf.Sin(t * Mathf.PI) * Mathf.Abs(endPos.x - startPos.x) * 0.5f;
            selectedView.transform.position = currentPos;
        }).SetEase(Ease.Linear));


        sequence.AppendCallback(() =>
        {
            pool.Push(selectedView);
            cardSystem.AddCard(selectedView.Origin);
            selectedView = null;

            nextButton.SetActive(true);

            endProcess?.Invoke();
        });
    }
    public void OnClickReset()
    {
        // 전체 Hover Cancel, Hold Cancel
        for (int i = 0; i < pool.Actives.Count; i++)
        {
            pool.Actives[i].HoverCancel();
            pool.Actives[i].HoldCancel();
        }

        selectedView = null;

        // 선택 버튼 Hide
        selectButton.Hide();
    }
    public void OnClickSkip()
    {
        ClearViews();
        ChangeWindow?.Invoke(WindowType.CardRewards, WindowMode.Revert);
    }
    public void OnClickNext()
    {
        ClearViews();
        ChangeWindow?.Invoke(WindowType.Victory, WindowMode.Single);
    }
    private void OnCardSelected(CardDisplayView view)
    {
        // 선택한 것 외에 Hover Cancel
        for (int i = 0; i < pool.Actives.Count; i++)
        {
            if (view.Index != pool.Actives[i].Index)
            {
                pool.Actives[i].HoverCancel();
            }
        }

        // Hold 업데이트
        selectedView?.HoldCancel();
        selectedView = view;
        selectedView.Hold();

        // 선택 버튼 Show
        selectButton.Show();
    }
    private void ClearViews()
    {
        List<CardDisplayView> views = new List<CardDisplayView>(pool.Actives);

        for (int i = 0; i < views.Count; i++)
        {
            pool.Push(views[i]);
        }

        selectedView = null;
    }
}

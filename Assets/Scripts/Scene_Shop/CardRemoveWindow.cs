using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class CardRemoveWindow : CardDisplayWindow
{
    [SerializeField] private GoldMonoSystem goldSystem;
    [SerializeField] private SideButton submitButton;

    [Header("[Fail React]")]
    [SerializeField] private int strength;
    [SerializeField] private float duration;
    [SerializeField] private int vibrato;

    private CardDisplayView currentView;
    protected override void OnEnable()
    {
        //
        List<CardEntry> cards = RunManager.Instance.CurrentData.Cards;
        for (int i = 0; i < cards.Count; i++)
        {
            CardDisplayView view = pool.Pop();

            view.Init(
                index: i,
                origin: cards[i].Origin,
                hoverScale: 1.1f,
                entry: cards[i]
            );

            view.transform.localScale = Vector3.one;
            view.transform.parent = parent;

            views.Add(view);
        }

        //
        foreach (CardDisplayView view in views)
        {
            view.BindOnClickListener(() => OnClickView(view));
        }

        submitButton.Hide();
    }
    public void OnClickSubmit()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        if (RunManager.Instance.CurrentData.Gold < 75)
        {
            submitButton.RectTransform.DOPunchPosition(
                punch: Random.insideUnitCircle * strength,
                duration: duration,
                vibrato: vibrato
            );

            return;
        }

        RunManager.Instance.CurrentData.SubtractGold(75);
        goldSystem.Refresh();

        RunManager.Instance.CurrentData.RemoveCard(currentView.Entry.Id, currentView.Entry.SubId);
        pool.Push(currentView);

        currentView = null;
    }
    public void OnClickBackground()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        if (currentView != null)
        {
            currentView.HoldCancel();
            currentView.HoverCancel();

            currentView = null;
        }

        submitButton.Hide();
    }
    private void OnClickView(CardDisplayView view)
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        if (currentView != null)
        {
            currentView.HoldCancel();
            currentView.HoverCancel();
        }

        view.Hover();
        view.Hold();
        currentView = view;

        submitButton.Show();
    }
}

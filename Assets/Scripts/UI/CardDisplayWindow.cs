using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CardDisplayWindow : UIWindow
{
    [SerializeField] private CardDisplayViewPool pool;
    [SerializeField] private Transform parent;

    private List<CardDisplayView> views = new List<CardDisplayView>();
    private Action disabledAction;
    private void OnEnable()
    {
        int id = 0;
        foreach (CardSO origin in PlayManager.Instance.CurrentData.Cards.Select(card => card.Origin))
        {
            CardDisplayView view = pool.Pop();

            view.Init(
                id: id++,
                origin: origin,
                hoverScale: 1.1f
            );

            view.transform.parent = parent;
            views.Add(view);
        }
    }
    private void OnDisable()
    {
        for (int i = 0; i < views.Count; i++)
        {
            pool.Push(views[i]);
        }

        views.Clear();
        disabledAction?.Invoke();
        disabledAction = null;
    }
    public void OnClickBack()
    {
        ChangeWindow(WindowType.CardDisplay, WindowMode.Revert);
    }
}

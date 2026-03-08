using System.Collections.Generic;
using UnityEngine;
public class CardDisplayWindow : UIWindow
{
    [SerializeField] private CardDisplayViewPool pool;
    [SerializeField] private Transform parent;

    protected List<CardDisplayView> views = new List<CardDisplayView>();
    
    protected override void OnEnable()
    {
        base.OnEnable();

        List<CardEntry> cards = PlayManager.Instance.CurrentData.Cards;
        for (int i = 0; i < cards.Count; i++)
        {
            CardDisplayView view = pool.Pop();

            view.Init(
                index: i,
                origin: cards[i].Origin,
                hoverScale: 1.1f
            );

            view.transform.localScale = Vector3.one;
            view.transform.parent = parent;

            views.Add(view);
        }
    }
    public void OnClickCancel()
    {
        for (int i = 0; i < views.Count; i++)
        {
            pool.Push(views[i]);
        }
        views.Clear();

        ChangeWindow(WindowType.CardDisplay, WindowMode.Revert);
    }
}

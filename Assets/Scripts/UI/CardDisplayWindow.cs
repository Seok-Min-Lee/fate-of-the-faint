using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CardDisplayWindow : UIWindow
{
    [SerializeField] private CardDisplayViewPool pool;
    [SerializeField] private Transform parent;

    protected List<CardDisplayView> views = new List<CardDisplayView>();
    protected override void OnEnable()
    {
        base.OnEnable();

        int id = 0;
        foreach (CardEntry entry in PlayManager.Instance.CurrentData.Cards)
        {
            CardDisplayView view = pool.Pop();

            view.Init(
                id: id++,
                entry: entry,
                origin: entry.Origin,
                hoverScale: 1.1f,
                parent: parent
            );

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
    }
    public void OnClickCancel()
    {
        ChangeWindow(WindowType.CardDisplay, WindowMode.Revert);
    }
}

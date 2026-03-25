using System.Collections.Generic;
using UnityEngine;
public class CardDisplayWindow : UIWindow
{
    [SerializeField] protected CardDisplayViewPool pool;
    [SerializeField] protected Transform parent;

    protected List<CardDisplayView> views = new List<CardDisplayView>();
    
    protected override void OnEnable()
    {
        base.OnEnable();

        List<CardEntry> cards = RunManager.Instance.CurrentData.Cards;
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
    protected virtual void OnDisable()
    {
        for (int i = 0; i < views.Count; i++)
        {
            pool.Push(views[i]);
        }
        views.Clear();
    }
    public virtual void OnClickCancel()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        ChangeWindow(WindowType.CardDisplay, WindowMode.Revert);
    }
}

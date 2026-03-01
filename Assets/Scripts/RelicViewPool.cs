using System.Collections.Generic;
using UnityEngine;

public class RelicViewPool : GameObjectPool<RelicView>
{
    [SerializeField] private RelicSimplePopup simplePopup;
    public List<RelicView> CreateViews(IEnumerable<RelicInstance> samples)
    {
        List<RelicView> views = new List<RelicView>();

        foreach (RelicInstance sample in samples)
        {
            RelicView view = Pop();
            view.Init(sample, simplePopup);
            views.Add(view);
        }

        return views;
    }
}

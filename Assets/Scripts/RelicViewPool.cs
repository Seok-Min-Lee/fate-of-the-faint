using System.Collections.Generic;
using UnityEngine;

public class RelicViewPool : GameObjectPool<RelicView>
{
    public List<RelicView> CreateViews(IEnumerable<RelicInstance> samples)
    {
        List<RelicView> views = new List<RelicView>();

        foreach (RelicInstance sample in samples)
        {
            RelicView view = Pop();
            view.Init(sample);
            views.Add(view);
        }

        return views;
    }
}

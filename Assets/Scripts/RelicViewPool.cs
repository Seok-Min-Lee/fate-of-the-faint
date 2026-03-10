using System;
using System.Collections.Generic;
using UnityEngine;

public class RelicViewPool : GameObjectPool<RelicView>
{
    [SerializeField] private RelicSimplePopup simplePopup;
    public RelicSimplePopup SimplePopup => simplePopup;
    public List<RelicView> CreateViews(IEnumerable<RelicInstance> samples, Action<RelicView> onClick)
    {
        List<RelicView> views = new List<RelicView>();

        foreach (RelicInstance sample in samples)
        {
            RelicView view = Pop();
            view.Init(sample, simplePopup);
            view.AddListener(onClick);
            views.Add(view);
        }

        return views;
    }
    public RelicView CreateView(RelicInstance instance)
    {
        RelicView view = Pop();
        view.Init(instance, simplePopup);

        return view;
    }
}

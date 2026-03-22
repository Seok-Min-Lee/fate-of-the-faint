using System;
using System.Collections.Generic;
using UnityEngine;

public class RelicInstanceViewPool : GameObjectPool<RelicInstanceView>
{
    [SerializeField] private TooltipView tooltip;
    public TooltipView Tooltip => tooltip;
    public List<RelicInstanceView> CreateViews(IEnumerable<RelicInstance> samples, Action<RelicInstanceView> onClick)
    {
        List<RelicInstanceView> views = new List<RelicInstanceView>();

        foreach (RelicInstance sample in samples)
        {
            RelicInstanceView view = Pop();
            view.Init(sample, tooltip);
            view.AddListener(onClick);
            views.Add(view);
        }

        return views;
    }
    public RelicInstanceView CreateView(RelicInstance instance)
    {
        RelicInstanceView view = Pop();
        view.Init(instance, tooltip);

        return view;
    }
}

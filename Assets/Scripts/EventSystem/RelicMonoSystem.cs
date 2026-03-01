using System;
using System.Collections.Generic;
using UnityEngine;
public class RelicMonoSystem : BaseMonoSystem
{
    [SerializeField] private RelicViewPool pool;
    private List<RelicInstance> relics;

    private EventBus eventBus;
    public void Init(EventBus eventBus, IEnumerable<RelicSO> relics, Action<RelicSO> onClick)
    {
        this.eventBus = eventBus;

        this.relics = new List<RelicInstance>();
        foreach(RelicSO so in relics)
        {
            RelicInstance instance = so.CreateInstance(eventBus);
            instance.Register();
            this.relics.Add(instance);
        }

        pool.CreateViews(this.relics);

        foreach (RelicView view in pool.Actives)
        {
            view.AddListener(onClick);
            eventBus.Subscribe<RelicActivated>(view.OnRelicActivated);
        }
    }
}

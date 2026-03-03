using System;
using System.Collections.Generic;
using UnityEngine;
public class RelicMonoSystem : BaseMonoSystem
{
    [SerializeField] private RelicViewPool pool;
    private List<RelicInstance> relics;

    private EventBus eventBus;
    private Action<RelicSO> onClick;
    public void Init(EventBus eventBus, IEnumerable<RelicSO> relics, Action<RelicSO> onClick)
    {
        this.eventBus = eventBus;
        this.onClick = onClick;

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

    public void AddRelic(RelicSO relic)
    {
        //
        PlayManager.Instance.CurrentData.AddRelic(relic);

        //
        RelicInstance newInstance = relic.CreateInstance(eventBus);
        newInstance.Register();
        relics.Add(newInstance);

        //
        RelicView newView = pool.CreateView(newInstance);
        newView.AddListener(onClick);
        eventBus.Subscribe<RelicActivated>(newView.OnRelicActivated);

        //
        eventBus.Publish<RelicAdded>(new RelicAdded(
            context: new EventContext(this, null, null, null),
            motion: null,
            source: relic
        ));
    }
}

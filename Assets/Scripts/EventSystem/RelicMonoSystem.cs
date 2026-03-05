using System;
using System.Collections.Generic;
using UnityEngine;
public class RelicMonoSystem : BaseMonoSystem
{
    [SerializeField] private RelicViewPool pool;
    private List<RelicInstance> relics => PlayManager.Instance.CurrentData.Relics;

    private EventBus eventBus;
    private Action<RelicView> onClick;
    private void Start()
    {
        pool.CreateViews(relics);
    }
    public void Init(EventBus eventBus, Action<RelicView> onClick)
    {
        this.eventBus = eventBus;
        this.onClick = onClick;

        foreach(RelicInstance instance in relics)
        {
            instance.Register(eventBus);
        }

        foreach (RelicView view in pool.Actives)
        {
            view.AddListener(onClick);
            eventBus.Subscribe<RelicActivated>(view.OnRelicActivated);
        }
    }

    public void AddRelic(RelicSO relic)
    {
        //
        RelicInstance newInstance = relic.CreateInstance();
        PlayManager.Instance.CurrentData.AddRelic(newInstance);
        newInstance.Register(eventBus);
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

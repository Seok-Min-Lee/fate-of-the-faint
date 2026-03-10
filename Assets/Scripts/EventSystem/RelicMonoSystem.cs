using System;
using System.Collections.Generic;
using UnityEngine;
public class RelicMonoSystem : BaseMonoSystem
{
    [SerializeField] private RelicViewPool pool;
    [SerializeField] private UIWindowManager windowManager;

    private EventBus eventBus;
    private Action<RelicView> onClick;
    private void Start()
    {
        pool.CreateViews(
            samples: PlayManager.Instance.CurrentData.Relics,
            onClick: (view) => OnClickRelic(view)
        );
    }
    public void Init(EventBus eventBus)
    {
        this.eventBus = eventBus;

        foreach (RelicInstance instance in PlayManager.Instance.CurrentData.Relics)
        {
            instance.Register(eventBus);
        }

        foreach (RelicView view in pool.Actives)
        {
            eventBus.Subscribe<RelicActivated>(view.OnRelicActivated);
        }
    }
    public void AddRelic(RelicSO relic)
    {
        //
        RelicInstance newInstance = relic.CreateInstance();
        PlayManager.Instance.CurrentData.AddRelic(newInstance);

        //
        RelicView newView = pool.CreateView(newInstance);
        newView.AddListener(onClick);

        if (eventBus == null)
        {
            return;
        }

        newInstance.Register(eventBus);
        eventBus.Subscribe<RelicActivated>(newView.OnRelicActivated);
        eventBus.Publish<RelicAdded>(new RelicAdded(
            context: new EventContext(this, null, null, null),
            motion: null,
            source: relic
        ));
    }
    public void OnClickRelic(RelicView relic)
    {
        if (!windowManager.TryGetWindow(WindowType.Relic, out UIWindow window) ||
            window is not RelicWindow relicWindow)
        {
            return;
        }

        relicWindow.Bind(relic);

        if (!window.gameObject.activeSelf)
        {
            windowManager.ActivateWindow(WindowType.Relic, WindowMode.Single);
        }
    }
    private void CreateViews()
    {

    }
}

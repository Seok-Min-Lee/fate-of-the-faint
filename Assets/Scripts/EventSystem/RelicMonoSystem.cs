using System;
using UnityEngine;
public class RelicMonoSystem : BaseMonoSystem
{
    [SerializeField] private RelicViewPool pool;
    [SerializeField] private UIWindowManager windowManager;

    private EventBus eventBus;
    private Action<RelicView> onClick;
    private void Start()
    {
        CreateViews();
    }
    public void Init(EventBus eventBus)
    {
        this.eventBus = eventBus;

        foreach (RelicInstance instance in RunManager.Instance.CurrentData.Relics)
        {
            instance.Register(eventBus);
        }

        CreateViews();

        foreach (RelicView view in pool.Actives)
        {
            eventBus.Subscribe<RelicActivated>(view.OnRelicActivated);
        }
    }
    public void AddRelic(RelicSO relic)
    {
        //
        RelicInstance newInstance = relic.CreateInstance();
        RunManager.Instance.CurrentData.AddRelic(newInstance);

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
        if (pool.Actives.Count > 0 || RunManager.Instance.CurrentData.Relics.Count <= 0)
        {
            return;
        }

        pool.CreateViews(
            samples: RunManager.Instance.CurrentData.Relics,
            onClick: (view) => OnClickRelic(view)
        );
    }
}

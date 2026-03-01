using System;

public abstract class RelicInstance
{
    protected readonly EventBus EventBus;
    public string Id { get; private set; }
    public RelicSO Origin { get; private set; }

    public RelicInstance(EventBus eventBus, RelicSO origin)
    {
        EventBus = eventBus;
        Origin = origin;

        Id = origin.Id;
    }

    public abstract void Register();
    protected void Activate(EventContext context, MotionContext motion, Action action)
    {
        EventBus.Publish<RelicActivated>(new RelicActivated(
            context: context.OverwriteNew(this),
            motion: motion,
            source: this
        ));

        action?.Invoke();
    }
}

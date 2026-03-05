using System;

public abstract class RelicInstance
{
    protected EventBus EventBus;
    public string Id { get; private set; }
    public RelicSO Origin { get; private set; }

    public RelicInstance(RelicSO origin)
    {
        //EventBus = eventBus;
        Origin = origin;

        Id = origin.Id;
    }

    public abstract void Register(EventBus eventBus);
    protected void Activate(EventContext context, MotionContext motion, Action action)
    {
        EventBus.Publish<RelicActivated>(new RelicActivated(
            context: context.RewriteNew(this),
            motion: motion,
            source: this
        ));

        action?.Invoke();
    }
}

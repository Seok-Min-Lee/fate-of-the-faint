using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PWR_", menuName = "Scriptable Objects/CardSO/Power Card")]
public abstract class PowerCardSO : CardSO
{
    public abstract PowerInstance CreateInstance(EventBus eventBus);
    //protected abstract string GetDescription();
}
public abstract class PowerInstance
{
    protected readonly EventBus EventBus;
    public CardSO Origin { get; private set; }

    public PowerInstance(EventBus eventBus, CardSO origin)
    {
        EventBus = eventBus;
        Origin = origin;
    }

    public abstract void Register();
    public abstract void Unregister();
    protected void Activate(EventContext context, MotionContext motion, Action action)
    {
        EventBus.Publish<PowerActivated>(new PowerActivated(
            context: context.RewriteNew(this),
            motion: motion,
            source: this
        ));

        action?.Invoke();
    }
}
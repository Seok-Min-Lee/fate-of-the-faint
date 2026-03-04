using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PowerSystem : BaseSystem, ICombatEnded
{
    private readonly EventBus eventBus;
    private List<PowerInstance> powers = new List<PowerInstance>();
    public PowerSystem(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }
    public void OnCombatEnded(CombatEnded e)
    {
        for (int i = 0; i < powers.Count; i++)
        {
            powers[i].Unregister();
        }
        powers.Clear();
    }
    public void AddPower(PowerCardSO power)
    {
        PowerInstance newInstance = power.CreateInstance(eventBus);
        newInstance.Register();

        powers.Add(newInstance);
        //
        eventBus.Publish<PowerAdded>(new PowerAdded(
            context: new EventContext(this, null, null, null),
            motion: null,
            source: newInstance
        ));
    }
}

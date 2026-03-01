using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CreateAssetMenu(fileName = "Regular Relic ", menuName = "Scriptable Objects/Relic/Regular Relic")]
public class RegularRelicSO : RelicSO, INormalEffect
{
    [SerializeField] private RelicTriggerEvent triggerEvent;
    [SerializeField] private RelicTarget target;
    [SerializeField] private RelicEffect effect;
    [SerializeField] private int value;
    public RelicTriggerEvent TriggerEvent => triggerEvent;
    public RelicTarget Target => target;
    public RelicEffect Effect => effect;
    public int Value => value;
    public override RelicInstance CreateInstance(EventBus eventBus)
    {
        return new RegularRelicInstance(eventBus: eventBus, origin: this);
    }
}
public class RegularRelicInstance : RelicInstance
{
    private RelicTriggerEvent triggerEvent;
    private RelicTarget target;
    private RelicEffect effect;
    private int value;
    public RegularRelicInstance(EventBus eventBus, RegularRelicSO origin) : base(eventBus, origin)
    {
        triggerEvent = origin.TriggerEvent;
        target = origin.Target;
        effect = origin.Effect;
        value = origin.Value;
    }
    public override void Register()
    {
    }
    
}

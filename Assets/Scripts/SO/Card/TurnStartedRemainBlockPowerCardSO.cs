using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "pwr_turnStartedRemainBlock_", menuName = "Scriptable Objects/CardSO/Turn Started Remain Block Power Card")]
public class TurnStartedRemainBlockPowerCardSO : PowerCardSO
{
    public override PowerInstance CreateInstance(EventBus eventBus)
    {
        return new TurnStartedRemainBlockPowerInstance(eventBus, this);
    }
}
public class TurnStartedRemainBlockPowerInstance : PowerInstance
{
    public TurnStartedRemainBlockPowerInstance(EventBus eventBus, TurnStartedRemainBlockPowerCardSO origin) : base(eventBus, origin)
    {
    }
    public override void Register()
    {
    }
    public override void Unregister()
    {
    }
}

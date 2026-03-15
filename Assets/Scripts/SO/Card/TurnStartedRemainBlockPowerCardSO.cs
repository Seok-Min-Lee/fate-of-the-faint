using UnityEngine;

[CreateAssetMenu(fileName = "pwr_turnStartedRemainBlock_", menuName = "Scriptable Objects/Card/Turn Started Remain Block Power Card")]
public class TurnStartedRemainBlockPowerCardSO : PowerCardSO
{
    public override PowerInstance CreateInstance(EventBus eventBus)
    {
        return new TurnStartedRemainBlockPowerInstance(eventBus, this);
    }
    protected override string GetDescription()
    {
        return description;
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

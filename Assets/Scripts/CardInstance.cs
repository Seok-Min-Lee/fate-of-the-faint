public class CardInstance
{
    public CardInstance(
        string instanceId, 
        CardSO origin, 
        int costForTurn, 
        int costForCombat
    ) 
    {
        InstanceId = instanceId;
        Origin = origin;
        CostForTurn = costForTurn;
        CostForCombat = costForCombat;
    }
    public string InstanceId { get; private set; }
    public CardSO Origin { get; private set; }

    public int CostForTurn { get; private set; }
    public int CostForCombat { get; private set; }
}

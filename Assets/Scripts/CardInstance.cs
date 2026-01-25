using UnityEngine;

public class CardInstance
{
    public CardInstance(
        string instanceId, 
        CardSO baseDef, 
        int costForTurn, 
        int costForCombat
    ) 
    {
        InstanceId = instanceId;
        BaseDef = baseDef;
        CostForTurn = costForTurn;
        CostForCombat = costForCombat;
    }
    public string InstanceId { get; private set; }
    public CardSO BaseDef { get; private set; }

    public int CostForTurn { get; private set; }
    public int CostForCombat { get; private set; }
}

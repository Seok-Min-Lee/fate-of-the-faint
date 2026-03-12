using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardInstance
{
    public CardInstance(string instanceId, CardEntry entry) 
    {
        InstanceId = instanceId;
        Entry = entry;

        Cost = Origin.Cost;
    }
    public void AddModification(CostModification modification)
    {
        costModifications.Add(modification);
        CalculateCost();
    }
    public void RemoveModifications(CostModificationScope scope)
    {
        List<CostModification> candidates = new List<CostModification>(costModifications.Where(m => m.Scope == scope));
        foreach (CostModification modification in candidates)
        {
            costModifications.Remove(modification);
        }
        CalculateCost();
    }
    private void CalculateCost()
    {
        int total = Origin.Cost;

        for (int i = 0; i < costModifications.Count; i++)
        {
            total += costModifications[i].Amount;
        }

        Cost = Mathf.Max(0, total);
    }
    public string InstanceId { get; private set; }
    public CardEntry Entry { get; private set; }
    public int Cost { get; private set; }
    public CardSO Origin => Entry.Origin;
    public bool ExistModifier => costModifications.Count > 0;
    private List<CostModification> costModifications = new List<CostModification>();
}
public struct CostModification
{
    public CostModification(CostModificationScope scope, int amount, object source)
    {
        Scope = scope;
        Amount = amount;
        Source = source;
    }
    public CostModificationScope Scope { get; private set; }
    public int Amount { get; private set; }
    public object Source { get; private set; }
}
public enum CostModificationScope
{
    Action,
    Turn,
    Combat
}

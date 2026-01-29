using System;
using System.Collections.Generic;
using System.Linq;

public static class DebugGrouper
{
    public static DebugTree BuildTree(IEnumerable<EventTrace> traces)
    {
        List<CombatNode> combats = new List<CombatNode>();

        var combatGroups = traces
            .GroupBy(t => t.CombatId)
            .OrderBy(g => g.Key);

        foreach (IGrouping<int, EventTrace> combatGroup in combatGroups)
        {
            CombatNode combatNode = new CombatNode(combatGroup.Key);

            var turnGroups = combatGroup
                .GroupBy(t => t.TurnId)
                .OrderBy(g => g.Min(x => x.Time));

            foreach (IGrouping<int, EventTrace> turnGroup in turnGroups)
            {
                TurnNode turnNode = new TurnNode(turnGroup.Key);

                turnNode.TurnEvents.AddRange(turnGroup
                    .Where(t => t.ActionId == null)
                    .OrderBy(t => t.Time));

                var actionGroups = turnGroup
                    .Where(t => t.ActionId != null)
                    .GroupBy(t => t.ActionId!.Value)
                    .OrderBy(g => g.Min(x => x.Time));

                foreach (IGrouping<Guid, EventTrace> actionGroup in actionGroups)
                {
                    turnNode.Actions.Add(new ActionNode(
                        actionId: actionGroup.Key,
                        events: actionGroup.OrderBy(t => t.Time)
                    ));
                }

                combatNode.Turns.Add(turnNode);
            }

            combats.Add(combatNode);
        }

        return new DebugTree(combats);
    }
}

public class DebugTree
{
    public DebugTree(IEnumerable<CombatNode> combats)
    {
        Combats = new List<CombatNode>(combats);
    }
    public List<CombatNode> Combats { get; private set; }
}

public class CombatNode
{
    public CombatNode(int combatId)
    {
        CombatId = combatId;
        Turns = new List<TurnNode>();
    }
    public int CombatId { get; private set; }
    public List<TurnNode> Turns { get; private set; }
}

public class TurnNode
{
    public TurnNode(int turnId)
    {
        TurnId = turnId;
        TurnEvents = new List<EventTrace>();
        Actions = new List<ActionNode>();
    }
    public int TurnId { get; private set; }

    // ActionId가 null인 이벤트(턴 시작/끝 등)
    public List<EventTrace> TurnEvents { get; private set; }

    public List<ActionNode> Actions { get; private set; }
}

public class ActionNode
{
    public ActionNode(Guid actionId, IEnumerable<EventTrace> events)
    {
        ActionId = actionId;
        Events = new List<EventTrace>(events);
    }
    public Guid ActionId { get; private set; }
    public List<EventTrace> Events { get; private set; }
}


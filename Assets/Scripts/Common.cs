using UnityEngine;

public static class Common
{
    public static EventContext CreateContext(EventContext context, object source)
    {
        return new EventContext(
            source: source,
            action: context.Action,
            turn: context.Turn,
            combat: context.Combat
        );
    }
}

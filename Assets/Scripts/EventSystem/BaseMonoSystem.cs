using UnityEngine;

public class BaseMonoSystem : MonoBehaviour
{
    protected virtual EventContext CreateContext(EventContext context)
    {
        return new EventContext(
            source: this,
            action: context.Action,
            turn: context.Turn,
            combat: context.Combat
        );
    }
}

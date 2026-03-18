using DG.Tweening;
using System.Collections;
using UnityEngine;

public class CameraMonoSystem : BaseMonoSystem
{
    [Range(0, 1f)] public float duration;
    [Range(0, 1f)] public float strength;
    [Range(0, 64)] public int vibrato;

    private EventBus eventBus;
    public void Init(EventBus eventBus)
    {
        this.eventBus = eventBus;

        eventBus.Subscribe<AttackDeclared>(OnAttackDeclared);
    }
    private void OnDisable()
    {
        eventBus.Unsubscribe<AttackDeclared>(OnAttackDeclared);
    }

    private void OnAttackDeclared(AttackDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        Vector2 dir;
        if (e.Source is PlayerInstance)
        {
            dir = new Vector2(1, 1);
        }
        else if (e.Target is PlayerInstance)
        {
            dir = new Vector2(-1, -1);
        }
        else
        {
            return;
        }

        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Entity,
            command: () => CameraPunchCor(dir),
            source: this
        ));
    }
    private IEnumerator CameraPunchCor(Vector2 direction)
    {
        transform.DOPunchPosition(direction.normalized * strength, duration, vibrato, snapping: false);
        yield break;
    }
}

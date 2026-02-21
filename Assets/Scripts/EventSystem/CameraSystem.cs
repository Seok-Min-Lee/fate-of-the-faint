using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMonoSystem : BaseMonoSystem
{
    [Range(0, 1f)] public float duration;
    [Range(0, 1f)] public float strength;
    [Range(0, 64)] public int vibrato;

    private EventBus eventBus;
    private PlayerInstance player;
    public void Init(EventBus eventBus, PlayerInstance player)
    {
        this.eventBus = eventBus;
        this.player = player;

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

        if (e.Source == player)
        {
            e.Motion.AddTask(new MotionTask(
                priority: MotionPriority.Entity,
                command: () => CameraPunchCor(new Vector2(1, 1)),
                source: this
            ));
        }

        if (e.Target == player)
        {
            e.Motion.AddTask(new MotionTask(
                priority: MotionPriority.Entity,
                command: () => CameraPunchCor(new Vector2(-1, -1)),
                source: this
            ));
        }
    }
    private IEnumerator CameraPunchCor(Vector2 direction)
    {
        transform.DOPunchPosition(direction.normalized * strength, duration, vibrato, snapping: false);
        yield break;
    }
}

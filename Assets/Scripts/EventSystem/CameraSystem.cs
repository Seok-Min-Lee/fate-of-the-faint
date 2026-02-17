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
    private AnimationMonoSystem animationSystem;
    private PlayerInstance player;
    public void Init(EventBus eventBus, PlayerInstance player, AnimationMonoSystem animationSystem)
    {
        this.eventBus = eventBus;
        this.player = player;
        this.animationSystem = animationSystem;

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
            animationSystem.Register(
                priority: AnimationPriority.Target,
                command: () => CameraPunchCor(new Vector2(1, 1))
            );
        }

        if (e.Target == player)
        {
            animationSystem.Register(
                priority: AnimationPriority.Target,
                command: () => CameraPunchCor(new Vector2(-1, -1))
            );
        }
    }
    private IEnumerator CameraPunchCor(Vector2 direction)
    {
        transform.DOPunchPosition(direction.normalized * strength, duration, vibrato, snapping: false);
        yield break;
    }
}

using DG.Tweening;
using System.Collections;
using UnityEngine;
/// <summary>
/// 공격 등의 이벤트 시 카메라 연출(흔들림 등)을 지시하는 시스템
/// </summary>
public class CameraMonoSystem : BaseMonoSystem
{
    [Range(0, 1f)] public float duration; // 흔들림 지속 시간
    [Range(0, 1f)] public float strength; // 흔들림 강도
    [Range(0, 64)] public int vibrato;    // 진동 횟수

    private EventBus eventBus;

    /// <summary>
    /// CameraMonoSystem 초기화 및 이벤트 구독
    /// </summary>
    public void Init(EventBus eventBus)
    {
        this.eventBus = eventBus;

        eventBus.Subscribe<AttackDeclared>(OnAttackDeclared);
    }

    private void OnDisable()
    {
        eventBus.Unsubscribe<AttackDeclared>(OnAttackDeclared);
    }

    /// <summary>
    /// 공격 선언 시 카메라 연출 큐 등록
    /// </summary>
    private void OnAttackDeclared(AttackDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 대상에 따른 카메라 흔들림 방향 결정
        Vector2 dir = e.Target switch
        {
            PlayerInstance => new Vector2(-1, -1), // 플레이어 피격 (좌하단)
            EnemyInstance => new Vector2(1, 1),    // 적 피격 (우상단)
            _ => Vector2.zero
        };

        if (dir == Vector2.zero)
        {
            return;
        }

        // 모션 큐의 Entity 우선순위로 카메라 연출 등록
        e.Motion.AddTask(new MotionTask(
            priority: MotionPriority.Entity,
            command: () => CameraPunchCor(dir),
            source: this
        ));
    }

    /// <summary>
    /// 실제 카메라 흔들림(Punch) 애니메이션 실행 코루틴
    /// </summary>
    private IEnumerator CameraPunchCor(Vector2 direction)
    {
        transform.DOPunchPosition(direction.normalized * strength, duration, vibrato, snapping: false);
        yield break;
    }
}

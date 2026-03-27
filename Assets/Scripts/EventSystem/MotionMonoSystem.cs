using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// 모션(애니메이션, 연출) 큐를 관리하고 순차 재생하는 시스템
/// </summary>
public class MotionMonoSystem : BaseMonoSystem
{
    private EventBus eventBus;
    private readonly Queue<Func<IEnumerator>> queue = new Queue<Func<IEnumerator>>(); // 모션 재생 큐

    public bool IsPlaying { get; private set; } // 모션 재생 여부

    /// <summary>
    /// MotionMonoSystem 초기화
    /// </summary>
    public void Init(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    /// <summary>
    /// 수집된 모션들을 큐에 정렬하여 추가하고 재생 시작
    /// </summary>
    public void Play(EventContext context, MotionContext motion)
    {
        // 우선순위에 따라 정렬 후 큐에 삽입
        EnqueueAfterSort(motion);

        // 이미 재생 중이면 추가만 하고 리턴
        if (IsPlaying)
        {
            return;
        }

        // 재생 시작
        StartCoroutine(PlayCor(context));
    }

    /// <summary>
    /// 큐에 담긴 모션들을 순차적으로 실행하는 코루틴
    /// </summary>
    private IEnumerator PlayCor(EventContext context)
    {
        IsPlaying = true;

        // 애니메이션 시작 이벤트 발행
        eventBus.Publish<AnimationStarted>(new AnimationStarted(context.RewriteNew(this)));

        // 큐가 빌 때까지 순차 재생
        while (queue.Count > 0)
        {
            Func<IEnumerator> motion = queue.Dequeue();
            yield return motion();
        }

        // 애니메이션 종료 이벤트 발행
        eventBus.Publish<AnimationEnded>(new AnimationEnded(context.RewriteNew(this)));
        
        IsPlaying = false;
    }

    /// <summary>
    /// MotionContext의 태스크들을 우선순위(Priority) 기준으로 정렬 후 큐에 삽입
    /// </summary>
    private void EnqueueAfterSort(MotionContext motion)
    {
        foreach (MotionTask task in motion.Tasks.OrderBy(c => c.Priority))
        {
            queue.Enqueue(task.Command);
        }
    }
}

/// <summary>
/// 특정 액션에서 발생한 모션 태스크들의 모음(컨텍스트)
/// </summary>
public class MotionContext
{
    public MotionContext(object source)
    {
        Source = source;
        tasks = new List<MotionTask>();
    }
    public object Source { get; private set; }
    private List<MotionTask> tasks; // 모션 태스크 목록

    /// <summary>
    /// 모션 태스크 추가
    /// </summary>
    public void AddTask(MotionTask command)
    {
        tasks.Add(command);
    }
    public IReadOnlyList<MotionTask> Tasks => tasks;
}

/// <summary>
/// 개별 모션 태스크 정보
/// </summary>
public struct MotionTask
{
    public MotionTask(MotionPriority priority, Func<IEnumerator> command, object source)
    {
        Priority = priority;
        Command = command;
        Source = source;
    }
    public MotionPriority Priority { get; private set; } // 재생 우선순위
    public Func<IEnumerator> Command { get; private set; } // 실제 실행할 코루틴
    public object Source { get; private set; }
}

/// <summary>
/// 모션 재생 우선순위 정의 (낮을수록 먼저 실행)
/// </summary>
public enum MotionPriority
{
    Start,
    Announce,
    Window,
    Card,
    Actor,
    Entity,
    Target,
    End,
}
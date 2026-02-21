using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class MotionMonoSystem : BaseMonoSystem
{
    private EventBus eventBus;
    private readonly Queue<Func<IEnumerator>> queue = new Queue<Func<IEnumerator>>();
    public bool IsPlaying { get; private set; }
    public void Init(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }
    public void Play(EventContext context, MotionContext motion)
    {
        EnqueueAfterSort(motion);

        if (IsPlaying)
        {
            return;
        }

        StartCoroutine(PlayCor(context));
    }

    private IEnumerator PlayCor(EventContext context)
    {
        IsPlaying = true;
        eventBus.Publish<AnimationStarted>(new AnimationStarted(CreateContext(context)));

        while (queue.Count > 0)
        {
            Func<IEnumerator> motion = queue.Dequeue();
            yield return motion();
        }

        eventBus.Publish<AnimationEnded>(new AnimationEnded(CreateContext(context)));
        IsPlaying = false;
    }
    private void EnqueueAfterSort(MotionContext motion)
    {
        foreach (MotionTask task in motion.Tasks.OrderBy(c => c.Priority))
        {
            queue.Enqueue(task.Command);
        }
    }
}
public class MotionContext
{
    public MotionContext(object source)
    {
        Source = source;
        tasks = new List<MotionTask>();
    }
    public object Source { get; private set; }
    private List<MotionTask> tasks;
    public void AddTask(MotionTask command)
    {
        tasks.Add(command);
    }
    public IReadOnlyList<MotionTask> Tasks => tasks;
}
public struct MotionTask
{
    public MotionTask(MotionPriority priority, Func<IEnumerator> command, object source)
    {
        Priority = priority;
        Command = command;
        Source = source;
    }
    public MotionPriority Priority { get; private set; }
    public Func<IEnumerator> Command { get; private set; }
    public object Source { get; private set; }
}
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

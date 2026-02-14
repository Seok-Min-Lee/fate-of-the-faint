using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationMonoSystem : BaseMonoSystem
{
    private EventBus eventBus;
    private readonly Queue<Func<IEnumerator>> queue = new Queue<Func<IEnumerator>>();
    public bool IsPlaying { get; private set; }
    public void Init(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }
    public void Enqueue(Func<IEnumerator> command)
    {
        if (command == null)
        {
            return;
        }

        queue.Enqueue(command);
    }
    public void PlayQueue(EventContext context)
    {
        if (IsPlaying)
        {
            return;
        }

        StartCoroutine(PlayQueueCor(context));
    }

    private IEnumerator PlayQueueCor(EventContext context)
    {
        IsPlaying = true;
        eventBus.Publish<AnimationStarted>(new AnimationStarted(CreateContext(context)));

        while (queue.Count > 0)
        {
            Func<IEnumerator> animation = queue.Dequeue();
            yield return animation();
        }

        eventBus.Publish<AnimationEnded>(new AnimationEnded(CreateContext(context)));
        IsPlaying = false;
    }
}

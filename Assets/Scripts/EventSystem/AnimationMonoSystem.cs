using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationMonoSystem : BaseMonoSystem
{
    private EventBus eventBus;
    private readonly Dictionary<AnimationPriority, List<Func<IEnumerator>>> dictionary = new Dictionary<AnimationPriority, List<Func<IEnumerator>>>();
    private readonly Queue<Func<IEnumerator>> queue = new Queue<Func<IEnumerator>>();
    public bool IsPlaying { get; private set; }
    public void Init(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }
    public void Register(AnimationPriority priority, Func<IEnumerator> command)
    {
        if (command == null)
        {
            return;
        }

        if (!dictionary.ContainsKey(priority))
        {
            dictionary.Add(priority, new List<Func<IEnumerator>>());
        }
        dictionary[priority].Add(command);

        if (IsPlaying)
        {
            Enqueue();
        }
    }
    public void PlayQueue(EventContext context)
    {
        if (IsPlaying)
        {
            return;
        }

        Enqueue();

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
    private void Enqueue()
    {
        foreach (KeyValuePair<AnimationPriority, List<Func<IEnumerator>>> kvp in dictionary.OrderBy(k => k.Key))
        {
            foreach (Func<IEnumerator> command in kvp.Value)
            {
                queue.Enqueue(command);
            }
        }
        dictionary.Clear();
    }
}
public enum AnimationPriority
{
    UIWindow,
    CardHand,
    Actor,
    Target,
    Entity
}

using System.Collections.Generic;
using UnityEngine;
public class GameObjectPool<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] protected T prefab;

    public IReadOnlyList<T> Actives => actives;

    protected Queue<T> queue = new Queue<T>();
    protected List<T> actives = new List<T>();

    public void Push(T Poolable)
    {
        Poolable.transform.SetParent(transform);
        Poolable.gameObject.SetActive(false);

        actives.Remove(Poolable);
        queue.Enqueue(Poolable);
    }
    public T Pop(bool isActive = true)
    {
        T Poolable = queue.Count > 0 ?
                     queue.Dequeue() :
                     GameObject.Instantiate<T>(prefab, transform);

        if (Poolable.gameObject.activeSelf != isActive)
        {
            Poolable.gameObject.SetActive(isActive);
        }

        actives.Add(Poolable);

        return Poolable;
    }
}

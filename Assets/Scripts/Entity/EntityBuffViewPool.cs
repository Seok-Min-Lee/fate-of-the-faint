using System.Collections.Generic;
using UnityEngine;

public class EntityBuffViewPool : MonoBehaviour
{
    [SerializeField] private EntityBuffView prefab;

    private List<EntityBuffView> actives = new List<EntityBuffView>();
    private Queue<EntityBuffView> queue = new Queue<EntityBuffView>();

    public EntityBuffView Pop()
    {
        EntityBuffView view = queue.Count > 0 ?
                            queue.Dequeue() :
                            GameObject.Instantiate<EntityBuffView>(prefab);

        view.gameObject.SetActive(false);
        
        actives.Add(view);

        return view;
    }
    public void Push(EntityBuffView view)
    {
        view.gameObject.SetActive(false);
        queue.Enqueue(view);
        actives.Remove(view);
    }
}

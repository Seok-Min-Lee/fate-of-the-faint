using System.Collections.Generic;
using UnityEngine;

public class CardDisplayViewPool : GameObjectPool<CardDisplayView>
{
    //[SerializeField] CardDisplayView prefab;
    //public IReadOnlyList<CardDisplayView> Actives => actives;
    //private Queue<CardDisplayView> queue = new Queue<CardDisplayView>();
    //private List<CardDisplayView> actives = new List<CardDisplayView>(); 
    //public void Push(CardDisplayView cardView)
    //{
    //    cardView.gameObject.SetActive(false);
    //    cardView.transform.parent = transform;

    //    actives.Remove(cardView);
    //    queue.Enqueue(cardView);
    //}
    //public CardDisplayView Pop()
    //{
    //    CardDisplayView cardView = queue.Count > 0 ?
    //                               queue.Dequeue() :
    //                               GameObject.Instantiate<CardDisplayView>(prefab, transform);

    //    cardView.gameObject.SetActive(true);

    //    actives.Add(cardView);

    //    return cardView;
    //}
}

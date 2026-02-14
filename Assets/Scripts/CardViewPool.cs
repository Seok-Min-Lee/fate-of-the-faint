using System.Collections.Generic;
using UnityEngine;

public class CardViewPool : MonoBehaviour
{
    [SerializeField] CardView prefab;

    public IReadOnlyList<CardView> Actives => actives;

    private Queue<CardView> queue = new Queue<CardView>();

    private List<CardView> actives = new List<CardView>();

    public void Push(CardView cardView)
    {
        actives.Remove(cardView);
        queue.Enqueue(cardView);
    }
    public CardView Pop()
    {
        CardView cardView = queue.Count > 0 ?
                           queue.Dequeue() :
                           GameObject.Instantiate<CardView>(prefab, transform);

        cardView.gameObject.SetActive(false);

        actives.Add(cardView);

        return cardView;
    }
}

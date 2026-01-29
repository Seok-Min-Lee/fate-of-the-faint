using System.Collections.Generic;
using UnityEngine;

public class CardViewPool : MonoBehaviour
{
    [SerializeField] CardView attackViewPrefab;
    [SerializeField] CardView skillViewPrefab;
    [SerializeField] CardView powerViewPrefab;

    private Queue<CardView> AttackPool => attackQueue;
    private Queue<CardView> SkillPool => skillQueue;
    private Queue<CardView> PowerPool => powerQueue;

    public IReadOnlyList<CardView> Actives => actives;

    private Queue<CardView> attackQueue = new Queue<CardView>();
    private Queue<CardView> skillQueue = new Queue<CardView>();
    private Queue<CardView> powerQueue = new Queue<CardView>();

    private List<CardView> actives = new List<CardView>();

    public void Push(CardView cardView)
    {
        actives.Remove(cardView);

        switch (cardView.Type)
        {
            case CardView.ViewType.Attack:
                attackQueue.Enqueue(cardView);
                break;

            case CardView.ViewType.Skill:
                skillQueue.Enqueue(cardView);
                break;

            case CardView.ViewType.Power:
                powerQueue.Enqueue(cardView);
                break;

            default:
                return;
        }
    }
    public CardView Pop(CardType type)
    {
        CardView cardView;

        switch (type)
        {
            case CardType.Attack:
                cardView = attackQueue.Count > 0 ? 
                           attackQueue.Dequeue() : 
                           GameObject.Instantiate<CardView>(attackViewPrefab, transform);
                break;

            case CardType.Skill:
                cardView = skillQueue.Count > 0 ?
                           skillQueue.Dequeue() :
                           GameObject.Instantiate<CardView>(skillViewPrefab, transform);
                break;

            case CardType.Power:
                cardView = powerQueue.Count > 0 ?
                           powerQueue.Dequeue() :
                           GameObject.Instantiate<CardView>(powerViewPrefab, transform);
                break;

            default:
                return null;
        }

        actives.Add(cardView);

        return cardView;
    }
}

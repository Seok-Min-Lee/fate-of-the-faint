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

    public IReadOnlyList<CardView> AttackList => attackList;
    public IReadOnlyList<CardView> SkillList => skillList;
    public IReadOnlyList<CardView> PowerList => powerList;

    private Queue<CardView> attackQueue = new Queue<CardView>();
    private Queue<CardView> skillQueue = new Queue<CardView>();
    private Queue<CardView> powerQueue = new Queue<CardView>();

    private List<CardView> attackList = new List<CardView>();
    private List<CardView> skillList = new List<CardView>();
    private List<CardView> powerList = new List<CardView>();

    public void Push(CardView cardView)
    {
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
        switch (type)
        {
            case CardType.Attack:
                return attackQueue.Count > 0 ? 
                        attackQueue.Dequeue() : 
                        GameObject.Instantiate<CardView>(attackViewPrefab, transform);

            case CardType.Skill:
                return skillQueue.Count > 0 ?
                        skillQueue.Dequeue() :
                        GameObject.Instantiate<CardView>(skillViewPrefab, transform);

            case CardType.Power:
                return powerQueue.Count > 0 ?
                        powerQueue.Dequeue() :
                        GameObject.Instantiate<CardView>(powerViewPrefab, transform);

            default:
                return null;
        }
    }
}

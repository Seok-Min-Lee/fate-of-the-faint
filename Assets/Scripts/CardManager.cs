using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [SerializeField] private Transform handTransform;
    [SerializeField] private CardViewPool cardViewPool;

    [SerializeField] CardSO[] cardSOs;

    public List<CardInstance> drawPile = new List<CardInstance>();
    public List<CardInstance> hand = new List<CardInstance>();
    public List<CardInstance> discardPile = new List<CardInstance>();
    public List<CardInstance> exhaustPile = new List<CardInstance>();

    private void Start()
    {
        foreach (CardSO so in Utils.Shuffle(cardSOs))
        {
            drawPile.Add(new CardInstance(
                instanceId: "", 
                baseDef: so, 
                costForTurn: so.Cost, 
                costForCombat: so.Cost
            ));
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            DrawCard();
        }
    }

    private void DrawCard()
    {
        ChargeDeck();
        if (hand.Count == 7)
        {
            Debug.Log("Hand Full");
            return;
        }

        CardInstance cardInstance = drawPile.FirstOrDefault();

        CardView cardView = cardViewPool.Pop(cardInstance.BaseDef.Type);
        cardView.gameObject.SetActive(true);
        cardView.transform.parent = handTransform;

        cardView.Init(cardInstance);
        drawPile.Remove(cardInstance);
        hand.Add(cardInstance);
    }
    private void ChargeDeck()
    {
        if (drawPile.Count <= 0)
        {
            drawPile.AddRange(Utils.Shuffle(discardPile));
        }
    }
}

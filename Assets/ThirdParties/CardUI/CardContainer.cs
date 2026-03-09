using config;
using DefaultNamespace;
using events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardContainer : MonoBehaviour
{
    public ZoomConfig ZoomConfig => zoomConfig;
    public AnimationSpeedConfig AnimationSpeedConfig => animationSpeedConfig;
    public CardPlayConfig CardPlayConfig => cardPlayConfig;
    public EventsConfig EventsConfig => eventsConfig;
    public bool PreventCardInteraction => preventCardInteraction;
    [Header("Constraints")]
    [SerializeField] private bool forceFitContainer;
    [SerializeField] private bool preventCardInteraction;

    [Header("Alignment")]
    [SerializeField] private CardAlignment alignment = CardAlignment.Center;
    [SerializeField] private bool allowCardRepositioning = true;

    [Header("Rotation")]
    [SerializeField][Range(-90f, 90f)] private float maxCardRotation;
    [SerializeField] private float maxHeightDisplacement;
    [SerializeField] private ZoomConfig zoomConfig;
    [SerializeField] private AnimationSpeedConfig animationSpeedConfig;
    [SerializeField] private CardPlayConfig cardPlayConfig;

    [Header("Targetting")]
    [SerializeField] private AimCursor aimCursor;

    [Header("Events")]
    [SerializeField]
    private EventsConfig eventsConfig;

    public IReadOnlyList<CardView> Cards => cards;
    private List<CardView> cards = new();
    //private List<CardWrapper> cards = new();

    private RectTransform rectTransform;
    private CardView currentDraggedCard;
    //private CardWrapper currentDraggedCard;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        InitCards();

        if (aimCursor == null)
        {
            return;
        }
        aimCursor.Hide();
    }

    private void InitCards()
    {
        SetUpCards();
        SetCardsAnchor();
    }

    private void SetCardsRotation()
    {
        for (var i = 0; i < cards.Count; i++)
        {
            cards[i].SetTargetRotation(
                targetRotation: GetCardRotation(i),
                targetVerticalDisplacement: GetCardVerticalDisplacement(i)
            );
        }
    }

    private float GetCardVerticalDisplacement(int index)
    {
        if (cards.Count < 3) 
        {
            return 0;
        }
        // Associate a vertical displacement based on the index in the cards list
        // so that the center card is at max displacement while the edges are at 0 displacement
        return maxHeightDisplacement * (1 - Mathf.Pow(index - (cards.Count - 1) / 2f, 2) / Mathf.Pow((cards.Count - 1) / 2f, 2));
    }

    private float GetCardRotation(int index)
    {
        if (cards.Count < 3) 
        {
            return 0; 
        }
        // Associate a rotation based on the index in the cards list
        // so that the first and last cards are at max rotation, mirrored around the center
        return -maxCardRotation * (index - (cards.Count - 1) / 2f) / ((cards.Count - 1) / 2f);
    }

    void Update()
    {
        UpdateCards();
    }

    void SetUpCards()
    {
        cards.Clear();
        foreach (Transform card in transform)
        {
            CardView wrapper = card.GetComponent<CardView>();
            if (wrapper == null)
            {
                wrapper = card.gameObject.AddComponent<CardView>();
            }

            cards.Add(wrapper);

            //AddOtherComponentsIfNeeded(wrapper);

            // Pass child card any extra config it should be aware of
            //wrapper.Bind(
            //    zoomConfig: zoomConfig,
            //    eventsConfig: eventsConfig,
            //    cardPlayConfig: cardPlayConfig,
            //    animationSpeedConfig: animationSpeedConfig,
            //    preventCardInteraction: preventCardInteraction,
            //    container: this
            //);
        }
    }

    private void AddOtherComponentsIfNeeded(CardView wrapper)
    {
        Canvas canvas = wrapper.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = wrapper.gameObject.AddComponent<Canvas>();
        }

        canvas.overrideSorting = true;

        if (wrapper.GetComponent<GraphicRaycaster>() == null)
        {
            wrapper.gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void UpdateCards()
    {
        if (transform.childCount != cards.Count)
        {
            InitCards();
        }

        if (cards.Count == 0)
        {
            return;
        }

        SetCardsPosition();
        SetCardsRotation();
        SetCardsUILayers();
        UpdateCardOrder();
    }

    private void SetCardsUILayers()
    {
        for (var i = 0; i < cards.Count; i++)
        {
            cards[i].SetUILayer(zoomConfig.defaultSortOrder + i);
        }
    }

    private void UpdateCardOrder()
    {
        if (!allowCardRepositioning || currentDraggedCard == null) 
        {
            return; 
        }

        // Get the index of the dragged card depending on its position
        int newCardIdx = cards.Count(card => currentDraggedCard.transform.position.x > card.transform.position.x);
        int originalCardIdx = cards.IndexOf(currentDraggedCard);
        if (newCardIdx != originalCardIdx)
        {
            cards.RemoveAt(originalCardIdx);

            if (newCardIdx > originalCardIdx && newCardIdx < cards.Count - 1)
            {
                newCardIdx--;
            }

            cards.Insert(newCardIdx, currentDraggedCard);
        }
        // Also reorder in the hierarchy
        currentDraggedCard.transform.SetSiblingIndex(newCardIdx);
    }

    private void SetCardsPosition()
    {
        // Compute the total width of all the cards in global space
        float cardsTotalWidth = cards.Sum(card => card.Width * card.transform.lossyScale.x);
        // Compute the width of the container in global space
        float containerWidth = rectTransform.rect.width * transform.lossyScale.x;
        if (forceFitContainer && cardsTotalWidth > containerWidth)
        {
            DistributeChildrenToFitContainer(cardsTotalWidth);
        }
        else
        {
            DistributeChildrenWithoutOverlap(cardsTotalWidth);
        }
    }

    private void DistributeChildrenToFitContainer(float childrenTotalWidth)
    {
        // Get the width of the container
        float width = rectTransform.rect.width * transform.lossyScale.x;
        // Get the distance between each child
        float distanceBetweenChildren = (width - childrenTotalWidth) / (cards.Count - 1);
        // Set all children's positions to be evenly spaced out
        float currentX = transform.position.x - width / 2;
        foreach (CardView child in cards)
        {
            float adjustedChildWidth = child.Width * child.transform.lossyScale.x;
            child.SetTargetPosition (new Vector2(currentX + adjustedChildWidth / 2, transform.position.y));
            currentX += adjustedChildWidth + distanceBetweenChildren;
        }
    }

    private void DistributeChildrenWithoutOverlap(float childrenTotalWidth)
    {
        float currentPosition = GetAnchorPositionByAlignment(childrenTotalWidth);
        foreach (CardView child in cards)
        {
            float adjustedChildWidth = child.Width * child.transform.lossyScale.x;
            child.SetTargetPosition(new Vector2(currentPosition + adjustedChildWidth / 2, transform.position.y));
            currentPosition += adjustedChildWidth;
        }
    }

    private float GetAnchorPositionByAlignment(float childrenWidth)
    {
        float containerWidthInGlobalSpace = rectTransform.rect.width * transform.lossyScale.x;
        switch (alignment)
        {
            case CardAlignment.Left:
                return transform.position.x - containerWidthInGlobalSpace / 2;
            case CardAlignment.Center:
                return transform.position.x - childrenWidth / 2;
            case CardAlignment.Right:
                return transform.position.x + containerWidthInGlobalSpace / 2 - childrenWidth;
            default:
                return 0;
        }
    }

    private void SetCardsAnchor()
    {
        foreach (CardView child in cards)
        {
            child.SetAnchor(new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        }
    }

    public void OnCardDragStart(CardView card)
    {
        currentDraggedCard = card;
    }
    public void OnCardDragEnd()
    {
        if (currentDraggedCard == null)
        {
            return;
        }

        bool b = false;
        ITargetable target = null;
        if (currentDraggedCard.CardInstance.Origin.ExistTarget)
        {
            // 라치되었든 아니든, 종료 시점의 타겟을 한 번 확정
            target = aimCursor.RaycastTargetUnderCursor();
            b = target != null;
        }
        else
        {
            b = Utils.ExistPointInRect(
                point: Input.mousePosition, 
                rect: cardPlayConfig.PlayArea
            );
        }

        if (b)
        {
            eventsConfig?.OnCardPlayed?.Invoke(new CardPlayed(currentDraggedCard));
            currentDraggedCard.PlayCardStart(target);
        }

        currentDraggedCard = null;
    }

    public void DestroyCard(CardView card)
    {
        cards.Remove(card);
        eventsConfig.OnCardDestroy?.Invoke(new CardDestroy(card));
        //Destroy(card.gameObject);
    }
    public void UpdateTargetingUI(CardView card)
    {
        if (aimCursor == null)
        {
            return;
        }

        aimCursor.TakeAim();
    }

    public void HideTargetLine()
    {
        if (aimCursor == null)
        {
            return;
        }

        aimCursor.Hide();
    }
}

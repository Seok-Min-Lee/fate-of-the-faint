using config;
using DefaultNamespace;
using events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class CardContainer : MonoBehaviour
{
    [Header("Constraints")]
    [SerializeField]
    private bool forceFitContainer;

    [SerializeField]
    private bool preventCardInteraction;

    [Header("Alignment")]
    [SerializeField]
    private CardAlignment alignment = CardAlignment.Center;

    [SerializeField]
    private bool allowCardRepositioning = true;

    [Header("Rotation")]
    [SerializeField]
    [Range(-90f, 90f)]
    private float maxCardRotation;

    [SerializeField]
    private float maxHeightDisplacement;

    [SerializeField]
    private ZoomConfig zoomConfig;

    [SerializeField]
    private AnimationSpeedConfig animationSpeedConfig;

    [SerializeField]
    private CardPlayConfig cardPlayConfig;

    [Header("Targetting")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private LayerMask targetLayerMask; // Enemy/Player 레이어
    [SerializeField] private float targetRayDistance = 200f; 

    [SerializeField] private LineRenderer targetLine;
    [SerializeField] private Color noTargetColor = Color.gray;
    [SerializeField] private Color hasTargetColor = Color.red;

    [SerializeField] private Transform prepareArea;


    [Header("Events")]
    [SerializeField]
    private EventsConfig eventsConfig;

    private List<CardView> cards = new();
    //private List<CardWrapper> cards = new();

    private RectTransform rectTransform;
    private CardView currentDraggedCard;
    //private CardWrapper currentDraggedCard;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        InitCards();
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
            cards[i].targetRotation = GetCardRotation(i);
            cards[i].targetVerticalDisplacement = GetCardVerticalDisplacement(i);
        }
    }

    private float GetCardVerticalDisplacement(int index)
    {
        if (cards.Count < 3) return 0;
        // Associate a vertical displacement based on the index in the cards list
        // so that the center card is at max displacement while the edges are at 0 displacement
        return maxHeightDisplacement *
               (1 - Mathf.Pow(index - (cards.Count - 1) / 2f, 2) / Mathf.Pow((cards.Count - 1) / 2f, 2));
    }

    private float GetCardRotation(int index)
    {
        if (cards.Count < 3) return 0;
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
            var wrapper = card.GetComponent<CardView>();
            if (wrapper == null)
            {
                wrapper = card.gameObject.AddComponent<CardView>();
            }

            cards.Add(wrapper);

            AddOtherComponentsIfNeeded(wrapper);

            // Pass child card any extra config it should be aware of
            wrapper.zoomConfig = zoomConfig;
            wrapper.animationSpeedConfig = animationSpeedConfig;
            wrapper.eventsConfig = eventsConfig;
            wrapper.preventCardInteraction = preventCardInteraction;
            wrapper.container = this;
            wrapper.prepareArea = prepareArea;
        }
    }

    private void AddOtherComponentsIfNeeded(CardView wrapper)
    {
        var canvas = wrapper.GetComponent<Canvas>();
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
            cards[i].uiLayer = zoomConfig.defaultSortOrder + i;
        }
    }

    private void UpdateCardOrder()
    {
        if (!allowCardRepositioning || currentDraggedCard == null) return;

        // Get the index of the dragged card depending on its position
        var newCardIdx = cards.Count(card => currentDraggedCard.transform.position.x > card.transform.position.x);
        var originalCardIdx = cards.IndexOf(currentDraggedCard);
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
        var cardsTotalWidth = cards.Sum(card => card.width * card.transform.lossyScale.x);
        // Compute the width of the container in global space
        var containerWidth = rectTransform.rect.width * transform.lossyScale.x;
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
        var width = rectTransform.rect.width * transform.lossyScale.x;
        // Get the distance between each child
        var distanceBetweenChildren = (width - childrenTotalWidth) / (cards.Count - 1);
        // Set all children's positions to be evenly spaced out
        var currentX = transform.position.x - width / 2;
        foreach (CardView child in cards)
        {
            var adjustedChildWidth = child.width * child.transform.lossyScale.x;
            child.targetPosition = new Vector2(currentX + adjustedChildWidth / 2, transform.position.y);
            currentX += adjustedChildWidth + distanceBetweenChildren;
        }
    }

    private void DistributeChildrenWithoutOverlap(float childrenTotalWidth)
    {
        var currentPosition = GetAnchorPositionByAlignment(childrenTotalWidth);
        foreach (CardView child in cards)
        {
            var adjustedChildWidth = child.width * child.transform.lossyScale.x;
            child.targetPosition = new Vector2(currentPosition + adjustedChildWidth / 2, transform.position.y);
            currentPosition += adjustedChildWidth;
        }
    }

    private float GetAnchorPositionByAlignment(float childrenWidth)
    {
        var containerWidthInGlobalSpace = rectTransform.rect.width * transform.lossyScale.x;
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
        if (currentDraggedCard.CardInstance.BaseDef.Target == TargetType.EnemySingle)
        {
            // 라치되었든 아니든, 종료 시점의 타겟을 한 번 확정
            // (타겟팅 라치가 된 카드만 플레이하도록 강제하려면 조건 추가 가능)
            target = RaycastTargetUnderCursor(currentDraggedCard);
            b = target != null;
        }
        else
        {
            b = IsCursorInPlayArea();
        }

        if (b)
        {
            eventsConfig?.OnCardPlayed?.Invoke(new CardPlayed(currentDraggedCard));
            if (cardPlayConfig.destroyOnPlay)
            {
                currentDraggedCard.PlayCardStart(target);
            }
        }

        currentDraggedCard = null;
    }

    public void DestroyCard(CardView card)
    {
        cards.Remove(card);
        eventsConfig.OnCardDestroy?.Invoke(new CardDestroy(card));
        //Destroy(card.gameObject);
    }

    public ITargetable UpdateTargetingUI(CardView card)
    {
        if (targetLine == null || worldCamera == null)
        {
            return null;
        }

        targetLine.enabled = true;

        // 카드 중심 (Screen → World)
        Vector3 cardScreen = RectTransformUtility.WorldToScreenPoint(null, card.transform.position);
        Vector3 from = worldCamera.ScreenToWorldPoint(
            new Vector3(cardScreen.x, cardScreen.y, worldCamera.nearClipPlane + 1f)
        );

        // 기본 끝점: 마우스
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 to = worldCamera.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, worldCamera.nearClipPlane + 1f)
        );

        ITargetable target = RaycastTargetUnderCursor(card);
        bool hasTarget = target != null;

        // 타겟이 있으면 AimPoint로 스냅
        if (hasTarget)
        {
            Vector3 aim = target.AimPoint.position;
            to = aim;
        }

        targetLine.positionCount = 2;
        targetLine.SetPosition(0, from);
        targetLine.SetPosition(1, to);

        Color c = hasTarget ? hasTargetColor : noTargetColor;
        targetLine.startColor = c;
        targetLine.endColor = c;

        return target;
    }

    public void HideTargetLine()
    {
        if (targetLine != null)
        { 
            targetLine.enabled = false;
        }
    }

    private ITargetable RaycastTargetUnderCursor(CardView card)
    {
        if (worldCamera == null) 
        {
            return null;
        }

        Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);

        // --- Debug 기록 ---
        _lastTargetRay = ray;
        _hasLastRay = true;
        _lastRayHit = false;
        _lastHitCollider = null;
        _lastHitPoint = ray.origin + ray.direction * targetRayDistance;
        // -------------------

        if (Physics.Raycast(ray, out RaycastHit hit, targetRayDistance, targetLayerMask))
        {
            // --- Debug 기록 ---
            _lastRayHit = true;
            _lastHitPoint = hit.point;
            _lastHitCollider = hit.collider;
            // -------------------

            var t = hit.collider.GetComponentInParent<ITargetable>();
            if (t != null)
            {
                return t;
            }
        }

        return null;
    }
    public bool IsCursorInPlayArea()
    {
        if (cardPlayConfig.playArea == null) 
        {
            return false;
        }

        Vector3 cursorPosition = Input.mousePosition;
        RectTransform playArea = cardPlayConfig.playArea;
        Vector3[] playAreaCorners = new Vector3[4];
        playArea.GetWorldCorners(playAreaCorners);

        return cursorPosition.x > playAreaCorners[0].x &&
               cursorPosition.x < playAreaCorners[2].x &&
               cursorPosition.y > playAreaCorners[0].y &&
               cursorPosition.y < playAreaCorners[2].y;

    }
    public void SetTargetLine(CardView card, bool visible)
    {
        if (targetLine == null)
        {
            return;
        }

        targetLine.enabled = visible;

        if (!visible)
        {
            return;
        }

        // 카드 중심(화면 좌표) -> 현재 포인터(화면 좌표)
        Vector3 from = card.transform.position;
        Vector3 to = Input.mousePosition;

        targetLine.positionCount = 2;
        targetLine.SetPosition(0, from);
        targetLine.SetPosition(1, to);
    }

    [Header("Debug")]
    [SerializeField] private bool drawTargetRayGizmo = true;

    private Ray _lastTargetRay;
    private bool _hasLastRay;
    private bool _lastRayHit;
    private Vector3 _lastHitPoint;
    private Collider _lastHitCollider;
    private void OnDrawGizmos()
    {
        if (!drawTargetRayGizmo || !_hasLastRay)
        {
            return;
        }

        // 레이 자체
        Gizmos.color = _lastRayHit ? Color.green : Color.yellow;
        Gizmos.DrawLine(_lastTargetRay.origin, _lastHitPoint);

        // 히트 지점 표시
        if (_lastRayHit)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_lastHitPoint, 0.05f);

            // 히트한 콜라이더의 bounds 표시(선택)
            if (_lastHitCollider != null)
            {
                Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
                Gizmos.DrawWireCube(_lastHitCollider.bounds.center, _lastHitCollider.bounds.size);
            }
        }
    }
}

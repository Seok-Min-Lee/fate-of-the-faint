using config;
using DG.Tweening;
using events;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardView : CardDefaultView, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private const float EPS = 0.01f;

    [SerializeField] private ZoomConfig zoomConfig;
    [SerializeField] private EventsConfig eventsConfig;
    [SerializeField] private CardPlayConfig cardPlayConfig;
    [SerializeField] private AnimationSpeedConfig animationSpeedConfig;
    [SerializeField] private CardContainer container;
    [SerializeField] private bool preventCardInteraction;

    private RectTransform rectTransform;
    private Canvas canvas;

    private float targetRotation;
    private Vector2 targetPosition;
    private float targetVerticalDisplacement;
    private int uiLayer;

    private bool isHovered;
    private bool isDragged;

    private bool isUsable = true;

    public float Width => rectTransform.rect.width * rectTransform.localScale.x;

    private void Awake() 
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponent<Canvas>();
        canvas.overrideSorting = true;

        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update() 
    {
        if (!isUsable)
        {
            return;
        }
        UpdateRotation();
        UpdatePosition();
        UpdateScale();
        UpdateUILayer();
    }

    private void UpdateUILayer() 
    {
        if (!isHovered && !isDragged) 
        {
            canvas.sortingOrder = uiLayer;
        }
    }

    private void UpdatePosition() 
    {
        if (isDragged)
        {
            return;
        }

        Vector2 target = isHovered && zoomConfig.overrideYPosition != -1 ?
                        new Vector2(targetPosition.x, zoomConfig.overrideYPosition) :
                        new Vector2(targetPosition.x, targetPosition.y + targetVerticalDisplacement);

        float distance = Vector2.Distance(rectTransform.position, target);

        float repositionSpeed = rectTransform.position.y > target.y || rectTransform.position.y < 0 ?
                                animationSpeedConfig.releasePosition :
                                animationSpeedConfig.position;

        rectTransform.position = Vector2.MoveTowards(
            rectTransform.position,
            target,
            repositionSpeed * Time.deltaTime
        );
    }

    private void UpdateScale() 
    {
        float targetZoom = (isDragged || isHovered) && zoomConfig.zoomOnHover ?
                            zoomConfig.multiplier : 
                            1f;

        float delta = Mathf.Abs(rectTransform.localScale.x - targetZoom);

        float newZoom = Mathf.MoveTowards(
            rectTransform.localScale.x,
            targetZoom, 
            animationSpeedConfig.zoom * Time.deltaTime
        );

        rectTransform.localScale = new Vector3(newZoom, newZoom, 1);
    }

    private void UpdateRotation() 
    {
        float crtAngle = rectTransform.rotation.eulerAngles.z;
        // If the angle is negative, add 360 to it to get the positive equivalent
        crtAngle = crtAngle < 0 ? crtAngle + 360 : crtAngle;
        // If the card is hovered and the rotation should be reset, set the target rotation to 0
        float tempTargetRotation = (isHovered || isDragged) && zoomConfig.resetRotationOnZoom ? 
                                    0 :
                                    targetRotation;
        tempTargetRotation = tempTargetRotation < 0 ? 
                            tempTargetRotation + 360 : 
                            tempTargetRotation;
        float deltaAngle = Mathf.Abs(crtAngle - tempTargetRotation);

        if (deltaAngle > EPS)
        {
            // Adjust the current angle and target angle so that the rotation is done in the shortest direction
            float adjustedCurrent = deltaAngle > 180 && crtAngle < tempTargetRotation ? 
                                    crtAngle + 360 : 
                                    crtAngle;

            float adjustedTarget = deltaAngle > 180 && crtAngle > tempTargetRotation ?
                                    tempTargetRotation + 360 :
                                    tempTargetRotation;

            float newDelta = Mathf.Abs(adjustedCurrent - adjustedTarget);

            float nextRotation = Mathf.MoveTowards(
                adjustedCurrent,
                adjustedTarget,
                animationSpeedConfig.rotation * Time.deltaTime
            );

            rectTransform.rotation = Quaternion.Euler(0, 0, nextRotation);
        }
    }

    public void SetTargetPosition(Vector2 targetPosition)
    {
        this.targetPosition = targetPosition;
    }
    public void SetTargetRotation(float targetRotation, float targetVerticalDisplacement)
    {
        this.targetRotation = targetRotation;
        this.targetVerticalDisplacement = targetVerticalDisplacement;
    }
    public void SetUILayer(int uiLayer)
    {
        this.uiLayer = uiLayer;
    }
    public void SetAnchor(Vector2 min, Vector2 max) 
    {
        rectTransform.anchorMin = min;
        rectTransform.anchorMax = max;
    }

    public void OnPointerEnter(PointerEventData eventData) 
    {
        if (isDragged) 
        {
            // Avoid hover events while dragging
            return;
        }
        if (zoomConfig.bringToFrontOnHover) 
        {
            canvas.sortingOrder = zoomConfig.zoomedSortOrder;
        }

        eventsConfig?.OnCardHover?.Invoke(new CardHover(this));
        isHovered = true;
        effectGO.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) 
    {
        if (isDragged) 
        {
            // Avoid hover events while dragging
            return;
        }
        canvas.sortingOrder = uiLayer;
        isHovered = false;
        eventsConfig?.OnCardUnhover?.Invoke(new CardUnhover(this));
        effectGO.SetActive(false);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (preventCardInteraction) 
        {
            return;
        }

        isDragged = true;
        targetingLatched = false;
        container.HideTargetLine();

        container.OnCardDragStart(this);
        eventsConfig?.OnCardUnhover?.Invoke(new CardUnhover(this));
    }
    public void OnPointerUp(PointerEventData eventData) 
    {
        if (!Utils.ExistPointInRect(point: Input.mousePosition, rect: rectTransform))
        {
            canvas.sortingOrder = uiLayer;
            isHovered = false;
            eventsConfig?.OnCardUnhover?.Invoke(new CardUnhover(this));
            effectGO.SetActive(false);
        }

        isDragged = false;

        targetingLatched = false;
        container.HideTargetLine();

        container.OnCardDragEnd();
    }
    public void OnDrag(PointerEventData eventData)
    {
        // 1) 아직 라치 안 됐고, PlayArea에 닿으면 라치!
        if (!targetingLatched && 
            CardInstance.Origin.ExistTarget &&
            Utils.ExistPointInRect(point: eventData.position, rect: cardPlayConfig.PlayArea))
        {
            targetingLatched = true;
            lockedPosition = cardPlayConfig.PrepareArea.position;   // 닿는 순간 위치에 고정(원하면 스냅으로 변경 가능)
        }

        // 2) 라치 됐으면: 카드 고정 + 타겟팅 UI 업데이트(라인/색)
        if (targetingLatched)
        {
            rectTransform.position = lockedPosition;
            container.UpdateTargetingUI(this);
        }
        else
        {
            rectTransform.position = eventData.position;
        }
    }
    [Header("Custom")]
    [SerializeField] private GameObject effectGO;
    private CanvasGroup canvasGroup;
    public CardInstance CardInstance { get; private set; }
    public CardMonoSystem CardSystem { get; private set; }

    private bool targetingLatched;
    private Vector2 lockedPosition;
    public void PlayCardStart(ITargetable target)
    {
        CardSystem.PlayCardStart(cardView: this, target: target);
    }
    public void Init(CardInstance cardInstance, CardMonoSystem cardSystem, CardContainer cardContainer)
    {
        CardInstance = cardInstance;
        CardSystem = cardSystem;

        container = cardContainer;
        zoomConfig = cardContainer.ZoomConfig;
        eventsConfig = cardContainer.EventsConfig;
        cardPlayConfig = cardContainer.CardPlayConfig;
        animationSpeedConfig = cardContainer.AnimationSpeedConfig;
        preventCardInteraction = cardContainer.PreventCardInteraction;

        transform.parent = cardContainer.transform;

        base.Init(cardInstance.Origin);

        canvas.overrideSorting = true;
    }
    public void ModifiyCost()
    {
        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            cost.text = CardInstance.Cost.ToString();
            cost.color = CardInstance.ExistModifier ? Color.green : Color.white;
            cost.transform.localScale = Vector3.one * 1.25f;
        });
        seq.Append(cost.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
    }
    Sequence sequence;
    public Sequence Draw()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            isUsable = false;
            transform.position = cardPlayConfig.DrawArea.transform.position;
            transform.rotation = Quaternion.Euler(0, 0, -90);
            transform.localScale = Vector3.zero;
            canvasGroup.alpha = 1f;

            gameObject.SetActive(true);
        });

        sequence.Append(transform.DOMove(new Vector3(960,0), 0.3333f).SetEase(Ease.OutSine));
        sequence.Join(transform.DORotate(Vector3.zero, 0.3333f).SetEase(Ease.OutSine));
        sequence.Join(transform.DOScale(Vector3.one, 0.3333f).SetEase(Ease.OutSine));

        sequence.AppendCallback(() =>
        {
            isUsable = true;
        });

        return sequence;
    }
    public Sequence Discard()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() => 
        {
            isUsable = false;
        });
        sequence.Append(transform.DOMove(cardPlayConfig.DiscardArea.position, 0.3333f).SetEase(Ease.OutSine));
        sequence.Join(transform.DOScale(Vector3.zero, 0.3333f).SetEase(Ease.OutSine));
        sequence.Join(transform.DORotate(new Vector3(0, 0, -90), 0.3333f).SetEase(Ease.OutSine));
        sequence.AppendCallback(() =>
        {
            isUsable = true;
            gameObject.SetActive(false);
        });

        return sequence;
    }
    public Sequence Exhaust()
    {
        if (sequence != null)
        {
            sequence.Kill();
        }
        sequence = DOTween.Sequence();

        sequence.AppendCallback(() => 
        {
            isUsable = false;
        });
        sequence.Append(transform.DOLocalMoveY(transform.localPosition.y + 150, 0.5f).SetEase(Ease.OutSine));
        sequence.Join(canvasGroup.DOFade(0, 0.5f).SetEase(Ease.OutSine));
        sequence.AppendCallback(() =>
        {
            isUsable = true;
            gameObject.SetActive(false);
        });

        return sequence;
    }
}

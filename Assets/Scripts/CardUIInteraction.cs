using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class CardUIInteraction : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int HandIndex;
    [Header("Hover")]
    public float hoverLiftY = 80f;
    public float hoverScale = 1.15f;
    public float hoverRotateToZ = 0f;

    [Header("Drag")]
    public float dragScale = 1.10f;
    public bool rotateFlatWhileDrag = true;

    [SerializeField] private RectTransform overlayRoot;
    private RectTransform _handParent;
    private int _handSiblingIndex;

    private RectTransform _rt;
    private CanvasGroup _cg;

    private CardHandLayout _hand;

    private RectTransform _originalParent;
    private int _originalSiblingIndex;

    // “현재 레이아웃 기준” 원상복구용
    private Vector2 _baseAnchoredPos;
    private Quaternion _baseLocalRot;
    private Vector3 _baseLocalScale;

    private bool _dragging;

    private RectTransform _dragRoot;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();

        _hand = GetComponentInParent<CardHandLayout>();
        if (_hand == null)
            Debug.LogWarning($"[CardUIInteraction] CardHandLayout not found in parents: {name}");
    }

    private void Start()
    {
        ResolveDragRoot();
        CacheBaseState(); // 시작 시점 기준
    }

    private void OnDisable()
    {
        // 드롭 성공으로 Destroy/Disable 되더라도 상태가 꼬이지 않게 안전장치
        if (_cg != null) _cg.blocksRaycasts = true;

        if (_hand != null)
            _hand.LockLayout(false);
    }

    private void CacheBaseState()
    {
        _originalParent = _rt.parent as RectTransform;
        _originalSiblingIndex = _rt.GetSiblingIndex();

        _baseAnchoredPos = _rt.anchoredPosition;
        _baseLocalRot = _rt.localRotation;
        _baseLocalScale = _rt.localScale;
    }

    private void ResolveDragRoot()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null) _dragRoot = canvas.transform as RectTransform;
        if (_dragRoot == null) _dragRoot = transform.root as RectTransform;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_dragging) return;
        
        //CacheBaseState();

        if (_hand != null)
        {
            if (_hand.lockLayout) return;

            _hand.Rebuild();
            int idx = _hand.GetIndexOf(_rt);
            _hand.SetFocusIndex(idx);
        }

        // Hand 내부 순서 건드리지 말고 overlay로 이동
        if (overlayRoot != null)
        {
            _handParent = _rt.parent as RectTransform;
            _handSiblingIndex = _rt.GetSiblingIndex();
            _rt.SetParent(overlayRoot, worldPositionStays: true);
            _rt.SetAsLastSibling(); // overlay 안에서는 OK
        }

        ApplyHoverVisual(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_dragging) return;

        if (_hand != null) 
        {
            if (_hand.lockLayout) 
            {
                return; 
            }
            _hand.ClearFocus();
        }
        //if (_hand != null)
        //    _hand.ClearFocus();

        ApplyHoverVisual(false);

        // overlay에서 다시 Hand로 복귀 (원래 sibling index 유지)
        if (_handParent != null)
        {
            _rt.SetParent(_handParent, worldPositionStays: false);
            _rt.SetSiblingIndex(_handSiblingIndex);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragging = true;

        if (_hand != null)
            _hand.LockLayout(true);

        //CacheBaseState();

        // 드롭 타겟을 레이캐스트로 찾기 위해 blocksRaycasts 끔
        _cg.blocksRaycasts = false;

        // 드래그 중에는 항상 최상단으로
        _rt.SetParent(_dragRoot, worldPositionStays: true);
        _rt.SetAsLastSibling();

        _rt.localScale = Vector3.one * dragScale;
        if (rotateFlatWhileDrag)
            _rt.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_dragging) return;

        // anchoredPosition 기반보다 “월드 좌표 이동”이 Canvas 모드 차이에 강함
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _dragRoot, eventData.position, eventData.pressEventCamera, out var worldPoint))
        {
            _rt.position = worldPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_dragging) return;
        _dragging = false;

        // 먼저 복구(드롭 성공으로 Destroy되더라도 뒤가 덜 꼬임)
        _cg.blocksRaycasts = true;

        // 드롭 타겟 판정
        var target = FindDropTargetUnderPointer(eventData);
        if (target != null && target.CanAccept(this))
        {
            // 드롭 성공
            target.OnDrop(this);
            // 여기서 Destroy될 수 있으니 아래 로직은 “안전”해야 함 (OnDisable에도 안전장치 있음)
        }
        else
        {
            ReturnToHand();
        }

        if (_hand != null)
        {
            _hand.LockLayout(false);
            _hand.ClearFocus();
            _hand.RebuildAndLayout();
        }
    }

    private ICardDropTarget FindDropTargetUnderPointer(PointerEventData eventData)
    {
        var results = new List<RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);

        for (int i = 0; i < results.Count; i++)
        {
            var go = results[i].gameObject;
            if (go == null) continue;

            if (go == gameObject || go.transform.IsChildOf(transform)) continue;

            var target = go.GetComponentInParent<ICardDropTarget>();
            if (target != null)
                return target;
        }

        return null;
    }

    private void ApplyHoverVisual(bool on)
    {
        if (on)
        {
            _rt.localScale = Vector3.one * hoverScale;
            _rt.localRotation = Quaternion.Euler(0f, 0f, hoverRotateToZ);

            // “현재 레이아웃 위치” 기준으로 살짝 들어올리기
            _rt.anchoredPosition = _baseAnchoredPos + new Vector2(0f, hoverLiftY);

            _rt.SetAsLastSibling();
        }
        else
        {
            // 원상복구(레이아웃이 다시 잡아주더라도 기본값은 복구해두는 게 안전)
            _rt.localScale = _baseLocalScale;
            _rt.localRotation = _baseLocalRot;
            _rt.anchoredPosition = _baseAnchoredPos;
        }

        Debug.Log($"ApplyHoverVisual {on}");
    }

    private void ReturnToHand()
    {
        if (_originalParent == null) return;

        _rt.SetParent(_originalParent, worldPositionStays: false);
        _rt.SetSiblingIndex(_originalSiblingIndex);

        // 레이아웃이 다시 배치하겠지만, 복귀 프레임에서의 튐 방지용 기본 복구
        _rt.localScale = _baseLocalScale;
        _rt.localRotation = _baseLocalRot;
        _rt.anchoredPosition = _baseAnchoredPos;

        Debug.Log("ReturnToHand");
    }
}

public interface ICardDropTarget
{
    bool CanAccept(CardUIInteraction card);
    void OnDrop(CardUIInteraction card);
}

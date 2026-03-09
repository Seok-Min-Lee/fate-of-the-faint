using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AimCursor : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Camera cam;

    [SerializeField] private Sprite lockImage;
    [SerializeField] private Sprite unlockImage;

    [SerializeField] private LayerMask targetLayerMask;
    [SerializeField] private float targetRayDistance = 200f;
    public Image Image
    {
        get 
        {
            if (_image == null)
            {
                _image = GetComponent<Image>();
            }

            return _image;
        }
    }
    private Image _image;

    private Sequence sequence;
    public void TakeAim()
    {
        if (cam == null)
        {
            return;
        }
        gameObject.SetActive(true);

        // 기본 끝점: 마우스
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 to = cam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, cam.nearClipPlane + 1f)
        );

        ITargetable target = RaycastTargetUnderCursor();

        // 타겟이 있으면 AimPoint로 스냅
        if (target != null)
        {
            Lock(target.AimPoint.position);
        }
        else
        {
            Unlock(to);
        }
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    private void Lock(Vector3 worldPoint)
    {
        Move(worldPoint);

        Image.sprite = lockImage;
        Image.color = Color.red;

        sequence.Kill();
        sequence = null;
    }
    private void Unlock(Vector3 worldPoint)
    {
        Move(worldPoint);

        Image.sprite = unlockImage;
        Image.color = Color.black;

        Rotate();
    }
    private void Move(Vector3 worldPoint)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPoint);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            null,
            out Vector2 localPoint
        );

        Image.rectTransform.anchoredPosition = localPoint;
    }
    private void Rotate()
    {
        if (sequence != null)
        {
            return;
        }
        sequence = DOTween.Sequence();

        transform.localEulerAngles = Vector3.zero;
        transform.localScale = Vector3.one;

        sequence.Append(transform.DOLocalRotate(new Vector3(0, 0, 45), 0.75f));
        sequence.Join(transform.DOScale(Vector3.one * 0.8f, 0.75f));

        sequence.SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
    }
    public ITargetable RaycastTargetUnderCursor()
    {
        if (cam == null)
        {
            return null;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        _lastTargetRay = ray;
        _hasLastRay = true;
        _lastRayHit = false;
        _lastHitCollider = null;
        _lastHitPoint = ray.origin + ray.direction * targetRayDistance;

        if (Physics.Raycast(ray, out RaycastHit hit, targetRayDistance, targetLayerMask))
        {
            _lastRayHit = true;
            _lastHitPoint = hit.point;
            _lastHitCollider = hit.collider;

            ITargetable t = hit.collider.GetComponentInParent<ITargetable>();
            if (t != null && !t.Instance.IsDead)
            {
                return t;
            }
        }

        return null;
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

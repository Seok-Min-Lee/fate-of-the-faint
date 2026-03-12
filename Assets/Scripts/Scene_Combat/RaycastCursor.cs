using UnityEngine;

public class RaycastCursor<T> : MonoBehaviour where T : class
{
    [SerializeField] protected RectTransform canvasRect;
    [SerializeField] protected Camera cam;
    [SerializeField] protected LayerMask targetLayerMask;
    [SerializeField] protected float targetRayDistance = 200f;
    public T RaycastTargetUnderCursor()
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

            T t = hit.collider.GetComponent<T>();

            if (t != null)
            {
                return t;
            }

            return null;
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

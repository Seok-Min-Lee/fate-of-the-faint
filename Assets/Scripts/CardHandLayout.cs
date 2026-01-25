using System.Collections.Generic;
using UnityEngine;

#if DOTWEEN
using DG.Tweening;
#endif

/// <summary>
/// RectTransform 자식(카드)을 부채꼴(곡선)로 배치하는 컴포넌트.
/// - Hand(부모) 오브젝트에 붙인다.
/// - 카드들은 Hand의 직계 자식 RectTransform.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class CardHandLayout : MonoBehaviour
{
    [Header("Arc")]
    [Range(0f, 140f)]
    public float maxAngle = 60f;
    public float radius = 650f;
    public Vector2 centerPivot = new Vector2(0f, -420f);

    [Header("Auto Angle")]
    public bool autoAngle = true;
    public float anglePerCard = 6f;
    public float autoAngleMax = 70f;

    [Header("Shape Tuning")]
    public float edgeDropStrength = 2.0f;
    public bool rotateAlongArc = true;
    public float rotationMultiplier = 1.0f;

    [Header("Hover Gap")]
    [Tooltip("Hover 카드 주변으로 벌려줄 각도(도). 10~18 권장.")]
    public float hoverGapDegrees = 14f;

    [Header("Ordering")]
    public bool setSiblingIndex = true;

    [Header("Filtering")]
    public bool ignoreInactive = true;

    // ===== Runtime state =====
    private readonly List<RectTransform> _cards = new();
    private RectTransform _handRect;

    private int _focusIndex = -1;     // hover 중인 카드 인덱스
    private bool _lockLayout = false; // drag 중 레이아웃 고정

    public int focusIndex => _focusIndex;
    public bool lockLayout => _lockLayout;

    private void OnEnable()
    {
        _handRect = transform as RectTransform;
        RebuildAndLayout();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _handRect = transform as RectTransform;
        RebuildAndLayout();
    }
#endif

    private void OnTransformChildrenChanged()
    {
        RebuildAndLayout();
    }

    private void Update()
    {
        // ExecuteAlways에서 편집 중 파라미터 조정 시 바로 반영
        if (!Application.isPlaying)
            Layout();
    }

    public void RebuildAndLayout()
    {
        Rebuild();
        Layout();
    }

    public void Rebuild()
    {
        _cards.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i) as RectTransform;
            if (child == null) continue;
            if (ignoreInactive && !child.gameObject.activeInHierarchy) continue;
            _cards.Add(child);
        }

        _cards.Sort((a, b) =>
        {
            var oa = a.GetComponent<CardUIInteraction>();
            var ob = b.GetComponent<CardUIInteraction>();
            int ia = oa ? oa.HandIndex : 0;
            int ib = ob ? ob.HandIndex : 0;
            return ia.CompareTo(ib);
        });
    }

    public int GetIndexOf(RectTransform card)
    {
        for (int i = 0; i < _cards.Count; i++)
            if (_cards[i] == card) return i;
        return -1;
    }

    public void SetFocusIndex(int index)
    {
        _focusIndex = index;
        if (!_lockLayout) Layout();
    }

    public void ClearFocus()
    {
        _focusIndex = -1;
        if (!_lockLayout) Layout();
    }

    public void LockLayout(bool locked)
    {
        _lockLayout = locked;
        if (!_lockLayout) Layout();
    }

    public void Layout()
    {
        if (_lockLayout) return;

        if (_handRect == null) _handRect = transform as RectTransform;
        if (_handRect == null) return;

        int count = _cards.Count;
        if (count == 0) return;

        float usedAngle = maxAngle;
        if (autoAngle)
        {
            usedAngle = Mathf.Min(autoAngleMax, Mathf.Max(0f, (count - 1) * anglePerCard));
            usedAngle = Mathf.Min(usedAngle, maxAngle);
        }

        float half = usedAngle * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float t = (count == 1) ? 0.5f : i / (float)(count - 1);
            float angle = Mathf.Lerp(-half, half, t);

            // Hover gap: focus 카드 기준으로 좌/우 카드에 angle offset을 줘서 빈 공간 생성
            if (_focusIndex >= 0 && _focusIndex < count && count > 1)
            {
                if (i < _focusIndex) angle -= hoverGapDegrees * 0.5f;
                else if (i > _focusIndex) angle += hoverGapDegrees * 0.5f;
                // focus 카드 자체는 그대로(위로 띄우는 건 CardUIInteraction이 담당)
            }

            float rad = angle * Mathf.Deg2Rad;

            Vector2 pos = centerPivot + new Vector2(
                Mathf.Sin(rad) * radius,
                Mathf.Cos(rad) * radius
            );

            // 가장자리 아래로
            float denom = Mathf.Max(0.0001f, half);
            float edge01 = Mathf.Abs(angle) / denom; // 0~1+
            pos.y -= edge01 * edge01 * edgeDropStrength * half;

            Quaternion rot = Quaternion.identity;
            if (rotateAlongArc)
                rot = Quaternion.Euler(0f, 0f, -angle * rotationMultiplier);

            _cards[i].anchoredPosition = pos;
            _cards[i].localRotation = rot;

            if (setSiblingIndex)
                _cards[i].SetSiblingIndex(i);
        }
    }
}

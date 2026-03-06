using DG.Tweening;
using UnityEngine;

public class ParallaxImage : MonoBehaviour
{
    [SerializeField] private float moveAmount = 20f;  // 움직임의 최대 반경 (픽셀)
    [SerializeField] private float smoothSpeed = 5f;  // 움직임의 부드러움 정도
    public RectTransform RectTransform 
    {
        get
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            return _rectTransform;
        }
    }
    private RectTransform _rectTransform;
    public CanvasGroup CanvasGroup 
    {
        get
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            return _canvasGroup;
        }
    }
    private CanvasGroup _canvasGroup;

    private Vector2 startPosition;
    private bool isParallax = false;
    private void Start()
    {
        startPosition = RectTransform.anchoredPosition;
    }

    private void Update()
    {
        if (!isParallax)
        {
            return;
        }

        // 마우스의 현재 화면 좌표를 -1 ~ 1 사이의 값으로 정규화
        float mouseX = (Input.mousePosition.x / Screen.width) * 2f - 1f;
        float mouseY = (Input.mousePosition.y / Screen.height) * 2f - 1f;

        float deltaX = Mathf.Clamp(mouseX * moveAmount, -35, 35);
        float deltaY = Mathf.Clamp(mouseY * moveAmount, -35, 35);

        Vector2 targetPos = new Vector2(
            startPosition.x - deltaX,
            startPosition.y - deltaY
        );

        RectTransform.anchoredPosition = Vector2.Lerp(RectTransform.anchoredPosition, targetPos, Time.deltaTime * smoothSpeed);
    }
    public void StartParallax()
    {
        isParallax = true;
    }
}
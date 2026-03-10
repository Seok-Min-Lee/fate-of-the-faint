using DG.Tweening;
using UnityEngine;

public class EndingCredit : MonoBehaviour
{
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 endPosition;
    
    private RectTransform rectTransform;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = startPosition;
    }
    public Tween Play(float duration)
    {
        return rectTransform.DOAnchorPos(endPosition, duration);
    }
}

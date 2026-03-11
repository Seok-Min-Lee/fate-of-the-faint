using DG.Tweening;
using UnityEngine;

public class UICurtain : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Vector2 openOffsetMin;
    [SerializeField] private Vector2 closeOffsetMin;
    [SerializeField] private bool isRandomRoation;
    private void Start()
    {
        Open();
    }
    public Tween Open()
    {
        return Activate(openOffsetMin);
    }
    public Tween Close()
    {
        return Activate(closeOffsetMin);
    }
    private Tween Activate(Vector2 offset)
    {
        if (isRandomRoation)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0, 360));
        }

        return DOTween.To(() => rectTransform.offsetMin.x,
           x => rectTransform.offsetMin = new Vector2(x, rectTransform.offsetMin.y),
           offset.x,
           1f
        ).SetEase(Ease.OutCubic);
    }
}

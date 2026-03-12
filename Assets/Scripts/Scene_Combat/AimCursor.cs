using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AimCursor : RaycastCursor<ITargetable>
{
    [SerializeField] private Sprite lockImage;
    [SerializeField] private Sprite unlockImage;
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

        ITargetable target = RaycastTargetUnderCursor();

        if (target != null && !target.Instance.IsDead)
        {
            Lock(target.AimPoint.position);
        }
        else
        {
            Vector3 mouseScreen = Input.mousePosition;
            Vector3 to = cam.ScreenToWorldPoint(new Vector3(
                mouseScreen.x,
                mouseScreen.y,
                cam.nearClipPlane + 1f
            ));

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
        Image.color = Color.white;

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

        sequence.Append(transform.DOLocalRotate(new Vector3(0, 0, 120), 0.75f));
        sequence.Join(transform.DOScale(Vector3.one * 0.8f, 0.75f));

        sequence.SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
    }
}

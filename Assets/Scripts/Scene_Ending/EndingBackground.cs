using DG.Tweening;
using UnityEngine;

public class EndingBackground : MonoBehaviour
{
    [SerializeField] private Vector3 startScale;
    [SerializeField] private Vector3 endScale;

    private void Start()
    {
        transform.localScale = startScale;
    }
    public Tween Play(float duration)
    {
        return transform.DOScale(endScale, duration);
    }
}

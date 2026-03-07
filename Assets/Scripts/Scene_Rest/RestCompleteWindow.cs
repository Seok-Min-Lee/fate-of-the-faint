using DG.Tweening;
using System.Net;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RestCompleteWindow : UIWindow
{
    [SerializeField] private CardDisplayViewPool pool;
    [SerializeField] private Transform target;
    public void CompleteEnhance(CardSO after)
    {
        CardDisplayView view = pool.Pop();
        view.Init(
            id: 0,
            hoverScale: 1f,
            origin: after,
            parent: transform
        );
        view.transform.position = transform.position;

        Vector3 startPos = view.transform.position;
        Vector3 endPos = target.position;
        Sequence sequence = DOTween.Sequence();

        sequence.AppendInterval(1f);
        sequence.Append(view.transform.DOScale(Vector3.one * 0.1f, 0.5f));

        sequence.Append(DOVirtual.Float(0, 1, 0.5f, t => 
        {
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            currentPos.y -= Mathf.Sin(t * Mathf.PI) * 500;
            view.transform.position = currentPos;
        }).SetEase(Ease.Linear));

        sequence.AppendCallback(() =>
        {
            pool.Push(view);
        });

    }
    public void OnClickNext()
    {
        PlayManager.Instance.SaveData();
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MAP);
    }
}

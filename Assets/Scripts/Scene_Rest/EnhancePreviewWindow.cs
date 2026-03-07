using DG.Tweening;
using UnityEngine;

public class EnhancePreviewWindow : UIWindow
{
    [SerializeField] private CardDisplayViewPool pool;
    [SerializeField] private Transform parent;

    [SerializeField] private RestCompleteWindow completeWindow;

    private CardDisplayView beforeView;
    private CardDisplayView afterView;
    public void Bind(CardEntry before, CardSO after)
    {
        beforeView = pool.Pop();
        beforeView.Init(
            id: 0,
            hoverScale: 1f,
            entry: before,
            origin: before.Origin,
            parent: parent
        );
        beforeView.transform.SetAsFirstSibling();

        afterView = pool.Pop();
        afterView.Init(
            id: 0,
            hoverScale: 1f,
            origin: after,
            parent: parent
        );
        afterView.transform.SetAsLastSibling();
    }
    private void OnDisable()
    {
        if (beforeView != null)
        {
            pool.Push(beforeView);
            beforeView = null;
        }

        if (afterView != null)
        {
            pool.Push(afterView);
            afterView = null;
        }
    }

    public void OnClickCancel()
    {
        ChangeWindow(WindowType.EnhancePreview, WindowMode.Revert);
    }
    public void OnClickSelect()
    {
        PlayManager.Instance.CurrentData.RemoveCard(beforeView.Entry.Origin.Id, beforeView.Entry.SubId);
        PlayManager.Instance.CurrentData.AddCard(afterView.Origin);

        completeWindow.CompleteEnhance(afterView.Origin);
        
        ChangeWindow(WindowType.RestComplete, WindowMode.Single);
    }
}

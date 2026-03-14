using UnityEngine;

public class EnhancePreviewWindow : UIWindow
{
    [SerializeField] private CardDisplayViewPool pool;
    [SerializeField] private Transform parent;

    [SerializeField] private RestCompleteWindow completeWindow;

    private CardEntry before;
    private CardSO after;
    private CardDisplayView beforeView;
    private CardDisplayView afterView;
    public void Bind(CardEntry before, CardSO after)
    {
        this.before = before;
        this.after = after;

        beforeView = CreateView(before.Origin);
        beforeView.transform.SetAsFirstSibling();

        afterView = CreateView(after);
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
        RunManager.Instance.CurrentData.RemoveCard(before.Id, before.SubId);
        RunManager.Instance.CurrentData.AddCard(after);

        completeWindow.CompleteEnhance(
            before: before.Origin, 
            after: after
        );
        
        ChangeWindow(WindowType.RestComplete, WindowMode.Single);
    }
    private CardDisplayView CreateView(CardSO data)
    {
        CardDisplayView view = pool.Pop();

        view.Init(
            index: 0,
            origin: data
        );
        view.transform.localScale = Vector3.one;
        view.transform.parent = parent;

        return view;
    }
}

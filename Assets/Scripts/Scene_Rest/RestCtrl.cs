using DG.Tweening;
using UnityEngine;

public class RestCtrl : MonoBehaviour
{
    [SerializeField] private CanvasGroup dimmedCG;
    [SerializeField] private UIWindowManager windowManager;

    private void Start()
    {
        dimmedCG.alpha = 1f;
        dimmedCG.DOFade(0.75f, 2.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }
    public void OnClickCardDisplay()
    {
        windowManager.ActivateWindow(WindowType.CardDisplay, WindowMode.Single);
    }
    public void OnClickMap()
    {
        windowManager.ActivateWindow(WindowType.Map, WindowMode.Single);
    }
    public void OnClickSetting()
    {
        windowManager.ActivateWindow(WindowType.Setting, WindowMode.Single);
    }
}

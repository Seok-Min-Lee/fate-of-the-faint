using DG.Tweening;
using UnityEngine;

public class ShopCtrl : MonoBehaviour
{
    [SerializeField] private UICurtain curtain;
    [SerializeField] private UIWindowManager windowManager;
    
    private void Start()
    {
        if (windowManager.TryGetWindow(WindowType.Shop, out UIWindow window) &&
            window is ShopWindow shopWindow)
        {
            shopWindow.Init();
        }
    }
    public void OnClickShop()
    {
        windowManager.ActivateWindow(WindowType.Shop, WindowMode.Single);
    }
    public void OnClickNext()
    {
        curtain.Close().OnComplete(() =>
        {
            RunManager.Instance.SaveData();
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MAP);
        });
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

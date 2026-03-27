using DG.Tweening;
using UnityEngine;

public class ShopCtrl : BaseSceneCtrl
{
    [SerializeField] private UICurtain curtain;
    
    private void Start()
    {
        AudioManager.Instance.PlayBGM(SoundKey.NormalBGM);

        if (windowManager.TryGetWindow(WindowType.Shop, out UIWindow window) &&
            window is ShopWindow shopWindow)
        {
            shopWindow.Init();
        }
    }
    public void OnClickShop()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        windowManager.ActivateWindow(WindowType.Shop, WindowMode.Single);
    }
    public void OnClickNext()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        curtain.Close().OnComplete(() =>
        {
            RunManager.Instance.SaveData();
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MAP);
        });
    }
}

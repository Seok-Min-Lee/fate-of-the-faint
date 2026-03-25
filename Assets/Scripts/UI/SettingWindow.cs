using UnityEngine;

public class SettingWindow : UIWindow
{
    public void OnClickBack()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        ChangeWindow(WindowType.Setting, WindowMode.Revert);
    }
    public void OnClickHome()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.HOME);
    }
    public void OnClickExit()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        Application.Quit();
    }
}

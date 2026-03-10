using UnityEngine;

public class SettingWindow : UIWindow
{
    public void OnClickBack()
    {
        ChangeWindow(WindowType.Setting, WindowMode.Revert);
    }
    public void OnClickHome()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.HOME);
    }
    public void OnClickExit()
    {
        Application.Quit();
    }
}

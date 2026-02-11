using UnityEngine;

public class SettingWindow : UIWindow
{
    public void OnClickBack()
    {
        ChangeWindow(WindowType.Setting, WindowMode.Revert);
    }
}

using UnityEngine;

public class MapWindow : UIWindow
{
    public void OnClickBack()
    {
        ChangeWindow(WindowType.Map, WindowMode.Revert);
    }
}

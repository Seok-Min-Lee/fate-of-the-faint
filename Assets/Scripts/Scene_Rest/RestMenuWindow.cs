public class RestMenuWindow : UIWindow
{
    public void OnClickRest()
    {
        ChangeWindow(WindowType.RestComplete, WindowMode.Single);
    }
    public void OnClickEnhance()
    {
        ChangeWindow(WindowType.EnhanceDisplay, WindowMode.Single);
    }
}

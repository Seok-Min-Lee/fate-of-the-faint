public class MapWindow : UIWindow
{
    public void OnClickBack()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        ChangeWindow(WindowType.Map, WindowMode.Revert);
    }
}

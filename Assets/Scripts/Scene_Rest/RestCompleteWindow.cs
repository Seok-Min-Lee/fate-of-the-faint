using UnityEngine;

public class RestCompleteWindow : UIWindow
{
    public void OnClickNext()
    {
        PlayManager.Instance.SaveData();
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MAP);
    }
}

using System.Collections;
using UnityEngine;

public class InitCtrl : MonoBehaviour
{
    private IEnumerator Start()
    {
        Debug.Log(RunManager.Instance == null);
        while (!RunManager.Instance.isLoad)
        {
            yield return new WaitForSeconds(1f);
        }

        Debug.Log(AudioManager.Instance == null);
        AudioManager.Instance.Load(() => UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.HOME));
    }
}

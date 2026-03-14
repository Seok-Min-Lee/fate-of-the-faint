using System.Collections;
using UnityEngine;

public class InitCtrl : MonoBehaviour
{
    private IEnumerator Start()
    {
        while (!RunManager.Instance.isLoad)
        {
            yield return new WaitForSeconds(1f);
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.HOME);
    }
}

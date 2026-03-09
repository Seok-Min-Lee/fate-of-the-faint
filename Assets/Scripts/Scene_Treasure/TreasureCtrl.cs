using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreasureCtrl : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TreasurePopup popup;
    [SerializeField] Button nextButton;
    [SerializeField] TextMeshProUGUI tipText;
    private void Start()
    {
        nextButton.gameObject.SetActive(false);

        Utils.TMPDOText(tipText, 2f);
    }
    public void OnClickBox()
    {
        button.interactable = false;
        popup.Init();
    }
    public void OnClickNext()
    {
        PlayManager.Instance.SaveData();
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MAP);
    }
    public void ShowNext()
    {
        nextButton.gameObject.SetActive(true);
    }
}

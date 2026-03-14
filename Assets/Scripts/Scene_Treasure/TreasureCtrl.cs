using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreasureCtrl : MonoBehaviour
{
    [SerializeField] private UIWindowManager windowManager;
    [SerializeField] private UICurtain curtain;

    [SerializeField] private Button button;
    [SerializeField] private TreasurePopup popup;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI tipText;
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
        curtain.Close().OnComplete(() =>
        {
            RunManager.Instance.SaveData();
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MAP);
        });
    }
    public void ShowNext()
    {
        nextButton.gameObject.SetActive(true);
    }
    public void OnClickCardDisplay()
    {
        windowManager.ActivateWindow(WindowType.CardDisplay, WindowMode.Single);
    }
    public void OnClickMap()
    {
        windowManager.ActivateWindow(WindowType.Map, WindowMode.Single);
    }
    public void OnClickSetting()
    {
        windowManager.ActivateWindow(WindowType.Setting, WindowMode.Single);
    }
}

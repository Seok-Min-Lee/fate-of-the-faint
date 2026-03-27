using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreasureCtrl : BaseSceneCtrl
{
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
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        button.interactable = false;
        popup.Init();
    }
    public void OnClickNext()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

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
}

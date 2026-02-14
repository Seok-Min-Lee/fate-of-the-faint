using DG.Tweening;
using TMPro;
using UnityEngine;

public class DefeatWindow : UIMotionWindow
{
    [SerializeField] private CanvasGroup dimmedCG;
    [SerializeField] private CanvasGroup contentCG;
    [SerializeField] private TextMeshProUGUI headText;

    private void Awake()
    {
        _handler.Add(MotionKey.WindowShow, Show);
    }
    public void OnClickBack()
    {
        PlayManager.Instance.ClearPlayData();
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.HOME);
    }
    private Sequence Show()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            ChangeWindow(WindowType.Defeat, WindowMode.Single);
            dimmedCG.alpha = 0f;
            contentCG.alpha = 0f;
            headText.text = string.Empty;
        });
        sequence.Append(dimmedCG.DOFade(1f, 1f));
        sequence.Append(contentCG.DOFade(1f, 0.5f));
        sequence.JoinCallback(() =>
        {
            headText.text = "You are Slain!";
            Utils.TMPDOText(headText, 1f);
        });

        return sequence;
    }
}

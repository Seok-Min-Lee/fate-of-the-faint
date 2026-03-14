using DG.Tweening;
using UnityEngine;

public class HomeCtrl : MonoBehaviour
{
    [SerializeField] private ParallaxImage background;
    [SerializeField] private CanvasGroup dimmedCG;
    [SerializeField] private CanvasGroup titleCG;
    [SerializeField] private CanvasGroup menuCG;

    [SerializeField] private MenuButton continueButton;
    private void Start()
    {
        background.CanvasGroup.alpha = 0f;
        titleCG.alpha = 0f;
        menuCG.alpha = 0f;
        menuCG.blocksRaycasts = false;

        continueButton.gameObject.SetActive(RunManager.Instance.CurrentData != null);

        Show();
    }
    private void Show()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(background.CanvasGroup.DOFade(1f, 1f).SetEase(Ease.InCubic));
        sequence.Append(titleCG.DOFade(1f, 1f).SetEase(Ease.InCubic));
        sequence.Join(menuCG.DOFade(1f, 1f).SetEase(Ease.InCubic));
        sequence.AppendCallback(() => 
        {
            background.StartParallax();

            menuCG.blocksRaycasts = true;

            dimmedCG.alpha = 1f;
            dimmedCG.DOFade(0.5f, 7.5f)
                    .SetEase(Ease.InOutExpo)
                    .SetLoops(-1, LoopType.Yoyo);
        });
    }

    public void OnClickContinue()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MAP);
    }
    public void OnClickNewGame()
    {
        RunManager.Instance.ClearPlayData();
        RunManager.Instance.SaveData();
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MAP);
    }
    public void OnClickExit()
    {
        Application.Quit();
    }
}

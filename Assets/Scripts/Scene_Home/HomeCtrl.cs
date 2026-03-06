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

        continueButton.gameObject.SetActive(PlayManager.Instance.CurrentData != null);

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
        PlayManager.Instance.ClearPlayData();
        PlayManager.Instance.SavePlayData();
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MAP);
    }
    public void OnClickExit()
    {
        Application.Quit();
    }
}

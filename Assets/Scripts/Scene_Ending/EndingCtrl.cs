using DG.Tweening;
using UnityEngine;

public class EndingCtrl : MonoBehaviour
{
    [SerializeField] private CanvasGroup dimmedCG;
    [SerializeField] private CanvasGroup homeButtonCG;
    [SerializeField] private GameObject touchSign;

    [SerializeField] private EndingBackground background;
    [SerializeField] private EndingCredit endingCredit;
    [SerializeField] private float duration;
    private void Start()
    {
        homeButtonCG.blocksRaycasts = false;
        dimmedCG.alpha = 1f;
        homeButtonCG.alpha = 0f;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(dimmedCG.DOFade(0f, 1f));

        sequence.Append(background.Play(duration));
        sequence.Join(endingCredit.Play(duration));
        sequence.InsertCallback(3f, () => touchSign.SetActive(true));

        sequence.Append(homeButtonCG.DOFade(1f, 0.5f));
        sequence.AppendCallback(() =>
        {
            touchSign.SetActive(false);
            homeButtonCG.blocksRaycasts = true;
        });
    }
    public void OnClickBackground()
    {
        homeButtonCG.blocksRaycasts = true;
        homeButtonCG.alpha += 0.1f;
    }
    public void OnClickHome()
    {
        PlayManager.Instance.RemovePlayData();
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.HOME);
    }
}

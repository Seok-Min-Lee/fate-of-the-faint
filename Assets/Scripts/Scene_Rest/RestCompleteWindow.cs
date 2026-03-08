using DG.Tweening;
using TMPro;
using UnityEngine;

public class RestCompleteWindow : UIWindow
{
    [Header("[Heal]")]
    [SerializeField] private CanvasGroup healTextCG;
    [SerializeField] private HpMonoSystem hpSystem;
    [SerializeField] private ParticleSystem healParticle;

    [Header("[Enhance]")]
    [SerializeField] private CardDisplayViewPool pool;
    [SerializeField] private ParticleSystem[] enhanceParticles;
    [SerializeField] private Transform cardDeck;
    [SerializeField] private int strength;
    [SerializeField] private float duration;
    [SerializeField] private int vibrato;
    public void CompleteHeal()
    {
        RectTransform healTextTransform = healTextCG.GetComponent<RectTransform>();

        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            hpSystem.Refresh();
            healTextCG.alpha = 0f;
        });

        sequence.Append(healTextCG.DOFade(1f, 1f).SetEase(Ease.InCubic));
        sequence.Join(healTextTransform.DOLocalMoveY(150, 1f).SetEase(Ease.OutCubic));
        sequence.JoinCallback(() => healParticle.Play());

        sequence.Append(healTextCG.DOFade(0f, 4f).SetEase(Ease.InCubic));
        sequence.Join(healTextTransform.DOLocalMoveY(200, 4f).SetEase(Ease.InCubic));
    }
    public void CompleteEnhance(CardSO before, CardSO after)
    {
        healTextCG.gameObject.SetActive(false);
        healParticle.gameObject.SetActive(false);

        CardDisplayView view = pool.Pop();

        view.Init(
            index: 0,
            hoverScale: 1f,
            origin: before
        );

        view.transform.position = transform.position;
        view.transform.localScale = Vector3.one * 1.25f;
        view.transform.parent = transform;

        //
        Vector3 startPos = view.transform.position;
        Vector3 endPos = cardDeck.position;

        Sequence sequence = DOTween.Sequence();

        sequence.AppendInterval(1f);

        sequence.Append(ModifyMotion(view.Cost, after.Cost.ToString()));
        sequence.Join(PunchMotion(view));
        sequence.JoinCallback(() => enhanceParticles[0].Play());

        sequence.Append(ModifyMotion(view.Name, after.Name));
        sequence.Join(PunchMotion(view));
        sequence.JoinCallback(() => enhanceParticles[1].Play());

        sequence.Append(ModifyMotion(view.Desc, after.Description));
        sequence.Join(PunchMotion(view));
        sequence.JoinCallback(() => enhanceParticles[2].Play());

        sequence.Append(view.transform.DOScale(Vector3.one * 0.05f, 0.5f));

        sequence.Append(DOVirtual.Float(0, 1, 0.5f, t => 
        {
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            currentPos.x -= Mathf.Sin(t * Mathf.PI) * Mathf.Abs(endPos.x - startPos.x) * 0.5f;
            view.transform.position = currentPos;
        }).SetEase(Ease.Linear));

        sequence.AppendCallback(() =>
        {
            pool.Push(view);
        });
    }

    public void OnClickNext()
    {
        PlayManager.Instance.SaveData();
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MAP);
    }

    private Sequence ModifyMotion(TextMeshProUGUI component, string value)
    {
        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() =>
        {
            component.text = value;
            component.transform.localScale = Vector3.one * 2f;
        });
        seq.Append(component.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        return seq;
    }

    private Tweener PunchMotion(CardDisplayView view)
    {
        return view.RectTransform.DOPunchPosition(
            punch: Random.insideUnitCircle * strength,
            duration: duration,
            vibrato: vibrato
        );
    }
}

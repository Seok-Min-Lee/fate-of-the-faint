using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class DefeatWindow : UIMotionWindow
{
    [SerializeField] private CanvasGroup dimmedCG;
    [SerializeField] private CanvasGroup contentCG;
    [SerializeField] private CanvasGroup homeButtonCG;
    [SerializeField] private Transform recordParent;
    [SerializeField] private RecordView recordViewPrefab;

    [SerializeField] private UICurtain curtain;

    protected override void Awake()
    {
        _handler.Add(MotionKey.WindowShow, Show());
    }
    public void OnClickBack()
    {
        curtain.Close().OnComplete(() =>
        {
            RunManager.Instance.RemovePlayData();
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.HOME);
        });
    }
    private Func<IEnumerator> Show()
    {
        return () => ShowCor();
    }
    private IEnumerator ShowCor()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            ChangeWindow(WindowType.Defeat, WindowMode.Single);
            dimmedCG.alpha = 0f;
            contentCG.alpha = 0f;
            homeButtonCG.alpha = 0f;
        });
        sequence.Append(dimmedCG.DOFade(1f, 1f));
        sequence.Append(contentCG.DOFade(1f, 0.5f));

        foreach (PlayRecord record in RunManager.Instance.CurrentData.Records.Values)
        {
            sequence.AppendCallback(() =>
            {
                RecordView view = GameObject.Instantiate<RecordView>(recordViewPrefab, recordParent);
                view.Init($"{record.Id}: {record.Value}");
            });
            sequence.AppendInterval(0.2f);
        }

        sequence.Append(homeButtonCG.DOFade(1f, 0.5f));

        yield return sequence.WaitForCompletion();
    }
}

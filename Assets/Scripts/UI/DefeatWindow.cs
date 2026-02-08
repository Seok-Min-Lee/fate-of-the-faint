using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DefeatWindow : UIWindow
{
    [SerializeField] private CanvasGroup dimmedCG;
    [SerializeField] private CanvasGroup contentCG;
    [SerializeField] private TextMeshProUGUI headText;

    private void Awake()
    {
        _handler.Add(MotionKey.WindowShow, Show);
        gameObject.SetActive(false);
        dimmedCG.alpha = 0f;
        contentCG.alpha = 0f;
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
            gameObject.SetActive(true);
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

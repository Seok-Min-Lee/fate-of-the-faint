using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VictoryWindow : UIWindow
{
    [SerializeField] private CanvasGroup dimmedCG;
    [SerializeField] private CanvasGroup contentCG;
    [SerializeField] Transform rewardParent;
    [SerializeField] RewardButton rewardButtonPrefab;
    [SerializeField] TextMeshProUGUI tipText;

    [SerializeField] RewardPair[] rewardPairs;

    private Queue<RewardButton> rewardButtonQueue = new Queue<RewardButton>();

    private void Awake()
    {
        _handler.Add(MotionKey.WindowShow, Show);
        gameObject.SetActive(false);
    }
    public void OnClickGoldButton()
    {
        //PlayManager.Instance.CurrentData.AddGold();
    }
    public void OnClickRelicButton()
    {
        //PlayManager.Instance.CurrentData.AddRelic();
    }
    public void OnClickCardButton()
    {

    }
    public void OnClickRewardCard()
    {
        //PlayManager.Instance.CurrentData.AddCard();
    }
    public void OnClickNext()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MAP);
    }
    private Sequence Show()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            gameObject.SetActive(true);
            dimmedCG.alpha = 0f;
            contentCG.alpha = 0f;
            tipText.text = string.Empty;
            AddReward();
        });
        sequence.Append(dimmedCG.DOFade(1f, 1f));
        sequence.Append(contentCG.DOFade(1f, 0.5f));
        sequence.JoinCallback(() => 
        {
            tipText.text = "Tip: Every path you take will always have at aleast 1 treasure room.";
            Utils.TMPDOText(tipText, 2f);
        });

        return sequence;
    }

    private void AddReward()
    {
        int num = UnityEngine.Random.Range(1, 4);

        for (int i = 0; i < num; i++)
        {
            RewardButton rb = rewardButtonQueue.Count > 0 ?
                            rewardButtonQueue.Dequeue() :
                            GameObject.Instantiate(rewardButtonPrefab, rewardParent);

            RewardPair rp = rewardPairs[UnityEngine.Random.Range(1, rewardPairs.Length)];

            rb.Init(this, rp.image, rp.name);
        }
    }
    public void Charge(RewardButton button)
    {
        rewardButtonQueue.Enqueue(button);
    }
}
[Serializable]
public struct RewardPair
{
    public Sprite image;
    public string name;
}
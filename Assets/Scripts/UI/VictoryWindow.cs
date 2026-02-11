using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VictoryWindow : UIMotionWindow
{
    [SerializeField] CardRewardWindow cardRewardWindow;

    [SerializeField] private CanvasGroup dimmedCG;
    [SerializeField] private CanvasGroup contentCG;
    [SerializeField] Transform rewardParent;
    [SerializeField] RewardButton rewardButtonPrefab;
    [SerializeField] TextMeshProUGUI tipText;

    [SerializeField] RewardPreset[] rewardPresets;

    private Queue<RewardButton> rewardButtonQueue = new Queue<RewardButton>();

    private void Awake()
    {
        _handler.Add(MotionKey.WindowShow, Show);
    }
    public void OnClickGoldButton(RewardButton button)
    {
        button.PushToPool();
    }
    public void OnClickRelicButton(RewardButton button)
    {
        button.PushToPool();
    }
    public void OnClickCardButton(RewardButton button)
    {
        cardRewardWindow.Init();
        cardRewardWindow.Bind(button);
        ChangeWindow(WindowType.CardRewards, WindowMode.Single);
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
            ChangeWindow(WindowType.Victory, WindowMode.Single);
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
            RewardButton button = rewardButtonQueue.Count > 0 ?
                                rewardButtonQueue.Dequeue() :
                                GameObject.Instantiate(rewardButtonPrefab, rewardParent);

            //RewardPreset preset = rewardPresets[UnityEngine.Random.Range(0, rewardPresets.Length)];
            RewardPreset preset = rewardPresets[1];

            Action<RewardButton> onClick = null;
            switch (preset.type)
            {
                case RewardPreset.Type.Gold:
                    onClick = (button) => OnClickGoldButton(button); 
                    break;
                case RewardPreset.Type.Card:
                    onClick = (button) => OnClickCardButton(button);
                    break;
                case RewardPreset.Type.Relic:
                    onClick = (button) => OnClickRelicButton(button);
                    break;
            }

            button.Init(
                pool: this, 
                sprite: preset.image,
                text: preset.name,
                onClick: onClick
            );
        }
    }
    public void Charge(RewardButton button)
    {
        rewardButtonQueue.Enqueue(button);
    }
}
[Serializable]
public struct RewardPreset
{
    public enum Type
    {
        Gold,
        Card,
        Relic
    }
    public Type type;
    public Sprite image;
    public string name;
}
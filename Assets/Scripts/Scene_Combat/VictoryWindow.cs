using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class VictoryWindow : UIMotionWindow
{
    [SerializeField] private CombatManager combatCtrl;
    [SerializeField] private RelicMonoSystem relicSystem;
    [SerializeField] private GoldMonoSystem goldSystem;
    [SerializeField] private CardRewardWindow cardRewardWindow;

    [SerializeField] private CanvasGroup dimmedCG;
    [SerializeField] private CanvasGroup contentCG;
    [SerializeField] private RewardButtonPool rewardButtonPool;
    [SerializeField] private Transform rewardParent;
    [SerializeField] private TextMeshProUGUI tipText;

    [SerializeField] private UICurtain curtain;

    [SerializeField] private Sprite cardIcon;
    [SerializeField] private Sprite goldIcon;

    private Dictionary<RewardButton, List<CardSO>> cardSampleDic = new Dictionary<RewardButton, List<CardSO>>();
    protected override void Awake()
    {
        _handler.Add(MotionKey.WindowShow, Show);
    }
    public void OnClickNext()
    {
        curtain.Close().OnComplete(() =>
        {
            if (PlayManager.Instance.MapGraph.LatestNode.Type == MapNodeType.Boss)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.ENDING);
            }
            else
            {
                combatCtrl.CombatSystem.Save();
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MAP);
            }
        });
    }
    private Sequence Show()
    {
        string tipStr = tipText.text;
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            ChangeWindow(WindowType.Victory, WindowMode.Single);
            dimmedCG.alpha = 0f;
            contentCG.alpha = 0f;
            tipText.text = string.Empty;
        });
        sequence.Append(dimmedCG.DOFade(1f, 1f));
        sequence.Append(contentCG.DOFade(1f, 0.5f));
        sequence.JoinCallback(() => 
        {
            tipText.text = tipStr;
            Utils.TMPDOText(tipText, 2f);
        });

        return sequence;
    }

    public void OnClickGoldReward(RewardButton button, int amount)
    {
        rewardButtonPool.Push(button);
        goldSystem.Add(amount);
    }
    public void OnClickRelicReward(RewardButton button, RelicSO relic)
    {
        relicSystem.AddRelic(relic);
        rewardButtonPool.Push(button);
    }
    public void OnClickCardReward(RewardButton button)
    {
        if (!cardSampleDic.TryGetValue(button, out List<CardSO> samples))
        {
            return;
        }

        cardRewardWindow.Init(samples, () => SelectCard(button));
        ChangeWindow(WindowType.CardRewards, WindowMode.Single);
    }
    public void Init(int gold)
    {
        AddGoldRewardButton(gold);
        AddCardRewardButton();
        AddRelicRewardButton();
    }
    private void AddGoldRewardButton(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        RewardButton button = rewardButtonPool.Pop();

        button.Init(
            parent: rewardParent,
            sprite: goldIcon,
            text: $"{amount} 골드",
            onClick: (button) => OnClickGoldReward(button, amount)
        );
    }
    private void AddCardRewardButton()
    {
        //if (UnityEngine.Random.Range(0, 10) == 0)
        //{
        //    return;
        //}

        RewardButton button = rewardButtonPool.Pop(); 

        button.Init(
            parent: rewardParent,
            sprite: cardIcon,
            text: "카드 선택",
            onClick: (button) => OnClickCardReward(button)
        );

        int count = PlayManager.Instance.CurrentData.RewardCardOptionCount;
        List<CardSO> samples = Utils.PickRandom<CardSO>(PlayManager.Instance.Catalog.CardList.Where(x => x.UpgradeCard != null), count);

        cardSampleDic.Add(button, samples);
    }
    private void AddRelicRewardButton()
    {
        //if (UnityEngine.Random.Range(0, 10) == 0)
        //{
        //    return;
        //}

        //
        HashSet<RelicSO> hashset = PlayManager.Instance.CurrentData.Relics.Select(x => x.Origin).ToHashSet();
        List<RelicSO> candidates = PlayManager.Instance.Catalog.RelicList
                                   .Where(candidate => !hashset.Contains(candidate))
                                   .ToList();

        if (candidates.Count == 0)
        {
            return;
        }
        RelicSO reward = candidates[UnityEngine.Random.Range(0, candidates.Count)];

        RewardButton button = rewardButtonPool.Pop();

        button.Init(
            parent: rewardParent,
            sprite: reward.Icon,
            text: reward.DisplayName,
            onClick: (button) => OnClickRelicReward(button, reward)
        );
    }
    private void SelectCard(RewardButton button)
    {
        cardSampleDic.Remove(button);
        rewardButtonPool.Push(button);
    }
}
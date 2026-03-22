using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopWindow : UIWindow
{
    [SerializeField] private CardShopViewPool cardPool;
    [SerializeField] private RelicShopViewPool relicPool;

    [SerializeField] private RelicMonoSystem relicSystem;
    [SerializeField] private GoldMonoSystem goldSystem;
    [SerializeField] private PurchasePopup popup;
    [SerializeField] private TooltipView tooltip;
    protected override void OnEnable()
    {
        base.OnEnable();
        popup.gameObject.SetActive(false);
    }
    public void Init()
    {
        List<CardSO> cards = RunManager.Instance.GetUnupgradedCards(5);
        for (int i = 0; i < cards.Count; i++)
        {
            CardShopView view = cardPool.Pop();

            int price = cards[i].Rarity switch
            {
                CardRarity.Common => 0,
                CardRarity.Uncommon => 30,
                CardRarity.Rare => 60,
                _ => 0
            };
            price += UnityEngine.Random.Range(50, 100);

            view.Init(
                data: cards[i], 
                price: price,
                onClick: (source) => OnClickGoods(source)
            );
        }

        List<RelicSO> relics = RunManager.Instance.GetUnacquiredRelics(3);
        for (int i = 0; i < relics.Count; i++)
        {
            RelicShopView view = relicPool.Pop();

            view.Init(
                data: relics[i],
                tooltip: tooltip,
                price: UnityEngine.Random.Range(100, 200),
                onClick: (source) => OnClickGoods(source)
            );
        }
    }
    public void OnClickCancel()
    {
        ChangeWindow(WindowType.Shop, WindowMode.Revert);
    }
    public void OnClickRemove()
    {
        ChangeWindow(WindowType.CardRemove, WindowMode.Single);
    }
    private void OnClickGoods(IShopView source)
    {
        Action action = null;
        if (source is CardShopView card)
        {
            action = () => OnPurchaseCard(card);
        }
        else if (source is RelicShopView relic)
        {
            action = () => OnPurchaseRelic(relic);
        }
        else
        {
            return;
        }

        popup.Init(action);
    }
    private void OnPurchaseCard(CardShopView view)
    {
        if (RunManager.Instance.CurrentData.Gold < view.Price)
        {
            view.FailedToPurchase();
            return;
        }

        RunManager.Instance.CurrentData.SubtractGold(view.Price);
        goldSystem.Refresh();

        RunManager.Instance.CurrentData.AddCard(view.Origin);
        cardPool.Push(view);
    }
    private void OnPurchaseRelic(RelicShopView view)
    {
        if (RunManager.Instance.CurrentData.Gold < view.Price)
        {
            view.FailedToPurchase();
            return;
        }

        RunManager.Instance.CurrentData.SubtractGold(view.Price);
        goldSystem.Refresh();

        relicSystem.AddRelic(view.Origin);
        relicPool.Push(view);
    }
}

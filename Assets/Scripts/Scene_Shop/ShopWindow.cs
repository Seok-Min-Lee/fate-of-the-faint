using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopWindow : UIWindow
{
    [SerializeField] private CardShopViewPool cardPool;
    [SerializeField] private RelicShopViewPool relicPool;

    [SerializeField] private RelicMonoSystem relicSystem;
    [SerializeField] private GoldMonoSystem goldSystem;

    [SerializeField] private HorizontalLayoutGroup cardLayout;
    [SerializeField] private HorizontalLayoutGroup relicLayout;

    [SerializeField] private PurchasePopup popup;
    [SerializeField] private TooltipView tooltip;
    protected override void OnEnable()
    {
        base.OnEnable();
        popup.gameObject.SetActive(false);

        // 레이아웃 컴포넌트는 자식 오브젝트 위치만 잡고 비활성화
        StartCoroutine(Cor());
        IEnumerator Cor()
        {
            yield return new WaitForEndOfFrame();

            cardLayout.enabled = false;
            relicLayout.enabled = false;
        }
    }
    public void Init()
    {
        // 카드 상품 생성
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

        // 유물 상품 생성
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
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        ChangeWindow(WindowType.Shop, WindowMode.Revert);
    }
    public void OnClickRemove()
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

        ChangeWindow(WindowType.CardRemove, WindowMode.Single);
    }
    private void OnClickGoods(IShopView source)
    {
        AudioManager.Instance.PlaySFX(SoundKey.TouchSFX);

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
        // 골드량 체크
        if (RunManager.Instance.CurrentData.Gold < view.Price)
        {
            view.FailedToPurchase();
            return;
        }

        // 골드 업데이트
        RunManager.Instance.CurrentData.SubtractGold(view.Price);
        goldSystem.Refresh();

        // 카드 업데이트
        RunManager.Instance.CurrentData.AddCard(view.Origin);
        view.SuccessedToPurchase().OnComplete(() => cardPool.Push(view));
    }
    private void OnPurchaseRelic(RelicShopView view)
    {
        // 골드량 체크
        if (RunManager.Instance.CurrentData.Gold < view.Price)
        {
            view.FailedToPurchase();
            return;
        }

        // 골드 업데이트
        RunManager.Instance.CurrentData.SubtractGold(view.Price);
        goldSystem.Refresh();

        // 유물 업데이트
        relicSystem.AddRelic(view.Origin);
        view.SuccessedToPurchase().OnComplete(() => relicPool.Push(view));
    }
}

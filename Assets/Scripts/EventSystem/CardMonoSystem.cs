using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// 전투 내 카드 덱(뽑을 카드, 패, 버린 카드, 소멸 덱) 풀 관리 및 드로우/사용 사이클 총괄 시스템
/// </summary>
public class CardMonoSystem : BaseMonoSystem
{
    private EventBus eventBus;
    private ActionSystem actionSystem;
    private PowerSystem powerSystem;

    [SerializeField] private CardContainer cardHand;
    [SerializeField] private CardViewPool cardViewPool;

    [SerializeField] private TextMeshProUGUI drawPileText;
    [SerializeField] private TextMeshProUGUI discardPileText;
    [SerializeField] private TextMeshProUGUI exhaustPileText;

    private List<CardInstance> cardInstanceAll = new List<CardInstance>();
    private List<CardInstance> drawPile = new List<CardInstance>();
    private List<CardInstance> hand = new List<CardInstance>();
    private List<CardInstance> discardPile = new List<CardInstance>();
    private List<CardInstance> exhaustPile = new List<CardInstance>();

    private CardInstance cardInstance;
    private ITargetable target;

    /// <summary>
    /// 시스템 초기화 시 전역 런 매니저 덱의 카드 구조를 인게임 컨텍스트용 인스턴스 배열로 구성 (셔플)
    /// </summary>
    public void Init(
        EventBus eventBus,
        ActionSystem actionSystem,
        PowerSystem powerSystem
    )
    {
        this.eventBus = eventBus;
        this.actionSystem = actionSystem;
        this.powerSystem = powerSystem;

        // 1. 원본 세이브 데이터(Run 데이터) 순회하며 현재 전투용 카드 복제 객체 할당
        List<CardInstance> instances = new List<CardInstance>();
        foreach (CardEntry entry in RunManager.Instance.CurrentData.Cards)
        {
            instances.Add(new CardInstance(
                instanceId: $"{entry.Id}_{entry.SubId}",
                entry: entry
            ));
        }

        // 2. 관리 배열 모두 비우고 셔플을 통해 최초 뽑을 카드 더미 구조 구축 반영
        cardInstanceAll.Clear();
        cardInstanceAll.AddRange(instances);
        drawPile.AddRange(Utils.Shuffle(instances));

        // 3. UI 갱신 반영
        UpdateUI();
    }
    /// <summary>
    /// 카드 사용 개시 시 에너지 결제 심사 및 카드 종류에 따른 목적지(소멸/버림) 분기 처리
    /// </summary>
    public void PlayCardStart(CardView cardView, ITargetable target)
    {
        this.cardInstance = cardView.CardInstance;
        this.target = target;

        actionSystem.ExcuteAction(source: this, type: ActionType.PlayerCardPlay, (eventContext, motionContext) =>
        {
            bool existModifier = cardInstanceAll.Any(c => c.ExistModifier);

            // 1. 카드 사용 선언 이벤트 발행 (UI 및 연출 동기화 선행 진행)
            eventBus.Publish<CardPlayDeclared>(new CardPlayDeclared(
                context: eventContext,
                motion: motionContext,
                cardView: cardView
            ));

            RequestContext requestContext = new RequestContext(source: this);

            // 2. 카드 비용만큼 코스트(에너지) 결제 차감 요청 발행
            eventBus.Publish<EnergyChangeRequested>(new EnergyChangeRequested(
                context: eventContext,
                motion: motionContext,
                request: requestContext,
                amount: -cardInstance.Cost
            ));

            // 3. 결제 승인 성공 시 동작 수행
            if (requestContext.isResult)
            {
                // 소멸(이례) 속성을 지녔는가 판별하여 맞는 더미로 영구 폐기 및 버리기 이동
                if (cardView.CardInstance.Origin.IsExhausted)
                {
                    ExhaustCard(
                        cardView: cardView,
                        context: eventContext,
                        motion: motionContext
                    );
                }
                else
                {
                    DiscardCard(
                        cardView: cardView,
                        context: eventContext,
                        motion: motionContext
                    );
                }
            }
            // 4. 결제 실패 시 타겟팅과 캐싱 객체 모두 해제 처리
            else
            {
                cardView = null;
                cardInstance = null;
                target = null;
            }

            // 5.이번 액션 한정으로 할인/코스트 추가 보정 등 일시적 수치 원복 및 초기화
            if (existModifier)
            {
                RemoveModificationsByScope(CostModificationScope.Action);
            }
        });
    }
    /// <summary>
    /// 전투 수명 주기를 갖는 비용 증감 효과 객체 만료 일괄 초기화
    /// </summary>
    public void OnCombatEnded(CombatEnded e)
    {
        if (e.Result != CombatState.Victory)
        {
            return;
        }

        RemoveModificationsByScope(CostModificationScope.Combat);
    }

    /// <summary>
    /// 플레이어 시작 단위 턴 개시 시 기본 패 드로우 루프(5장) 가동
    /// </summary>
    public void OnPlayerTurnStarted(PlayerTurnStarted e) 
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        for (int i = 0; i < 5; i++)
        {
            DrawCard(context: e.Context, motion: e.Motion);
        }
    }

    /// <summary>
    /// 단위 턴 종료 시 남은 패 전체 일괄 소모 및 턴 보유 스코프 보정 효과 초기화
    /// </summary>
    public void OnPlayerTurnEnded(PlayerTurnEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        ClearCardHand(
            context: e.Context.RewriteNew(this), 
            motion: e.Motion
        );

        RemoveModificationsByScope(CostModificationScope.Turn);
    }

    /// <summary>
    /// 카드 코스트 지불 확정 시 세부 분류군(공격/스킬/파워 등) 본연의 대상 타겟 효과 적용 프로세스 실행
    /// </summary>
    public void OnEnergyResolved(EnergyResolved e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EventContext context = e.Context.RewriteNew(this);

        // 1. 공격군 카드 처리
        if (cardInstance.Origin is AttackCardSO)
        {
            eventBus.Publish<AttackPlayed>(new AttackPlayed(
                context: context,
                motion: e.Motion,
                source: context.Combat.Player
            ));

            AttackCardSO cardSO = cardInstance.Origin as AttackCardSO;

            ApplyEffects(
                context: e.Context,
                motion: e.Motion,
                effects: cardSO.Effects
            );
        }
        // 2. 스킬군 카드 처리
        else if (cardInstance.Origin is SkillCardSO)
        {
            eventBus.Publish<SkillPlayed>(new SkillPlayed(
                context: context,
                motion: e.Motion,
                source: context.Combat.Player
            ));

            SkillCardSO cardSO = cardInstance.Origin as SkillCardSO;

            ApplyEffects(
                context: e.Context,
                motion: e.Motion,
                effects: cardSO.Effects
            );
        }
        // 3. 파워(지속/수명 스택)군 카드 처리
        else
        {
            eventBus.Publish<PowerPlayed>(new PowerPlayed(
                context: context,
                motion: e.Motion,
                source: context.Combat.Player
            ));

            PowerCardSO cardSO = cardInstance.Origin as PowerCardSO;

            powerSystem.AddPower(cardSO); // 파워 버프 구조 편입
        }
    }
    /// <summary>
    /// 타 시스템에 의한 강제 특정 매수 드로우 선언 처리
    /// </summary>
    public void OnDrawCardDeclared(DrawCardDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        EventContext context = e.Context.RewriteNew(this);

        for (int i = 0; i < e.Amount; i++)
        {
            DrawCard(context: context, motion: e.Motion);
        }
    }

    /// <summary>
    /// 특정 효과에 의한 전체 카드 코스트 비용 조작 선언 처리 UI 일괄 전파 갱신
    /// </summary>
    public void OnModifyCostDeclared(ModifyCostDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 전체 카드(덱/무덤 무관) 내부 수치 모델 비용 조작 처리
        foreach (CardInstance instance in cardInstanceAll)
        {
            instance.AddModification(new CostModification(
                scope: e.Scope,
                amount: -e.Amount,
                source: this
            ));
        }

        // 활성화(핸드) 카드 UI 외형 수치 텍스트 업데이트
        foreach (CardView view in cardViewPool.Actives)
        {
            view.ModifiyCost();
        }
    }
    /// <summary>
    /// 단일 카드가 소유 범주로 갖고 있는 전체 상태이상 및 데미지 배열 반복 연산 수행
    /// </summary>
    private void ApplyEffects(IEnumerable<EffectSO> effects, EventContext context, MotionContext motion)
    {
        List<Action> applies = new List<Action>();

        foreach (EffectSO effect in effects)
        {
            // 1. 단일 대상 또는 모든 대상 판별 배열 컴파일 추출
            List<EntityInstance> targets = GetTargets(
                type: effect.TargetType, 
                context: context
            );

            // 2. 단일 타겟에 대한 세부 단위 효과 처리 로직 콜백 대리자 취합
            Action apply = effect.Apply(
                eventBus: eventBus, 
                context: context,
                motion: motion, 
                source: context.Combat.Player, 
                targets: targets
            );

            // 3. 콜백 오류 무결성 검증
            if (apply == null)
            {
                throw new InvalidOperationException("Effect Apply returned null Action");
            }

            applies.Add(apply);
        }

        // 4. 모인 모든 효과 콜백 다중 전파 실시
        foreach (Action apply in applies)
        {
            apply.Invoke();
        }
    }

    /// <summary>
    /// 이넘 식별자에 따라 효과가 도달할 오브젝트 인스턴스 배열을 반환 구성
    /// </summary>
    private List<EntityInstance> GetTargets(TargetType type, EventContext context)
    {
        List<EntityInstance> targets = new List<EntityInstance>();

        switch (type)
        {
            case TargetType.Player:
                targets.Add(context.Combat.Player);
                break;
            case TargetType.EnemySingle:
                targets.Add(target.Instance);
                break;
            case TargetType.EnemyAll:
                targets.AddRange(context.Combat.Enemies);
                break;
        }

        return targets;
    }
    /// <summary>
    /// 단일 객체 카드 드로우 조작 및 대기열 모션 큐 등록
    /// </summary>
    private void DrawCard(EventContext context, MotionContext motion)
    {
        // 1. 패 숫자 상한(기본 10장) 도달 시 드로우 불가
        if (hand.Count == 10)
        {
            return;
        }

        // 2. 뽑을 더미가 고갈됐을 시 무덤을 덱으로 변환하여 충전
        ChargeCard(context: context, motion: motion);

        // 3. 내부 보유 리스트 주소 변경 조작 
        CardInstance cardInstance = drawPile.FirstOrDefault();
        drawPile.Remove(cardInstance);
        hand.Add(cardInstance);

        // 4. 단일 카드 획득 UI 애니메이션 진행 모션 큐 추가
        motion.AddTask(new MotionTask(
            priority: MotionPriority.Card,
            command: () => DrawCardCor(cardInstance),
            source: this
        ));

        // 5. 처리 종료 통보 이벤트 알림
        eventBus.Publish<CardDrawed>(new CardDrawed(
            context: context.RewriteNew(this),
            motion: motion
        ));
    }

    /// <summary>
    /// 단일 카드를 패에서 제외 후 버린 카드 더미(무덤)로 이관 조작 수행
    /// </summary>
    private void DiscardCard(CardView cardView, EventContext context, MotionContext motion)
    {
        // 1. 연산 리스트 변동 캐싱
        hand.Remove(cardView.CardInstance);
        discardPile.Add(cardView.CardInstance);

        // 2. 단일 카드 파기 UI 애니메이션 대기 큐 진행
        motion.AddTask(new MotionTask(
            priority: MotionPriority.Card,
            command: () => DiscardCardCor(cardView),
            source: this
        ));

        // 3. 대상 카드 파기 완료 통보 알림
        eventBus.Publish<CardDiscarded>(new CardDiscarded(
            context: context.RewriteNew(this),
            motion: motion
        ));
    }

    /// <summary>
    /// 여러 카드를 한 번에 패에서 제외시켜 대량의 버린 카드 더미(무덤)로 단체 이관 루프 조작
    /// </summary>
    private void DiscardCards(IEnumerable<CardView> views, EventContext context, MotionContext motion)
	{
        // 1. 단체 처리 루프 연산 리스트 변동
        foreach (CardView view in views)
		{
			hand.Remove(view.CardInstance);
			discardPile.Add(view.CardInstance);
		}

		// 2. 단체 카드 파기 다중 UI 애니메이션 대기 큐 진행
		motion.AddTask(new MotionTask(
			priority: MotionPriority.Card,
			command: () => DiscardCardsCor(views),
			source: this
		));

		// 3. 다중 대상 카드 통보 알림
		eventBus.Publish<CardDiscarded>(new CardDiscarded(
            context: context.RewriteNew(this),
            motion: motion
        ));
    }

    /// <summary>
    /// 특정 효과 등을 가진 단일 카드를 일시적 영구 추방(소멸 더미) 상태로 이관 조작 지정 수행
    /// </summary>
    private void ExhaustCard(CardView cardView, EventContext context, MotionContext motion)
    {
        // 1. 영구 소멸 풀 데이터 이관
        hand.Remove(cardView.CardInstance);
        exhaustPile.Add(cardView.CardInstance);

        // 2. 영구 연소(소멸) UI 애니메이션 대기 큐 진행 지정
        motion.AddTask(new MotionTask(
            priority: MotionPriority.Card,
            command: () => ExhaustCardCor(cardView),
            source: this
        ));

        // 3. 소멸 완료 이벤트 전파
        eventBus.Publish<CardExhausted>(new CardExhausted(
            context: context.RewriteNew(this),
            motion: motion
        ));
    }

    /// <summary>
    /// 드로우 파일이 고갈되었을 때, 버린 카드 더미를 섞어 덱을 충전 재생구축 수행
    /// </summary>
    private void ChargeCard(EventContext context, MotionContext motion)
    {
        // 잔여 여부 확인
        if (drawPile.Count > 0)
        {
            return;
        }

        // 섞어서 신규 목록 편입
        drawPile.AddRange(Utils.Shuffle(discardPile));
        discardPile.Clear();

        UpdateUI();

        // 덱 리셰플 알림 전달
        eventBus.Publish<CardCharged>(new CardCharged(
            context: context.RewriteNew(this),
            motion: motion
        ));
    }

    /// <summary>
    /// 손패에 가지고 있는 전체 단일 카드 목록들을 역순으로 비우기 처리 조달
    /// </summary>
    private void ClearCardHand(EventContext context, MotionContext motion)
    {
		DiscardCards(
			views: cardHand.Cards.Reverse(),
			context: context,
			motion: motion
		);
	}
    /// <summary>
    /// 카드 드로우 UI 애니메이션 동작 코루틴
    /// </summary>
    IEnumerator DrawCardCor(CardInstance cardInstance)
    {
        CardView cardView = cardViewPool.Pop(false);
        cardView.Init(
            cardInstance: cardInstance,
            cardSystem: this,
            cardContainer: cardHand
        );

        cardView.Draw();
        yield return new WaitForSeconds(0.1f);
        UpdateUI();
    }

    /// <summary>
    /// 카드 단일 버림 UI 애니메이션 동작 코루틴
    /// </summary>
    IEnumerator DiscardCardCor(CardView cardView)
    {
        cardHand.DestroyCard(cardView);

        yield return cardView.Discard().WaitForCompletion();

		cardViewPool.Push(cardView);
		UpdateUI();
    }

    /// <summary>
    /// 카드 영구 소멸 UI 애니메이션 동작 코루틴
    /// </summary>
    IEnumerator ExhaustCardCor(CardView cardView)
    {
        cardHand.DestroyCard(cardView);

        yield return cardView.Exhaust().WaitForCompletion();

        cardViewPool.Push(cardView);
        UpdateUI();
    }

    /// <summary>
    /// 카드 다중 일괄 버림 간격 순차 UI 애니메이션 동작 코루틴
    /// </summary>
    IEnumerator DiscardCardsCor(IEnumerable<CardView> views)
    {
        List<CardView> copies = new List<CardView>(views);

        for (int i = 0; i < copies.Count; i++)
        {
            CardView view = copies.ElementAt(i);
			cardHand.DestroyCard(view);

            if (i < copies.Count - 1)
            {
                view.Discard();
                yield return new WaitForSeconds(0.05f);
			}
            else
            {
                yield return view.Discard().WaitForCompletion();
			}

			cardViewPool.Push(view);
		    UpdateUI();
		}
	}

    /// <summary>
    /// 적용 수명이 끝난 특정 스코프 조건 카드 비용 조작(할인/가중 등) 효과의 일괄 초기화
    /// </summary>
    private void RemoveModificationsByScope(CostModificationScope scope)
    {
        foreach (CardInstance c in cardInstanceAll)
        {
            c.RemoveModifications(scope);
        }

        foreach (CardView view in cardViewPool.Actives)
        {
            view.ModifiyCost();
        }
    }

    /// <summary>
    /// 좌우 하단에 표기되는 덱, 무덤, 소멸 매수 텍스트 UI 수치 강제 동기화
    /// </summary>
    public void UpdateUI()
    {
        drawPileText.text = drawPile.Count.ToString();
        discardPileText.text = discardPile.Count.ToString();
        exhaustPileText.text = exhaustPile.Count.ToString();
    }

    /// <summary>
    /// 세이브 파일 보존용 데이터에 신규 카드 획득 내역 추가
    /// </summary>
    public void AddCard(CardSO card)
    {
        RunManager.Instance.CurrentData.AddCard(card);
    }
}

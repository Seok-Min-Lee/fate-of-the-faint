using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 에너지(코스트) 총량 관리 및 점진/소모 흐름 제어 시스템
/// </summary>
public class EnergySystem : BaseSystem
{
    private readonly EventBus eventBus;
    public int MaxEnergy; // 최대 에너지 보유량
    public int Energy;    // 현재 에너지 보유량
    
    public EnergySystem(EventBus eventBus)
    {
        this.eventBus = eventBus;

        // 게임 시작 시 초기화된 유닛 데이터 기반 최대 마나 설정
        MaxEnergy = RunManager.Instance.CurrentData.Energy;
        Energy = MaxEnergy;
    }

    /// <summary>
    /// 단위 턴 시작 시 코스트 회복 처리
    /// </summary>
    public void OnPlayerTurnStarted(PlayerTurnStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 1. 현재 결손된 에너지 수치 파악용 컨텍스트 구성
        EnergyContext energyContext = new EnergyContext(amount: MaxEnergy - Energy, source: this);

        // 2. 유물/버프 등에 의한 에너지 추가 회복/증감 보정 요청
        eventBus.Publish<EnergyChargeRequested>(new EnergyChargeRequested(
            context: e.Context.RewriteNew(this),
            motion: e.Motion,
            energy: energyContext
        ));

        // 3. 누적 보정치 연산을 거친 최종 회복량 산출
        int sum = energyContext.Calculate();

        // 4. 산출된 회복량만큼 실제 시스템 데이터 갱신
        EnergyChanged(
            amount: sum,
            context: e.Context,
            motion: e.Motion
        );
    }

    /// <summary>
    /// 단위 턴 종료 시 코스트 초기화
    /// </summary>
    public void OnPlayerTurnEnded(PlayerTurnEnded e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 1. 남은 에너지 전체 소멸 및 0으로 강제 고정
        Energy = 0;
    }

    /// <summary>
    /// 카드 사용 등에 의한 에너지 소모 요청 처리
    /// </summary>
    public void OnEnergyChangeRequested(EnergyChangeRequested e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 1. 소모량이 양수이거나 현재 남은 마나보다 요구치가 많은지 검증 (불가 시 리턴)
        if (e.Amount > 0 || Mathf.Abs(e.Amount) > Energy)
        {
            return;
        }

        // 2. 에너지 소모 판정 승인
        e.Request.isResult = true;

        // 3. 실제 에너지 차감 및 변동 알림 이벤트 발행
        EnergyChanged(
            amount: e.Amount,
            context: e.Context,
            motion: e.Motion
        );

        // 4. 소모 및 결제 프로세스 확정 알림 발행
        eventBus.Publish(new EnergyResolved(
            context: e.Context.RewriteNew(this), 
            motion: e.Motion
        ));
    }

    /// <summary>
    /// 강제 추가 에너지 획득 지정 처리
    /// </summary>
    public void OnGainEnergyDeclared(GainEnergyDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 1. 선언된 추가 획득량만큼 현재 에너지 증감
        EnergyChanged(
            amount: e.Amount,
            context: e.Context,
            motion: e.Motion
        );
    }

    /// <summary>
    /// 내부 에너지 수치 갱신 동기화 프로세스
    /// </summary>
    private void EnergyChanged(int amount, EventContext context, MotionContext motion)
    {
        // 1. 반영 전 수치 캐싱
        int startAmount = Energy;
        
        // 2. 요청액만큼 합산 갱신
        Energy += amount;

        // 3. UI 및 연출을 위한 수치 변동 알림 이벤트 전파
        eventBus.Publish<EnergyChanged>(new EnergyChanged(
            context: context.RewriteNew(this),
            motion: motion,
            startAmount: startAmount, 
            endAmount: Energy,
            maxAmount: MaxEnergy
        ));
    }
}

/// <summary>
/// 개별 에너지 연산 데이터 컨텍스트 (보정치 누적 판독용)
/// </summary>
public class EnergyContext
{
    public EnergyContext(int amount, object source)
    {
        Amount = amount;
        Source = source;
        Modifications = new List<EnergyModification>();
    }
    public object Source { get; private set; } // 발생 주체
    private int Amount;                        // 기본 변동 수치
    
    private List<EnergyModification> Modifications; // 에너지 보정치 목록

    /// <summary>
    /// 합연산 보정치 추가
    /// </summary>
    public void Add(int value, object source)
    {
        Modifications.Add(new EnergyModification(
            type: EnergyModificationType.Add,
            value: value,
            source: source
        ));
    }

    /// <summary>
    /// 차감연산 보정치 추가
    /// </summary>
    public void Subtract(int value, object source)
    {
        Modifications.Add(new EnergyModification(
            type: EnergyModificationType.Subtract,
            value: value,
            source: source
        ));
    }

    /// <summary>
    /// 곱연산 보정치 추가
    /// </summary>
    public void Multiply(float value, object source)
    {
        Modifications.Add(new EnergyModification(
            type: EnergyModificationType.Multiply,
            value: value,
            source: source
        ));
    }

    /// <summary>
    /// 누적된 보정치를 반영한 최종 가감 수치 산출
    /// </summary>
    public int Calculate()
    {
        int sum = Amount;

        // 보정 연산 우선순위에 따라 오름차순 정렬 후 적용 처리
        List<EnergyModification> ordered = Modifications.OrderBy(m => m.Type).ToList();
        foreach (EnergyModification dm in ordered)
        {
            switch (dm.Type)
            {
                case EnergyModificationType.Add:
                    sum += (int)dm.Value;
                    break;
                case EnergyModificationType.Subtract:
                    sum -= (int)dm.Value;
                    break;
                case EnergyModificationType.Multiply:
                    sum = (int)(sum * dm.Value);
                    break;
            }
        }

        return sum;
    }
}

/// <summary>
/// 개별 에너지 보정치 수치 정보
/// </summary>
public struct EnergyModification
{
    public EnergyModification(EnergyModificationType type, float value, object source)
    {
        Type = type;
        Value = value;
        Source = source;
    }
    public EnergyModificationType Type; // 연산 타입
    public float Value;                 // 보정 수치
    public object Source;               // 원인 주체
}

/// <summary>
/// 에너지 보정 연산 우선순위 지정 (낮은 숫자일수록 우선 계산)
/// </summary>
public enum EnergyModificationType
{
    Add = 1,      // 더하기
    Subtract = 3, // 빼기
    Multiply = 2, // 곱하기
}

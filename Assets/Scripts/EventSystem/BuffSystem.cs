using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 버프/디버프 부여 연산 및 흐름 관리 시스템
/// </summary>
public class BuffSystem : BaseSystem
{
    private readonly EventBus eventBus;
    public BuffSystem(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    /// <summary>
    /// 버프 선언 시 보정치 계산 및 부여 확정 이벤트 발행
    /// </summary>
    public void OnBuffDeclared(BuffDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 버프 계산용 컨텍스트 초기화
        BuffContext buff = new BuffContext(
            type: e.Type,
            amount: e.Amount,
            source: e.Source,
            target: e.Target
        );

        // 버프 수치 보정 요청 이벤트 발행 (유물, 파워 등 개입)
        eventBus.Publish<BuffRequested>(new BuffRequested(
            context: e.Context.RewriteNew(this),
            buff: buff
        ));

        // 누적 보정치 연산 적용 및 부여할 최종 수치 이벤트에 포함해 발행
        eventBus.Publish<BuffResolved>(new BuffResolved(
            context: e.Context.RewriteNew(this),
            motion: e.Motion,
            source: buff.Source,
            target: buff.Target,
            type: buff.Type,
            amount: buff.Calculate()
        ));
    }
}

/// <summary>
/// 개별 버프 연산 데이터 컨텍스트 (보정치 누적)
/// </summary>
public class BuffContext
{
    public BuffContext(BuffType type, int amount, object source, object target)
    {
        Type = type;
        Amount = amount;
        Source = source;
        Target = target;
        Modifications = new List<BuffModification>();
    }
    public BuffType Type { get; private set; } // 버프 종류
    public int Amount { get; private set; }    // 기본 수치
    public object Source { get; private set; } // 발생 주체
    public object Target { get; private set; } // 부여 대상
    
    private List<BuffModification> Modifications; // 보정치 목록

    /// <summary>
    /// 합연산 보정치 추가
    /// </summary>
    public void Add(int value, object source)
    {
        Modifications.Add(new BuffModification(
            type: BuffModificationType.Add,
            value: value,
            source: source
        ));
    }

    /// <summary>
    /// 차감연산 보정치 추가
    /// </summary>
    public void Subtract(int value, object source)
    {
        Modifications.Add(new BuffModification(
            type: BuffModificationType.Subtract,
            value: value,
            source: source
        ));
    }

    /// <summary>
    /// 곱연산 보정치 추가
    /// </summary>
    public void Multiply(float value, object source)
    {
        Modifications.Add(new BuffModification(
            type: BuffModificationType.Multiply,
            value: value,
            source: source
        ));
    }

    /// <summary>
    /// 누적된 보정치를 반영한 최종 수치 산출
    /// </summary>
    public int Calculate()
    {
        int sum = Amount;

        // 연산 우선순위에 따라 오름차순 정렬 후 적용
        List<BuffModification> ordered = Modifications.OrderBy(m => m.Type).ToList();
        foreach (BuffModification dm in ordered)
        {
            switch (dm.Type)
            {
                case BuffModificationType.Add:
                    sum += (int)dm.Value;
                    break;
                case BuffModificationType.Subtract:
                    sum -= (int)dm.Value;
                    break;
                case BuffModificationType.Multiply:
                    sum = (int)(sum * dm.Value);
                    break;
            }
        }

        return sum;
    }
}

/// <summary>
/// 개별 버프 수치 보정 정보
/// </summary>
public struct BuffModification
{
    public BuffModification(BuffModificationType type, float value, object source)
    {
        Type = type;
        Value = value;
        Source = source;
    }
    public BuffModificationType Type; // 연산 타입
    public float Value;               // 보정 수치
    public object Source;             // 원인 주체
}

/// <summary>
/// 버프 보정 연산 우선순위 지정 (낮은 숫자일수록 우선 계산)
/// </summary>
public enum BuffModificationType
{
    Add = 1,      // 더하기
    Subtract = 3, // 빼기
    Multiply = 2, // 곱하기
}
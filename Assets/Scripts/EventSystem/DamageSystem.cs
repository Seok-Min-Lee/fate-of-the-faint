using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 데미지 연산 및 적용 흐름 관리 시스템
/// </summary>
public class DamageSystem : BaseSystem
{
    private readonly EventBus eventBus;
    public DamageSystem(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    /// <summary>
    /// 공격 선언 시 데미지 계산 및 결과 적용 이벤트 발행
    /// </summary>
    public void OnAttackDeclared(AttackDeclared e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        // 데미지 계산용 컨텍스트 생성
        DamageContext damage = new DamageContext(
            amount: e.Amount,
            source: e.Source,
            target: e.Target
        );

        // 데미지 보정 요청 이벤트 발행 (유물, 버프 등 개입)
        eventBus.Publish<DamageRequested>(new DamageRequested(
            context: e.Context.RewriteNew(this),
            motion: e.Motion,
            damage: damage
        ));

        // 최종 데미지 합계 도출
        int sum = damage.Calculate();

        // 최종 데미지 적용 및 결과 확정 이벤트 발행
        eventBus.Publish<DamageResolved>(new DamageResolved(
            context: e.Context.RewriteNew(this),
            motion: e.Motion,
            source: damage.Source,
            target: damage.Target,
            amount: Mathf.Max(0, sum), // 데미지가 음수가 되지 않도록 보정
            repeat: e.Repeat
        ));
    }
}

/// <summary>
/// 개별 데미지 연산 데이터 컨텍스트 (보정치 누적)
/// </summary>
public class DamageContext
{
    public DamageContext(int amount, object source, object target)
    {
        Amount = amount;
        Source = source;
        Target = target;
        Modifications = new List<DamageModification>();
    }
    private int Amount;
    public object Source { get; private set; }
    public object Target { get; private set; }

    private List<DamageModification> Modifications; // 데미지 보정치 목록

    /// <summary>
    /// 합연산 보정치 추가
    /// </summary>
    public void Add(int value, object source)
    {
        Modifications.Add(new DamageModification(
            type: DamageModificationType.Add, 
            value: value, 
            source: source
        ));
    }

    /// <summary>
    /// 차감연산 보정치 추가
    /// </summary>
    public void Subtract(int value, object source)
    {
        Modifications.Add(new DamageModification(
            type: DamageModificationType.Subtract,
            value: value,
            source: source
        ));
    }

    /// <summary>
    /// 곱연산 보정치 추가
    /// </summary>
    public void Multiply(float value, object source)
    {
        Modifications.Add(new DamageModification(
            type: DamageModificationType.Multiply,
            value: value,
            source: source
        ));
    }

    /// <summary>
    /// 누적된 보정치를 반영한 최종 데미지 산출
    /// </summary>
    public int Calculate()
    {
        int sum = Amount;

        // 연산 우선순위에 따라 오름차순 정렬 후 적용
        List<DamageModification> ordered = Modifications.OrderBy(m => m.Type).ToList();
        foreach (DamageModification dm in ordered)
        {
            switch (dm.Type)
            {
                case DamageModificationType.Add:
                    sum += (int)dm.Value;
                    break;
                case DamageModificationType.Subtract:
                    sum -= (int)dm.Value;
                    break;
                case DamageModificationType.Multiply:
                    sum = (int)(sum * dm.Value);
                    break;
            }
        }

        return sum;
    }
}

/// <summary>
/// 개별 데미지 보정치 정보 (타입, 수치 등)
/// </summary>
public struct DamageModification
{
    public DamageModification(DamageModificationType type, float value, object source)
    {
        Type = type;
        Value = value;
        Source = source;
    }
    public DamageModificationType Type; // 연산 타입
    public float Value;                 // 보정 수치
    public object Source;               // 원인 주체
}

/// <summary>
/// 데미지 연산 순방향 지정 (낮은 숫자일수록 우선 계산)
/// </summary>
public enum DamageModificationType
{
    Add = 1,      // 더하기
    Subtract = 3, // 빼기 
    Multiply = 2, // 곱하기 (우선순위 2)
}
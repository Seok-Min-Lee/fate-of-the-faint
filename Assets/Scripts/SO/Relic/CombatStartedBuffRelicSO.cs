using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Combat Started Buff Relic ", menuName = "Scriptable Objects/Relic/Combat Started Buff Relic")]
public class CombatStartedBuffRelicSO: RelicSO
{
    [SerializeField] private RelicTarget target;
    [SerializeField] private BuffType buff;
    [SerializeField] private int value;
    public RelicTarget Target => target;
    public BuffType Buff => buff;
    public int Value => value;
    public override RelicInstance CreateInstance()
    {
        return new CombatStartedBuffRelicInstance(this);
    }
}

public class CombatStartedBuffRelicInstance : RelicInstance, ICombatStarted
{
    private RelicTarget target;
    private BuffType buff;
    private int value;
    public CombatStartedBuffRelicInstance(CombatStartedBuffRelicSO origin) : base(origin)
    {
        target = origin.Target;
        buff = origin.Buff;
        value = origin.Value;
    }
    public override void Register(EventBus eventBus)
    {
        EventBus = eventBus;
        EventBus.Subscribe<CombatStarted>(OnCombatStarted);
    }
    public void OnCombatStarted(CombatStarted e)
    {
        if (e.Context.Combat.state != CombatState.Combat)
        {
            return;
        }

        List<EntityInstance> targets = new List<EntityInstance>();
        switch (target)
        {
            case RelicTarget.Player:
                targets.Add(e.Context.Combat.Player);
                break;
            case RelicTarget.EnemyAll:
                targets.AddRange(e.Context.Combat.Enemies);
                break;
            case RelicTarget.EnemyRandom:
                targets.Add(e.Context.Combat.Enemies[Random.Range(0, e.Context.Combat.Enemies.Count)]);
                break;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].ApplyBuff(
                eventBus: EventBus,
                context: e.Context,
                motion: e.Motion,
                type: buff, 
                delta: value
            );
        }
    }
}
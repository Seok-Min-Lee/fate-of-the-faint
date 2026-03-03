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
    public override RelicInstance CreateInstance(EventBus eventBus)
    {
        return new CombatStartedBuffRelicInstance(eventBus: eventBus, origin: this);
    }
}

public class CombatStartedBuffRelicInstance : RelicInstance, ICombatStarted
{
    private RelicTarget target;
    private BuffType buff;
    private int value;
    public CombatStartedBuffRelicInstance(EventBus eventBus, CombatStartedBuffRelicSO origin) : base(eventBus, origin)
    {
        target = origin.Target;
        buff = origin.Buff;
        value = origin.Value;
    }
    public override void Register()
    {
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
            int startAmount = targets[i].Getbuff(buff);
            targets[i].ApplyBuff(buff, value);

            EventBus.Publish<BuffChanged>(new BuffChanged(
                context: e.Context.RewriteNew(this),
                motion: e.Motion,
                target: targets[i],
                type: buff,
                startAmount: startAmount,
                endAmount: targets[i].Getbuff(buff)
            ));
        }
    }
}
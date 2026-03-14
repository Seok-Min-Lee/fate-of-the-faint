using System;
using System.Collections.Generic;

public class PlayerInstance : EntityInstance
{
    public PlayerInstance(PlayerSO data, int maxHp, int currentHp)
    {
        Id = Guid.NewGuid();

        Data = data;
        MaxHp = maxHp;
        CurrentHp = currentHp;

        Block = 0;
        buffs = new Dictionary<BuffType, int>();
    }
    public PlayerSO Data { get; private set; }
}

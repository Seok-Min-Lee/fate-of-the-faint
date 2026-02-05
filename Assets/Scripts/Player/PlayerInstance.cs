using System.Collections.Generic;

public sealed class PlayerInstance
{
    public PlayerInstance(PlayerSO data, int maxHp, int currentHp)
    {
        Data = data;
        MaxHp = maxHp;
        CurrentHp = currentHp;

        Block = 0;
        Energy = data.BaseEnergy;
        buffs = new Dictionary<BuffType, int>();
    }
    public PlayerSO Data { get; private set; }
    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }
    public int Block { get; private set; }
    public int Energy { get; private set; }

    private readonly Dictionary<BuffType, int> buffs;
}

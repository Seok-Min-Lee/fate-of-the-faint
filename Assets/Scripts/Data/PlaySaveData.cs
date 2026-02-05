using System;
using System.Collections.Generic;

[Serializable]
public class PlaySaveData
{
    public string playerId;

    public int currentHp;
    public int maxHp;
    public int gold;

    public RunRngStateSaveData rng;
    public List<int> nodes;

    public List<string> cardIds;
    public List<string> relicIds;
    public List<string> potionIds;
}

[Serializable]
public struct RunRngStateSaveData
{
    public int seed;
    public int calls;
}

using System;
using System.Collections.Generic;

[Serializable]
public class RunSaveData
{
    public string playerId;

    public int currentHp;
    public int maxHp;
    public int gold;
    public int energy;
    public int rewardCardOptionCount;

    public RunRngStateSaveData rng;
    public List<int> nodes;

    public List<string> cardIds;
    public List<string> relicIds;
    public List<string> potionIds;

    public List<PlayRecord> records;
}

[Serializable]
public struct RunRngStateSaveData
{
    public int seed;
    public int calls;
}

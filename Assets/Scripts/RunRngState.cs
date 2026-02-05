using System;

public sealed class RunRngState
{
    public int Seed { get; private set; }
    public int Calls { get; private set; }

    private readonly Random rng;

    public RunRngState(int seed) : this(seed, 0)
    {
    }

    public RunRngState(int seed, int calls)
    {
        Seed = seed;
        Calls = Math.Max(0, calls);

        rng = new Random(Seed);

        int i = 0;
        while (i < Calls)
        {
            rng.Next();
            i++;
        }
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        Calls++;
        return rng.Next(minInclusive, maxExclusive);
    }

    public float NextFloat01()
    {
        Calls++;
        return (float)rng.NextDouble();
    }
}

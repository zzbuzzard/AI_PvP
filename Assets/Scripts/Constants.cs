using System.Collections;
using System.Collections.Generic;

public enum GameType
{
    SHOOT,
    TANK,
    SWORD
}

public static class Constants
{
    // Change this line to change gamemode
    // You may also need to modify Neat.GetMatches(), to return the correct number of players for this gamemode
    public const GameType GAME_TYPE = GameType.SWORD;

    public static readonly int[] NumInputs  = { ShootGame.numInputs,  TankGame.numInputs,  SwordGame.numInputs };
    public static readonly int[] NumOutputs = { ShootGame.numOutputs, TankGame.numOutputs, SwordGame.numOutputs };

    public static readonly int numInputs  = NumInputs[(int)GAME_TYPE];
    public static readonly int numOutputs = NumOutputs[(int)GAME_TYPE];

    public static int seed = 0;

    private static System.Random r = new System.Random(666);

    // (empty)
    public static GenericPlayer swordGameEnemy = new NeatPlayer(new NeatNet(new Genome(new List<ConnectionGene>())));

    static Constants()
    {

    }

    public static Game GameConstructor(GenericPlayer[] s)
    {
        switch (GAME_TYPE)
        {
            case GameType.SHOOT:
                return new ShootGame(s);
            case GameType.SWORD:
                return new SwordGame(s[0], swordGameEnemy, seed);
            case GameType.TANK:
                return new TankGame(s[0], seed);
        }
    }

    public static float Rand(float a, float b)
    {
        return ((float)r.NextDouble()) * (b - a) + a;
    }
    // inclusive a, exclusive b
    public static int RandInt(int a, int b)
    {
        return r.Next(a, b);
    }

    public const float weightChangeChance = 0.8f;
    public const float weightChangeRange = 0.2f;

    public const float disjointCoeff = 1.0f;
    public const float excessCoeff = 1.0f;
    public const float weightCoeff = 2.0f;

    public const float pruneChance = 0.25f;

    public const int goalNumSpecies = 8;
    public const float similarityVariation = 0.75f;
    public const float similarityThreshold = 2.5f; // Was 1.5

    public const float breedSpeciesPercent = 0.25f;
}

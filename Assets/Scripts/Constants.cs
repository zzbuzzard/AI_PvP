using System.Collections;
using System.Collections.Generic;

public static class Constants
{
    static Constants()
    {

    }

    // To change the game type
    //  1) Change the 6 lines of code below
    //  2) Change HumanPlayer.hs to do input for this game
    public static Game GameConstructor(GenericPlayer[] s)
    {
        // return new ShootGame(s);
        return new TankGame(s[0], 3);
    }
    public const int numInputs = TankGame.numInputs;
    public const int numOutputs = TankGame.numOutputs;

    private static System.Random r = new System.Random(666);
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
    public const float similarityVariation = 0.5f;
    public const float similarityThreshold = 1.5f; // Was 0.5

    public const float breedSpeciesPercent = 0.25f;
}

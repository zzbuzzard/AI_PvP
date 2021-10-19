using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public const float weightChangeChance = 0.8f;
    public const float weightChangeRange = 0.2f;

    public const float disjointCoeff = 1.0f;
    public const float excessCoeff = 1.0f;
    public const float weightCoeff = 2.0f;

    public const float pruneChance = 0.25f;

    public const int goalNumSpecies = 8;
    public const float similarityVariation = 0.5f;
    public const float similarityThreshold = 3.0f; // Was 0.5

    public const float breedSpeciesPercent = 0.25f;

    public const int numInputs = InputOutput.numInputs;
    public const int numOutputs = InputOutput.numOutputs;
}

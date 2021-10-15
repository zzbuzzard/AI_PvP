using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public const float weightChangeChance = 0.1f;
    public const float weightChangeRange = 0.5f;

    public const float disjointCoeff = 0.5f;
    public const float excessCoeff = 0.3f;
    public const float weightCoeff = 0.2f;

    public const float similarityThreshold = 0.35f; // Was 0.5
    public const float breedSpeciesPercent = 0.25f;

    public const int numInputs = InputOutput.numInputs;
    public const int numOutputs = InputOutput.numOutputs;
}

using System;
using System.Collections;
using System.Collections.Generic;
using static Constants;

// An abstract class for AI players
public class AIPlayer : GenericPlayer
{
    protected NeuralNet mnet;
    float[] prevInput = new float[numOutputs];

    public static AIPlayer MakeLayeredAIPlayer()
    {
        int[] levels = new int[] { numInputs, 15, numOutputs };
        int[] linear = new int[] { 0, 4, 0 };

        return new AIPlayer(new LayeredNeuralNet(levels, linear));
    }

    public AIPlayer(NeuralNet brain)
    {
        mnet = brain;
    }

    public override float[] GetOutput(Game g, float[] input)
    {
        return mnet.Evaluate(input);
    }

    public AIPlayer BreedPlayer(AIPlayer otherParent)
    {
        return new AIPlayer(mnet.Breed(otherParent.mnet));
    }
}

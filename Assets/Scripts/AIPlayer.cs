using System;
using System.Collections;
using System.Collections.Generic;
using static Constants;

// An abstract class for AI players
public class AIPlayer : GenericPlayer
{
    // Input every 4 frames (that is, 7 times a sec)
    const int whichFrameInput = 4;

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
        if (g.framesPassed % whichFrameInput != 0)
            return prevInput;

        prevInput = mnet.Evaluate(input);
        return prevInput;
    }

    public AIPlayer BreedPlayer(AIPlayer otherParent)
    {
        return new AIPlayer(mnet.Breed(otherParent.mnet));
    }
}

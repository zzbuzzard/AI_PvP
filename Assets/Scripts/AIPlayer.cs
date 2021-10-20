using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InputOutput;

// An abstract class for AI players
public class AIPlayer : GenericPlayer
{
    // Input every 4 frames (that is, 7 times a sec)
    const int whichFrameInput = 4;

    protected NeuralNet mnet;
    GameInput prevInput = new GameInput(0, false, false, 0.0f);

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

    public override GameInput GetInput(ShooterGame game)
    {
        if (game.framesPassed % whichFrameInput != 0)
            return prevInput;

        float[] inputArr = InputOutput.GetInput(this, game);
        float[] outputArr = mnet.Evaluate(inputArr);

        GameInput g = InputOutput.GetOutput(outputArr);
        prevInput = g;
        return g;
    }


    public AIPlayer BreedPlayer(AIPlayer otherParent)
    {
        return new AIPlayer(mnet.Breed(otherParent.mnet));
    }
}

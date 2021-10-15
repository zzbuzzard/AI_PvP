using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NeuralNet
{
    public static float ActivationFunction(float f)
    {
        // return (f > 0.0f ? f : f / 8);  // Leaky relu
        return (float)Math.Tanh(f);
    }

    public abstract float[] Evaluate(float[] input);

    public abstract NeuralNet Breed(NeuralNet net);
}

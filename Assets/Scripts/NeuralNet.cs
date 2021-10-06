using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NeuralNet
{
    // Tanh
    public static float ActivationFunction(float f)
    {
        return (float)Math.Tanh(f);
    }

    public abstract float[] Evaluate(float[] input);

    public abstract NeuralNet Breed(NeuralNet net);
}

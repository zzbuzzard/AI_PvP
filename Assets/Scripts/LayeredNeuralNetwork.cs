using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayeredNeuralNetwork
{
    int[] levels;
    int N;

    int[] linearLevels; // linearLevels[i] = number of nodes which are NOT activated in this layer

    // weights has length N-1
    // weights[i] has dimension [levels[i], levels[i+1]]
    public float[][,] weights;

    // nodes has length N
    // nodes[i] has length levels[i] : gives node amounts
    public float[][] nodes;

    public float[][] bias;

    public static float ActivationFunction(float f)
    {
        return (float)Math.Tanh(f);
    }

    public LayeredNeuralNetwork(int[] levels, int[] linearLevels)
    {
        this.levels = levels;
        N = levels.Length;

        this.linearLevels = linearLevels;

        nodes = new float[N][];
        for (int i = 0; i < N; i++)
        {
            nodes[i] = new float[levels[i]];
        }

        bias = new float[N][];
        for (int i = 0; i < N; i++)
        {
            bias[i] = new float[levels[i]];
        }

        weights = new float[N - 1][,];
        for (int i = 0; i < N - 1; i++)
        {
            weights[i] = new float[levels[i], levels[i + 1]];
        }
    }

    public LayeredNeuralNetwork(int[] levels) : this(levels, Util.Repeat(0, levels.Length))
    {
    }

    // Assumes nodes[0] is set to the inputs
    // Output is in nodes[-1]
    public void Calculate()
    {
        for (int i=0; i<N-1; i++)
        {
            // Zero nodes[i+1]
            for (int j = 0; j < levels[i + 1]; j++)
            {
                nodes[i + 1][j] = bias[i+1][j];
            }

            for (int j=0; j<levels[i]; j++)
            {
                for (int k=0; k<levels[i+1]; k++)
                {
                    nodes[i + 1][k] += nodes[i][j] * weights[i][j, k];
                }
            }

            if (i != N-2)
            // Apply activation function (but not for last level)
            // Reserve linearLevels; we don't apply ActivationFunction to these
            for (int j = 0; j < levels[i + 1] - linearLevels[i + 1]; j++)
                    nodes[i + 1][j] = ActivationFunction(nodes[i + 1][j]);
        }
    }

    public float[] GetOutput(float[] input)
    {
        if (input.Length == levels[0])
        {
            nodes[0] = input;
            Calculate();
            return nodes[nodes.Length - 1];
        }
        return null;
    }
}
 
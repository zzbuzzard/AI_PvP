using System;
using System.Collections;
using System.Collections.Generic;

public class LayeredNeuralNet : NeuralNet
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

    public LayeredNeuralNet(int[] levels, int[] linearLevels)
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

    public LayeredNeuralNet(int[] levels) : this(levels, Util.Repeat(0, levels.Length))
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

    public override float[] Evaluate(float[] input)
    {
        if (input.Length == levels[0])
        {
            nodes[0] = input;
            Calculate();
            return nodes[nodes.Length - 1];
        }
        return null;
    }


    const float mutateChance = 0.1f;
    const float mutateAmount = 2.0f;

    public override NeuralNet Breed(NeuralNet g_net)
    {
        // Unsafe cast; if bred with another neuralnet, it will crash
        LayeredNeuralNet net = (LayeredNeuralNet)g_net;

        LayeredNeuralNet p = new LayeredNeuralNet(levels, linearLevels);

        float a;
        for (int i = 0; i < p.weights.Length; i++)
        {
            for (int j = 0; j < levels[i]; j++)
            {
                for (int k = 0; k < levels[i + 1]; k++)
                {
                    a = UnityEngine.Random.Range(0.0f, 1.0f);
                    p.weights[i][j, k] = weights[i][j, k] * a + net.weights[i][j, k] * (1 - a);

                    if (UnityEngine.Random.Range(0.0f, 1.0f) < mutateChance)
                        p.weights[i][j, k] += UnityEngine.Random.Range(-mutateAmount, mutateAmount);
                }
            }
        }

        for (int i = 0; i < p.bias.Length; i++)
        {
            for (int j = 0; j < levels[i]; j++)
            {
                a = UnityEngine.Random.Range(0.0f, 1.0f);
                p.bias[i][j] = bias[i][j] * a + net.bias[i][j] * (1 - a);

                if (UnityEngine.Random.Range(0.0f, 1.0f) < mutateChance)
                    p.bias[i][j] += UnityEngine.Random.Range(-mutateAmount, mutateAmount);
            }
        }

        return p;
    }
}
 
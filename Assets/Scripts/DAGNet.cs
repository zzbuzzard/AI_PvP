using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for a non-layered neural network
public class DAGNet
{
    int N;    // Number of nodes
    List<int>[] edges;
    List<float>[] edgeWeights;

    List<float> nodeActivation;
    float[] outputArray;

    List<int> topoSort;

    int numInputs, numOutputs;

    public DAGNet(int numInputs, int numOutputs, List<ConnectionGene> genes)
    {
        this.numInputs = numInputs;
        this.numOutputs = numOutputs;
        outputArray = new float[numOutputs];

        N = numInputs + numOutputs;
        foreach (ConnectionGene c in genes) {
            if (c.disabled) continue;

            if (c.fromNode + 1 > N) N = c.fromNode + 1;
            if (c.toNode + 1 > N) N = c.toNode + 1;
        }

        edges = new List<int>[N];
        edgeWeights = new List<float>[N];

        for (int i=0; i<N; i++)
        {
            edges[i] = new List<int>();
            edgeWeights[i] = new List<float>();
        }

        foreach (ConnectionGene c in genes)
        {
            if (c.disabled) continue;

            int a = c.fromNode;
            int b = c.toNode;
            float w = c.weight;

            // a ---w---> b
            edges[a].Add(b);
            edgeWeights[a].Add(w);
        }

        // Generate topological sort
        bool[] vis = new bool[N];
        // Start from each input
        for (int i=0; i<numInputs; i++)
        {
            TopoSort(i, ref vis);
        }
        topoSort.Reverse();

        // TODO: Remove check
        if (topoSort.Count > N)
        {
            Debug.LogError("Topological sort is too large!");
        }
    }

    private void TopoSort(int n, ref bool[] visited)
    {
        if (visited[n]) return;
        visited[n] = true;

        foreach (int m in edges[n])
        {
            TopoSort(m, ref visited);
        }

        topoSort.Add(n);
    }

    // Given an input array, evaluate the neural network to produce an output array
    public float[] Evaluate(float[] inputs)
    {
        if (inputs.Length != numInputs)
        {
            Debug.LogError("Input array provided had size " + inputs.Length + ", expected " + numInputs);
            return null;
        }

        // Copy inputs to nodeActivation
        for (int i = 0; i < numInputs; i++) nodeActivation[i] = inputs[i];

        // Set everything else to 0
        for (int i = numInputs + 1; i < N; i++) nodeActivation[i] = 0;

        // Use topological sort
        foreach (int n in topoSort)
        {
            // Do not apply activation function to inputs or outputs
            if (n >= numInputs + numOutputs)
                nodeActivation[n] = NeuralNet.ActivationFunction(nodeActivation[n]);

            for (int j = 0; j < edges[n].Count; j++)
            {
                int m = edges[n][j];
                float w = edgeWeights[n][j];

                nodeActivation[m] += w * nodeActivation[n];
            }
        }

        // Set + return the output values
        for (int i = 0; i < numOutputs; i++)
            outputArray[i] = nodeActivation[i + numInputs];

        return outputArray;
    }
}





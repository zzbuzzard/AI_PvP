using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constants;

public class Test
{
    [RuntimeInitializeOnLoadMethod]
    public static void RunTests()
    {
        Debug.Log("Number of inputs: " + numInputs);
        Debug.Log("Running tests");
        DagTest1();
        DagTest2();
        DagTest3();

        GenomeTest1();
        GenomeTest2();
        Debug.Log("Tests completed");
    }

    const int safe = numInputs + numOutputs;

    // Tests a network with one input, one output node
    private static void DagTest1()
    {
        List<ConnectionGene> genes = new List<ConnectionGene>() {
            new ConnectionGene(0, 0, numInputs, false, 10.0f)
        };
        DAGNet d = new DAGNet(genes);

        float[] input = new float[numInputs]; input[0] = 1.0f;
        float[] output = d.Evaluate(input);

        float expected_output = input[0] * 10.0f;

        Debug.Assert(Mathf.Abs(expected_output - output[0]) < 0.001f,
            "Difference was " + Mathf.Abs(expected_output * input[0]) + ", when it should be 0.0f");
    }

    // Tests a network with one input, one central, one output node
    private static void DagTest2()
    {
        List<ConnectionGene> genes = new List<ConnectionGene>() {
            new ConnectionGene(0, 0, safe+5, false, 10.0f),
            new ConnectionGene(0, safe+5, numInputs, false, 5.0f)
        };
        DAGNet d = new DAGNet(genes);

        float[] input = new float[numInputs]; input[0] = 1.0f;
        float[] output = d.Evaluate(input);

        float expected_output = 5.0f * NeuralNet.ActivationFunction(input[0] * 10.0f);

        Debug.Assert(Mathf.Abs(expected_output - output[0]) < 0.001f,
            "Difference was " + Mathf.Abs(expected_output * input[0]) + ", when it should be 0.0f");
    }


    // Tests a network with this shape

    //      O
    //    / ^ \
    //  O   |  O
    //    \ | /
    //      O
    private static void DagTest3()
    {
        List<ConnectionGene> genes = new List<ConnectionGene>() {
            new ConnectionGene(0, 0, safe+5, false, 7.0f),
            new ConnectionGene(0, 0, safe+7, false, 2.0f),
            new ConnectionGene(0, safe+7, safe+5, false, 3.0f),
            new ConnectionGene(0, safe+5, numInputs, false, 1.0f),
            new ConnectionGene(0, safe+7, numInputs, false, 2.0f)
        };
        DAGNet d = new DAGNet(genes);

        float[] input = new float[numInputs]; input[0] = 1.0f;
        float[] output = d.Evaluate(input);

        float expected7 = NeuralNet.ActivationFunction(input[0] * 2.0f);
        float expected5 = NeuralNet.ActivationFunction(input[0] * 7.0f + expected7 * 3.0f);
        float expected_output = expected5 + expected7 * 2.0f;

        Debug.Assert(Mathf.Abs(expected_output - output[0]) < 0.001f,
            "Difference was " + Mathf.Abs(expected_output * input[0]) + ", when it should be 0.0f");
    }


    private static void GenomeTest1()
    {
        Genome a = new Genome(new List<ConnectionGene>() {
            new ConnectionGene(1, 0, 1, false, 5.0f)
        });

        Genome b = new Genome(new List<ConnectionGene>() {
            new ConnectionGene(1, 0, 1, false, 5.0f)
        });

        float f = a.GetSimilarity(b);
        Debug.Assert(Mathf.Abs(f) < 0.001f, "Genome should have 0 similarity to itself");
    }

    private static void GenomeTest2()
    {
        Genome a = new Genome(new List<ConnectionGene>() {
            new ConnectionGene(1, 0, 1, false, 5.0f)
        });

        Genome b = new Genome(new List<ConnectionGene>() {
            new ConnectionGene(1, 0, 1, false, 3.0f)
        });

        float sim = a.GetSimilarity(b);
        float expected_sim = Constants.weightCoeff * 2.0f;

        Debug.Assert(Mathf.Abs(sim - expected_sim) < 0.001f, "Similarity was supposed to be " + expected_sim +
            "but it was " + sim);
    }

}

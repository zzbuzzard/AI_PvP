using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NEATNet : NeuralNet
{
    public readonly Genome genoType;
    public readonly DAGNet phenoType;

    public NEATNet(Genome g) : this(g, g.MakeNet())
    {
    }

    public NEATNet(Genome g, DAGNet p)
    {
        genoType = g;
        phenoType = p;
    }

    // TODO: Breed
    public override NeuralNet Breed(NeuralNet net)
    {
        throw new System.NotImplementedException();
    }

    public override float[] Evaluate(float[] input)
    {
        return phenoType.Evaluate(input);
    }
}

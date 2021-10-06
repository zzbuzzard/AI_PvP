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

    // Breeds two NEAT nets by crossing over their genomes, and returning a new NEAT net from this
    public override NeuralNet Breed(NeuralNet g_net)
    {
        NEATNet net = (NEATNet)g_net;
        Genome breedGenome = Genome.Crossover(genoType, net.genoType);
        return new NEATNet(breedGenome);
    }

    public override float[] Evaluate(float[] input)
    {
        return phenoType.Evaluate(input);
    }
}

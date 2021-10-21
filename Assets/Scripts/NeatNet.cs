using System.Collections;
using System.Collections.Generic;

public class NeatNet : NeuralNet
{
    public readonly Genome genoType;
    public readonly DAGNet phenoType;

    public NeatNet(Genome g) : this(g, g.MakeNet())
    {
    }

    public NeatNet(Genome g, DAGNet p)
    {
        genoType = g;
        phenoType = p;
    }

    // Breeds two NEAT nets by crossing over their genomes, and returning a new NEAT net from this
    public override NeuralNet Breed(NeuralNet g_net)
    {
        NeatNet net = (NeatNet)g_net;
        Genome breedGenome = Genome.Crossover(genoType, net.genoType);
        return new NeatNet(breedGenome);
    }

    public override float[] Evaluate(float[] input)
    {
        return phenoType.Evaluate(input);
    }
}

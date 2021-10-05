using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum NodeType
{
    INPUT, OUTPUT, HIDDEN
}
public struct NodeGene
{
    int ID;
    NodeType type;
}
public struct ConnectionGene
{
    int innovationNumber;
    int fromNode;
    int toNode;
    bool disabled;
    float weight;
}

public class Genome
{
    private int inputs, outputs;
    public readonly List<ConnectionGene> genes;

    public Genome(int inputs, int outputs)
    {
        this.inputs = inputs;
        this.outputs = outputs;
        this.genes = new List<ConnectionGene>();
    }

    public Genome(int inputs, int outputs, List<ConnectionGene> genes)
    {
        this.inputs = inputs;
        this.outputs = outputs;
        this.genes = genes;
    }

    static public Genome Mutate(Genome old)
    {
        var genes = new List<ConnectionGene>();

        return new Genome(1,2);
    }


    public DAGNet MakeNet()
    {
        return new DAGNet();
    }


}

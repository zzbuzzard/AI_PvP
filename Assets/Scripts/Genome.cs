using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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



    public DAGNet MakeNet()
    {
        return new DAGNet();
    }


}

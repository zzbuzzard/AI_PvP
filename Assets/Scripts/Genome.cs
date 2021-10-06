using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 0 ... inputs-1                      are input nodes
// inputs ... inputs + outputs-1       are output nodes
// inputs + outputs                    onwards, are hidden nodes

//public enum NodeType
//{
//    INPUT, OUTPUT, HIDDEN
//}
//public struct NodeGene
//{
//    int ID;
//    NodeType type;
//}
public struct ConnectionGene
{
    public int innovationNumber;
    public int fromNode;
    public int toNode;
    public bool disabled;
    public float weight;

    public ConnectionGene(int innovationNumber, int fromNode, int toNode, bool disabled, float weight)
    {
        this.innovationNumber = innovationNumber;
        this.fromNode = fromNode;
        this.toNode = toNode;
        this.disabled = disabled;
        this.weight = weight;
    }

    public ConnectionGene Disable()
    {
        return new ConnectionGene(innovationNumber, fromNode, toNode, true, weight);
    }
}

public class Genome
{
    private static int globalInnovationNumber = 0;

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

    // Directly modifies 'old'
    // If my understanding is right, we never mutate and keep an old copy
    static public void StructuralMutate(Genome old)
    {
        // Highest ID of any node in this genome
        int maxNode = old.GetMaxNodeNumber();

        // If old.genes.Count == 0, we should always make a new connection (not split)
        if (old.genes.Count == 0 || Random.Range(0.0f, 1.0f) < 0.5f)
        {
            // New connection
            // 1) Generate connection map
            // 2) Pick start (must be non-output)
            // 3) Pick end (must be non-input)
            // 4) Check that resultant graph is acyclic (return to step 2 if not)

            // Generate connection map
            IDictionary<int, List<int>> adjacencyMap = new Dictionary<int, List<int>>();
            foreach (ConnectionGene c in old.genes)
            {
                if (c.disabled) continue;

                if (!adjacencyMap.ContainsKey(c.fromNode))
                    adjacencyMap[c.fromNode] = new List<int>();
                adjacencyMap[c.fromNode].Add(c.toNode);
            }

            // (defaults to all false)
            bool[] visited = new bool[maxNode + 1];


            // Loop until we find a valid pair    (maybe just try a few times then give up idk)
            while (true) // TODO: uhh... this won't always terminate. for example if we have two nodes 1 and 2, and the edge 1->2 exists, we cant add any new ones.
            {
                // Must be non-output
                int start = Random.Range(0, maxNode + 1 - old.outputs);
                if (start >= old.inputs) start += old.outputs;
                // shift (0 ... whatever) to (0 ... old.inputs-1) U (old.inputs + old.outputs ... maxnode)   i.e. not an output

                // Must be non-input
                int end = Random.Range(0, maxNode + 1 - old.inputs);
                end += old.inputs;   // shift (0 ... whatever) to (inputs ... maxnode)   i.e. not an input

                // Now, we must check
                // 1) Does this edge already exist?           O(E)
                // 2) Does adding this edge produce a cycle?  O(V + E)

                // Check if the edge already exists, using the adjacencyMap
                bool edgeExists = false;
                if (adjacencyMap.ContainsKey(start))
                {
                    foreach (int i in adjacencyMap[start])
                    {
                        // Damn, start -> end already exists
                        if (i == end)
                        {
                            edgeExists = true;
                            break;
                        }
                    }
                }
                if (edgeExists) continue; // Start again with new start/end, as this edge already exists :(


                // Check for end->start path; if it exists, we cannot add the start->end edge.
                if (CanReach(end, start, adjacencyMap, visited))
                {
                    // This is invalid, so we try again
                    // Reset visited to all false
                    System.Array.Clear(visited, 0, visited.Length);
                }
                else
                {
                    // Valid! make connection and escape loop
                    float weight = Random.Range(-1.0f, 1.0f);
                    ConnectionGene c = new ConnectionGene(globalInnovationNumber++, start, end, false, weight);
                    old.genes.Add(c);
                    break;
                }
            }
        }
        else
        {
            // Split edge
            // 1) Pick random (and non-disabled) edge from connections
            // 2) Disable it
            // 3) Add new node
            // 4) Add new connections

            int index = Random.Range(0, old.genes.Count);
            while (old.genes[index].disabled) index = Random.Range(0, old.genes.Count);   // Reshuffle until we get a non-disabled one; if we have a disabled node we should always have at least one non-disabled one, so this will terminate

            // old.genes[index].disabled = true;           // Doesn't work because structs are weird and possibly immutable
            old.genes[index] = old.genes[index].Disable();

            // Get node IDs
            int new_id = maxNode + 1;
            int from_id = old.genes[index].fromNode;
            int to_id = old.genes[index].toNode;

            float oldWeight = old.genes[index].weight;

            // replace from->to with from->new->to
            ConnectionGene c1 = new ConnectionGene(globalInnovationNumber++, from_id, new_id, false, oldWeight / 2);
            ConnectionGene c2 = new ConnectionGene(globalInnovationNumber++, new_id, to_id, false, oldWeight / 2);

            old.genes.Add(c1);
            old.genes.Add(c2);
        }
    }

    const float weightChangeChance = 0.2f;
    const float weightChangeRange = 0.5f;
    public static void WeightMutate(Genome old)
    {
        for (int i=0; i<old.genes.Count; i++)
        {
            // Mutate iff not disabled and random chance
            if (!old.genes[i].disabled && Random.Range(0.0f, 1.0f) <= weightChangeChance)
            {
                //old.genes[i].weight += Random.Range(-weightChangeRange, weightChangeRange); // This doesn't work because of struct

                // Modify weight and ... construct a new instance
                float newWeight = old.genes[i].weight + Random.Range(-weightChangeRange, weightChangeRange);
                old.genes[i] = new ConnectionGene(old.genes[i].innovationNumber, old.genes[i].fromNode, old.genes[i].toNode, old.genes[i].disabled, newWeight);
            }
        }
    }

    // Structurally or weighturally mutate
    public static void Mutate(Genome old)
    {
        if (Random.Range(0, 1.0f) < 0.5f) StructuralMutate(old);
        else WeightMutate(old);
    }

    public DAGNet MakeNet()
    {
        return new DAGNet(inputs, outputs, genes);
    }

    // TODO: If this is any kind of bottleneck, it could be sped up by carrying this value around
    public int GetMaxNodeNumber()
    {
        int max = inputs + outputs - 1; // this is the ID of the last output
        foreach (ConnectionGene c in genes)
        {
            if (c.fromNode > max) max = c.fromNode;
            if (c.toNode > max) max = c.toNode;
        }
        return max;
    }


    // TODO: Calculate similarity for speciation by calculating disjoint, excess etc nodes
    public float GetSimilarity(Genome other)
    {
        return 0.0f;
    }

    // TODO: Create a new Genome which is a crossover between a and b
    // Note: I believe this has to leave a and b unmodified, as they will be used for future crossovers
    public static Genome Crossover(Genome a, Genome b)
    {
        return null;
    }


    // Returns whether you can reach b from a using the edges in 'adj'
    // Runtime O(V+E), where V is num vertices and E is num edges in the graph
    public static bool CanReach(int a, int b, IDictionary<int, List<int>> adj, bool[] visited)
    {
        if (a == b) return true;

        if (visited[a]) return false;
        visited[a] = true;

        if (adj.ContainsKey(a))
        {
            foreach (int next in adj[a])
            {
                if (CanReach(next, b, adj, visited)) return true;
            }
        }

        return false;
    }
}

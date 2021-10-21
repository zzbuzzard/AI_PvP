using System;
using System.Collections;
using System.Collections.Generic;
using static Constants;


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

    public ConnectionGene Enable()
    {
        return new ConnectionGene(innovationNumber, fromNode, toNode, false, weight);
    }

    internal ConnectionGene DiEnsable()
    {
        if (this.disabled)
        {
            return Enable();
        }
        else
        {
            return Disable();
        }
    }
}

public class Genome
{
    private static int globalInnovationNumber = 0;
    public float fitness = 0; // TODO: ...

    public readonly List<ConnectionGene> genes;

    public Genome() : this(new List<ConnectionGene>())
    {
    }

    public Genome(List<ConnectionGene> genes)
    {
        this.genes = genes;
    }

    public static void NewEdgeMutate(Genome old)
    {
        // Highest ID of any node in this genome
        int maxNode = old.GetMaxNodeNumber();

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

        int loops = 10;
        // Loop until we find a valid pair    (maybe just try a few times then give up idk) this is ugly wtf
        while (0 != loops--)
        {
            // Must be non-output
            int start = RandInt(0, maxNode + 1 - numOutputs);
            if (start >= numInputs) start += numOutputs;
            // shift (0 ... whatever) to (0 ... old.inputs-1) U (old.inputs + old.outputs ... maxnode)   i.e. not an output

            // Must be non-input
            int end = RandInt(0, maxNode + 1 - numInputs);
            end += numInputs;   // shift (0 ... whatever) to (inputs ... maxnode)   i.e. not an input

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
                float weight = Rand(-1.0f, 1.0f);
                ConnectionGene c = new ConnectionGene(globalInnovationNumber++, start, end, false, weight);
                old.genes.Add(c);
                break;
            }
        }

    }

    // Directly modifies 'old'
    // If my understanding is right, we never mutate and keep an old copy
    static public void NewNodeMutate(Genome old)
    {
        if (old.genes.Count == 0) return;

        // Highest ID of any node in this genome
        int maxNode = old.GetMaxNodeNumber();

        // Split edge
        // 1) Pick random (and non-disabled) edge from connections
        // 2) Disable it
        // 3) Add new node
        // 4) Add new connections

        int index = RandInt(0, old.genes.Count);
        int _ = 5;
        while (_-->0 && old.genes[index].disabled) index = RandInt(0, old.genes.Count);

        // old.genes[index].disabled = true;           // Doesn't work because structs are weird and possibly immutable
        // old.genes[index] = old.genes[index].Disable();

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

        old.genes.RemoveAt(index);
    }

    public static void WeightMutate(Genome old)
    {
        for (int i=0; i<old.genes.Count; i++)
        {
            if (Rand(0.0f, 1.0f) <= weightChangeChance)
            {
                // Mutate iff not disabled and random chance
                if (!old.genes[i].disabled)
                {
                    float newWeight;
                    // UNIFORMLY PERTURB
                    if (Rand(0.0f, 1.0f) < 0.9f)
                    {
                        newWeight = old.genes[i].weight * Rand(1.0f-weightChangeRange, 1.0f+weightChangeRange);
                    }
                    // Random new value
                    else
                    {
                        newWeight = Rand(-1.0f, 1.0f);

                    }
                    old.genes[i] = new ConnectionGene(old.genes[i].innovationNumber, old.genes[i].fromNode, old.genes[i].toNode, old.genes[i].disabled, newWeight);
                }
            }
        }
    }

    public static void DisableMutate(Genome old)
    {
        if (old.genes.Count == 0) return;
        int i = RandInt(0, old.genes.Count);
        old.genes.RemoveAt(i);
//        old.genes[i] = old.genes[i].DiEnsable();
    }

    // Structurally or weighturally mutate
    public static void Mutate(Genome old)
    {
        if (Rand(0, 1.0f) < 0.1f)  DisableMutate(old);    // 10%
        if (Rand(0, 1.0f) < 0.01f) NewNodeMutate(old);    // 1%
        if (Rand(0, 1.0f) < 0.1f)  NewEdgeMutate(old);    // 10%
        if (Rand(0, 1.0f) < 0.8f)  WeightMutate(old);     // 80%
    }

    public DAGNet MakeNet()
    {
        return new DAGNet(genes);
    }

    // TODO: If this is any kind of bottleneck, it could be sped up by carrying this value around
    public int GetMaxNodeNumber()
    {
        int max = numInputs + numOutputs - 1; // this is the ID of the last output
        foreach (ConnectionGene c in genes)
        {
            if (c.fromNode > max) max = c.fromNode;
            if (c.toNode > max) max = c.toNode;
        }
        return max;
    }


    public float GetSimilarity(Genome other)
    {
        //All innovation numbers
        HashSet<int> innovationNumbers = new HashSet<int>();
        IDictionary<int, ConnectionGene> ourGenes = new Dictionary<int, ConnectionGene>();
        IDictionary<int, ConnectionGene> otherGenes = new Dictionary<int, ConnectionGene>();

        int ourMax = 0;
        int otherMax = 0;

        foreach (ConnectionGene gene in genes)
        {
            ourGenes.Add(gene.innovationNumber, gene);
            innovationNumbers.Add(gene.innovationNumber);
            if (gene.innovationNumber > ourMax) ourMax = gene.innovationNumber;
        }

        foreach (ConnectionGene gene in other.genes)
        {
            otherGenes.Add(gene.innovationNumber, gene);
            innovationNumbers.Add(gene.innovationNumber);
            if (gene.innovationNumber > otherMax) otherMax = gene.innovationNumber;
        }

        float delta = 0;

        int maxGenes = Math.Max(ourGenes.Count, otherGenes.Count);
        if(maxGenes == 0) // Nobody has any genes; just return 0
            return 0.0f;

        float weightDifferences = 0;
        int matchingGenes = 0;

        // Calculate via the formula c1E/N, + c2D/N + c3W
        foreach (int inum in innovationNumbers)
        {
            // Shared gene
            if (ourGenes.ContainsKey(inum) && otherGenes.ContainsKey(inum))
            {
                weightDifferences += weightCoeff * Math.Abs(ourGenes[inum].weight - otherGenes[inum].weight);
                matchingGenes++;
            }
            // Disjoint/Excess
            else
            {
                // Excess
                if(inum > ourMax || inum > otherMax)
                {
                    delta += excessCoeff / maxGenes;
                }
                // Disjoint
                else
                {
                    delta += disjointCoeff / maxGenes;
                }
            }
        }
        if (matchingGenes != 0) weightDifferences /= matchingGenes;
        
        delta += weightDifferences;

        return delta;

    }

    public string GenomeStats()
    {
        int d = 0; foreach (ConnectionGene g in genes) d += g.disabled?1:0;

        HashSet<int> usedNums = new HashSet<int>();
        foreach (ConnectionGene g in genes) {
            usedNums.Add(g.fromNode);
            usedNums.Add(g.toNode);
        }

        int wasted = 0;
        for (int k=numInputs + numOutputs; k<GetMaxNodeNumber(); k++)
        {
            if (!usedNums.Contains(k))
                wasted++;
        }

        return "Number of genes: " + genes.Count +
               "\nNumber of nodes: " + GetMaxNodeNumber() +
               "\nWasted nodes: " + wasted +
               "\nDisabled genes: " + d;
    }

    // Note: I believe this has to leave a and b unmodified, as they will be used for future crossovers
    public static Genome Crossover(Genome a, Genome b)
    {
        //All innovation numbers
        HashSet<int> innovationNumbers = new HashSet<int>();
        IDictionary<int, ConnectionGene> aGenes = new Dictionary<int, ConnectionGene>();
        IDictionary<int, ConnectionGene> bGenes = new Dictionary<int, ConnectionGene>();

        foreach (ConnectionGene gene in a.genes)
        {
            aGenes.Add(gene.innovationNumber, gene);
            innovationNumbers.Add(gene.innovationNumber);
        }

        foreach (ConnectionGene gene in b.genes)
        {
            bGenes.Add(gene.innovationNumber, gene);
            innovationNumbers.Add(gene.innovationNumber);
        }

        IDictionary<int, ConnectionGene> dominantParent;
        if (a.fitness > b.fitness)      dominantParent = aGenes;
        else if (b.fitness > a.fitness) dominantParent = bGenes;
        else                            dominantParent = (Rand(0.0f, 1.0f) > 0.5f ? aGenes : bGenes);
        
        List<ConnectionGene> newGenes = new List<ConnectionGene>();
        foreach (int inum in innovationNumbers)
        {
            if (aGenes.ContainsKey(inum) && bGenes.ContainsKey(inum))
            {
                newGenes.Add(dominantParent[inum]);
            }
            else
            {
                if (aGenes.ContainsKey(inum)) newGenes.Add(aGenes[inum]);
                else newGenes.Add(bGenes[inum]);
            }
        }

        // TODO: Need to check if cyclic
        return new Genome(newGenes);
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

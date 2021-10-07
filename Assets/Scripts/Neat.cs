using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// There is a fixed number of points in total
// For a population of size N, there are 10N points in total.
public class Neat : Genetic
{
    class NeatComparator : IComparer<NeatPlayer>
    {
        public int Compare(NeatPlayer x, NeatPlayer y)
        {
            return x.fitness.CompareTo(y.fitness);
        }
    }
    
    class Species
    {
        public List<NeatPlayer> players { get; private set; }

        public Species(List<NeatPlayer> players)
        {
            this.players = players;
        }

        const float similarityThreshold = 0.1f;
        public bool IsInSpecies(NeatPlayer q)
        {
            NeatPlayer p = players[UnityEngine.Random.Range(0, players.Count)];
            return p.GetGenome().GetSimilarity(q.GetGenome()) <= similarityThreshold;
        }

        public void Add(NeatPlayer p)
        {
            players.Add(p);
        }
    }

    List<NeatPlayer> ais;
    int N;

    public const int FFA_size = 8;

    public Neat(List<NeatPlayer> initialPopulation)
    {
        N = initialPopulation.Count;

        ais = new List<NeatPlayer>();
        for (int i = 0; i < N; i++)
        {
            ais.Add(initialPopulation[i]);
        }
    }

    public override List<GenericPlayer> GetPopulation()
    {
        List<GenericPlayer> players = new List<GenericPlayer>();

        for (int i = 0; i < N; i++)
        {
            players.Add(ais[i]);
        }

        return players;
    }

    // TODO speciation
    private List<Species> Speciate()
    {
        return null;
    }

    // Create match for allSpecies[speciesIndex].population[index]
    //private List<NeatPlayer> MatchMake(int speciesIndex, int index, List<Species> allSpecies)
    //{
    //    List<NeatPlayer> l = new List<NeatPlayer>();
    //    for (int i=0; i<allSpecies.Count; i++)
    //    {
    //        l.Add(allSpecies[i].players[0]);
    //    }

    //    l.Add(allSpecies[speciesIndex].players[index]);

    //    return l;
    //}

    // TODO some kind of sensible matchmaking
    // TODO use speciation
    // TODO repopulate species with breeding etc
    public override void Increment()
    {
        List<Species> species = new List<Species>(); // TODO: Call function for speciation

        Debug.Log("Number of species: " + species.Count);


        //for (int j = 0; j < FFA_size; j++)
        //{
        //    roundPlayers.Add(null);
        //    //scores.Add(new Pair<float, int>(0.0f, 0));
        //}

        //for (int i = 0; i < N / FFA_size; i++)
        //{
        //    for (int j = 0; j < FFA_size; j++)
        //    {
        //        roundPlayers[j] = ais[i * FFA_size + j];
        //    }

        //    // TODO: Shuffle? but then issues in next loop
        //    Game.SimulateGame(roundPlayers);

        //    // Min score is the one with the largest index
        //    float minScore = ais[(i + 1) * FFA_size - 1].fitness;

        //    for (int j = 0; j < FFA_size; j++)
        //    {
        //        ((NeatPlayer)roundPlayers[j]).fitness = Genetic.GetScore1(roundPlayers[j]);
        //        //scores[j].fst = Genetic.GetScore1(roundPlayers[j]);
        //        //scores[j].snd = j;
        //    }
            //scores.Sort();

            //// scores[0] is smallest so worst
            //// scores[FFA_size-1] is biggest so best

            //float contribute = Math.Min(minScore, minContribute); // Everyone contributes this amount

            //int totalUnits = (FFA_size * (FFA_size - 1)) / 2;
            //float unitPoint = contribute * FFA_size / totalUnits;

            //for (int j = 0; j < FFA_size; j++)
            //{
            //    int ind = scores[j].snd;
            //    ais[i * FFA_size + ind].fitness += unitPoint * j - contribute;
            //}
        //}


    }
}
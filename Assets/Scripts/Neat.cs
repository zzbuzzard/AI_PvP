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
        public List<NeatPlayer> players;

        public Species()
        {
            this.players = new List<NeatPlayer>();
        }
        public Species(NeatPlayer player)
        {
            this.players = new List<NeatPlayer>();
            this.players.Add(player);
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

        public float GetFitness()
        {
                float fitness = 0;
                foreach (NeatPlayer player in players)
                {
                fitness += player.fitness;
                }

                return fitness / players.Count;
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

    // TODO some kind of sensible matchmaking
    // TODO use speciation
    // TODO repopulate species with breeding etc


    private List<Species> GetSpecies(List<NeatPlayer> players)
    {
        List<Species> species = new List<Species>();
        foreach(NeatPlayer player in players)
        {
            bool found_species = false;
            foreach(Species speshee in species)
            {
                if (speshee.IsInSpecies(player))
                {
                    speshee.Add(player);
                    found_species = true;
                    break;
                }
            }
            if (found_species)
            {
                continue;
            }
            else
            {
                Species speshee = new Species(player);
                species.Add(speshee);
            }
        }
        return species;
    }

    void AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAa(List<Species> species)
    {
        return;
    }

    // Speciate
    // Evaluate fitness of everyone
    // Work out population size
    // Create new populations by breeding
    public override void Increment()
    {
        List<GenericPlayer> roundPlayers = new List<GenericPlayer>();
        List<Species> species = GetSpecies(ais);

        AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAa(species);

        float averageFitness = 0;

        foreach (NeatPlayer player in ais)
        {
            averageFitness += player.fitness;
        }
        averageFitness /= ais.Count;

        List<Tuple<int, Species>> speciesSizes = new List<Tuple<int, Species>>();
        foreach (var speshee in species)
        {
            float fitdiff = Math.Abs(speshee.GetFitness() - averageFitness);
            speciesSizes.Add(new Tuple<int, Species>((int)(fitdiff / 10), speshee));
        }

        ais.Clear();
        foreach (var spesheeSize in speciesSizes)
        {
            for (int i = 0; i < spesheeSize.Item1; i++)
            {
                var currspeshee = spesheeSize.Item2;
                var player = currspeshee.players[0].BreedPlayer(currspeshee.players[1]);
                ais.Add((NeatPlayer)player);
            }
        }
    }
}
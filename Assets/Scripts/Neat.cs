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
            return -x.fitness.CompareTo(y.fitness);
        }
    }
    
    class Species
    {
        public List<NeatPlayer> players { get; private set; }

        public Species()
        {
            this.players = new List<NeatPlayer>();
        }
        public Species(NeatPlayer player)
        {
            this.players = new List<NeatPlayer>();
            this.players.Add(player);
        }

        const float similarityThreshold = 0.2f;
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

            return fitness;
        }
    }

    List<NeatPlayer> ais;
    int N;

    List<GenericPlayer> enemies;

    public Neat(List<NeatPlayer> initialPopulation)
    {
        N = initialPopulation.Count;

        enemies = new List<GenericPlayer>() { new RobotPlayer1(), new RobotPlayer2(), new RobotPlayer3() };

        ais = new List<NeatPlayer>();
        for (int i = 0; i < N; i++)
        {
            ais.Add(initialPopulation[i]);
        }
    }

    public override List<GenericPlayer> GetPopulation()
    {
        List<GenericPlayer> players = new List<GenericPlayer>();

        ais.Sort(new NeatComparator());

        for (int i = 0; i < ais.Count; i++)
        {
            players.Add(ais[i]);
        }

        return players;
    }


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

    private void EvaluateAllFitness(List<Species> species)
    {
        float maxFitness = 0.0f;

        foreach (Species s in species)
        {
            foreach (NeatPlayer p in s.players)
            {
                p.fitness = 0.0f;

                foreach (GenericPlayer enemy in enemies)
                {
                    List<GenericPlayer> gs;

                    gs = new List<GenericPlayer>() { p, enemy };
                    Game.SimulateGame(gs);
                    p.fitness += Genetic.GetScore1(p);

                    gs = new List<GenericPlayer>() { enemy, p };
                    Game.SimulateGame(gs);
                    p.fitness += Genetic.GetScore1(p);
                }

                if (p.fitness > maxFitness) maxFitness = p.fitness;
            }
        }

        Debug.Log("Max fitness: " + maxFitness);

        // Adjusting
        foreach (Species s in species)
        {
            foreach (NeatPlayer p in s.players)
            {
                p.fitness /= s.players.Count;
            }

            s.players.Sort(new NeatComparator());
        }
    }

    const float breedSpeciesPercent = 0.25f;
    const int goalNumSpecies = 5;

    // Speciate
    // Evaluate fitness of everyone
    // Work out population size
    // Create new populations by breeding
    public override void Increment()
    {
        List<Species> species = GetSpecies(ais);
        Debug.Log("Number of species: " + species.Count + " and pop size is " + ais.Count);

        EvaluateAllFitness(species);

        float averageFitness = 0;

        foreach (NeatPlayer player in ais)
        {
            averageFitness += player.fitness;
        }
        averageFitness /= ais.Count;

        ais.Clear();

        for (int spIndex = 0; spIndex < species.Count; spIndex++)
        {
            Species speshee = species[spIndex];

            float myfit = speshee.GetFitness();
            int size = (int)(myfit / averageFitness);

            if (size == 0) size = 1;

            if (spIndex == species.Count - 1)
            {
                size = N - ais.Count; 
            }

//            Debug.Log("Hi im a species and my new size is " + size);

            for (int i = 0; i < size - 1; i++)
            {
                int a = (int)(speshee.players.Count * UnityEngine.Random.Range(0.0f, breedSpeciesPercent));
                int b = (int)(speshee.players.Count * UnityEngine.Random.Range(0.0f, breedSpeciesPercent));
                Genome genom = Genome.Crossover(speshee.players[a].GetGenome(), speshee.players[b].GetGenome());

                int NUM_MUTATE = (species.Count < goalNumSpecies ? 5 : 1);

                for (int _=0; _<NUM_MUTATE; _++)
                    Genome.Mutate(genom);

                ais.Add(new NeatPlayer(new NeatNet(genom)));
            }
            
            if (size > 0)
                ais.Add(speshee.players[0]); // Keep the best one >:)
        }
    }
}
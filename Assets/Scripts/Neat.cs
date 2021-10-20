using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constants;

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

    int generation = 0;
    const int addNewOn = 100;
    const int maxNumEnemies = 1;

    List<NeatPlayer> ais;
    int N;

    List<GenericPlayer> enemies;

    public Neat(List<NeatPlayer> initialPopulation)
    {
        N = initialPopulation.Count;

        enemies = new List<GenericPlayer>() { new NeatPlayer(new NeatNet(new Genome())) };

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

    private float maxFitness = 0.0f;
    private NeatPlayer fittest = null;

    private void EvaluateFitness(NeatPlayer p)
    {
        float trialWeight = 100.0f;

        p.fitness = 0.0f;

        int gamesPlayed = 0;
        int games = 10;
        //foreach (GenericPlayer enemy in enemies)
        for(int _ = 0; _ < games; _++)
        {
            List<GenericPlayer> gs;
            GenericPlayer enemy = ais[UnityEngine.Random.Range(0, ais.Count)];
            gs = new List<GenericPlayer>() { p, enemy };
            Game.SimulateGame(gs);
            p.fitness += Genetic.GetScore1(p);
            gamesPlayed++;

            gs = new List<GenericPlayer>() { enemy, p };
            Game.SimulateGame(gs);
            p.fitness += Genetic.GetScore1(p);
            gamesPlayed++;
        }

        p.fitness /= gamesPlayed;

        foreach (Trial t in Trial.trials)
        {
            p.fitness += trialWeight * Game.Trial(p, t);
        }

        // If it's actually any good, prioritise less complex species
        //if (p.fitness > 50.0f)
        //{
        //    p.fitness = Mathf.Max(0.0f, p.fitness + (200 - p.GetGenome().genes.Count / 20.0f));
        //}
    }

    private void EvaluateAllFitness(List<Species> species)
    {
        maxFitness = 0.0f;
        fittest = null;

        foreach (Species s in species)
        {
            foreach (NeatPlayer p in s.players)
            {
                EvaluateFitness(p);

                if (fittest == null || p.fitness > maxFitness)
                {
                    maxFitness = p.fitness;
                    fittest = p;
                }
            }

            // Sort by fitness
            s.players.Sort(new NeatComparator());
        }
    }

    private void PrintInfo(List<Species> species)
    {
        Debug.Log("Number of species: " + species.Count + " and pop size is " + ais.Count +
                  "\nFittest has fitness " + maxFitness +
                  "\nStats: " + fittest.GetGenome().GenomeStats()
            );
    }

    // Speciate
    // Evaluate fitness of everyone
    // Work out population size
    // Create new populations by breeding
    int increments = 0;
    public override void Increment()
    {
        
        List<Species> species = GetSpecies(ais);

        EvaluateAllFitness(species);
        increments++;
        if(increments == 100) { 
            PrintInfo(species);
            increments = 0;
        }

        float averageFitness = 0;
        foreach (Species s in species)
        {
            foreach (NeatPlayer player in s.players)
            {
                averageFitness += player.fitness / s.players.Count;
            }
        }
        averageFitness /= ais.Count;

        float avg_fit_real = 0.0f;
        foreach (NeatPlayer p in ais)
            avg_fit_real += p.fitness;
        avg_fit_real /= ais.Count;

        ais.Clear();

        for (int spIndex = 0; spIndex < species.Count; spIndex++)
        {
            Species speshee = species[spIndex];

            float myfit = speshee.GetFitness();
            int size = (int)(myfit / averageFitness);

            // Last one: make sure we return to the normal population size
            if (spIndex == species.Count - 1)
                size = N - ais.Count; 

            for (int i = 0; i < size - 1; i++)
            {
                int a = (int)(speshee.players.Count * UnityEngine.Random.Range(0.0f, breedSpeciesPercent));
                int b = (int)(speshee.players.Count * UnityEngine.Random.Range(0.0f, breedSpeciesPercent));
                Genome genom = Genome.Crossover(speshee.players[a].GetGenome(), speshee.players[b].GetGenome());

                Genome.Mutate(genom);

                ais.Add(new NeatPlayer(new NeatNet(genom)));
            }

            if (size != 0)
                ais.Add(speshee.players[0]); // Keep the best one >:)
        }

        generation++;

        if (generation % addNewOn == 0)
        {
            Debug.Log("Adding a new enemy! There are now " + enemies.Count + " enemies");
            enemies.Add(fittest);

            if (enemies.Count > maxNumEnemies)
                enemies.RemoveAt(0);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using static Constants;
using static UnityEngine.Debug;

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

        public bool IsInSpecies(NeatPlayer q, float threshold)
        {
            NeatPlayer p = players[RandInt(0, players.Count)];
            return p.GetGenome().GetSimilarity(q.GetGenome()) <= threshold;
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
    const int maxNumEnemies = 2;
    int previousNumSpecies = 1;
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

    public override GenericPlayer[] GetPopulation()
    {
        GenericPlayer[] players = new GenericPlayer[ais.Count];

        ais.Sort(new NeatComparator());

        for (int i = 0; i < ais.Count; i++)
            players[i] = ais[i];

        return players;
    }

    private List<Species> Speciate(List<NeatPlayer> players)
    {
        float threshold = similarityThreshold;
        if (previousNumSpecies > goalNumSpecies) threshold += similarityVariation;
        if (previousNumSpecies < goalNumSpecies) threshold -= similarityVariation;

        List<Species> species = new List<Species>();
        foreach(NeatPlayer player in players)
        {
            bool found_species = false;
            foreach(Species speshee in species)
            {
                if (speshee.IsInSpecies(player, threshold))
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

    private void PruneAll()
    {
        if (maxFitness > 0.1f)
        {
            Log("Pruning all");
            for (int i = 0; i < ais.Count; i++) ais[i] = RemoveExcess(ais[i]);
        }
        else Log("Too stupid to prune");
    }

    private NeatPlayer RemoveExcess(NeatPlayer p)
    {
        float fitness = p.fitness;
        int removed = 0;

        Genome g = p.GetGenome();

        for (int i=g.genes.Count-1; i>=0; i--)
        {
            ConnectionGene c = g.genes[i];
            g.genes.RemoveAt(i);

            NeatPlayer p2 = new NeatPlayer(new NeatNet(g));

            EvaluateFitness(p2);
            if (p2.fitness < fitness)
            {
                g.genes.Add(c);
            }
            else
            {
                fitness = p2.fitness;
                removed++;
            }
        }

        NeatPlayer p3 = new NeatPlayer(new NeatNet(g));
        EvaluateFitness(p3);

        //Debug.Log("Removed " + removed + " genes");

        return p3;
    }

    // Sets p.fitness
    private bool usesPvp = false;
    private void EvaluateFitness(NeatPlayer p)
    {
        float trialWeight = 100.0f;
        float uruseless = 1.0f;
        float uselesstoo = 0f;
        p.fitness = 0.1f;
        
        //if (usesPvp)
        //{
        //    int gamesPlayed = 0;

        //    foreach (GenericPlayer enemy in enemies)
        //    {
        //        GenericPlayer[] gs;
        //        Game g;
        //        float score;

        //        gs = new GenericPlayer[2] { p, enemy };
        //        g = GameConstructor(gs);
        //        score = g.SimulateGame()[0];       // We are first in the list
        //        p.fitness += score;
        //        gamesPlayed++;

        //        gs = new GenericPlayer[2] { enemy, p };
        //        g = GameConstructor(gs);
        //        score = g.SimulateGame()[1];       // We are second in the list
        //        p.fitness += score;
        //        gamesPlayed++;
        //    }

        //    p.fitness /= gamesPlayed;
        //}

        //foreach (Trial t in Trial.trials)
        //{
        //    p.fitness += trialWeight * ShootGame.Trial(p, t);
        //}

        // TODO: bool for usesSinglePlayer?
        p.fitness += GameConstructor(new GenericPlayer[] { p }).SimulateGame()[0];

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
        float t = 0;

        int[] species_counts = new int[species.Count];
        float[] species_avg_fit = new float[species.Count];

        for (int i = 0; i < species.Count; i++)
        {
            species_counts[i] = species[i].players.Count;
            species_avg_fit[i] = 0;

            foreach (NeatPlayer p in species[i].players)
            {
                t += p.fitness;
                species_avg_fit[i] += p.fitness;
            }
            species_avg_fit[i] /= species[i].players.Count;
        }

        t /= ais.Count;

        Log("Number of species: " + species.Count + " and pop size is " + ais.Count +
                  "\nMax fitness: " + maxFitness +
                  "\nStats: " + fittest.GetGenome().GenomeStats() +
                  "\nAverage fitness: " + t +
                  "\nSpecies sizes: " + Util.GetArrString(species_counts) +
                  "\nSpecies fitns: " + Util.GetArrString(species_avg_fit)
            );
    }

    // Speciate
    // Evaluate fitness of everyone
    // Work out population size
    // Create new populations by breeding
    public override void Increment()
    {
        //Change position generating seed;
        Constants.seed += 1;
        List<Species> species = Speciate(ais);
        EvaluateAllFitness(species);

        //if (generation % 150 == 0)
        //{
        //    PruneAll();
        //}

        if (generation % 10 == 0)
        {
            PrintInfo(species);
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

            // Prune the best r% before breeding
            //int max_index = (int)(speshee.players.Count * breedSpeciesPercent);
            //for (int i = 0; i < max_index; i++)
            //{
            //    if (UnityEngine.Random.Range(0.0f, 1.0f) < pruneChance)
            //        speshee.players[i] = RemoveExcess(speshee.players[i]);
            //}


            float myfit = speshee.GetFitness();
            int size = (int)(myfit / averageFitness);

            // Last one: make sure we return to the normal population size
            if (spIndex == species.Count - 1)
                size = N - ais.Count;

            for (int i = 0; i < size - 1; i++)
            {
                int a = (int)(speshee.players.Count * Rand(0.0f, breedSpeciesPercent));
                int b = (int)(speshee.players.Count * Rand(0.0f, breedSpeciesPercent));
                Genome genom = Genome.Crossover(speshee.players[a].GetGenome(), speshee.players[b].GetGenome());

                Genome.Mutate(genom);

                ais.Add(new NeatPlayer(new NeatNet(genom)));
            }

            if (size != 0)
                ais.Add(speshee.players[0]); // Keep the best one >:)
        }

        generation++;
        previousNumSpecies = species.Count;

        if (generation % addNewOn == 0)
        {
            Log("Adding a new enemy! There are now " + enemies.Count + " enemies");
            enemies.Add(RemoveExcess(fittest));

            if (enemies.Count > maxNumEnemies)
                enemies.RemoveAt(0);
        }


        //if (generation == 500)
        //{
        //    Debug.Log("ENTERING WALL MODE");
        //    foreach (Trial t in Trial.trials) ((TargetPractice)t).zeroMap = false;
        //}

        if (generation == 1000)
        {
            Log("ENTERING PVP MODE");
            usesPvp = true;
        }
    }
}
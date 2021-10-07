using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Controller : MonoBehaviour
{
    public TextMeshProUGUI genText;
    public GameDisplay d;
    Genetic population;

    void Start()
    {
        List<NeatPlayer> pop = new List<NeatPlayer>();

        // Populate with 100 layered AI players
        for (int i=0; i<30; i++)
        {
            pop.Add(new NeatPlayer(new NeatNet(new Genome(AIPlayer.numInputs, AIPlayer.numOutputs))));
            //pop.Add(AIPlayer.MakeLayeredAIPlayer());
        }

        population = new Neat(pop);
    }

    int generation = 0;
    void Update()
    {
        if (!d.gameInProgress)
        {
            population.Increment();
            generation++;
            genText.text = "Generation " + generation;
        }
    }

    public void ShowGame()
    {
        if (d.gameInProgress) StopGame();

        List<GenericPlayer> ps = population.GetPopulation();
        List<GenericPlayer> qs = new List<GenericPlayer>();
        for (int i = 0; i < ps.Count; i++)
            qs.Add(ps[i]);

        d.Simulate(qs);
    }

    public void PlayGame()
    {
        if (d.gameInProgress) StopGame();

        List<GenericPlayer> ps = population.GetPopulation();
        List<GenericPlayer> qs = new List<GenericPlayer>();
        for (int i = 0; i < RankedGenetic.FFA_size - 1; i++)
            qs.Add(ps[i]);

        qs.Add(new HumanPlayer());

        d.Simulate(qs, true);
    }

    public void StopGame()
    {
        d.EndSimulation();
    }
}

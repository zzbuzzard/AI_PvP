using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;

public class Controller : MonoBehaviour
{
    public TextMeshProUGUI genText;
    public TextMeshProUGUI gameText;

    public GameDisplay d;
    Genetic population;
    private Thread _t1;

    private List<GenericPlayer[]> currentlyShowingMatches;

    int generation = 0;
    bool threadRunningGames = false;

    void Start()
    {
        List<NeatPlayer> pop = new List<NeatPlayer>();

        for (int i=0; i<100; i++)
        {
            pop.Add(new NeatPlayer(new NeatNet(new Genome())));
            //pop.Add(AIPlayer.MakeLayeredAIPlayer());
        }

        population = new Neat(pop);
        //_t1 = new Thread(_Update);

        _t1 = new Thread(() =>
        {
            Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest; // yom yom
            while (true)
            {
                if (!d.gameInProgress)
                {
                    threadRunningGames = true;
                    population.Increment();
                    generation++;
                }
                else
                {
                    threadRunningGames = false;
                }
            }
        });

        _t1.Start();
    }

    void Update()
    {
        genText.text = "Generation " + generation;

        if (d.gameInProgress)
        {
            gameText.enabled = true;
            gameText.text = "Game " + d.GetCurrentGameNumber() + " / " + d.GetGameCount();
        }
        else
        {
            gameText.enabled = false;
        }

        //while (true)
        //{
        //if (!d.gameInProgress)
        //{
        //    population.Increment();
        //    generation++;
        //    genText.text = "Generation " + generation;
        //}
        //}
    }

    private void WaitThread()
    {
        while (threadRunningGames) ;
    }

    public void ShowGame()
    {
        if (d.gameInProgress) StopGame();
        d.gameInProgress = true;
        WaitThread();

        List<Game> toSimulate = new List<Game>();

        foreach (GenericPlayer[] ps in population.GetMatches())
        {
            toSimulate.Add(Constants.GameConstructor(ps));
        }

        d.Simulate(toSimulate);
    }

    public void ShowTrialN(int n)
    {
        ShowTrial(Trial.trials[n]);
    }

    public void ShowTrial(Trial t)
    {
        if (d.gameInProgress) StopGame();
        d.gameInProgress = true;
        WaitThread();

        List<GenericPlayer[]> ps = population.GetMatches();
        
        d.Simulate(t.CreateTrial(ps[0][0]));
    }


    public void PlayGame()
    {
        if (d.gameInProgress) StopGame();
        d.gameInProgress = true;
        WaitThread();

        d.Simulate(Constants.GameConstructor(new GenericPlayer[] { new HumanPlayer() }), true);

        // PvP version: play against an enemy
        //GenericPlayer[] ps = population.GetPopulation();
        //GenericPlayer[] qs = new GenericPlayer[2] { ps[0], new HumanPlayer() };
        //d.Simulate(Constants.GameConstructor(qs), true);
    }

    public void StopGame()
    {
        d.EndSimulation();
    }
}

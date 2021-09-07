using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public GameDisplay d;
    RankedGenetic r;

    void Start()
    {
        List<AIPlayer> pop = new List<AIPlayer>();

        for (int i=0; i<100; i++)
        {
            pop.Add(new AIPlayer());
        }

        r = new RankedGenetic(pop);
    }

    int t = 0;
    void Update()
    {
        if (!d.gameInProgress)
        {
            print("Step");
            r.Increment();
            t++;
            if (t % 100 == 0)
            {
                List<GenericPlayer> ps = r.GetPopulation();
                List<GenericPlayer> qs = new List<GenericPlayer>();
                for (int i = 0; i < RankedGenetic.FFA_size - 1; i++)
                    qs.Add(ps[i]);

                qs.Add(new HumanPlayer());

                d.Simulate(qs);
            }
        }
    }
}

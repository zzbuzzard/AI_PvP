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

        for (int i=0; i<50; i++)
        {
            pop.Add(new AIPlayer(Random.Range(10, 10000)));
        }

        r = new RankedGenetic(pop);
    }

    int t = 0;
    void Update()
    {
        if (!d.gameInProgress)
        {
            r.Increment();
            t++;
            if (t % 200 == 0)
            {
                List<GenericPlayer> ps = r.GetPopulation();
                List<GenericPlayer> qs = new List<GenericPlayer>();
                for (int i = 0; i < RankedGenetic.FFA_size; i++)
                    qs.Add(ps[i]);

                d.Simulate(qs);
            }
        }
    }
}

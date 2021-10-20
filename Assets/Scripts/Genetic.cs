using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Abstract class for a genetic algorithm
public abstract class Genetic
{
    // Get the population, in no particular order
    public abstract List<GenericPlayer> GetPopulation();

    // Increments the genetic algorithm, creating a new population
    public abstract void Increment();

    // Bigger score is better; here's one random metric
    public static float GetScore1(GenericPlayer p)
    {
        //float accuracy = p.shotsHit / (float)(p.shotsFired == 0 ? 1 : p.shotsFired);

        float endBonus = 0.0f;
        if (p.endType == EndType.KILLED)  endBonus = 0.25f;
        if (p.endType == EndType.TIMEOUT) endBonus = 0.5f;
        if (p.endType == EndType.WON)     endBonus = 1.0f;
        //if (p.endType == EndType.WALL)    endBonus = -1000.0f;

        return endBonus;

        //return Mathf.Max(0.0f,
        //                 p.shotsHit * 50                        // 50 points per hit
        //               + lifeProp * lifeProp * 20               // 20 bonus points for max life
        //               + endBonus                               // 100 bonus points for a win
        //               + p.frameOfDeath * Game.spf / 2.0f       // 0.5 points per second
        //               + p.shotsFired / 5.0f
        //               // TODO: Force them to move left/right
        //               );

//        float fitness = 
//             + endBonus
////             + percentageDiedBefore * 6              // up to 6 points for rank
//             + p.shotsHit * 30                       // 30 points per hit shot
//             + p.playersKilled * 30                  // 30 points per kill
//             + 5 * accuracy                          // 5 points for 100% accuracy
////             - p.shotsFired                          // -1 point per shot fired
//             ;
//        return Mathf.Max(fitness, 0.0f);

        //             + p.life                               // 3 points for a perfect run, 2 for one hit, 1 for two hits
        //             + p.frameOfDeath * Game.spf / 20.0f    // number of seconds alive;  one minute is 3 points
    }
}

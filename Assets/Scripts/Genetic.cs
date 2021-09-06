using System.Collections;
using System.Collections.Generic;

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
        float accuracy = p.shotsHit / (float)(p.shotsFired == 0 ? 1 : p.shotsFired);

        return p.life * p.life                      // 9 points for a perfect run, 4 for one hit, 1 for two hits
             + p.frameOfDeath * Game.spf / 12.0f    // number of seconds alive;  one minute is 5 points
             + p.shotsHit * 5                       // 5 points per hit shot
             + p.playersKilled * 5                  // bonus 5 points for a kill
             + 5 * accuracy                         // bonus 5 points for 100% accuracy
             ;
    }
}

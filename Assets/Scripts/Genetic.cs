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

        float endBonus = 0.0f;
        if (p.endType == EndType.KILLED)  endBonus = 0.0f;
        if (p.endType == EndType.TIMEOUT) endBonus = -8.0f;
        if (p.endType == EndType.WON)     endBonus = 8.0f;

        return
             + endBonus
             + p.diedBefore * 3
             + p.shotsHit * 5                       // 5 points per hit shot
             + p.playersKilled * 5                  // bonus 5 points for a kill
             + 3 * accuracy                         // bonus 3 points for 100% accuracy
             ;

        //             + p.life                               // 3 points for a perfect run, 2 for one hit, 1 for two hits
        //             + p.frameOfDeath * Game.spf / 20.0f    // number of seconds alive;  one minute is 3 points
    }
}

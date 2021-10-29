using System.Collections;
using System.Collections.Generic;

// Abstract class for a genetic algorithm
public abstract class Genetic
{
    // Get the population, in no particular order
    public abstract List<GenericPlayer[]> GetMatches();

    // Increments the genetic algorithm, creating a new population
    public abstract void Increment();
}

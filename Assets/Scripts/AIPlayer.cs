using System;
using System.Collections;
using System.Collections.Generic;

// An abstract class for AI players
public class AIPlayer : GenericPlayer
{
    // Todo: store neural net

    int specialNumber;

    public AIPlayer(int num)
    {
        specialNumber = num;
    }

    public override GameInput GetInput(Game game)
    {
        // TODO: Neural network lol

        int q = game.GetHashCode() % specialNumber;

        int hor = (q % 3) - 1; // -1, 0, 1
        bool jump = q < (specialNumber / 2);
        bool shoot = q < (specialNumber / 3);
        float ang = 2 * (float)Math.PI * q / (float)specialNumber;

        return new GameInput((sbyte)hor, jump, shoot, ang);
    }

    public AIPlayer Breed(AIPlayer otherParent)
    {
        return new AIPlayer((specialNumber + otherParent.specialNumber) / 2);
    }
}

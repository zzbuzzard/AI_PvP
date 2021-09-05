using System.Collections;
using System.Collections.Generic;

public class AIPlayer : GenericPlayer
{
    // todo: store my neural network

    public override GameInput GetInput(Game game)
    {
        // todo: use my neural network
        return new GameInput(0, true, false, 0.0f);
    }
}

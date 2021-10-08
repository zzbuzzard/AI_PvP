using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Sits still, shoots left
public class RobotPlayer0 : GenericPlayer
{
    public override GameInput GetInput(Game game)
    {
        return new GameInput(0, false, false, 0.0f);
    }
}



// Sits still, shoots left
public class RobotPlayer1 : GenericPlayer
{
    public override GameInput GetInput(Game game)
    {
        return new GameInput(0, false, true, 0.0f);
    }
}


// Sits still, shoots left, jumps
public class RobotPlayer2 : GenericPlayer
{
    public override GameInput GetInput(Game game)
    {
        return new GameInput(0, true, true, 0.0f);
    }
}



// Sits still, shoots directly at other player
public class RobotPlayer3 : GenericPlayer
{
    public override GameInput GetInput(Game game)
    {
        foreach (GenericPlayer p in game.players)
        {
            if (p.gameID == gameID) continue;

            float angle = Mathf.Atan2(p.y - y, p.x - x);

            return new GameInput(0, false, true, angle);
        }

        return new GameInput(0, false, false, 0.0f);
    }
}



using System.Collections;
using System.Collections.Generic;

/*

// Sits still, does nothing, like me
public class RobotPlayer0 : GenericPlayer
{
    public override GameInput GetInput(ShootGame game)
    {
        return new GameInput(0, false, false, 0.0f);
    }
}



// Sits still, shoots left/right
public class RobotPlayer1 : GenericPlayer
{
    private bool left = true;
    public override GameInput GetInput(ShootGame game)
    {
        left = !left;
        return new GameInput(0, false, true, left ? 0.0f : Mathf.PI);
    }
}


// Sits still, shoots left/right, jumps
public class RobotPlayer2 : GenericPlayer
{
    private bool left = true;
    public override GameInput GetInput(ShootGame game)
    {
        left = !left;
        return new GameInput(0, true, true, left ? 0.0f : Mathf.PI);
    }
}



// Sits still, shoots directly at other player
public class RobotPlayer3 : GenericPlayer
{
    public override GameInput GetInput(ShootGame game)
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


*/
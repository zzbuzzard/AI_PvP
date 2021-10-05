using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EndType
{
    KILLED,
    TIMEOUT,
    WON,
    WALL
}

public abstract class GenericPlayer
{
    private static int playerID;

    public int myPlayerID { get; private set; }

    public const int maxlife = 3;
    public const int numjumps = 2;

    public float x, y, vx, vy;
    public int gameID, life;
    public int jumps;
    public int lastShootFrame;
    public bool jumpLast, onFloor;


    // Performance stats:
    public int shotsFired,
               shotsHit,
               playersKilled,
               frameOfDeath,   // if they win, frameOfDeath is the last frame of the game + 1
               diedBefore;     // how many other players died before me? e.g. if all die at the same time, then 0

    public EndType endType;    // How did I die?

    public GenericPlayer()
    {
        myPlayerID = playerID++;
    }

    public void Spawn(float x, float y, int gameID)
    {
        this.gameID = gameID;
        this.x = x;
        this.y = y;
        vx = 0;
        vy = 0;
        life = maxlife;

        onFloor = false;
        jumps = 0;
        jumpLast = false;

        this.endType = EndType.TIMEOUT;
        lastShootFrame = -10;

        // Stats
        shotsFired = 0;
        shotsHit = 0;
        playersKilled = 0;
        frameOfDeath = 0;
        diedBefore = 0;
    }

    public abstract GameInput GetInput(Game game);
}

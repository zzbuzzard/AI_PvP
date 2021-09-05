using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericPlayer
{
    public float x, y, vx, vy;
    public int gameID, life;
    public bool onFloor;

    // Performance stats:
    public int shotsFired,
        shotsHit,
        playersKilled,
        frameOfDeath; // if they win, frameOfDeath is the last frame of the game + 1

    public void Spawn(float x, float y, int gameID)
    {
        this.gameID = gameID;
        this.x = x;
        this.y = y;
        vx = 0;
        vy = 0;
        life = 3;
        onFloor = false;

        // Stats
        shotsFired = 0;
        shotsHit = 0;
        playersKilled = 0;
        frameOfDeath = 0;
    }

    public abstract GameInput GetInput(Game game);
}

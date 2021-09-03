using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GenericPlayer
{
    public float x, y, vx, vy;
    public int life;
    public bool onFloor;

    public void Spawn(float x, float y)
    {
        this.x = x;
        this.y = y;
        vx = 0;
        vy = 0;
        life = 3;
        onFloor = false;
    }

    public abstract GameInput GetInput(Game game);
}

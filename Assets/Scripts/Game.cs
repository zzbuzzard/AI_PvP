using System.Collections;
using System.Collections.Generic;

public enum MapBlock
{
    EMPTY,
    WALL
}

// Stores the input of one player to the game
// TODO: 3 * 2 * 2 = 12 states <= 2^8, could be packaged into one byte for speed
public struct GameInput
{
    public sbyte hdir;  // -1 means left, 1 means right, 0 means neither
    public bool jump;   // jump button currently held down?
    public bool shoot;  // shoot button currently held down?

    public GameInput(sbyte hdir, bool jump, bool shoot)
    {
        this.hdir = hdir;
        this.jump = jump;
        this.shoot = shoot;
    }
}

struct Bullet
{
    float x, y, dir;
}

public class Game
{
    const float gravity = 1.0f;
    const float playerJumpVelocity = 2.0f;
    const float playerMoveSpeed = 1.0f;

    // The map is stored as a grid, where map[x][y] gives the block at (x, y)
    // (0, 0) is the bottom left.
    MapBlock[,] map;
    int xsize = 10;
    int ysize = 10;

    int timer;

    List<GenericPlayer> players;
    GameInput[] inputs;

    public Game(List<GenericPlayer> players)
    {
        timer = 1000;
        this.players = players;

        inputs = new GameInput[players.Count];

        map = new MapBlock[xsize, ysize];
        for (int x=0; x<xsize; x++)
        {
            map[x, 0] = MapBlock.WALL;
            for (int y=1; y<ysize; y++)
            {
                map[x, y] = MapBlock.EMPTY;
            }
        }
    }

    public void Step()
    {
        // 1) get inputs
        for (int i=0; i<players.Count; i++) {
            inputs[i] = players[i].GetInput(this);
        }

        // 2) move players
        for (int i = 0; i < players.Count; i++) {
            players[i].vx = inputs[i].hdir * playerMoveSpeed;
            if (inputs[i].jump && players[i].onFloor)
            {
                players[i].onFloor = false;
                players[i].vy = playerJumpVelocity;
            }
            else
            {
                players[i].vy -= gravity;
            }
            if (inputs[i].shoot)
            {
                // TODO: Shoot
            }
        }

        // 3) move bullets etc
        // 4) check for collisions
    }
}


// Game has a number of Players
// Player implements Move()
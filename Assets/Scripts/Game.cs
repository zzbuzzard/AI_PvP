using System;
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
    public float shootAngle;

    public GameInput(sbyte hdir, bool jump, bool shoot, float shootAngle)
    {
        this.hdir = hdir;
        this.jump = jump;
        this.shoot = shoot;
        this.shootAngle = shootAngle;
    }
}

public class Bullet
{
    public int shooterID;
    public float x, y, vx, vy;
    public Bullet(int shooterID, float x, float y, float vx, float vy)
    {
        this.shooterID = shooterID;
        this.x = x;
        this.y = y;
        this.vx = vx;
        this.vy = vy;
    }
}

public class Game
{
    const float dt = 1 / 30.0f;
    const float gravity = 5.0f;
    const float playerJumpVelocity = 5.0f;
    const float playerMoveSpeed = 2.0f;
    const float bulletMoveSpeed = 5.0f;

    public static float playerSize = 0.8f, playerSize2 = 1.0f;

    // The map is stored as a grid, where map[x][y] gives the block at (x, y)
    // (0, 0) is the bottom left.
    // All squares are centered, so this means there is a block with center at (0, 0)
    private MapBlock[,] map;
    public static int xsize { get; private set; } = 15;
    public static int ysize { get; private set; } = 10;

    int framesPassed;

    public List<Bullet> bullets;
    List<GenericPlayer> players;
    GameInput[] inputs;

    public MapBlock GetTile(int x, int y)
    {
        if (x < 0 || x >= xsize || y < 0 || y >= ysize) return MapBlock.EMPTY;
        return map[x, y];
    }

    public Game(List<GenericPlayer> players)
    {
        framesPassed = 0;

        bullets = new List<Bullet>();
        this.players = players;

        inputs = new GameInput[players.Count];
        map = new MapBlock[xsize, ysize];

        // Initiate map with a single row of walls on the floor
        for (int x=0; x<xsize; x++)
        {
            map[x, 0] = MapBlock.WALL;
            for (int y=1; y<ysize; y++)
            {
                map[x, y] = MapBlock.EMPTY;
            }
        }

        map[4, 1] = MapBlock.WALL;
        map[8, 2] = MapBlock.WALL;
        map[10, 4] = MapBlock.WALL;

        // Initiate players evenly spaced at (x, 1)

        int space = xsize / players.Count;
        for (int i=0; i<players.Count; i++)
        {
            players[i].Spawn(space * i, 1.0f, i);
        }
    }

    public void Step()
    {
        // 1) get inputs
        for (int i=0; i<players.Count; i++) {
            inputs[i] = players[i].GetInput(this);
        }

        // 2) move players - do NOT question this code as it seems to work
        for (int i = 0; i < players.Count; i++) {
            players[i].vx = inputs[i].hdir * playerMoveSpeed;

            if (players[i].onFloor)
            {
                if (inputs[i].jump)
                {
                    players[i].onFloor = false;
                    players[i].vy = playerJumpVelocity;
                }
            }
            else
            {
                players[i].vy -= gravity * dt;
            }


            players[i].x += players[i].vx * dt;

            float xmin = players[i].x - playerSize2 / 2.0f;
            float xmax = players[i].x + playerSize2 / 2.0f;
            int x1 = (int)(xmin + 0.5f);
            int x2 = (int)(xmax + 0.5f);

            float ymin = players[i].y - playerSize / 2.0f;
            float ymax = players[i].y + playerSize / 2.0f;
            int y1 = (int)(ymin + 0.5f);
            int y2 = (int)(ymax + 0.5f);

            // Check for right wall collision
            if (players[i].vx > 0)
            {
                if (GetTile(x2, y1) == MapBlock.WALL || GetTile(x2, y2) == MapBlock.WALL)
                {
                    players[i].x = x2 - 0.5f - playerSize2 / 2.0f;
                }
            }
            else if (players[i].vx < 0)
            {
                // We hit left wall, push right
                if (GetTile(x1, y1) == MapBlock.WALL || GetTile(x1, y2) == MapBlock.WALL)
                {
                    players[i].x = x1 + 0.5f + playerSize2 / 2.0f;
                }
            }

            players[i].y += players[i].vy * dt;

            xmin = players[i].x - playerSize / 2.0f;
            xmax = players[i].x + playerSize / 2.0f;

            x1 = (int)(xmin + 0.5f);
            x2 = (int)(xmax + 0.5f);

            ymin = players[i].y - playerSize2 / 2.0f;
            ymax = players[i].y + playerSize2 / 2.0f;

            y1 = (int)(ymin + 0.5f);
            y2 = (int)(ymax + 0.5f);

            players[i].onFloor = false;

            if (players[i].vy > 0)
            {
                // We hit a ceiling, push down
                if (GetTile(x1, y2) == MapBlock.WALL || GetTile(x2, y2) == MapBlock.WALL)
                {
                    players[i].y = y2 - 0.5f - playerSize2 / 2.0f;
                    players[i].vy = Math.Min(0, players[i].vy); // Allow them to continue falling if they are falling
                }
            }
            else
            {
                // We hit a floor, push up
                if (GetTile(x1, y1) == MapBlock.WALL || GetTile(x2, y1) == MapBlock.WALL)
                {
                    players[i].onFloor = true;
                    players[i].y = y1 + 0.5f + playerSize2 / 2.0f;
                    players[i].vy = Math.Max(0, players[i].vy); // Allow them to continue jumping if they are jumping
                }
            }


            if (inputs[i].shoot)
            {
                float vx = bulletMoveSpeed * (float)Math.Cos(inputs[i].shootAngle);
                float vy = bulletMoveSpeed * (float)Math.Sin(inputs[i].shootAngle);

                bullets.Add(new Bullet(players[i].gameID, players[i].x, players[i].y, vx, vy));
            }

        }

        bool hit = false;

        // 3) move bullets, check for bullet collisions
        for (int i=0; i<bullets.Count; i++)
        {
            bullets[i].x += bullets[i].vx * dt;
            bullets[i].y += bullets[i].vy * dt;

            hit = false;

            // TODO: Check for player collision
            for (int j=0; j<players.Count; j++)
            {
                if (players[j].gameID == bullets[i].shooterID) continue; // Don't collide with shooter

                // Check for collision
                if (players[j].x - playerSize2 / 2.0f <= bullets[i].x &&
                    players[j].x + playerSize2 / 2.0f >= bullets[i].x &&
                    players[j].y - playerSize2 / 2.0f <= bullets[i].y &&
                    players[j].y + playerSize2 / 2.0f >= bullets[i].y)
                {
                    hit = true;

                    // TODO: Increase shooter's score
                    // TODO: Decrease hit person's life
                }
            }

            // Check for wall collision
            if (hit || bullets[i].x < -0.5f || bullets[i].x > xsize + 0.5f || bullets[i].y < -0.5f || bullets[i].y > ysize + 0.5f || 
                GetTile((int)(bullets[i].x + 0.5f), (int)(bullets[i].y + 0.5f)) == MapBlock.WALL)
            {
                bullets.RemoveAt(i);
                i--;
            }
        }

        framesPassed++;
    }
}


// Game has a number of Players
// Player implements Move()
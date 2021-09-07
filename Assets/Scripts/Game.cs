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
    public const float spf = 1 / 30.0f; // Seconds per frame
    const float gravity = 5.0f;
    const float playerJumpVelocity = 5.0f;
    const float playerMoveSpeed = 2.0f;
    const float bulletMoveSpeed = 10.0f; // previously 5

    const float reloadTime = 0.5f;
    const int frameReloadTime = (int)(reloadTime / spf);

    const float maxMatchTime = 50.0f; // Max 50sec matches

    public static float playerSize = 0.8f, playerSize2 = 1.0f;

    // The map is stored as a grid, where map[x][y] gives the block at (x, y)
    // (0, 0) is the bottom left.
    // All squares are centered, so this means there is a block with center at (0, 0)
    private MapBlock[,] map;
    public static int xsize { get; private set; } = 40;
    public static int ysize { get; private set; } = 10;

    public int framesPassed { get; private set; }

    public List<Bullet> bullets;
    public List<GenericPlayer> players { get; private set; }
    GameInput[] inputs;

    public static void SimulateGame(List<GenericPlayer> p)
    {
        Game g = new Game(p);
        while (!g.Step()) { }
    }

    public MapBlock GetTile(int x, int y)
    {
        if (x < 0 || x >= xsize || y < 0 || y >= ysize) return MapBlock.WALL;
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

        // map size 80
        // 4 players
        // 10, 30, 50, 70
        // walls at 20, 40, 60

        // Initiate players evenly spaced at (x, 1)

        List<int> spawnOrder = new List<int>();
        for (int i = 0; i < players.Count; i++) spawnOrder.Add(i);
        Util.Shuffle(spawnOrder);

        float spacing = xsize / players.Count;
        for (int i=0; i<players.Count; i++)
        {
            // Note ID is always index
            players[spawnOrder[i]].Spawn(spacing * (i+0.5f), 1.0f, i);

            // Walls between em
            if (i > 0)
            {
                map[(int)(spacing * i), 1] = MapBlock.WALL;
            }
        }
    }

    // Returns true iff the game has ended
    public bool Step()
    {
        bool gameOver = true;
        GenericPlayer winner = null;
        // Check if the game is over
        for (int i=0; i<players.Count; i++)
        {
            if (players[i].life > 0)
            {
                if (winner != null)
                {
                    gameOver = false;
                    break;
                }
                winner = players[i];
            }
        }

        // If the match has been going on for more than (maxMatchTime) seconds, it's over
        if (framesPassed > maxMatchTime / spf)
        {
            // TODO: Set all frameOfDeath
            gameOver = true;
            winner = null;
        }

        if (gameOver)
        {
            // TODO: Produce ranking for players
            // NOTE: Draws should go down, so if 4 people survive at the end then they are all 4th, one person was 5th
            if (winner != null) winner.frameOfDeath = framesPassed;
            return true;
        }

        // Get all players' inputs.
        // It's important this happens before any movement takes place, so that the order of the loop is irrelevant
        for (int i=0; i<players.Count; i++) {
            if (players[i].life <= 0) continue; // Ignore the dead
            inputs[i] = players[i].GetInput(this);
        }

        // Move players, collide with walls
        for (int i = 0; i < players.Count; i++) {
            if (players[i].life <= 0) continue; // Ignore the dead

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
                players[i].vy -= gravity * spf;
            }


            players[i].x += players[i].vx * spf;

            float xmin = players[i].x - playerSize2 / 2.0f;
            float xmax = players[i].x + playerSize2 / 2.0f;
            int x1 = (int)Math.Floor(xmin + 0.5f);             // Prevents edge case with (int)(-0.5) = 0
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

            players[i].y += players[i].vy * spf;

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


            // Shoot
            if (inputs[i].shoot)
            {
                if (players[i].lastShootFrame < framesPassed - frameReloadTime)
                {
                    players[i].lastShootFrame = framesPassed;

                    float vx = bulletMoveSpeed * (float)Math.Cos(inputs[i].shootAngle);
                    float vy = bulletMoveSpeed * (float)Math.Sin(inputs[i].shootAngle);

                    bullets.Add(new Bullet(players[i].gameID, players[i].x, players[i].y, vx, vy));

                    players[i].shotsFired++;
                }
            }

        }

        bool hit = false;

        // Move bullets, check for bullet collisions (with walls and players)
        for (int i=0; i<bullets.Count; i++)
        {
            bullets[i].vy -= gravity * spf;

            bullets[i].x += bullets[i].vx * spf;
            bullets[i].y += bullets[i].vy * spf;

            hit = false;

            for (int j=0; j<players.Count; j++)
            {
                if (players[j].gameID == bullets[i].shooterID
                    || players[j].life <= 0) continue; // Don't collide with shooter or dead people

                // Check for collision
                if (players[j].x - playerSize2 / 2.0f <= bullets[i].x &&
                    players[j].x + playerSize2 / 2.0f >= bullets[i].x &&
                    players[j].y - playerSize2 / 2.0f <= bullets[i].y &&
                    players[j].y + playerSize2 / 2.0f >= bullets[i].y)
                {
                    hit = true;

                    // Increment shooter's counter
                    players[bullets[i].shooterID].shotsHit += 1;

                    // Deal damage
                    players[j].life -= 1;

                    // If it was a kill, increase the shooter's counter
                    if (players[j].life <= 0)
                    {
                        players[j].frameOfDeath = framesPassed;
                        players[bullets[i].shooterID].playersKilled += 1;
                    }

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
        return false;
    }
}


// Game has a number of Players
// Player implements Move()
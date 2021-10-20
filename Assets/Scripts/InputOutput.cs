using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Takes a ShooterGame object and produces a list of input floats
public static class InputOutput
{
    // Inputs:
    //  1/0 encoding of a grid around the player
    public const int gridX = 1; // (on either side)
    public const int gridY = 1; // (on either side)
    public const int inputsGrid = (2 * gridX + 1) * (2 * gridY + 1);

    //  My info:
    //   - Position (2)
    //   - Health (1)
    //   - Velocity (2)
    //   - Num jumps (1)
    //   - JumpLast (1)
    public const int inputsMe = 7;

    //  Per player
    //   - Position              (2)
    //   - Health                (1)
    //   - dx, dy normalised     (2)
    //   - distance              (1)
    //   - velocity              (2)
    public const int numPlayers = 1;
    public const int inputsPerPlayer = 8;

    //  Per bullet
    //   - position            (2)
    //   - dx, dy normalised   (2)
    //   - distance            (1)
    //   - velocity            (2)
    public const int numBullets = 5;
    public const int inputsPerBullet = 7;

    public const int numInputs = inputsGrid
                               + inputsMe
                               + inputsPerPlayer * numPlayers
                               + inputsPerBullet * numBullets
                               + 1; // BONUS INPUT
    // Outputs:
    //  direction (2)
    //  is shooting (1)
    //  angle (1)
    //  jump (1) - Y/N
    public const int numOutputs = 6;

    private static float[] inputArr = new float[numInputs];
    private static float[] outputArr = new float[numOutputs];

    private static void SetGrid(int index, GenericPlayer p, ShooterGame game)
    {
        int xpos = (int)p.x;
        int ypos = (int)p.y;

        for (int i = xpos - gridX; i <= xpos + gridX; i++)
        {
            for (int j = ypos - gridY; j <= ypos + gridY; j++)
            {
                int num = (game.GetTile(i, j) == MapBlock.EMPTY ? 0 : 1);
                inputArr[index++] = num;
            }
        }
    }

    private static void SetMyPlayer(int index, GenericPlayer p, ShooterGame game)
    {
        inputArr[index]     = p.x / ShooterGame.xsize;
        inputArr[index + 1] = p.y / ShooterGame.ysize;          // Position
        inputArr[index + 2] = p.life / (float)GenericPlayer.maxlife;   // My life
        inputArr[index + 3] = p.vx;
        inputArr[index + 4] = p.vy;                       // Velocity
        inputArr[index + 5] = p.jumps / (float)GenericPlayer.numjumps;// Number of jumps remaining
        inputArr[index + 6] = p.jumpLast ? 1.0f : 0.0f;   // Was jump held last time? TODO: Add memory and remove
    }

    private static void SetPlayer(int index, GenericPlayer p, ShooterGame game, GenericPlayer q)
    {
        if (q != null)
        {
            float dx = q.x - p.x;
            float dy = q.y - p.y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            inputArr[index] = q.x / ShooterGame.xsize;
            inputArr[index + 1] = q.y / ShooterGame.ysize;          // Position
            inputArr[index + 2] = q.life / (float)GenericPlayer.maxlife;   // Health
            inputArr[index + 3] = dx / dist;
            inputArr[index + 4] = dy / dist;                 // dx, dy
            inputArr[index + 5] = dist / ShooterGame.xsize;         // dist
            inputArr[index + 6] = q.vx;
            inputArr[index + 7] = q.vy;                      // velocity
        }
        else
        {
            // No player: set life = 0. dist = 5
            inputArr[index + 2] = 0.0f;
            inputArr[index + 5] = 5.0f;
        }
    }

    private static void SetBullet(int index, GenericPlayer p, ShooterGame game, Bullet b)
    {
        if (b != null)
        {
            float dx = b.x - p.x;
            float dy = b.y - p.y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            inputArr[index]     = b.x / ShooterGame.xsize;
            inputArr[index + 1] = b.y / ShooterGame.ysize;          // Position
            inputArr[index + 2] = dx / dist;
            inputArr[index + 3] = dy / dist;                 // dx, dy
            inputArr[index + 4] = dist / ShooterGame.xsize;         // dist
            inputArr[index + 5] = b.vx;
            inputArr[index + 6] = b.vy;                      // velocity
        }
        else
        {
            // if null, set distance to 5 and leave the rest on 0
            inputArr[index + 4] = 5.0f;
        }
    }

    // Gets a float[]
    public static float[] GetInput(GenericPlayer p, ShooterGame game)
    {
        // Zero inputs
        // TODO: This is acc over the top a bit, we set most directly
        Array.Clear(inputArr, 0, numInputs);

        int offset = 0;

        SetGrid(offset, p, game);
        offset += inputsGrid;

        SetMyPlayer(offset, p, game);
        offset += inputsMe;

        // TODO: Sort players if we're doing more than 1v1s
        int count = 0;
        for (int i=0; count<numPlayers; i++)
        {
            if (i < game.players.Count)
            {
                if (game.players[i].gameID == p.gameID) continue;

                SetPlayer(offset, p, game, game.players[i]);
            }
            else
                SetPlayer(offset, p, game, null);
            offset += inputsPerPlayer;

            count++;
        }

        Debug.Assert(count == numPlayers, "Only counted " + count + " players, instead of " + numPlayers);
        Debug.Assert(offset == inputsGrid + inputsMe + inputsPerPlayer * numPlayers, "Incorrect offset");

        
        List<Bullet> bullets = new List<Bullet>();

        // Collect all bullets *not shot by us*
        foreach (Bullet b in game.bullets)
        {
            if (b.shooterID != p.gameID)
                bullets.Add(b);
        }

        // Sort the bullets by distance to us
        bullets.Sort(new BulletComparer(p));

        for (int i=0; i<numBullets; i++)
        {
            if (i < bullets.Count)
            {
                SetBullet(offset, p, game, bullets[i]);
                offset += inputsPerBullet;
            }
            else
            {
                SetBullet(offset, p, game, null);
                offset += inputsPerBullet;
            }
        }

        inputArr[inputArr.Length - 1] = 1.0f;

        Debug.Assert(offset == inputsGrid + inputsMe + inputsPerPlayer * numPlayers + inputsPerBullet * numBullets, "Incorrect offset");

        return inputArr;
    }

    public static GameInput GetOutput(float[] outputArr)
    {
        Debug.Assert(outputArr.Length == numOutputs, "Number of outputs given was " +
                                         outputArr.Length + " when it should have been " + numOutputs);

        float left  = outputArr[0];
        float right = outputArr[1];

        float shoot = outputArr[2];
        float dy = outputArr[3];
        float dx = outputArr[4];

        float jump = outputArr[5];

        // Does this even work
        float angle = Mathf.Atan2(dy, dx);

        bool bshoot = false,
             bjump = false;
        sbyte hor = 0;

        if (left > 1.0f) hor--;
        if (right > 1.0f) hor++;

        if (shoot > 1.0f) bshoot = true;
        if (jump > 1.0f) bjump = true;

        return new GameInput(hor, bjump, bshoot, angle);
    }
}

class BulletComparer : IComparer<Bullet>
{
    float cx, cy;

    public BulletComparer(GenericPlayer p)
    {
        cx = p.x;
        cy = p.y;
    }

    // TODO: Verify that this actually sorts the bullets, putting the closest ones by the player
    public int Compare(Bullet a, Bullet b)
    {
        float d1 = (a.x - cx) * (a.x - cx) + (a.y - cy) * (a.y - cy);
        float d2 = (b.x - cx) * (b.x - cx) + (b.y - cy) * (b.y - cy);
        return d1.CompareTo(d2);
    }
}
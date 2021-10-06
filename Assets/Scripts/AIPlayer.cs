using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// An abstract class for AI players
public class AIPlayer : GenericPlayer
{
    // Inputs:
    //  One hot encoding of a 5x5 grid around me (25 squares * 2 types = 50)

    //  My info:
    //   - Position (2)
    //   - Health (1)
    //   - Velocity (2)
    //   - On floor (1)
    //   - Num jumps (1)
    //   - JumpLast (1)

    //  Per player (reserved for FFA_size-1):
    //   - Position              (2)
    //   - Is alive              (1)
    //   - Health                (1)
    //   - dx, dy normalised     (2)
    //   - distance              (1)
    //   - velocity              (2)

    // Total of 8 * (FFA_size - 1)

    //  Per bullet
    //   - Is alive            (1)
    //   - position            (2)
    //   - dx, dy normalised   (2)
    //   - distance            (1)
    //   - velocity            (2)

    // Applied on either side, so (2,2) means a 5x5 grid
    static readonly int gridX = 2;
    static readonly int gridY = 2;

    static readonly int inputsGrid = (2*gridX+1) * (2*gridY+1);
    static readonly int inputsMe = 8;
    static readonly int inputsPerPlayer = 9;
    static readonly int inputsPerBullet = 8;

    static readonly int maxNumPlayers = 5;
    static readonly int numPlayers = Math.Min(RankedGenetic.FFA_size - 1, maxNumPlayers);
    static readonly int numBullets = 5;

    static readonly int numInputs = inputsGrid + inputsMe + inputsPerPlayer * numPlayers + inputsPerBullet * numBullets;

    // Outputs:
    //  left right (2)
    //  shoot (1)
    //  angle (2)
    //  jump (1)
    const int numOutputs = 6;

    // Input every 4 frames (that is, 7 times a sec)
    const int whichFrameInput = 4;

    Pair<float, int>[] playerSorter;

    NeuralNet mnet;
    GameInput prevInput = new GameInput(0, false, false, 0.0f);

    float[] inputArr, outputArr;

    public static AIPlayer MakeLayeredAIPlayer()
    {
        int[] levels = new int[] { numInputs, 15, numOutputs };
        int[] linear = new int[] { 0, 4, 0 };

        return new AIPlayer(new LayeredNeuralNet(levels, linear));
    }

    public AIPlayer(NeuralNet brain)
    {
        inputArr = new float[numInputs];

        mnet = brain;

        playerSorter = new Pair<float, int>[RankedGenetic.FFA_size - 1];
        for (int i = 0; i < RankedGenetic.FFA_size - 1; i++) playerSorter[i] = new Pair<float, int>(float.PositiveInfinity, -1);
    }

    public override GameInput GetInput(Game game)
    {
        if (game.framesPassed % whichFrameInput != 0)
            return prevInput;

        // Zero inputs
        // TODO: This is acc over the top a bit, we set most directly
        Array.Clear(inputArr, 0, numInputs);

        int offset = 0;

        // Grid
        {
            int xpos = (int)x;
            int ypos = (int)y;
            for (int i = xpos - gridX; i <= xpos + gridX; i++)
            {
                for (int j = ypos - gridY; j <= ypos + gridY; j++)
                {
                    int num = (game.GetTile(i, j) == MapBlock.EMPTY ? 0 : 1);

                    int index = (i - xpos + gridX) * (gridY * 2 + 1) + (j - ypos + gridY);

                    inputArr[index + offset] = 1.0f;
                }
            }

            offset += inputsGrid;
        }

        // Myself
        {
            inputArr[offset]     = x / Game.xsize;
            inputArr[offset + 1] = y / Game.ysize;          // Position
            inputArr[offset + 2] = life / (float)maxlife;   // My life
            inputArr[offset + 3] = vx;
            inputArr[offset + 4] = vy;                      // Velocity
            inputArr[offset + 5] = onFloor ? 1.0f : 0.0f;   // On floor
            inputArr[offset + 6] = jumps / (float)numjumps; // Number of jumps remaining
            inputArr[offset + 7] = jumpLast ? 1.0f : 0.0f;  // Was jump held last time? TODO: Add memory and remove

            offset += inputsMe;
        }

        // Players
        {
            int j = 0;
            for (int i = 0; i < RankedGenetic.FFA_size; i++)
            {
                if (game.players[i].gameID != gameID)
                {
                    if (game.players[i].life > 0)
                    {
                        playerSorter[j].fst = Vector2.Distance(new Vector2(x, y),
                            new Vector2(game.players[i].x, game.players[i].y));
                    }
                    else
                    {
                        // It's dead: sort it to the end
                        playerSorter[j].fst = float.PositiveInfinity;
                    }
                    playerSorter[j].snd = i;
                    j++;
                }
            }
            Array.Sort(playerSorter);

            //  Per player (reserved for FFA_size-1):
            //   - Is alive              (1)
            //   - Health                (1)
            //   - Position              (2)
            //   - dx, dy normalised     (2)
            //   - distance              (1)
            //   - velocity              (2)
            for (int i = 0; i < numPlayers; i++)
            {
                float dist = playerSorter[i].fst;
                GenericPlayer p = game.players[playerSorter[i].snd];
                if (p.life > 0)
                {
                    float dx = p.x - x;
                    float dy = p.y - y;

                    inputArr[offset] = 1.0f;                      // Is alive
                    inputArr[offset + 1] = p.life / (float)maxlife;   // Health
                    inputArr[offset + 2] = p.x / Game.xsize;
                    inputArr[offset + 3] = p.y / Game.ysize;          // Position
                    inputArr[offset + 4] = dx / dist;
                    inputArr[offset + 5] = dy / dist;                 // dx, dy
                    inputArr[offset + 6] = dist;                      // dist
                    inputArr[offset + 7] = p.vx;
                    inputArr[offset + 8] = p.vy;                      // velocity
                }
                else
                {
                    inputArr[offset] = 0.0f;
                    //inputArr[offset + 6] = 100.0f; Set distance to very large
                }

                offset += inputsPerPlayer;
            }
        }


        // Bullets
        {
            List<Pair<float, int>> bulletSort = new List<Pair<float,int>>();

            for (int i = 0; i < game.bullets.Count; i++)
            {
                if (game.bullets[i].shooterID == gameID) continue; // Don't see my own bullets, as this isn't much use

                float dist = Vector2.Distance(new Vector2(x, y),
                    new Vector2(game.bullets[i].x, game.bullets[i].y));

                bulletSort.Add(new Pair<float, int>(dist, i));
            }
            bulletSort.Sort();

            //  Per bullet
            //   - Is alive            (1)
            //   - position            (2)
            //   - dx, dy normalised   (2)
            //   - distance            (1)
            //   - velocity            (2)
            for (int i = 0; i < numBullets; i++)
            {
                if (i < bulletSort.Count)
                {
                    float dist = bulletSort[i].fst;
                    Bullet p = game.bullets[bulletSort[i].snd];

                    float dx = p.x - x;
                    float dy = p.y - y;

                    inputArr[offset] = 1.0f;                          // Is alive
                    inputArr[offset + 1] = p.x / Game.xsize;
                    inputArr[offset + 2] = p.y / Game.ysize;          // Position
                    inputArr[offset + 3] = dx / dist;
                    inputArr[offset + 4] = dy / dist;                 // dx, dy
                    inputArr[offset + 5] = dist;                      // dist
                    inputArr[offset + 6] = p.vx;
                    inputArr[offset + 7] = p.vy;                      // velocity
                }
                else
                {
                    inputArr[offset] = 0.0f;
                }

                offset += inputsPerBullet;
            }
        }

        outputArr = mnet.Evaluate(inputArr);

        // Outputs:
        //  left (1)
        //  shoot (1)
        //  angle (2)
        //  jump (1)

        // All in the range -1 ... 1
        float left   = outputArr[0];
        float right  = outputArr[1];
        float shoot  = outputArr[2];
        float jump   = outputArr[3];
        float anglex = outputArr[4];
        float angley = outputArr[5];

        float angle = Mathf.Atan2(angley, anglex);

        bool bshoot = false,
             bjump = false;
        sbyte hor = 0;

        if (left > 1.0f) hor--;
        if (right > 1.0f) hor++;

        if (shoot > 1.0f) bshoot = true;
        if (jump > 1.0f) bjump = true;

        GameInput g = new GameInput(hor, bjump, bshoot, angle);
        prevInput = g;
        return g;
    }


    public AIPlayer BreedPlayer(AIPlayer otherParent)
    {
        return new AIPlayer(mnet.Breed(otherParent.mnet));
    }
}

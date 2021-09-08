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
    const int gridX = 2;
    const int gridY = 2;

    const int inputsGrid = (2*gridX+1) * (2*gridY+1);
    const int inputsMe = 6;
    const int inputsPerPlayer = 9;
    const int inputsPerBullet = 8;

    const int numPlayers = RankedGenetic.FFA_size - 1;
    const int numBullets = 5;

    const int numInputs = inputsGrid + inputsMe + inputsPerPlayer * numPlayers + inputsPerBullet * numBullets;

    // Outputs:
    //  left right (2)
    //  shoot (1)
    //  angle (2)
    //  jump (1)
    const int numOutputs = 6;

    //static int[] levels = new int[]{ numInputs, 15, numOutputs };
    static int[] levels = new int[] { numInputs, 16, 10, numOutputs };
    static int[] linear = new int[] { 0, 4, 3, 0 };

    // Input every 4 frames (that is, 7 times a sec)
    const int whichFrameInput = 4;

    Pair<float, int>[] playerSorter;

    NeuralNet mnet;
    GameInput prevInput = new GameInput(0, false, false, 0.0f);

    public AIPlayer() : this(new NeuralNet(levels, linear))
    {
    }

    public AIPlayer(NeuralNet brain)
    {
        mnet = brain;

        playerSorter = new Pair<float, int>[numPlayers];
        for (int i = 0; i < numPlayers; i++) playerSorter[i] = new Pair<float, int>(float.PositiveInfinity, -1);
    }

    public override GameInput GetInput(Game game)
    {
        if (game.framesPassed % whichFrameInput != 0)
            return prevInput;

        // Zero inputs
        // TODO: This is acc over the top a bit, we set most directly
        Array.Clear(mnet.nodes[0], 0, numInputs);

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

                    mnet.nodes[0][index + offset] = 1.0f;
                }
            }

            offset += inputsGrid;
        }

        // Myself
        {
            // Player inputs: position, health, velocity
            //   - Position (2)
            //   - Health (1)
            //   - Velocity (2)
            mnet.nodes[0][offset]     = x / Game.xsize;
            mnet.nodes[0][offset + 1] = y / Game.ysize;
            mnet.nodes[0][offset + 2] = life / (float)maxlife;
            mnet.nodes[0][offset + 3] = vx;
            mnet.nodes[0][offset + 4] = vy;
            mnet.nodes[0][offset + 5] = onFloor?1.0f:0.0f;

            offset += inputsMe;
        }

        // Players
        {
            int j = 0;
            for (int i = 0; i < game.players.Count; i++)
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
            for (int i = 0; i < playerSorter.Length; i++)
            {
                float dist = playerSorter[i].fst;
                GenericPlayer p = game.players[playerSorter[i].snd];
                if (p.life > 0)
                {
                    float dx = p.x - x;
                    float dy = p.y - y;

                    mnet.nodes[0][offset] = 1.0f;                      // Is alive
                    mnet.nodes[0][offset + 1] = p.life / (float)maxlife;   // Health
                    mnet.nodes[0][offset + 2] = p.x / Game.xsize;
                    mnet.nodes[0][offset + 3] = p.y / Game.ysize;          // Position
                    mnet.nodes[0][offset + 4] = dx / dist;
                    mnet.nodes[0][offset + 5] = dy / dist;                 // dx, dy
                    mnet.nodes[0][offset + 6] = dist;                      // dist
                    mnet.nodes[0][offset + 7] = p.vx;
                    mnet.nodes[0][offset + 8] = p.vy;                      // velocity
                }
                else
                {
                    mnet.nodes[0][offset] = 0.0f;
                    //mnet.nodes[0][offset + 6] = 100.0f; Set distance to very large
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

                    mnet.nodes[0][offset] = 1.0f;                          // Is alive
                    mnet.nodes[0][offset + 1] = p.x / Game.xsize;
                    mnet.nodes[0][offset + 2] = p.y / Game.ysize;          // Position
                    mnet.nodes[0][offset + 3] = dx / dist;
                    mnet.nodes[0][offset + 4] = dy / dist;                 // dx, dy
                    mnet.nodes[0][offset + 5] = dist;                      // dist
                    mnet.nodes[0][offset + 6] = p.vx;
                    mnet.nodes[0][offset + 7] = p.vy;                      // velocity
                }
                else
                {
                    mnet.nodes[0][offset] = 0.0f;
                }

                offset += inputsPerBullet;
            }
        }

        mnet.Calculate();

        // Outputs:
        //  left (1)
        //  shoot (1)
        //  angle (2)
        //  jump (1)

        // All in the range -1 ... 1
        float left   = mnet.nodes[mnet.nodes.Length - 1][0];
        float right  = mnet.nodes[mnet.nodes.Length - 1][1];
        float shoot  = mnet.nodes[mnet.nodes.Length - 1][2];
        float jump   = mnet.nodes[mnet.nodes.Length - 1][3];
        float anglex = mnet.nodes[mnet.nodes.Length - 1][4];
        float angley = mnet.nodes[mnet.nodes.Length - 1][5];

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


    const float mutateChance = 0.1f;
    const float mutateAmount = 2.0f;
    public AIPlayer Breed(AIPlayer otherParent)
    {
        NeuralNet p = new NeuralNet(levels, linear);
        float a;
        for (int i=0; i<p.weights.Length; i++)
        {
            for (int j=0; j<levels[i]; j++)
            {
                for (int k=0; k<levels[i+1]; k++)
                {
                    a = UnityEngine.Random.Range(0.0f, 1.0f);
                    p.weights[i][j, k] = mnet.weights[i][j, k] * a + otherParent.mnet.weights[i][j, k] * (1 - a);

                    if (UnityEngine.Random.Range(0.0f, 1.0f) < mutateChance)
                        p.weights[i][j, k] += UnityEngine.Random.Range(-mutateAmount, mutateAmount);
                }
            }
        }

        for (int i=0; i<p.bias.Length; i++)
        {
            for (int j=0; j<levels[i]; j++)
            {
                a = UnityEngine.Random.Range(0.0f, 1.0f);
                p.bias[i][j] = mnet.bias[i][j] * a + otherParent.mnet.bias[i][j] * (1 - a);

                if (UnityEngine.Random.Range(0.0f, 1.0f) < mutateChance)
                    p.bias[i][j] += UnityEngine.Random.Range(-mutateAmount, mutateAmount);
            }
        }

        // TODO: Node biases

        return new AIPlayer(p);
    }
}

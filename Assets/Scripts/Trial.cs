using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A trial modifies the Game every step before input is given
public abstract class Trial
{
    public static readonly Trial[] trials = new Trial[] { new TargetPractice(2, true), new TargetPractice(4, true), new TargetPractice(7, true)};

    protected System.Random r;

    protected static void ZeroGame(Game g)
    {
        for (int x = 0; x < Game.xsize; x++)
        {
            for (int y = 1; y < Game.ysize; y++)
            {
                g.SetTile(x, y, MapBlock.EMPTY);
            }
        }
    }

    public abstract Game CreateTrial(GenericPlayer p);  // Note: trials are one player (but could change)
    public abstract bool Apply(Game g);                 // Modify the Game object, return true iff trial over
    public abstract float GetScore(Game g);             // Get the score of the player in the trial
}

public class TargetPractice : Trial
{
    float x, y;
    int hits, seed;

    float totWaitTime, lastHitTime;
    public bool zeroMap;

    const float trialTime = 30.0f;

    class TargetPlayer : GenericPlayer
    {
       public override GameInput GetInput(Game game) { return GameInput.nothing; }
    }

    public TargetPractice(int seed, bool zeroMap)
    {
        this.seed = seed;
        this.zeroMap = zeroMap;
    }

    // Just changes x, y vars
    private void MovePosition()
    {
        float xmin = 1.0f,
              xmax = Game.xsize - 1,
              ymin = 1.0f,
              ymax = Game.ysize - 1;

        ymax *= Mathf.Min(1.0f, (hits + 1) / 12.0f); 

        x = (float)r.NextDouble() * (xmax - xmin) + xmin;
        y = (float)r.NextDouble() * (ymax - ymin) + ymin;
    }

    public override bool Apply(Game g)
    {
        if (g.players[1].life != GenericPlayer.maxlife)
        {
            float time = g.framesPassed * Game.spf;
            totWaitTime += 1.0f / (time - lastHitTime + 1.0f); 
            lastHitTime = time;

            hits += GenericPlayer.maxlife - g.players[1].life;

            MovePosition();

            g.players[1].life = GenericPlayer.maxlife;
        }

        g.players[1].vx = 0.0f;
        g.players[1].vy = 0.0f;
        g.players[1].x = x;
        g.players[1].y = y;

        return g.framesPassed * Game.spf > trialTime;
    }

    public override Game CreateTrial(GenericPlayer p)
    {
        r = new System.Random(seed);
        hits = 0;
        totWaitTime = 0.0f;
        lastHitTime = 0.0f;

        TargetPlayer t = new TargetPlayer();
        Game g = new Game(new List<GenericPlayer>() { p, t }, this);

        if (zeroMap)
            ZeroGame(g);

        // Spawn the player randomly!
        MovePosition();
        p.x = x;
        p.y = y;

        MovePosition();

        return g;
    }

    public override float GetScore(Game g)
    {
        // Note: totFrames used to be g.framesPassed but this encouraged the AI to kill themselves as soon as they could

        float maxHits = trialTime / Game.reloadTime;

        return hits / maxHits + totWaitTime / 100.0f;
    }
}



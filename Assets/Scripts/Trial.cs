using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A trial modifies the ShooterGame every step before input is given
public abstract class Trial
{
    public static readonly Trial[] trials = new Trial[] { new TargetPractice(2, true), new TargetPractice(4, true), new TargetPractice(7, true)};

    protected System.Random r;

    protected static void ZeroGame(ShooterGame g)
    {
        for (int x = 0; x < ShooterGame.xsize; x++)
        {
            for (int y = 1; y < ShooterGame.ysize; y++)
            {
                g.SetTile(x, y, MapBlock.EMPTY);
            }
        }
    }

    public abstract ShooterGame CreateTrial(GenericPlayer p);  // Note: trials are one player (but could change)
    public abstract bool Apply(ShooterGame g);                 // Modify the ShooterGame object, return true iff trial over
    public abstract float GetScore(ShooterGame g);             // Get the score of the player in the trial
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
       public override GameInput GetInput(ShooterGame game) { return GameInput.nothing; }
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
              xmax = ShooterGame.xsize - 1,
              ymin = 1.0f,
              ymax = ShooterGame.ysize - 1;

        ymax *= Mathf.Min(1.0f, (hits + 1) / 12.0f); 

        x = (float)r.NextDouble() * (xmax - xmin) + xmin;
        y = (float)r.NextDouble() * (ymax - ymin) + ymin;
    }

    public override bool Apply(ShooterGame g)
    {
        if (g.players[1].life != GenericPlayer.maxlife)
        {
            float time = g.framesPassed * ShooterGame.spf;
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

        return g.framesPassed * ShooterGame.spf > trialTime;
    }

    public override ShooterGame CreateTrial(GenericPlayer p)
    {
        r = new System.Random(seed);
        hits = 0;
        totWaitTime = 0.0f;
        lastHitTime = 0.0f;

        TargetPlayer t = new TargetPlayer();
        ShooterGame g = new ShooterGame(new List<GenericPlayer>() { p, t }, this);

        if (zeroMap)
            ZeroGame(g);

        // Spawn the player randomly!
        MovePosition();
        p.x = x;
        p.y = y;

        MovePosition();

        return g;
    }

    public override float GetScore(ShooterGame g)
    {
        // Note: totFrames used to be g.framesPassed but this encouraged the AI to kill themselves as soon as they could

        float maxHits = trialTime / ShooterGame.reloadTime;

        return hits / maxHits + totWaitTime / 100.0f;
    }
}



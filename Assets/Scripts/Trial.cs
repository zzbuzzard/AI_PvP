using System;
using System.Collections;
using System.Collections.Generic;

// A trial modifies the Game every step before input is given
public abstract class Trial
{
    public static readonly Trial[] trials = new Trial[] { new TargetPractice(2, true), new TargetPractice(4, true), new TargetPractice(7, true)};

    protected System.Random r;

    protected static void ZeroGame(ShootGame g)
    {
        for (int x = 0; x < ShootGame.xsize; x++)
        {
            for (int y = 1; y < ShootGame.ysize; y++)
            {
                g.SetTile(x, y, MapBlock.EMPTY);
            }
        }
    }

    public abstract ShootGame CreateTrial(GenericPlayer p);  // Note: trials are one player (but could change)
    public abstract bool Apply(ShootGame g);                 // Modify the Game object, return true iff trial over
    public abstract float GetScore(ShootGame g);             // Get the score of the player in the trial
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
        private static float[] nothing = new float[Constants.numOutputs];
       public override float[] GetOutput(Game game, float[] input) { return nothing; }
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
              xmax = ShootGame.xsize - 1,
              ymin = 1.0f,
              ymax = ShootGame.ysize - 1;

        ymax *= Math.Min(1.0f, (hits + 1) / 12.0f); 

        x = (float)r.NextDouble() * (xmax - xmin) + xmin;
        y = (float)r.NextDouble() * (ymax - ymin) + ymin;
    }

    public override bool Apply(ShootGame g)
    {
        if (g.info[1].life != ShootGame.PlayerInfo.maxlife)
        {
            float time = g.framesPassed * ShootGame.spf;
            totWaitTime += 1.0f / (time - lastHitTime + 1.0f); 
            lastHitTime = time;

            hits += ShootGame.PlayerInfo.maxlife - g.info[1].life;

            MovePosition();

            g.info[1].life = ShootGame.PlayerInfo.maxlife;
        }

        g.info[1].vx = 0.0f;
        g.info[1].vy = 0.0f;
        g.info[1].x = x;
        g.info[1].y = y;

        return g.framesPassed * ShootGame.spf > trialTime;
    }

    public override ShootGame CreateTrial(GenericPlayer p)
    {
        r = new System.Random(seed);
        hits = 0;
        totWaitTime = 0.0f;
        lastHitTime = 0.0f;

        TargetPlayer t = new TargetPlayer();
        ShootGame g = new ShootGame(new GenericPlayer[] { p, t }, this);

        // We are index 0

        if (zeroMap)
            ZeroGame(g);

        // Spawn the player randomly!
        MovePosition();
        g.info[0].x = x;
        g.info[1].y = y;

        MovePosition();

        return g;
    }

    public override float GetScore(ShootGame g)
    {
        // Note: totFrames used to be g.framesPassed but this encouraged the AI to kill themselves as soon as they could

        float maxHits = trialTime / ShootGame.reloadTime;

        return hits / maxHits + totWaitTime / 100.0f;
    }
}



using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankGame : Game
{
    public class TankGameDrawer : GameDrawer
    {
        TankGame g;

        List<GameObject> tankObjs;
        GameObject goalObj;

        public static readonly GameObject tankPrefab;
        public static readonly GameObject goalPrefab;

        static TankGameDrawer()
        {
            tankPrefab = Resources.Load<GameObject>("Prefabs/TankTank");
            goalPrefab = Resources.Load<GameObject>("Prefabs/TankGoal");
        }

        public TankGameDrawer(TankGame g, MonoBehaviour m) : base(m)
        {
            this.g = g;

            tankObjs = new List<GameObject>();
            for (int i=0; i<g.playerObjs.Length; i++)
            {
                tankObjs.Add(MonoBehaviour.Instantiate(tankPrefab));
            }
            goalObj = MonoBehaviour.Instantiate(goalPrefab);
        }

        public override void Draw()
        {
            for (int i=0; i<tankObjs.Count; i++)
            {
                tankObjs[i].transform.position = Translate(g.playerObjs[i].location);
                tankObjs[i].transform.rotation = Quaternion.Euler(0, 0, g.playerObjs[i].angle);
            }

            goalObj.transform.position = Translate(g.goal);
        }

        public override void Cleanup()
        {
            // Clean up previous simulation
            foreach (GameObject x in tankObjs) MonoBehaviour.Destroy(x);
            MonoBehaviour.Destroy(goalObj);

            tankObjs.Clear();
        }

        // Game coords to world coords
        public static Vector2 Translate(Vector2 pos)
        {
            return pos;
        }
    }

    private Vector2 goal;
    private System.Random random;

    private PhysicsSystem physicsSystem;
    float maxX = 5;
    float maxY = 5;

    private static float dt = 0.01f;
    private static float spf = 1 / 30.0f;
    private int physSteps = (int)(spf / dt);

    private PhysObject[] playerObjs;
    private const float tankMass = 1.0f;

    public TankGame(GenericPlayer player, int seed) : base(new GenericPlayer[] {player})
    {
        this.maxMatchTime = 30.0f;
        physicsSystem = new PhysicsSystem();

        playerObjs = new PhysObject[players.Length];

        // Populate player objects with tank physics objects
        for (int i=0; i< playerObjs.Length; i++)
        {
            playerObjs[i] = new PhysObject(tankMass, new Vector2(i * 10, 0));
            physicsSystem.AddObject(playerObjs[i]);
        }

        random = new System.Random(seed);
        goal = new Vector2(0, 0);

        UpdateGoal();
    }

    int goalsScored = 0;
    float bonusTime = 0.0f;
    float lastTime = 0.0f;

    float increase_time = 10.0f;
    const float time_per_goal = 9.0f;

    private void UpdateGoal()
    {
        if (framesPassed > 0)
        {
            goalsScored++;
            float time = framesPassed * spf;
            bonusTime += (time - lastTime) * (time - lastTime);
            lastTime = time;

            maxMatchTime += increase_time;
            increase_time *= 0.8f;
        }

        float x = (float)random.NextDouble();
        x *= maxX * 2;
        x -= maxX;
        float y = (float)random.NextDouble();
        y *= maxY * 2;
        y -= maxY;

        goal.x = x;
        goal.y = y;        
    }

    public override GameDrawer GetDrawer(MonoBehaviour m)
    {
        return new TankGameDrawer(this, m);
    }

    public const int numInputs = 8;
    public const int numOutputs = 2;
    private static float[] inputArr = new float[numInputs];
    private static float[] outputArr;
    

    public override float[] GetInput(int i)
    {
        PhysObject p = playerObjs[i];

        // All inputs set directly, no need to zero the array

        Vector2 off = goal - p.location;

        // Offset information
        inputArr[0] = off.x;
        inputArr[1] = off.y;
        inputArr[2] = off.magnitude;
        inputArr[3] = Mathf.Atan2(off.y, off.x);  // -pi to pi

        // My angle and angular momentum
        inputArr[4] = p._angle;
        inputArr[5] = p.spinSpeed;

        inputArr[6] = p.velocity.magnitude;
        inputArr[7] = 1.0f;

        return inputArr;
    }

    public override float GetScore(int i)
    {
        return 100 * goalsScored + 0.01f / (0.01f + Vector2.SqrMagnitude(playerObjs[i].location - goal));
    }

    const float goal_hit_threshold = 0.5f;

    public override bool Step()
    {
        base.Step();
        for (int i=0; i<players.Length; i++)
        {
            PhysObject p = playerObjs[i];

            if((goal - p.location).magnitude < goal_hit_threshold){
                UpdateGoal();
            }
            outputArr = players[i].GetOutput(this, GetInput(i));

            Force leftForce = new Force(Vector2.left, Vector2.up * 30.0f * Mathf.Clamp(outputArr[0],-1.0f, 1.0f));
            p.AddForce(leftForce);

            Force rightForce = new Force(Vector2.right, Vector2.up * 30.0f * Mathf.Clamp(outputArr[1], -1.0f, 1.0f));
            p.AddForce(rightForce);
        }

        //TODO more physics steps?
        for (int i=0; i<1; i++)
        {
            physicsSystem.Step(dt);
        }

        if (framesPassed * spf - lastTime > time_per_goal)
        {
            return true;
        }

        if (framesPassed * spf > maxMatchTime)
        {
            return true;
        }
        return false;
    }

}

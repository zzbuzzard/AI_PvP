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

        public TankGameDrawer(TankGame g)
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
                tankObjs[i].transform.position = GameToWorld(g.playerObjs[i].location);
                tankObjs[i].transform.rotation = Quaternion.Euler(0, 0, g.playerObjs[i].angle);
            }

            goalObj.transform.position = GameToWorld(g.goal);
        }

        public override void Cleanup()
        {
            // Clean up previous simulation
            foreach (GameObject x in tankObjs) MonoBehaviour.Destroy(x);
            MonoBehaviour.Destroy(goalObj);

            tankObjs.Clear();
        }

        public override Vector2 GameToWorld(Vector2 pos)
        {
            return pos;
        }

        public override Vector2 WorldToGame(Vector2 pos)
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
        this.maxMatchTime = 15.0f;
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

    float goal_base_dist = 0.0f;
    Vector2 prevGoal = Vector2.zero;

    float increase_time = 7.0f;
    public static float time_per_goal = 100.0f;

    const float min_goal_dist_sq = 5.0f * 5.0f;
    const float max_goal_dist_sq = 12.0f * 12.0f;

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

        for (int i=0; i<10; i++)
        {
            Vector2 goal2 = new Vector2((float)random.NextDouble() * maxX * 2 - maxX,
                                        (float)random.NextDouble() * maxY * 2 - maxY);

            float d = Vector2.SqrMagnitude(goal2 - goal);
            if (d >= min_goal_dist_sq && d <= max_goal_dist_sq)
            {
                prevGoal = goal;
                goal = goal2;
                goal_base_dist = Vector2.Distance(prevGoal, goal);
                return;
            }
        }

        // nothing was found, boo
        prevGoal = goal;
        goal = new Vector2((float)random.NextDouble() * maxX * 2 - maxX,
                           (float)random.NextDouble() * maxY * 2 - maxY);
        goal_base_dist = Vector2.Distance(prevGoal, goal);
    }

    public override GameDrawer GetDrawer()
    {
        return new TankGameDrawer(this);
    }

    public const int numInputs = 13;
    public const int numOutputs = 2;
    private static float[] inputArr = new float[numInputs];
    private static float[] outputArr;
    

    public override float[] GetInput(int i)
    {
        PhysObject p = playerObjs[i];

        // All inputs set directly, no need to zero the array

        Vector2 off = goal - p.location;

        float off_angle  = Mathf.Atan2(off.y, off.x);
        float vel_angle  = Mathf.Atan2(p.velocity.y, p.velocity.x);
        float loc_angle  = Mathf.Atan2(p.location.y, p.location.x);

        float diff_ang       = Mathf.DeltaAngle(p.angle,                   off_angle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        float diff_to_centre = Mathf.DeltaAngle(p.angle,                   loc_angle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        float diff_vel       = Mathf.DeltaAngle(vel_angle * Mathf.Rad2Deg, off_angle * Mathf.Rad2Deg) * Mathf.Deg2Rad;

        //inputArr[3] = Mathf.Cos(p._angle);
        //inputArr[4] = Mathf.Sin(p._angle);
        //inputArr[6] = p._angle;

        // Offset: magnitude, angle, angle to turn
        inputArr[0] = off.x / off.magnitude;
        inputArr[1] = off.y / off.magnitude;
        inputArr[2] = off_angle;
        inputArr[3] = diff_ang;

        // My angle, and angle to centre
        inputArr[4] = p._angle;
        inputArr[5] = diff_to_centre;

        // My velocity, velocity angle to center
        inputArr[6] = vel_angle;
        inputArr[7] = p.velocity.magnitude;
        inputArr[8] = diff_vel;

        inputArr[9] = p.location.magnitude;
        inputArr[10] = loc_angle;
    
        inputArr[11] = p.spinSpeed;
        inputArr[12] = 1.0f;
        
        return inputArr;
    }

    public override float GetScore(int i)
    {
        return goalsScored + Mathf.Max((1.0f - Vector2.Distance(playerObjs[i].location, goal) / goal_base_dist), 0);
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

            Force leftForce = new Force(Vector2.left, Vector2.up * 30.0f * (float)Math.Tanh(outputArr[0]));
            p.AddForce(leftForce);

            Force rightForce = new Force(Vector2.right, Vector2.up * 30.0f * (float)Math.Tanh(outputArr[1]));
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

    public void SetGoal(Vector2 g)
    {
        goal = g;
    }
}

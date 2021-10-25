using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankGame : Game
{
    private Vector2 goal;
    private System.Random random;

    private PhysicsSystem physicsSystem;
    float maxX = 10;
    float maxY = 10;

    private static float dt = 0.01f;
    private static float spf = 1 / 30;
    private int physSteps = (int)(spf / dt);

    private PhysObject[] playerObjs;
    private const float tankMass = 1.0f;

    public TankGame(GenericPlayer player, int seed) : base(new GenericPlayer[] {player})
    {
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
    private void UpdateGoal()
    {
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
        throw new System.NotImplementedException();
    }

    public const int numInputs = 9;
    public const int numOutputs = 2;
    private static float[] inputArr = new float[numInputs];
    private static float[] outputArr = new float[numOutputs];
    

    public override float[] GetInput(int i)
    {
        PhysObject p = playerObjs[i];

        // All inputs set directly, no need to zero the array

        inputArr[0] = p.location.x;
        inputArr[1] = p.location.y;
        inputArr[2] = goal.x;
        inputArr[3] = goal.y;
        inputArr[4] = p.angle;
        inputArr[5] = p.spinSpeed;
        inputArr[6] = (goal - p.location).magnitude;
        inputArr[7] = Vector2.Angle(Vector2.up, goal) - p.angle;
        inputArr[8] = 1.0f;

        return inputArr;
    }

    public override float GetScore(int i)
    {
        return goalsScored + 100 / framesPassed;
    }

    public override bool Step()
    {
        inputArr = GetInput(0);
        for (int i=0; i<players.Length; i++)
        {
            PhysObject p = playerObjs[i];

            if((goal - p.location).magnitude < 0.1f){
                goalsScored++;
                UpdateGoal();
            }
            outputArr = players[i].GetOutput(this, inputArr);

            Force leftForce = new Force(Vector2.left, Vector2.up * outputArr[0]);
            p.AddForce(leftForce);

            Force rightForce = new Force(Vector2.right, Vector2.up * outputArr[0]);
            p.AddForce(rightForce);
        }

        //TODO more physics steps?
        for(int i=0; i<1; i++)
        {
            physicsSystem.Step(dt);
        }

        base.Step();
        if(framesPassed * spf > maxMatchTime)
        {
            return true;
        }
        return false;
    }

}

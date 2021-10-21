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


    public TankGame(GenericPlayer player, int seed) : base(new GenericPlayer[] {player})
    {
        physicsSystem = new PhysicsSystem();
        TankPlayer t = (TankPlayer)player;
        physicsSystem.AddObject(t.physicsObject);
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

    const int numInputs = 9;
    const int numOutputs = 2;
    private static float[] inputArr = new float[numInputs];
    private static float[] outputArr = new float[numOutputs];
    

    public override float[] GetInput(int i)
    {
        // Zero inputs
        // TODO: This is acc over the top a bit, we set most directly
        Array.Clear(inputArr, 0, numInputs);
        TankPlayer t = (TankPlayer)players[i];

        inputArr[0] = t.physicsObject.location.x;
        inputArr[1] = t.physicsObject.location.y;
        inputArr[2] = goal.x;
        inputArr[3] = goal.y;
        inputArr[4] = t.physicsObject.angle;
        inputArr[5] = t.physicsObject.spinSpeed;
        inputArr[6] = (goal - t.physicsObject.location).magnitude;
        inputArr[7] = Vector2.Angle(Vector2.up, goal) - t.physicsObject.angle;
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
        foreach(TankPlayer player in players)
        {
            if((goal - player.physicsObject.location).magnitude < 0.1f){
                goalsScored++;
                UpdateGoal();
            }
            outputArr = player.GetOutput(this, inputArr);

            Force leftForce = new Force(Vector2.left, Vector2.up * outputArr[0]);
            player.physicsObject.AddForce(leftForce);



            Force rightForce = new Force(Vector2.right, Vector2.up * outputArr[0]);
            player.physicsObject.AddForce(rightForce);
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

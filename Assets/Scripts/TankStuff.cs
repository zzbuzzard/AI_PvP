using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct TankInput
{
    float leftpwr;
    float Right_Pwr;
}

//What you pass to the neural net imo
public struct TankData
{
    float x;
    float y;
    float goalx;
    float goaly;
    float angle;
    float forwardspeed;
    float anglespeed;
    float targetdistance;
    float one; // please no but if we have to
}

public class TankStuff : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

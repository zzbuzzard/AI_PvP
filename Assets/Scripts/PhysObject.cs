using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Force
{
    public Vector2 offset;
    public Vector2 force;
}

public class PhysObject
{

    public float mass;
    public float spinInertia;

    public Vector2 location;
    public Vector2 velocity;

    public float angle;
    public float spinSpeed;

    public List<Force> forces;


    PhysObject(float mass, Vector2 location, float angle=0)
    {
        this.mass = mass;
        this.location = location;
        this.angle = angle;

        this.forces = new List<Force>();
    }

    public void AddForce(Force f)
    {
        forces.Add(f);
    }

    public void Update(float dt)
    {
        Vector2 acc = new Vector2(0, 0);
        float spinacc = 0;
        foreach(Force f in forces)
        {
            acc += f.force;
            spinacc += Vector3.Cross(f.force, f.offset).z;

        }
        acc /= mass;
        spinacc /= spinInertia;

        location += dt * velocity + 0.5f * acc * dt * dt;
        velocity += acc * dt;

        angle += dt * spinSpeed + 0.5f * spinacc * dt * dt;
        spinSpeed += dt * spinacc;

        forces.Clear();
    }
}

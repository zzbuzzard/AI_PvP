using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Force
{
    public Vector2 offset;
    public Vector2 force;

    public Force(Vector2 offset, Vector2 force)
    {
        this.offset = offset;
        this.force = force;
    }
}

public class PhysObject
{
    public float mass;
    public float spinInertia;

    public Vector2 location;
    public Vector2 velocity;

    public float angle;
    public float spinSpeed;

    public PhysObject(float mass, Vector2 location, float angle=0, float spinInertia=1.0f)
    {
        this.mass = mass;
        this.location = location;
        this.angle = angle;
        this.spinSpeed = 0f;
        this.spinInertia = spinInertia;
        this.velocity = new Vector2(0,0);
    }

    Vector2 acc;
    float spinacc;

    public void AddForce(Force f)
    {
        acc += f.force;
        spinacc += Vector3.Cross(f.force, f.offset).z;
    }

    public void Update(float dt)
    {
        acc /= mass;
        spinacc /= spinInertia;

        location += dt * velocity + 0.5f * dt * dt * acc;
        velocity += acc * dt;

        angle += dt * spinSpeed + 0.5f * spinacc * dt * dt;
        spinSpeed += dt * spinacc;

        acc = new Vector2(0.0f, 0f);
        spinacc = 0.0f;
    }
}

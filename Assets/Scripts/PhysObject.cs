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

    public void RotateByMatrix(Matrix4x4 m)
    {
        this.offset = m.MultiplyVector(this.offset);
        this.force = m.MultiplyVector(this.force);
    }
}

public class PhysObject
{
    public float mass;
    public float spinInertia;

    public Vector2 location;
    public Vector2 velocity;


    private float _angle;
    public float angle
    {
        get
        {
            return _angle;
        }
        set
        {
            _angle = value;
            rotationMatrix[0, 0] = Mathf.Cos(_angle);
            rotationMatrix[0, 1] = -Mathf.Sin(_angle);
            rotationMatrix[1, 0] = Mathf.Sin(_angle);
            rotationMatrix[1, 1] = Mathf.Cos(_angle);
        }
    }
    public float spinSpeed;
    public Matrix4x4 rotationMatrix;

    public PhysObject(float mass, Vector2 location, float angle=0, float spinInertia=0.5f)
    {
        this.mass = mass;
        this.location = location;
        this.rotationMatrix = new Matrix4x4(Vector4.zero, Vector4.zero, Vector4.zero, Vector4.zero);
        this.angle = angle;
        this.spinSpeed = 0f;
        this.spinInertia = spinInertia;
        this.velocity = new Vector2(0,0);
    }

    Vector2 acc = new Vector2(0.0f, 0.0f);
    float spinacc = 0.0f;

    public void AddForce(Force f)
    {
        f.RotateByMatrix(rotationMatrix);
        acc += f.force;
        spinacc += Vector3.Cross(new Vector3(f.force.x, f.force.y, 0f), new Vector3(f.offset.x, f.offset.y, 0f)).z;
    }

    public void Update(float dt)
    {
        acc /= mass;
        spinacc /= spinInertia;

        location += dt * velocity + 0.5f * dt * dt * acc;
        velocity += acc * dt;

        angle += Mathf.Rad2Deg * dt * spinSpeed + 0.5f * spinacc * dt * dt;
        spinSpeed += dt * spinacc;

        acc = new Vector2(0.0f, 0f);
        spinacc = 0.0f;
    }
}

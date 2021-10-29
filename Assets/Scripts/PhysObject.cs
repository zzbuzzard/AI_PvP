using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Force
{
    public Vector3 offset;
    public Vector3 force;

    public Force(Vector2 offset, Vector2 force)
    {
        this.offset = new Vector3(offset.x, offset.y, 0f);
        this.force = new Vector3(force.x, force.y, 0f);
    }

    public void RotateByMatrix(Matrix4x4 m)
    {
        this.offset = m.MultiplyVector(this.offset);
        this.force = m.MultiplyVector(this.force);
        if(this.offset.z > 0.1f)
        {
            Debug.Log("AAAAAAAAAAAAA");
        }
        if (this.force.z > 0.1f)
        {
            Debug.Log("OH GOD");
        }
    }
}

public class PhysObject
{
    public float mass;
    public float spinInertia;


    public Vector2 location;
    public Vector2 velocity;
    public float forwarddamping = 5.0f;
    public float sidewaysdamping = 100.0f;
    public float rotationaldamping = 5.0f;

    //Radians
    public float _angle { get; private set;  }

    //Degrees
    public float angle
    {
        get
        {
            return Mathf.Rad2Deg * _angle;
        }
        set
        {
            _angle = Mathf.Deg2Rad * value;
            rotationMatrix[0, 0] = Mathf.Cos(_angle);
            rotationMatrix[0, 1] = -Mathf.Sin(_angle);
            rotationMatrix[1, 0] = Mathf.Sin(_angle);
            rotationMatrix[1, 1] = Mathf.Cos(_angle);
        }
    }

    //Radians per sec
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
        acc += new Vector2(f.force.x, f.force.y);
        spinacc -= Vector3.Cross(f.force, f.offset).z;
    }

    public void Update(float dt)
    {
        //acc -= velocity * linear_damping;
        Vector2 forward = (Vector2)rotationMatrix.MultiplyVector(Vector3.up);
        Vector2 right = (Vector2)rotationMatrix.MultiplyVector(Vector3.right);
        acc -= forward * Vector2.Dot(forward, velocity) * forwarddamping;
        acc -= right * Vector2.Dot(right, velocity) * sidewaysdamping;
        acc /= mass;
        spinacc -= spinSpeed * rotationaldamping;
        spinacc /= spinInertia;

        location += dt * velocity + 0.5f * dt * dt * acc;
        velocity += acc * dt;

        angle += Mathf.Rad2Deg * dt * spinSpeed + 0.5f * spinacc * dt * dt;
        spinSpeed += dt * spinacc;

        acc = new Vector2(0.0f, 0f);
        spinacc = 0.0f;
    }
}

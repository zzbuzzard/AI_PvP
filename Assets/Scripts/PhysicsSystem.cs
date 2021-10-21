using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsSystem
{
    private List<PhysObject> objects;

    public PhysicsSystem()
    {
        this.objects = new List<PhysObject>();
    }

    public void AddObject(PhysObject obj)
    {
        objects.Add(obj);
    }

    public void Step(float dt)
    {
        foreach(PhysObject obj in objects)
        {
            obj.Update(dt);
        }
    }
}

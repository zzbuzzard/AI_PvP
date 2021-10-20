using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsSystem
{
    private List<PhysObject> objects;

    PhysicsSystem()
    {
        this.objects = new List<PhysObject>();
    }

    void Step(float dt)
    {
        foreach(PhysObject obj in objects)
        {
            obj.Update(dt);
        }
    }
}

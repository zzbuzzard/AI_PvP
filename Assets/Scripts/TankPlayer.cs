using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPlayer : NeatPlayer
{
    public PhysObject physicsObject;
    public TankPlayer(NeatNet brain) : base(brain)
    {
    }
}

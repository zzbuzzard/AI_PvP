using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeatPlayer : AIPlayer
{
    public float fitness
    {
        get
        {
            return ((NeatNet)mnet).genoType.fitness;
        }
        set
        {
            ((NeatNet)mnet).genoType.fitness = value;
        }
    }

    public Genome GetGenome()
    {
        return ((NeatNet)mnet).genoType;
    }
    
    public NeatPlayer(NeatNet brain) : base(brain)
    {
    }

}


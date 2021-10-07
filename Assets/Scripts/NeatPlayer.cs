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
            return ((NEATNet)mnet).genoType.fitness;
        }
        set
        {
            ((NEATNet)mnet).genoType.fitness = value;
        }
    }

    public Genome GetGenome()
    {
        return ((NEATNet)mnet).genoType;
    }
    
    public NeatPlayer(NEATNet brain) : base(brain)
    {
    }
}


using System.Collections;
using System.Collections.Generic;

public enum EndType
{
    KILLED,
    TIMEOUT,
    WON,
    WALL
}

public abstract class GenericPlayer
{
    private static int playerID;

    public int myPlayerID { get; private set; }


    public GenericPlayer()
    {
        myPlayerID = playerID++;
    }

    public abstract float[] GetOutput(Game g, float[] input);
}

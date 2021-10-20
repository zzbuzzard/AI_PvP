using System;
using System.Collections;
using System.Collections.Generic;

public class TankGame
{
    public const float spf = 1 / 30.0f; // Seconds per frame


    public float maxMatchTime = 50.0f; // Max 50sec matches

    public int framesPassed { get; private set; }

    public List<GenericPlayer> players { get; private set; }
    GameInput[] inputs;


    public static void SimulateGame(List<GenericPlayer> p)
    {
        ShooterGame g = new ShooterGame(p);
        while (!g.Step()) { }
    }


    public TankGame(List<GenericPlayer> players)
    {
        framesPassed = 0;

        this.players = players;

        inputs = new GameInput[players.Count];

    }


    // Returns true iff the game has ended
    public bool Step()
    {

        // Get all players' inputs.
        // It's important this happens before any movement takes place, so that the order of the loop is irrelevant
        for (int i=0; i<players.Count; i++) {
            if (players[i].life <= 0) continue; // Ignore the dead
            inputs[i] = players[i].GetInput(this);
        }

        // Move players
        for (int i = 0; i < players.Count; i++) {


        }

        



        framesPassed++;
        return false;
    }
}

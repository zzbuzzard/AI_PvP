using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Game
{
    public int framesPassed { get; private set; }
    public GenericPlayer[] players { get; private set; }
    public float maxMatchTime;

    public abstract float GetScore(int i);
    public abstract GameDrawer GetDrawer(MonoBehaviour m);
    public abstract float[] GetInput(int i);

    public Game(GenericPlayer[] players, float matchTime = 50.0f)
    {
        this.players = players;
        this.maxMatchTime = matchTime;
    }

    // Step the game until it finishes, return the player scores
    public float[] SimulateGame()
    {
        while (!Step());
        float[] scores = new float[players.Length];
        for (int i = 0; i < players.Length; i++) scores[i] = GetScore(i);
        return scores;
    }

    // Returns true if game has now ended
    public virtual bool Step()
    {
        framesPassed++;
        return false;
    }
}

public abstract class GameDrawer
{
    protected MonoBehaviour m;

    public GameDrawer(MonoBehaviour m)
    {
        this.m = m;
    }

    public abstract Vector2 GameToWorld(Vector2 gamePoint);
    public abstract Vector2 WorldToGame(Vector2 worldPoint);

    public abstract void Draw();
    public abstract void Cleanup();
}
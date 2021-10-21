using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDisplay : MonoBehaviour
{
    Game g;
    GameDrawer d;

    public bool gameInProgress = false;

    public void Simulate(Game game, bool timeless = false)
    {
        if (d != null)
            d.Cleanup();

        g = game;
        d = g.GetDrawer(this);

        gameInProgress = true;

        if (timeless)
            g.maxMatchTime = float.PositiveInfinity;
    }


    private void Start()
    {
    }

    // Run the simulation, if we're doing that
    private void FixedUpdate()
    {
        if (!gameInProgress) return;

        if (g.Step())
        {
            EndSimulation();
            return;
        }

        d.Draw();
    }

    public void EndSimulation()
    {
        gameInProgress = false;
        g = null;
    }

    // Used for HumanPlayer only, due to FixedUpdate input issues
    public static bool mouseClicked;
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            mouseClicked = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDisplay : MonoBehaviour
{
    Game currentGame;
    GameDrawer currentDrawer;

    List<Game> gameList;
    int gameListIndex = -1;

    public bool gameInProgress = false;

    public void Simulate(List<Game> game, bool timeless = false)
    {
        gameList = game;
        gameListIndex = -1;
        NextGame(timeless);
    }

    public int GetCurrentGameNumber()
    {
        if (gameInProgress) return gameListIndex + 1;
        return 0;
    }

    public int GetGameCount()
    {
        if (gameInProgress) return gameList.Count;
        return 0;
    }

    private void NextGame(bool timeless = false)
    {
        gameListIndex++;

        if (gameListIndex < gameList.Count)
        {
            if (currentDrawer != null)
                currentDrawer.Cleanup();

            currentGame = gameList[gameListIndex];
            currentDrawer = currentGame.GetDrawer();

            gameInProgress = true;

            if (timeless)
                currentGame.maxMatchTime = float.PositiveInfinity;
        }
        else
        {
            EndSimulation();
        }
    }

    public void Simulate(Game game, bool timeless = false)
    {
        Simulate(new List<Game> { game }, timeless);
    }


    private void Start()
    {
    }

    // Run the simulation, if we're doing that
    private void FixedUpdate()
    {
        if (!gameInProgress) return;

        if (currentGame.Step())
        {
            NextGame();
            return;
        }

        currentDrawer.Draw();
    }

    public void EndSimulation()
    {
        gameInProgress = false;
        currentGame = null;
    }

    // Used for HumanPlayer only, due to FixedUpdate input issues
    public static bool mouseClicked;
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            mouseClicked = true;

            if (gameInProgress && Constants.GAME_TYPE == GameType.TANK)
            {
                Vector2 pos = currentDrawer.WorldToGame(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                ((TankGame)currentGame).SetGoal(pos);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDisplay : MonoBehaviour
{
    ShooterGame g;
    List<GenericPlayer> players;
    List<GameObject> playerObjs;
    List<GameObject> bulletObjs;
    List<GameObject> wallObjs;

    public GameObject playerPrefab;
    public GameObject wallPrefab;
    public GameObject bulletPrefab;

    public bool gameInProgress = false;

    public void Simulate(ShooterGame g, bool timeless = false)
    {
        // Clean up previous simulation
        foreach (GameObject x in playerObjs) Destroy(x);
        foreach (GameObject x in bulletObjs) Destroy(x);
        foreach (GameObject x in wallObjs) Destroy(x);

        playerObjs.Clear();
        bulletObjs.Clear();
        wallObjs.Clear();

        gameInProgress = true;

        players = g.players;

        foreach (GenericPlayer p in players)
            MakePlayer(p);

        this.g = g;
        for (int x = 0; x < ShooterGame.xsize; x++)
        {
            for (int y = 0; y < ShooterGame.ysize; y++)
            {
                if (g.GetTile(x, y) == MapBlock.WALL)
                {
                    Vector2 pos = Translate(new Vector2(x, y));
                    wallObjs.Add(Instantiate(wallPrefab, pos, Quaternion.identity));
                }
            }
        }

        if (timeless)
            g.maxMatchTime = float.PositiveInfinity;
    }


    private void Start()
    {
        Camera.main.orthographicSize = ShooterGame.xsize / 3.0f;

        playerObjs = new List<GameObject>();
        bulletObjs = new List<GameObject>();
        wallObjs   = new List<GameObject>();
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

        for (int i=0; i<players.Count; i++)
        {
            if (players[i].life <= 0)
            {
                playerObjs[i].SetActive(false);
            }
            else
            {
                playerObjs[i].transform.position = Translate(new Vector2(players[i].x, players[i].y));
            }
        }

        while (bulletObjs.Count > g.bullets.Count)
        {
            Destroy(bulletObjs[bulletObjs.Count - 1]);
            bulletObjs.RemoveAt(bulletObjs.Count - 1);
        }
        for (int i=0; i<g.bullets.Count; i++)
        {
            if (i >= bulletObjs.Count) bulletObjs.Add(Instantiate(bulletPrefab));
            bulletObjs[i].transform.position = Translate(new Vector2(g.bullets[i].x, g.bullets[i].y));
        }
    }


    public void EndSimulation()
    {
        gameInProgress = false;
        g = null;
    }

    private void MakePlayer(GenericPlayer p)
    {
        GameObject mPref = Instantiate(playerPrefab);
        mPref.transform.GetChild(0).GetComponent<TextMesh>().text = "" + p.myPlayerID;
        playerObjs.Add(mPref);
    }

    // ShooterGame coords to world coords
    public static Vector2 Translate(Vector2 pos)
    {
        Vector2 centre = new Vector2(ShooterGame.xsize / 2, ShooterGame.ysize / 2);
        return pos - centre;
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

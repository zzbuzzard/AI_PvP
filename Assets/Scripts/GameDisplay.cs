using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDisplay : MonoBehaviour
{
    Game g;
    List<GenericPlayer> players;
    List<GameObject> playerObj;
    List<GameObject> bulletObj;

    public GameObject playerPrefab;
    public GameObject wallPrefab;
    public GameObject bulletPrefab;

    private void MakePlayer(GenericPlayer p)
    {
        players.Add(p);

        GameObject mPref = Instantiate(playerPrefab);
        playerObj.Add(mPref);
    }

    // Game coords to world coords
    public static Vector2 Translate(Vector2 pos)
    {
        Vector2 centre = new Vector2(Game.xsize / 2, Game.ysize / 2);
        return pos - centre;
    }

    void Start()
    {
        players = new List<GenericPlayer>();
        playerObj = new List<GameObject>();
        bulletObj = new List<GameObject>();

        MakePlayer(new HumanPlayer());
        MakePlayer(new AIPlayer());

        g = new Game(players);
        for (int x=0; x<Game.xsize; x++)
        {
            for (int y=0; y<Game.ysize; y++)
            {
                if (g.GetTile(x, y) == MapBlock.WALL)
                {
                    Vector2 pos = Translate(new Vector2(x, y));
                    Instantiate(wallPrefab, pos, Quaternion.identity);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        g.Step();
        for (int i=0; i<players.Count; i++)
        {
            if (players[i].life <= 0)
            {
                playerObj[i].SetActive(false);
            }
            else
            {
                playerObj[i].transform.position = Translate(new Vector2(players[i].x, players[i].y));
            }
        }

        while (bulletObj.Count > g.bullets.Count)
        {
            Destroy(bulletObj[bulletObj.Count - 1]);
            bulletObj.RemoveAt(bulletObj.Count - 1);
        }
        for (int i=0; i<g.bullets.Count; i++)
        {
            if (i >= bulletObj.Count) bulletObj.Add(Instantiate(bulletPrefab));
            bulletObj[i].transform.position = Translate(new Vector2(g.bullets[i].x, g.bullets[i].y));
        }
    }

    public static bool mouseClicked;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseClicked = true;
        }
    }
}

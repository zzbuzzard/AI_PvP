using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MapBlock
{
    EMPTY,
    WALL
}

// Stores the input of one player to the game
// TODO: 3 * 2 * 2 = 12 states <= 2^8, could be packaged into one byte for speed
public struct GameInput
{
    public sbyte hdir;  // -1 means left, 1 means right, 0 means neither
    public bool jump;   // jump button currently held down?
    public bool shoot;  // shoot button currently held down?
    public float shootAngle;

    public GameInput(sbyte hdir, bool jump, bool shoot, float shootAngle)
    {
        this.hdir = hdir;
        this.jump = jump;
        this.shoot = shoot;
        this.shootAngle = shootAngle;
    }

    public readonly static GameInput nothing = new GameInput(0, false, false, 0.0f);
}

public class Bullet
{
    public int shooterID, life;
    public float x, y, vx, vy;
    public Bullet(int shooterID, float x, float y, float vx, float vy, int life)
    {
        this.shooterID = shooterID;
        this.x = x;
        this.y = y;
        this.vx = vx;
        this.vy = vy;
        this.life = life;
    }
}

public class ShootGame : Game
{
    public struct PlayerInfo
    {
        public const int maxlife = 3;
        public const int numjumps = 2;

        public float x, y, vx, vy;
        public int gameID, life;
        public int jumps;
        public int lastShootFrame;
        public bool jumpLast, onFloor;


        // Performance stats:
        public int shotsFired,
                   shotsHit,
                   playersKilled,
                   frameOfDeath,   // if they win, frameOfDeath is the last frame of the game + 1
                   diedBefore;     // how many other players died before me? e.g. if all die at the same time, then 0

        public EndType endType;    // How did I die?

        public PlayerInfo(float x, float y, int gameID)
        {
            this.gameID = gameID;
            this.x = x;
            this.y = y;
            vx = 0;
            vy = 0;
            life = maxlife;

            onFloor = false;
            jumps = 0;
            jumpLast = false;

            endType = EndType.TIMEOUT;
            lastShootFrame = -10;

            // Stats
            shotsFired = 0;
            shotsHit = 0;
            playersKilled = 0;
            frameOfDeath = 0;
            diedBefore = 0;
        }
    }

    public class ShootGameDrawer : GameDrawer
    {
        ShootGame g;

        List<GameObject> playerObjs;
        List<GameObject> bulletObjs;
        List<GameObject> wallObjs;

        // TODO: Uhh
        public static GameObject playerPrefab;
        public static GameObject wallPrefab;
        public static GameObject bulletPrefab;

        static ShootGameDrawer()
        {
            playerPrefab = Resources.Load<GameObject>("Prefabs/ShootPlayer");
            wallPrefab   = Resources.Load<GameObject>("Prefabs/ShootWall");
            bulletPrefab = Resources.Load<GameObject>("Prefabs/ShootBullet");
        }

        public ShootGameDrawer(ShootGame g, MonoBehaviour m) : base(m)
        {
            this.g = g;

            Camera.main.orthographicSize = xsize / 3.0f;

            playerObjs = new List<GameObject>();
            bulletObjs = new List<GameObject>();
            wallObjs = new List<GameObject>();


            foreach (GenericPlayer p in g.players)
            {
                GameObject mPref = MonoBehaviour.Instantiate(playerPrefab);
                mPref.transform.GetChild(0).GetComponent<TextMesh>().text = "" + p.myPlayerID;
                playerObjs.Add(mPref);
            }

            this.g = g;
            for (int x = 0; x < xsize; x++)
            {
                for (int y = 0; y < ysize; y++)
                {
                    if (g.GetTile(x, y) == MapBlock.WALL)
                    {
                        Vector2 pos = Translate(new Vector2(x, y));
                        wallObjs.Add(MonoBehaviour.Instantiate(wallPrefab, pos, Quaternion.identity));
                    }
                }
            }

        }

        public override void Draw()
        {
            for (int i = 0; i < g.players.Length; i++)
            {
                if (g.info[i].life <= 0)
                    playerObjs[i].SetActive(false);
                else
                    playerObjs[i].transform.position = Translate(new Vector2(g.info[i].x, g.info[i].y));
            }

            while (bulletObjs.Count > g.bullets.Count)
            {
                MonoBehaviour.Destroy(bulletObjs[bulletObjs.Count - 1]);
                bulletObjs.RemoveAt(bulletObjs.Count - 1);
            }
            for (int i = 0; i < g.bullets.Count; i++)
            {
                if (i >= bulletObjs.Count) bulletObjs.Add(MonoBehaviour.Instantiate(bulletPrefab));
                bulletObjs[i].transform.position = Translate(new Vector2(g.bullets[i].x, g.bullets[i].y));
            }
        }

        public override void Cleanup()
        {
            // Clean up previous simulation
            foreach (GameObject x in playerObjs) MonoBehaviour.Destroy(x);
            foreach (GameObject x in bulletObjs) MonoBehaviour.Destroy(x);
            foreach (GameObject x in wallObjs) MonoBehaviour.Destroy(x);

            playerObjs.Clear();
            bulletObjs.Clear();
            wallObjs.Clear();
        }

        // Game coords to world coords
        public static Vector2 Translate(Vector2 pos)
        {
            Vector2 centre = new Vector2(xsize / 2, ysize / 2);
            return pos - centre;
        }
    }

    public const float spf = 1 / 30.0f; // Seconds per frame
    const float gravity = 5.0f;
    const float playerJumpVelocity = 5.0f;
    const float playerMoveSpeed = 2.5f;
    const float bulletMoveSpeed = 10.0f; // previously 8
    const int bulletFrameLife = 60; // was 40

    public const float reloadTime = 0.8f;
    public const int frameReloadTime = (int)(reloadTime / spf);

    public static float playerSize = 0.8f, playerSize2 = 1.0f;

    // The map is stored as a grid, where map[x][y] gives the block at (x, y)
    // (0, 0) is the bottom left.
    // All squares are centered, so this means there is a block with center at (0, 0)
    private MapBlock[,] map;
    public static int xsize { get; private set; } = 20;  // 40 x 10 works well with 5 - 8 players
    public static int ysize { get; private set; } = 10;

    public PlayerInfo[] info { get; private set; }
    private GameInput[] inputs;

    public List<Bullet> bullets;
    Trial trial;

    public static float Trial(GenericPlayer p, Trial t)
    {
        ShootGame g = t.CreateTrial(p);
        while (!g.Step()) { }
        return t.GetScore(g);
    }

    public void SetTile(int x, int y, MapBlock b)
    {
        map[x, y] = b;
    }

    public MapBlock GetTile(int x, int y)
    {
        if (x < 0 || x >= xsize || y < 0 || y >= ysize) return MapBlock.WALL;
        return map[x, y];
    }

    public ShootGame(GenericPlayer[] players) : base(players)
    {    
        bullets = new List<Bullet>();
        map = new MapBlock[xsize, ysize];
        info = new PlayerInfo[players.Length];
        inputs = new GameInput[players.Length];

        maxMatchTime = 50.0f;

        // Initiate map with a single row of walls on the floor
        for (int x=0; x<xsize; x++)
        {
            map[x, 0] = MapBlock.WALL;
            for (int y=1; y<ysize; y++)
            {
                map[x, y] = MapBlock.EMPTY;
            }
        }

        float spacing = xsize / players.Length;
        for (int i=0; i<players.Length; i++)
        {
            info[i] = new PlayerInfo(spacing * (i+0.5f), 1.0f, i);

            //if (i > 0)
            //{
                //map[(int)(spacing * i), 2] = MapBlock.WALL;
            //}
            //map[(int)(spacing * (i + 0.5f)), 4] = MapBlock.WALL;
        }

        //map[xsize / 2, 2] = MapBlock.WALL;
        //map[xsize / 2, 3] = MapBlock.WALL;
        //map[xsize / 2, 4] = MapBlock.WALL;
    }

    public ShootGame(GenericPlayer[] p, Trial t) : this(p)
    {
        trial = t;
    }

    private const int inputFrame = 4;

    // Returns true iff the game has ended
    public override bool Step()
    {
        base.Step();

        // During a trial, we don't check for winners
        if (trial == null)
        {
            bool gameOver = true;
            int winner = -1;

            // Check if the game is over
            for (int i = 0; i < players.Length; i++)
            {
                if (info[i].life > 0)
                {
                    if (winner != -1)
                    {
                        gameOver = false;
                        break;
                    }
                    winner = i;
                }
            }

            // If the match has been going on for more than (maxMatchTime) seconds, it's over
            if (framesPassed > maxMatchTime / spf)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    if (info[i].life > 0)
                    {
                        info[i].frameOfDeath = framesPassed;
                        info[i].endType = EndType.TIMEOUT;
                    }
                }

                gameOver = true;
                winner = -1; // Nobody wins if timeout
            }

            if (gameOver)
            {
                if (winner != -1)
                {
                    info[winner].frameOfDeath = framesPassed;
                    info[winner].endType = EndType.WON;
                }

                // Now, for each player, work out how many players died before them
                List<Pair<int, int>> deathTimes = new List<Pair<int, int>>();
                for (int i = 0; i < players.Length; i++)
                {
                    deathTimes.Add(new Pair<int, int>(info[i].frameOfDeath, i));
                }
                deathTimes.Sort();

                int diedBefore = 0;
                for (int i = 0; i < players.Length; i++)
                {
                    if (i > 0)
                    {
                        if (deathTimes[i - 1].fst != diedBefore)
                        {
                            diedBefore = i;
                        }
                        // otherwise, the same as before
                    }
                    info[deathTimes[i].snd].diedBefore = diedBefore;
                }

                return true;
            }
        }
        else
        {
            if (trial.Apply(this)) return true;
        }

        // Get all players' inputs.
        // It's important this happens before any movement takes place, so that the order of the loop is irrelevant

        if (framesPassed % inputFrame == 0)
        {
            for (int i = 0; i < info.Length; i++)
            {
                if (info[i].life <= 0) continue; // Ignore the dead
                inputs[i] = GetOutput(players[i].GetOutput(this, GetInput(i))); // This is where the magic happens
            }
        }

        // Move players, collide with walls
        for (int i = 0; i < info.Length; i++) {
            if (info[i].life <= 0) continue; // Ignore the dead

            info[i].vx = inputs[i].hdir * playerMoveSpeed;

            // Handle jumping
            if (info[i].onFloor)
            {
                info[i].jumps = PlayerInfo.numjumps;

                if (inputs[i].jump && !info[i].jumpLast)
                {
                    info[i].jumps--;
                    info[i].onFloor = false;
                    info[i].vy = playerJumpVelocity;
                }
            }
            else
            {
                if (info[i].jumps > 0 && inputs[i].jump && !info[i].jumpLast)
                {
                    info[i].jumps--;
                    info[i].vy = playerJumpVelocity;
                }
                else
                {
                    info[i].vy -= gravity * spf;
                }
            }
            info[i].jumpLast = inputs[i].jump;


            info[i].x += info[i].vx * spf;

            if (info[i].x < 0.0f || info[i].x > xsize-1)
            {
                info[i].life = 0;
                info[i].frameOfDeath = framesPassed;
                info[i].endType = EndType.WALL;
                continue;
            }

            float xmin = info[i].x - playerSize2 / 2.0f;
            float xmax = info[i].x + playerSize2 / 2.0f;
            int x1 = (int)Math.Floor(xmin + 0.5f);             // Prevents edge case with (int)(-0.5) = 0
            int x2 = (int)(xmax + 0.5f);

            float ymin = info[i].y - playerSize / 2.0f;
            float ymax = info[i].y + playerSize / 2.0f;
            int y1 = (int)(ymin + 0.5f);
            int y2 = (int)(ymax + 0.5f);

            // Check for right wall collision
            if (info[i].vx > 0)
            {
                if (GetTile(x2, y1) == MapBlock.WALL || GetTile(x2, y2) == MapBlock.WALL)
                {
                    info[i].x = x2 - 0.5f - playerSize2 / 2.0f;
                }
            }
            else if (info[i].vx < 0)
            {
                // We hit left wall, push right
                if (GetTile(x1, y1) == MapBlock.WALL || GetTile(x1, y2) == MapBlock.WALL)
                {
                    info[i].x = x1 + 0.5f + playerSize2 / 2.0f;
                }
            }

            info[i].y += info[i].vy * spf;

            if (info[i].y < 0.0f || info[i].y > ysize-1)
            {
                info[i].life = 0;
                info[i].frameOfDeath = framesPassed;
                info[i].endType = EndType.WALL;
                continue;
            }


            xmin = info[i].x - playerSize / 2.0f;
            xmax = info[i].x + playerSize / 2.0f;

            x1 = (int)(xmin + 0.5f);
            x2 = (int)(xmax + 0.5f);

            ymin = info[i].y - playerSize2 / 2.0f;
            ymax = info[i].y + playerSize2 / 2.0f;

            y1 = (int)(ymin + 0.5f);
            y2 = (int)(ymax + 0.5f);

            info[i].onFloor = false;

            if (info[i].vy > 0)
            {
                // We hit a ceiling, push down
                if (GetTile(x1, y2) == MapBlock.WALL || GetTile(x2, y2) == MapBlock.WALL)
                {
                    info[i].y = y2 - 0.5f - playerSize2 / 2.0f;
                    info[i].vy = Math.Min(0, info[i].vy); // Allow them to continue falling if they are falling
                }
            }
            else
            {
                // We hit a floor, push up
                if (GetTile(x1, y1) == MapBlock.WALL || GetTile(x2, y1) == MapBlock.WALL)
                {
                    info[i].onFloor = true;
                    info[i].y = y1 + 0.5f + playerSize2 / 2.0f;
                    info[i].vy = Math.Max(0, info[i].vy); // Allow them to continue jumping if they are jumping
                }
            }


            // Shoot
            if (inputs[i].shoot)
            {
                if (info[i].lastShootFrame < framesPassed - frameReloadTime)
                {
                    info[i].lastShootFrame = framesPassed;

                    float vx = bulletMoveSpeed * (float)Math.Cos(inputs[i].shootAngle);
                    float vy = bulletMoveSpeed * (float)Math.Sin(inputs[i].shootAngle);

                    bullets.Add(new Bullet(info[i].gameID, info[i].x, info[i].y, vx, vy, bulletFrameLife));

                    info[i].shotsFired++;
                }
            }

        }

        bool hit = false;

        // Move bullets, check for bullet collisions (with walls and players)
        for (int i=0; i<bullets.Count; i++)
        {
            bullets[i].vy -= gravity * spf;

            bullets[i].x += bullets[i].vx * spf;
            bullets[i].y += bullets[i].vy * spf;

            hit = false;

            bullets[i].life -= 1;

            if (bullets[i].life <= 0)
            {
                hit = true;
            }
            else
            {
                // Check for player collision
                for (int j = 0; j < info.Length; j++)
                {
                    if (info[j].gameID == bullets[i].shooterID
                        || info[j].life <= 0) continue; // Don't collide with shooter or dead people

                    // Check for collision
                    if (info[j].x - playerSize2 / 2.0f <= bullets[i].x &&
                        info[j].x + playerSize2 / 2.0f >= bullets[i].x &&
                        info[j].y - playerSize2 / 2.0f <= bullets[i].y &&
                        info[j].y + playerSize2 / 2.0f >= bullets[i].y)
                    {
                        hit = true;

                        // Increment shooter's counter
                        info[bullets[i].shooterID].shotsHit += 1;

                        // Deal damage
                        info[j].life -= 1;

                        // If it was a kill, increase the shooter's counter
                        if (info[j].life <= 0)
                        {
                            info[j].frameOfDeath = framesPassed;
                            info[bullets[i].shooterID].playersKilled += 1;
                        }

                    }
                }
            }

            // Check for wall collision
            if (hit || bullets[i].x < -0.5f || bullets[i].x > xsize + 0.5f || bullets[i].y < -0.5f || bullets[i].y > ysize + 0.5f || 
                GetTile((int)(bullets[i].x + 0.5f), (int)(bullets[i].y + 0.5f)) == MapBlock.WALL)
            {
                bullets.RemoveAt(i);
                i--;
            }
        }

        return false;
    }

    public override float GetScore(int i)
    {
        PlayerInfo p = info[i];

        float accuracy = p.shotsHit / (float)(p.shotsFired == 0 ? 1 : p.shotsFired);

        float endBonus = 0.0f;
        if (p.endType == EndType.KILLED)  endBonus = -10.0f;
        if (p.endType == EndType.TIMEOUT) endBonus = 0.0f;
        if (p.endType == EndType.WON)     endBonus = 100.0f;
        if (p.endType == EndType.WALL)    endBonus = -1000.0f;

        float percentageDiedBefore = p.diedBefore / (float)players.Length;
        float lifeProp = p.life / (float)PlayerInfo.maxlife;

        return Mathf.Max(0.0f,
                         p.shotsHit * 50                        // 50 points per hit
                       + lifeProp * lifeProp * 20               // 20 bonus points for max life
                       + endBonus                               // 100 bonus points for a win
                       + p.frameOfDeath * ShootGame.spf / 2.0f  // 0.5 points per second
                       + p.shotsFired / 5.0f
                       // TODO: Force them to move left/right
                       );
    }

    public override GameDrawer GetDrawer(MonoBehaviour m)
    {
        return new ShootGameDrawer(this, m);
    }


    // INPUT/OUTPUT STUFF

    // Inputs:
    //  1/0 encoding of a grid around the player
    public const int gridX = 1; // (on either side)
    public const int gridY = 1; // (on either side)
    public const int inputsGrid = (2 * gridX + 1) * (2 * gridY + 1);

    //  My info:
    //   - Position (2)
    //   - Health (1)
    //   - Velocity (2)
    //   - Num jumps (1)
    //   - JumpLast (1)
    public const int inputsMe = 7;

    //  Per player
    //   - Position              (2)
    //   - Health                (1)
    //   - dx, dy normalised     (2)
    //   - distance              (1)
    //   - velocity              (2)
    public const int numPlayers = 1;
    public const int inputsPerPlayer = 8;

    //  Per bullet
    //   - position            (2)
    //   - dx, dy normalised   (2)
    //   - distance            (1)
    //   - velocity            (2)
    public const int numBullets = 5;
    public const int inputsPerBullet = 7;

    public const int numInputs = inputsGrid
                                + inputsMe
                                + inputsPerPlayer * numPlayers
                                + inputsPerBullet * numBullets
                                + 1; // BONUS INPUT

    // Outputs:
        //  direction (2)
        //  is shooting (1)
        //  angle (1)
        //  jump (1) - Y/N
    public const int numOutputs = 6;

    private static float[] inputArr = new float[numInputs];
    private static float[] outputArr = new float[numOutputs];

    private void SetGrid(int index, PlayerInfo p)
    {
        int xpos = (int)p.x;
        int ypos = (int)p.y;

        for (int i = xpos - gridX; i <= xpos + gridX; i++)
        {
            for (int j = ypos - gridY; j <= ypos + gridY; j++)
            {
                int num = (GetTile(i, j) == MapBlock.EMPTY ? 0 : 1);
                inputArr[index++] = num;
            }
        }
    }

    private void SetMyPlayer(int index, PlayerInfo p)
    {
        inputArr[index] = p.x /  xsize;
        inputArr[index + 1] = p.y /  ysize;          // Position
        inputArr[index + 2] = p.life / (float)PlayerInfo.maxlife;   // My life
        inputArr[index + 3] = p.vx;
        inputArr[index + 4] = p.vy;                       // Velocity
        inputArr[index + 5] = p.jumps / (float)PlayerInfo.numjumps;// Number of jumps remaining
        inputArr[index + 6] = p.jumpLast ? 1.0f : 0.0f;   // Was jump held last time? TODO: Add memory and remove
    }

    private void SetPlayer(int index, PlayerInfo p)
    {
        // No player: set life = 0. dist = 5
        inputArr[index + 2] = 0.0f;
        inputArr[index + 5] = 5.0f;
    }

    private void SetPlayer(int index, PlayerInfo p, PlayerInfo q)
    {
        float dx = q.x - p.x;
        float dy = q.y - p.y;
        float dist = Mathf.Sqrt(dx * dx + dy * dy);

        inputArr[index] = q.x /  xsize;
        inputArr[index + 1] = q.y /  ysize;          // Position
        inputArr[index + 2] = q.life / (float)PlayerInfo.maxlife;   // Health
        inputArr[index + 3] = dx / dist;
        inputArr[index + 4] = dy / dist;                 // dx, dy
        inputArr[index + 5] = dist /  xsize;         // dist
        inputArr[index + 6] = q.vx;
        inputArr[index + 7] = q.vy;                      // velocity
    }

    private void SetBullet(int index, PlayerInfo p, Bullet b)
    {
        if (b != null)
        {
            float dx = b.x - p.x;
            float dy = b.y - p.y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            inputArr[index] = b.x /  xsize;
            inputArr[index + 1] = b.y /  ysize;          // Position
            inputArr[index + 2] = dx / dist;
            inputArr[index + 3] = dy / dist;                 // dx, dy
            inputArr[index + 4] = dist /  xsize;         // dist
            inputArr[index + 5] = b.vx;
            inputArr[index + 6] = b.vy;                      // velocity
        }
        else
        {
            // if null, set distance to 5 and leave the rest on 0
            inputArr[index + 4] = 5.0f;
        }
    }

    // Gets a float[]
    public override float[] GetInput(int index)
    {
        PlayerInfo p = info[index];

        // Zero inputs
        // TODO: This is acc over the top a bit, we set most directly
        Array.Clear(inputArr, 0, numInputs);

        int offset = 0;

        SetGrid(offset, p);
        offset += inputsGrid;

        SetMyPlayer(offset, p);
        offset += inputsMe;

        // TODO: Sort players if we're doing more than 1v1s
        int count = 0;
        for (int i = 0; count < numPlayers; i++)
        {
            if (i <  players.Length)
            {
                if ( info[i].gameID == p.gameID) continue;

                SetPlayer(offset, p, info[i]);
            }
            else
                SetPlayer(offset, p);
            offset += inputsPerPlayer;

            count++;
        }

        Debug.Assert(count == numPlayers, "Only counted " + count + " players, instead of " + numPlayers);
        Debug.Assert(offset == inputsGrid + inputsMe + inputsPerPlayer * numPlayers, "Incorrect offset");


        List<Bullet> bullets = new List<Bullet>();

        // Collect all bullets *not shot by us*
        foreach (Bullet b in bullets)
        {
            if (b.shooterID != p.gameID)
                bullets.Add(b);
        }

        // Sort the bullets by distance to us
        bullets.Sort(new BulletComparer(p));

        for (int i = 0; i < numBullets; i++)
        {
            if (i < bullets.Count)
            {
                SetBullet(offset, p, bullets[i]);
                offset += inputsPerBullet;
            }
            else
            {
                SetBullet(offset, p, null);
                offset += inputsPerBullet;
            }
        }

        inputArr[inputArr.Length - 1] = 1.0f;

        Debug.Assert(offset == inputsGrid + inputsMe + inputsPerPlayer * numPlayers + inputsPerBullet * numBullets, "Incorrect offset");

        return inputArr;
    }

    public GameInput GetOutput(float[] outputArr)
    {
        Debug.Assert(outputArr.Length == numOutputs, "Number of outputs given was " +
                                            outputArr.Length + " when it should have been " + numOutputs);
  
        float left = outputArr[0];
        float right = outputArr[1];

        float shoot = outputArr[2];
        float dy = outputArr[3];
        float dx = outputArr[4];

        float jump = outputArr[5];

        // Does this even work
        float angle = Mathf.Atan2(dy, dx);

        bool bshoot = false,
                bjump = false;
        sbyte hor = 0;

        if (left > 1.0f) hor--;
        if (right > 1.0f) hor++;

        if (shoot > 1.0f) bshoot = true;
        if (jump > 1.0f) bjump = true;

        return new GameInput(hor, bjump, bshoot, angle);
    }


    class BulletComparer : IComparer<Bullet>
    {
        float cx, cy;

        public BulletComparer(PlayerInfo p)
        {
            cx = p.x;
            cy = p.y;
        }

        // TODO: Verify that this actually sorts the bullets, putting the closest ones by the player
        public int Compare(Bullet a, Bullet b)
        {
            float d1 = (a.x - cx) * (a.x - cx) + (a.y - cy) * (a.y - cy);
            float d2 = (b.x - cx) * (b.x - cx) + (b.y - cy) * (b.y - cy);
            return d1.CompareTo(d2);
        }
    }

}



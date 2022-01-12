using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordGame : Game
{
    public class SwordGameDrawer : GameDrawer
    {
        SwordGame g;

        SwordPlayerView v1, v2;

        public static readonly GameObject playerPrefab;

        static SwordGameDrawer()
        {
            playerPrefab = Resources.Load<GameObject>("Prefabs/SwordPlayer");
        }

        public SwordGameDrawer(SwordGame g)
        {
            this.g = g;

            v1 = MonoBehaviour.Instantiate(playerPrefab).GetComponent<SwordPlayerView>();
            v2 = MonoBehaviour.Instantiate(playerPrefab).GetComponent<SwordPlayerView>();
        }

        public override void Draw()
        {
            v1.Display(g.p1);
            v2.Display(g.p2);
        }

        public override void Cleanup()
        {
            MonoBehaviour.Destroy(v1.gameObject);
            MonoBehaviour.Destroy(v2.gameObject);
        }

        public static Vector2 GameToWorldStatic(Vector2 pos)
        {
            return pos;
        }

        public override Vector2 GameToWorld(Vector2 pos)
        {
            return pos;
        }

        public override Vector2 WorldToGame(Vector2 pos)
        {
            return pos;
        }
    }

    public class Swinger
    {
        public const float RAD = 0.5f;
        public const float SWORD_RAD  = 1.0f;

        const float DRAG_CONST = 0.9f,
                    ANG_DRAG_CONST = 0.9f,
                    SPD = 1.0f,
                    ANG_SPD = 0.3f;

        public Vector2 pos, vel;
        public float ang, angvel;

        public int hits = 0;

        public void Step(float xi, float yi, float ai)
        {
            vel *= DRAG_CONST;
            angvel *= ANG_DRAG_CONST;

            vel += new Vector2(xi * SPD, yi * SPD);
            angvel += ai * ANG_SPD;

            pos += vel * spf;
            ang += angvel * spf; ang %= 2 * Mathf.PI; if (ang < 0) ang += 2 * Mathf.PI;
        }

        public P<Vector2, Vector2> StickPos()
        {
            Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));

            Vector2 start = pos + dir * RAD;
            Vector2 end = pos + dir * (RAD + SWORD_RAD);

            return new P<Vector2, Vector2>(start, end);
        }
        
        public Option<Vector2> SwordIntersect(Swinger o)
        {
            // r = pos + x * dir        rad<= x <=rad+sword

            // pos.x + a * dir.x = pos2.x + b * dir2.x
            // pos.y + a * dir.y = pos2.y + b * dir2.y

            // a = (pos2.x + b * dir2.x - pos.x) / dir.x

            // pos.y + (pos2.x + b * dir2.x - pos.x) / dir.x * dir.y = pos2.y + b * dir2.y
            // (pos.y - pos2.y) * dir.x + (pos2.x + b * dir2.x - pos.x) * dir.y = b * dir2.y * dir.x
            // (pos.y - pos2.y) * dir.x + (pos2.x - pos.x) * dir.y + b * dir2.x * dir.y = b * dir2.y * dir.x
            // (pos.y - pos2.y) * dir.x + (pos2.x - pos.x) * dir.y = b * (dir2.y * dir.x - dir2.x * dir.y)

            Vector2 dir  = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            Vector2 dir2 = new Vector2(Mathf.Cos(o.ang), Mathf.Sin(o.ang));

            float n = (pos.y - o.pos.y) * dir.x + (o.pos.x - pos.x) * dir.y;
            float d = dir.x * dir2.y - dir.y * dir2.x;

            if (d != 0 && dir.x != 0)
            {
                float b = n / d;
                float a = (o.pos.x + b * dir2.x - pos.x) / dir.x;

                if (a >= RAD && a <= RAD + SWORD_RAD && b >= RAD && b <= RAD + SWORD_RAD)
                {
                    return new Option<Vector2>(pos + a * dir);
                }
            }

            return new Option<Vector2>();
        }

        public bool SwordHit(Swinger o)
        {
            // | (pos + a * dir) - pos2| = rad
            // x: pos.x + a * dir.x - pos2.x
            // y: pos.y + a * dir.y - pos2.y

            // dx = pos.x - pos2.x
            // dy = pos.y - pos2.y

            // (dx + a * dir.x)^2 + (dy + a * dir.y)^2 = rad^2

            // dx * dx + 2 * dir.x * dx * a + a^2 * dir.x^2 + 
            // dy * dy + 2 * dir.y * dy * a + a^2 * dir.y^2 = rad ^ 2

            // a^2 (dir.x^2 + dir.y^2) + a (2 * dir.x * dx + 2 * dir.y * dy) + (dx * dx + dy * dy) = rad^2

            Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));

            float dx = pos.x - o.pos.x,
                  dy = pos.y - o.pos.y;

            float a = dir.x * dir.x + dir.y * dir.y;
            float b = 2 * dir.x * dx + 2 * dir.y * dy;
            float c = dx * dx + dy * dy - RAD * RAD;

            float sqrt = b * b - 4 * a * c;

            if (a != 0 && sqrt >= 0)
            {
                sqrt = Mathf.Sqrt(sqrt);

                float v1 = (b + sqrt) / (2 * a);
                float v2 = (b - sqrt) / (2 * a);

                if ((v1 >= RAD && v1 <= RAD + SWORD_RAD) || (v2 >= RAD && v2 <= RAD + SWORD_RAD))
                {
                    return true;
                }
            }

            return false;
        }

        public static void Collide(Swinger a, Swinger b)
        {
            // TODO

            Option<Vector2> o = a.SwordIntersect(b);
            if (!o.valid) return; // no collision

            Vector2 collision_point = o.val;

            // Do maths and stuff
        }
    }

    private System.Random random;

    private const float maxX = 5;
    private const float maxY = 5;
    private const float spf  = 1 / 30.0f;

    Swinger p1, p2;

    public SwordGame(GenericPlayer n1, GenericPlayer n2, int seed) : base(new GenericPlayer[] {n1, n2})
    {
        maxMatchTime = 15.0f;
        random = new System.Random(seed);

        p1 = new Swinger();
        p2 = new Swinger();

        p1.pos = new Vector2(0.0f, 0.0f);
        p2.pos = new Vector2(maxX, maxY);
    }

    public override GameDrawer GetDrawer()
    {
        return new SwordGameDrawer(this);
    }

    public const int numInputs = 12;
    public const int numOutputs = 3;
    private static float[] inputArr = new float[numInputs];
    private static float[] outputArr;
    
    public Swinger GetSwinger(int i)
    {
        return (i == 0 ? p1 : p2);
    }

    public override float[] GetInput(int i)
    {
        Swinger p = (i == 0 ? p1 : p2);
        Swinger q = (i == 0 ? p2 : p1);

        // All inputs set directly, no need to zero the array

        Vector2 off  = q.pos - p.pos;

        float off_angle  = Mathf.Atan2(off.y, off.x);
        float vel_angle  = Mathf.Atan2(p.vel.y, p.vel.x);
        float q_vel_angle = Mathf.Atan2(q.vel.y, q.vel.x);

        // Sword towards player
        float diff_ang = Mathf.DeltaAngle(p.ang * Mathf.Rad2Deg, off_angle * Mathf.Rad2Deg) * Mathf.Deg2Rad;

        // Vel towards player
        float diff_vel = Mathf.DeltaAngle(vel_angle * Mathf.Rad2Deg, off_angle * Mathf.Rad2Deg) * Mathf.Deg2Rad;

        int ind = 0;

        inputArr[ind++] = off_angle;
        inputArr[ind++] = vel_angle;
        inputArr[ind++] = q_vel_angle;

        inputArr[ind++] = p.vel.magnitude;
        inputArr[ind++] = q.vel.magnitude;

        inputArr[ind++] = off.magnitude * 3.0f / (maxX * 1.4f); // normalised to ~0..3

        inputArr[ind++] = diff_ang;
        inputArr[ind++] = diff_vel;

        inputArr[ind++] = p.ang;
        inputArr[ind++] = p.angvel;

        inputArr[ind++] = q.ang;
        inputArr[ind++] = q.angvel;
        
        return inputArr;
    }

    public override float GetScore(int i)
    {
        Swinger p = GetSwinger(i);
        Swinger q = GetSwinger(1-i);

        return Mathf.Max(1.0f / (0.5f + Vector2.Distance(p.pos, q.pos)) + q.hits - p.hits * 0.5f, 0.1f);
    }

    public override bool Step()
    {
        base.Step();

        if (framesPassed * spf >= maxMatchTime) return true;
        if (p1.hits > 10 || p2.hits > 10) return true;

        for (int i=0; i<players.Length; i++)
        {
            float[] inp = GetInput(i);
            float[] outp = players[i].GetOutput(this, inp);

            float moveAmt  = Mathf.Clamp01(outp[0]);
            float moveAng  = outp[1];
            float swordChange = Mathf.Clamp(outp[2], -Mathf.PI, Mathf.PI);

            Swinger s = GetSwinger(i);

            float x = Mathf.Cos(moveAng) * moveAmt,
                  y = Mathf.Sin(moveAng) * moveAmt;

            s.Step(x, y, swordChange);
        }

        Swinger.Collide(p1, p2);

        if (p1.SwordHit(p2)) p2.hits++;
        if (p2.SwordHit(p1)) p1.hits++;

        return false;
    }
}

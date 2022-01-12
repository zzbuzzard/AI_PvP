using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordPlayerView : MonoBehaviour
{
    public GameObject sword;

    public void Display(SwordGame.Swinger swinger)
    {
        transform.position = swinger.pos;

        float ang = swinger.ang - Mathf.PI;

        Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));

        float r = (2 * SwordGame.Swinger.RAD + SwordGame.Swinger.SWORD_RAD) / 2.0f;

        sword.transform.rotation = Quaternion.Euler(0.0f, 0.0f, ang * Mathf.Rad2Deg + 90.0f);
        sword.transform.position = (Vector2)transform.position + dir * r;
    }
}

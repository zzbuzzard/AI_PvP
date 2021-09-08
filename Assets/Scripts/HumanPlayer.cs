using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : GenericPlayer
{
    public override GameInput GetInput(Game game)
    {
        sbyte h = 0;
        bool jump = false;
        bool shoot = false;
        float angle = 0.0f;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) h--;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) h++;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)) jump = true;
        if (GameDisplay.mouseClicked)
        {
            GameDisplay.mouseClicked = false;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 d = mousePos - GameDisplay.Translate(new Vector2(x, y));
            angle = Mathf.Atan2(d.y, d.x);

            shoot = true;
        }

        return new GameInput(h, jump, shoot, angle);
    }
}

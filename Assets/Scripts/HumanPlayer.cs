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

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) h--;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) h++;
        if (Input.GetKey(KeyCode.Space)) jump = true;
        if (Input.GetMouseButtonDown(0)) shoot = true;

        return new GameInput(h, jump, shoot);
    }
}

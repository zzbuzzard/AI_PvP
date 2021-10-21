using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : GenericPlayer
{
    private float[] output = new float[Constants.numOutputs];
    private int index = -1;

    // ShootGame version
    public override float[] GetOutput(Game g, float[] input)
    {
        // Stupid:
        if (index == -1)
        {
            for (int i = 0; i < g.players.Length; i++)
            {
                if (g.players[i] == this)
                {
                    index = i;
                    break;
                }
            }
        }


        int h = 0;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) h--;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) h++;

        output[0] = (h < 0 ? 2.0f : 0.0f); // Left
        output[1] = (h > 0 ? 2.0f : 0.0f); // Right

        if (GameDisplay.mouseClicked)
        {
            GameDisplay.mouseClicked = false;

            float x = ((ShootGame)g).info[index].x;
            float y = ((ShootGame)g).info[index].y;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 d = mousePos - ShootGame.ShootGameDrawer.Translate(new Vector2(x, y));

            output[3] = d.y;
            output[4] = d.x;

            output[2] = 2.0f;

        }
        else
        {
            output[2] = 0.0f;
        }

        output[5] = ((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space)) ? 2.0f : 0.0f);


        return output;

    }
}

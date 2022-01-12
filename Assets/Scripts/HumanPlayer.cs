using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : GenericPlayer
{
    private float[] output = new float[Constants.numOutputs];
    private int index = -1;

    public override float[] GetOutput(Game g, float[] input)
    {
        switch (Constants.GAME_TYPE)
        {
            case GameType.SHOOT:
                return GetOutputShoot((ShootGame)g, input);
            case GameType.TANK:
                return GetOutputTank((TankGame)g, input);
            case GameType.SWORD:
                return GetOutputSword((SwordGame)g, input);
        }
    }

    // SwordGame version
    private float[] GetOutputSword(SwordGame g, float[] input)
    {
        int index = (g.players[0] == this ? 0 : 1);

        output[0] = output[1] = output[2] = 0;

        int H = 0, V = 0;
        if (Input.GetKey(KeyCode.UpArrow))    V++;
        if (Input.GetKey(KeyCode.DownArrow))  V--;
        if (Input.GetKey(KeyCode.RightArrow)) H++;
        if (Input.GetKey(KeyCode.LeftArrow))  H--;

        if (H != 0 || V != 0)
        {
            output[0] = 1.0f;
            if (H != 0)
                output[1] = Mathf.Atan2(V, H);
            else
                output[1] = V * Mathf.PI / 2;
        }

        if (GameDisplay.mouseClicked)
        {
            GameDisplay.mouseClicked = false;

            SwordGame.Swinger s = g.GetSwinger(index);

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 d = mousePos - SwordGame.SwordGameDrawer.GameToWorldStatic(s.pos);

            float angle = Mathf.Atan2(d.y, d.x);

            output[2] = -Mathf.DeltaAngle(s.ang * Mathf.Rad2Deg, angle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        }
        else
        {
            output[2] = 0.0f;
        }

        return output;
    }

    // TankGame version
    private float[] GetOutputTank(TankGame g, float[] input)
    {
        output[0] = output[1] = 0.0f;
        if (Input.GetKey(KeyCode.Q)) output[1] += 20;
        if (Input.GetKey(KeyCode.A)) output[1] -= 20;

        if (Input.GetKey(KeyCode.P)) output[0] += 20;
        if (Input.GetKey(KeyCode.L)) output[0] -= 20;

        return output;
    }

    // ShootGame version
    private float[] GetOutputShoot(ShootGame g, float[] input)
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

            float x = g.info[index].x;
            float y = g.info[index].y;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 d = mousePos - ShootGame.ShootGameDrawer.GameToWorldStatic(new Vector2(x, y));

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegendController: MonoBehaviour
{
    CertaintyToolInterface Interface;
    private bool LegendActive = false;
    public List<GameObject> LegendSteps;
    public List<string> TextSteps;
    void Init(CertaintyToolInterface CTInterface)
    {
        Interface = CTInterface;
    }

    public void UpdateLegend()
    {
        if (!LegendActive)
        {

        }
        else
        {

        }
    }

    public void CreateLegend()
    {
        int steps = Interface.MaxRating - Interface.MinRating;
        if (steps > 0)
        {
            for (int i = 0;  i < steps; i++)
            {
                int height = steps - i;
                GameObject legendstep = Instantiate(Resources.Load<GameObject>("Prefabs/MainMenu"));
                legendstep.transform.position += Vector3.up * height / 2;
            }
        }
        else
        {

        }
    }

    public void LoadLegend()
    {

    }
}

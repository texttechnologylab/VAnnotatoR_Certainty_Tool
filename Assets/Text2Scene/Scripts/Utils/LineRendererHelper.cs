using UnityEngine;


public class LineRendererHelper
{
    /*
     * 
     * https://gamedev.stackexchange.com/questions/126427/draw-circle-around-gameobject-to-indicate-radius
     */
    public static void LineToCircle(GameObject obj, int axis, int segments = 50)
    {
        LineRenderer line = obj.GetComponent<LineRenderer>();
        //line.tag = "rotationcircle";
        line.useWorldSpace = false;
        line.material = (Material)Resources.Load("Materials/UI/GoetheBlauUnlit");


        //line.widthMultiplier = 0.005f;
        line.positionCount = segments + 1;
        line.useWorldSpace = false;

        float a;
        float b;
        float y_offset = 0f;
        /*
        BoxCollider box = obj.GetComponent<BoxCollider>();
        if(box != null)
        {
            y_offset = box.bounds.size.y / 2f;
        }*/

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {

            if(axis == 0) //Rotation y
            {
                a = Mathf.Sin(Mathf.Deg2Rad * angle) * 0.5f + 0.05f;
                b = Mathf.Cos(Mathf.Deg2Rad * angle) * 0.5f + 0.05f;
                line.SetPosition(i, new Vector3(a, y_offset, b));
                line.material.SetColor("_Color", Color.green);
                line.startWidth = 0.005f;
                line.endWidth = 0.05f;
            }
            else if (axis == 1) // Rotation x
            {
                a = Mathf.Sin(Mathf.Deg2Rad * angle) * 0.5f + 0.05f;
                b = Mathf.Cos(Mathf.Deg2Rad * angle) * 0.5f + 0.05f;
                line.SetPosition(i, new Vector3(0, a + y_offset, b));
                line.material.SetColor("_Color", Color.blue);
                line.startWidth = 0.05f;
                line.endWidth = 0.005f;
            }
            else if (axis == 2) //Rotation z
            {
                a = Mathf.Sin(Mathf.Deg2Rad * angle) * 0.5f + 0.05f;
                b = Mathf.Cos(Mathf.Deg2Rad * angle) * 0.5f + 0.05f;
                line.SetPosition(i, new Vector3(a, b + y_offset, 0));
                line.material.SetColor("_Color", Color.red);
                line.startWidth = 0.05f;
                line.endWidth = 0.005f;
            }
            angle += (360f / segments);
        }
    }


}

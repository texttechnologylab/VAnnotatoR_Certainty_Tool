using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Rcc8Test
{
    //Based on "Identification of Relations in Region Connection Calculus: 9-Intersection Reduced to 3*-Intersection Predicates"
    //private static readonly float round_down = 0.95f;
    private static Vector3 diff;
    //private static Vector3 round;
    //https://answers.unity.com/questions/1355096/how-to-get-the-8-vertices-coordinates-of-a-box-col.html
    public static String Rcc8(GameObject figure, GameObject ground)
    {
        BoxCollider o1_collider = figure.GetComponent<BoxCollider>();
        BoxCollider o2_collider = ground.GetComponent<BoxCollider>();

        List<float> round = new List<float>();
        round.Add(o1_collider.size.x * o1_collider.transform.localScale.x);
        round.Add(o1_collider.size.y * o1_collider.transform.localScale.y);
        round.Add(o1_collider.size.z * o1_collider.transform.localScale.z);
        round.Add(o2_collider.size.x * o2_collider.transform.localScale.x);
        round.Add(o2_collider.size.y * o2_collider.transform.localScale.y);
        round.Add(o2_collider.size.z * o2_collider.transform.localScale.z);
        float min = Mathf.Min(round.ToArray());
        diff = new Vector3(min, min, min) / 5f;
        string returnvalue;

        if (BndBnd(o1_collider, o2_collider))
        {
            if (IntBnd(o1_collider, o2_collider))
            {
                if (BndInt(o1_collider, o2_collider))
                {
                    returnvalue = "PO";
                }
                else
                {
                    returnvalue = "TPPc";
                }
            }
            else
            {
                if (BndInt(o1_collider, o2_collider))
                {
                    returnvalue = "TPP";
                }
                else
                {
                    if (IntInt(o1_collider, o2_collider))
                    {
                        returnvalue = "EQ";
                    }
                    else
                    {
                        returnvalue = "EC";
                    }
                }
            }
        }
        else
        {
            if (IntBnd(o1_collider, o2_collider))
            {
                returnvalue = "NTTPc";
            }
            else
            {
                if (BndInt(o1_collider, o2_collider))
                {
                    returnvalue = "IN";
                }
                else
                {
                    returnvalue = "DC";
                }
            }
        }

        return returnvalue;

    }

    private static Boolean BndBnd(BoxCollider o1_collider, BoxCollider o2_collider)
    {
        Boolean i1 = InCheck(o1_collider, o2_collider);
        Boolean i2 = InCheck(o2_collider, o1_collider);
        return !(i1 || i2);
    }

    private static Boolean IntBnd(BoxCollider o1_collider, BoxCollider o2_collider)
    {
        Vector3 v = o1_collider.size;
        o1_collider.size -= diff;
        Boolean inside = InCheck(o1_collider, o2_collider);
        o1_collider.size = v;

        if (inside) return false;

        ExtDebug.DrawBox(o1_collider.transform.position, o1_collider.size * o1_collider.transform.localScale.x * 0.5f - diff, o1_collider.transform.rotation, Color.green);

        Collider[] hitColliders = Physics.OverlapBox(o1_collider.transform.position, o1_collider.size * o1_collider.transform.localScale.x * 0.5f - diff, o1_collider.transform.rotation);
        foreach(Collider c in hitColliders)
            if (c == o2_collider)
                return true;

        return false;
    }

    private static Boolean BndInt(BoxCollider o1_collider, BoxCollider o2_collider)
    {
        return IntBnd(o2_collider, o1_collider);
    }

    private static Boolean IntInt(BoxCollider o1_collider, BoxCollider o2_collider)
    {
        return InCheck(o1_collider, o2_collider);
    }

    
    private static Boolean InCheck(BoxCollider o1_collider, BoxCollider o2_collider)
    {
        //Check, if o1 is in o2
        bool inCheck = true;
        //All CornerPoints of o1 need to be in o2
        foreach (Vector3 vert in AllObjWorldCorners(o1_collider))
            inCheck &= PointInOABB(vert, o2_collider);
        return inCheck;
    }

    private static Vector3[] AllObjWorldCorners(BoxCollider obj)
    {
        //https://forum.unity.com/threads/get-vertices-of-box-collider.89301/
        Vector3[] vertices = new Vector3[8];
        Transform trans = obj.transform;
        Vector3 min = obj.center - obj.size * 0.5f;
        Vector3 max = obj.center + obj.size * 0.5f;
        vertices[0] = trans.TransformPoint(new Vector3(min.x, min.y, min.z));
        vertices[1] = trans.TransformPoint(new Vector3(min.x, min.y, max.z));
        vertices[2] = trans.TransformPoint(new Vector3(min.x, max.y, min.z));
        vertices[3] = trans.TransformPoint(new Vector3(min.x, max.y, max.z));
        vertices[4] = trans.TransformPoint(new Vector3(max.x, min.y, min.z));
        vertices[5] = trans.TransformPoint(new Vector3(max.x, min.y, max.z));
        vertices[6] = trans.TransformPoint(new Vector3(max.x, max.y, min.z));
        vertices[7] = trans.TransformPoint(new Vector3(max.x, max.y, max.z));
        return vertices;
    }

    private static bool PointInOABB(Vector3 point, BoxCollider box)
    {
        //Vector3 v = box.size;
        //box.size *= round_down;
        Vector3 offset = box.bounds.center - point;
        Ray inputRay = new Ray(point, offset.normalized);

        RaycastHit rHit;

        bool ret = false;
        if (!box.Raycast(inputRay, out rHit, offset.magnitude * 1.1f))
            ret = true;
        //box.size = v;
        return ret;
    }
}

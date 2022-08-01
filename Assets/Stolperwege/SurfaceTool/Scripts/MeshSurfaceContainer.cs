using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSurfaceContainer {

    public HashSet<MeshSurface> Surfaces { get; private set; }
    public Vector3 Normal { get; private set; }

    public MeshSurfaceContainer(Vector3 normal)
    {
        Surfaces = new HashSet<MeshSurface>();
        Normal = normal;
    }

    //public void FindNeighbours(InfoSurface surface)
    //{
    //    foreach (InfoSurface other in Surfaces)
    //    {
    //        Debug.Log(other == null);
    //        if (other.Equals(surface) || surface.IsNeighbour(other) == null) continue;
    //        else
    //        {
    //            if (surface.Editor.ToMerge != null)
    //            {
    //                other.ConnectOnClick = true;
    //                other.MergeStatus = InfoSurface.MergingStatus.Mergable;
    //            } else
    //            {
    //                other.ConnectOnClick = false;
    //                other.MergeStatus = InfoSurface.MergingStatus.None;
    //            }
                
    //        }
    //    }
            

    //}
}

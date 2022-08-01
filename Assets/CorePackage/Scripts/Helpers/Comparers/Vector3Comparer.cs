using System.Collections.Generic;
using UnityEngine;

namespace VectorComparers
{
    
    public class Vector3Comparer : IComparer<Vector3>
    {
        public enum OnAxis { X, Y, Z};
        public OnAxis Axis;

        public Vector3Comparer(OnAxis axis)
        {
            Axis = axis;
        }

        public int Compare(Vector3 first, Vector3 second)
        {
            if (Axis == OnAxis.X)
                return (int)((first.x - second.x) / Mathf.Abs(first.x - second.x));
            else if (Axis == OnAxis.Y)
                return (int)((first.y - second.y) / Mathf.Abs(first.y - second.y));
            else
                return (int)((first.z - second.z) / Mathf.Abs(first.z - second.z));

        }
    }
    
}
using System.Collections.Generic;
using UnityEngine;

namespace VectorComparers
{
    
    public class ClockwiseVector3Comparer : IComparer<Vector3>
    {

        public int Compare(Vector3 first, Vector3 second)
        {
            float angle1 = Mathf.Atan2(first.x, first.y) * Mathf.Rad2Deg;
            float angle2 = Mathf.Atan2(second.x, second.y) * Mathf.Rad2Deg;

            if (angle1 < 0f) angle1 += 360;
            if (angle2 < 0f) angle2 += 360;

            return angle1.CompareTo(angle2);
        }
    }
    
}

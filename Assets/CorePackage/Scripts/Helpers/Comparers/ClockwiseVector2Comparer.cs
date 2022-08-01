using System.Collections.Generic;
using UnityEngine;

namespace VectorComparers
{
    
    public class ClockwiseVector2Comparer : IComparer<Vector2>
    {
        public int Compare(Vector2 v1, Vector2 v2)
        {
            if (v1.x >= 0)
            {
                return v2.x < 0
                    ? -1
                    : -Comparer<float>.Default.Compare(v1.y, v2.y);
            }
            else
            {
                return v2.x >= 0
                    ? 1
                    : Comparer<float>.Default.Compare(v1.y, v2.y);
            }
        }
    }
    
}
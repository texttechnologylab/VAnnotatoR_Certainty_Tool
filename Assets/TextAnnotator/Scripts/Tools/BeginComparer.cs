using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnnotationBaseComparers
{

    public class BeginComparer : IComparer<AnnotationBase>
    {

        public int Compare(AnnotationBase first, AnnotationBase second)
        {
            if (first.Begin < second.Begin) return -1;
            else if (first.Begin == second.Begin) return 0;
            return 1;
        }
    }

}


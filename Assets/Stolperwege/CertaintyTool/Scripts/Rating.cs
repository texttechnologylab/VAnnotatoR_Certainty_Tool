using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rating : MonoBehaviour
{
    public int Name;
    public int Steps;

    public int Lowest { get; private set; }
    public int Highest { get; private set; }
    
    public bool inBoundaries(int lower, int upper)
    {
        if (lower >= Lowest && upper <= Highest && lower <= upper) return true;
        else return false;
    }
}

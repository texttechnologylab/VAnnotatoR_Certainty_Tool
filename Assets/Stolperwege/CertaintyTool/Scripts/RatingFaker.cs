using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatingFaker : MonoBehaviour
{
    public BuildingController BuildingController { get; private set; }
    public void Init()
    {
        BuildingController = transform.gameObject.GetComponent<BuildingController>();
        BuildingController.SetRatingBoundaries(0, 6);
        BuildingController.SetShowBoundaries(0, 6);
        int i = 0;
        foreach (SubBuildingController Subco in BuildingController.Children)
        {
            Subco.SetRating(i, true);
            // Debug.Log("Set Fake Rating: " + i + " for Object " + Subco.transform.gameObject.name);
            if (i < 6) i++;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelateableElement : MonoBehaviour {

    protected void OnTriggerEnter(Collider other)
    {
 

        //Interaktionen mit dem Finger
        if (other.name.Contains("Finger"))
        {
            StolperwegeRelationAnchor anchor = other.GetComponentInChildren<StolperwegeRelationAnchor>();
            if (anchor != null && anchor.Relation)
            {
                anchor.transform.parent = transform;

                return;
            }

            GameObject path = (GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeRelation"));

            Transform start = path.transform.Find("StartAnchor").transform, end = path.transform.Find("EndAnchor").transform;

            start.parent = transform;
            start.localPosition = Vector3.zero;
            end.parent = other.transform;
            end.localPosition = Vector3.zero;
        
        }

        //if (other.name.Equals("StolperwegeObjectRemover") && grabbing.Count == 1)
        //    ResetObject();

    }
}

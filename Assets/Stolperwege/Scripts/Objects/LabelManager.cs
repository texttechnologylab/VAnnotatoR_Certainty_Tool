using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelManager : MonoBehaviour {

    List<InverseRotation> labels;

    public static Vector3 orientation = Vector3.zero;

	// Use this for initialization
	void Awake () {
        labels = new List<InverseRotation>();
	}

    public void AddLabel(InverseRotation label)
    {
        labels.Add(label);
        SetPosition(label);
    }

    public void RmvLabel(InverseRotation label)
    {
        labels.Remove(label);
    }

    Transform player;

    // Update is called once per frame
    void Update () {
        
        

        if (player == null)
            player = StolperwegeHelper.CenterEyeAnchor?.transform;

        foreach (InverseRotation rot in labels)
        {
            if (rot == null) continue;

            if (rot.hover == null)
            {
                if(rot.DestroyWhenHoverNull)
                    Destroy(rot.gameObject);
                continue;
            }
                

            if (player != null && (player.position - rot.hover.transform.position).magnitude > 5 && rot.GetComponent<MeshRenderer>() != null && rot.hover.gameObject.GetComponent<Renderer>()!= null)
                rot.GetComponent<MeshRenderer>().enabled = rot.hover.gameObject.activeInHierarchy && rot.hover.gameObject.GetComponent<Renderer>().enabled;

            SetPosition(rot);

            
        }
	}

    void SetPosition(InverseRotation rot)
    {
        if (rot.hover != null)
        {
            rot.transform.position = rot.hover.transform.position + Vector3.up * 0.15f * rot.Scale;
            Vector3 orientation = (rot.Orientation.Equals(Vector3.zero)) ? Vector3.forward : rot.Orientation;
                rot.transform.forward = (rot.Orientation == Vector3.zero && StolperwegeHelper.CenterEyeAnchor != null) ? StolperwegeHelper.CenterEyeAnchor.transform.forward: orientation;
            rot.transform.localScale = rot.normalScale * rot.Textsize;
            if (rot.GetComponent<MeshRenderer>() != null && rot.hover.gameObject.GetComponent<Renderer>() != null)
                rot.GetComponent<MeshRenderer>().enabled = rot.hover.gameObject.activeInHierarchy && rot.hover.gameObject.GetComponent<Renderer>().enabled;
        }
    }
}

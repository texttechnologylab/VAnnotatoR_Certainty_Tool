using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityObjectObject : NetworkObject
{

    private float lastAngle;

    public override void SetupContent()
    {
        //icon(temporary)
        string iconName = "StolperwegeElements/unityObjectIcon";
        GameObject icon = (GameObject)GameObject.Instantiate(Resources.Load(iconName));
        //todo:
        //miniature object instead of icon

        icon.transform.parent = this.transform;
        icon.transform.position = this.transform.position;
        icon.transform.rotation = this.transform.rotation;
        icon.transform.localPosition = Vector3.zero;
        icon.transform.localScale = Vector3.one;
        transform.GetChild(0).GetComponent<TextMesh>().text = "unityObject";
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        // remove list
        // TODO define requirements for adding this object to a list
        /*
        else if (other.tag.Equals("MenuTile") && grabbing.Count == 1)
        {
            if (parentObject == other) return;
            if (other.transform.parent != null && other.transform.parent.GetComponent<NetworkObjectController>() != null)
                other.transform.parent.GetComponent<NetworkObjectController>().AddObjects(Referent);
        }
        */
    }

    public override ExpandView OnExpand()
    {
        dragging = false;
        Grabbing.Clear();
        resetScale();
        return null;
    }
    
}

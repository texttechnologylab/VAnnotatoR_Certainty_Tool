using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserObject : NetworkObject
{

    public string TemporaryPermission;
    public bool SetContainsRelation = false;

    public override void SetupContent()
    {
        transform.GetChild(0).GetComponent<TextMesh>().text = Referent.Value;
        //IconLabel = transform.GetChild(1).gameObject;
        //if (Icon.GetComponent<Renderer>() != null) Icon.GetComponent<Renderer>().enabled = false;
        //for (int i = 0; i < Icon.transform.childCount; i++) Icon.transform.GetChild(i).GetComponent<Renderer>().enabled = false;

        //interactsWithTrashcan = true;
    }

    public override void SetParentObject(GameObject parentObject)
    {
        base.SetParentObject(parentObject);
        //VirtualRoom room = parentObject.transform.parent.GetComponent<networkMenuTileScript>().Room;
        //StolperwegeSet list = parentObject.transform.parent.GetComponent<networkMenuTileScript>().List;
        //if (room != null) interactsWithTrashcan = room.GetPermissionLevel() > 2;
        //if (list != null) interactsWithTrashcan = list.GetPermissionLevel() > 2;
    }

    //protected override void OnTriggerEnter(Collider other)
    //{
    //    base.OnTriggerEnter(other);

    //    if (other.tag.Equals("TrashcanBottom") && releasedToDelete) StartCoroutine(DeleteAccess());

    //}

    public override void OnDrag()
    {
        return;
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class NetworkObject : StolperwegeObject
{

    protected TextMeshPro IconLabel;
    protected new TextMeshPro Label;
    public Vector3 PositionOnWindow;
    public bool FlyBackTimerOverride = false;

    public override StolperwegeElement Referent
    {

        get
        {
            return _referent;
        }

        set
        {
            if (value == null) return;
            
            _referent = value;
            gameObject.name = _referent.Value;
            _referent.StolperwegeObject = this;
            RelationsVisible = false;

        }
    }

    public override void Awake()
    {
        base.Awake();
        Label = transform.Find("Label").GetComponent<TextMeshPro>();
        IconLabel = transform.Find("IconLabel").GetComponent<TextMeshPro>();
        LookAtUserOnHold = true;
    }


    public abstract void SetupContent();

    public override ExpandView OnExpand()
    {
        dragging = false;
        Grabbing.Clear();
        transform.parent = null;
        transform.localScale = _normalScale;
        //ActivateExpandedMode();
        return null;
    }

    protected IEnumerator DeleteNetworkObject()
    {
        // Delete first each permission, that the object owns
        if (Referent is StolperwegeSet)
        {
            foreach (StolperwegePermission permission in ((StolperwegeSet)Referent).SetOfPermissions)
            {
                yield return StolperwegeInterface.RemoveElement("permission", permission);
            }
        }

        if (Referent is VirtualRoom)
        {
            foreach (StolperwegePermission permission in ((VirtualRoom)Referent).SetOfPermissions)
            {
                yield return StolperwegeInterface.RemoveElement("permission", permission);
            }
        }

        // Remove the Object in the 3D World from the list and delete the discoursereferent from the server
        //if (ParentObject != null) ParentObject.transform.parent.GetComponent<networkMenuTileScript>().RemoveObject(gameObject);
        yield return StolperwegeInterface.RemoveElement("discoursereferent", Referent);
        Destroy(gameObject);
    }

    protected IEnumerator DeleteAccess()
    {

        // Deleting operations on the Server-Side
        if (Referent is StolperwegeSet)
        {
            // if the element is a virtual room, delete the current user permission to it and remove him from the list
            yield return StolperwegeInterface.RemoveElement("permission", ((StolperwegeSet)Referent).PermissionOfCurrentUser);
            yield return StolperwegeInterface.RemoveRelation(StolperwegeInterface.CurrentUser, Referent, StolperwegeInterface.ContainsRelation);
        }

        if (Referent is VirtualRoom)
        {
            // if the element is a virtual room, delete the current user permission to it
            yield return StolperwegeInterface.RemoveElement("permission", ((VirtualRoom)Referent).PermissionOfCurrentUser);
        }

        if (Referent is StolperwegeUser)
        {
            //if (ParentObject != null && ParentObject.transform.parent.GetComponent<networkMenuTileScript>().Network3DObject != null)
            //{
            //    networkMenuTileScript tile = ParentObject.transform.parent.GetComponent<networkMenuTileScript>();

            //    // if the user is attached to a room, delete his permission to it
            //    if (tile.Room != null)
            //    {
            //        foreach (StolperwegePermission permission in tile.Room.PermissionElementMap.Keys)
            //        {
            //            if (tile.Room.PermissionElementMap[permission].Contains(Referent))
            //                yield return StolperwegeInterface.RmvRelation(Referent, permission, StolperwegeInterface.EquivalentRelation);
            //        }
            //    }

            //    // if the user is attached to a list, delete his permission to it, and also him from the list
            //    if (tile.List != null)
            //    {
            //        foreach (StolperwegePermission permission in tile.List.PermissionElementMap.Keys)
            //        {
            //            if (tile.Room.PermissionElementMap[permission].Contains(Referent))
            //                yield return StolperwegeInterface.RmvRelation(Referent, permission, StolperwegeInterface.EquivalentRelation);
            //        }
            //        if (tile.List.SetOfContainedElements.Contains(Referent))
            //        {
            //            tile.List.SetOfContainedElements.Remove(Referent);
            //            yield return StolperwegeInterface.RmvRelation(tile.List, Referent, StolperwegeInterface.ContainsRelation);
            //        }
            //    }
            //}
        }

        // Removing in the 3D World
        //if (ParentObject != null) ParentObject.transform.parent.GetComponent<networkMenuTileScript>().RemoveObject(gameObject);
        Destroy(gameObject);

    }

    protected override void LookAtUser()
    {
        Label.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
        Label.transform.Rotate(Vector3.up, 180, Space.Self);
        IconLabel.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
        IconLabel.transform.Rotate(Vector3.up, 180, Space.Self);
    }
}

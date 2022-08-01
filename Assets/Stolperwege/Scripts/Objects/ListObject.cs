using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ListObject : NetworkObject
{

    public string TemporaryPermission;
    public bool SetContainsRelation = false;

    private bool MessageActive = false;

    public override void SetupContent()
    {
        
        IconLabel.text = "\xf0cb";
        Label.text = Referent.Value;
        // TODO
        //deletingConfirmationNeeded = true;
        //interactsWithTrashcan = (!NetworkManagement.listNameMap.ContainsKey(name));
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        // TODO
        //if (other.tag.Equals("TrashcanBottom") && releasedToDelete)
        //{
        //    StolperwegePermission permission = ((StolperwegeSet)Referent).PermissionOfCurrentUser;

        //    if (((StolperwegeSet)Referent).GetPermissionLevel() < 3) StartCoroutine(DeleteAccess());
        //    else StartCoroutine(DeleteNetworkObject());
        //}
    }

    public override void OnDrag()
    {
        if (!Referent.Linked) Referent.Link();
        if (((StolperwegeSet)Referent).PermissionOfCurrentUser == null)
        {
            if (!MessageActive) StolperwegeHelper.StatusBox.SetInfoText("Sie haben keine Rechte für diese Liste.", true, 1.5f);
            return;
        }
        base.OnDrag();
    }

    public override bool OnDrop(Collider other)
    {
        bool result = base.OnDrop(other);
        if (MessageActive)
        {
            StolperwegeHelper.StatusBox.Reset();
            MessageActive = false;
        }
        return result;
    }

}

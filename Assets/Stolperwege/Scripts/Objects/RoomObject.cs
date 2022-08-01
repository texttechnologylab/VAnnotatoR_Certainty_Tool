using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Representation of virtualrooms as the visible UnityObject.
/// </summary>
public class RoomObject : NetworkObject {

    //private bool MessageActive = false;
    //private GameObject Portal;
    //private float PortalActivationTimer;
    //private bool PortalActivated = false;

    public override void Start()
    {
        base.Start();
        SetupContent();
    }

    public override void SetupContent()
    {
        IconLabel.text = "\xf52a";
        Label.text = Referent.Value;
    }

    //public void Update()
    //{

    //    if (!IsGrabbed && Portal != null && !PortalActivated)
    //    {
    //        Destroy(Portal);
    //        PortalActivationTimer = 0;
    //    }
    //}

    //protected override void OnTriggerEnter(Collider other)
    //{
    //    base.OnTriggerEnter(other);

    //    if (other.tag.Equals("TrashcanBottom") && releasedToDelete)
    //    {
    //        if (((VirtualRoom)Referent).GetPermissionLevel() < 3) StartCoroutine(DeleteAccess());
    //        else StartCoroutine(DeleteNetworkObject());
    //    }

    //    if (other.name.Contains("Finger") && IsGrabbed)
    //    {
    //        if (Portal == null) Portal = (GameObject)Instantiate(Resources.Load("portal"));
    //        Portal.GetComponent<portalController>().SetTransparency(0);
    //    }  
            
    //}

    //protected override void OnTriggerStay(Collider other)
    //{
    //    base.OnTriggerStay(other);

    //    if (other.name.Contains("Finger") && IsGrabbed && !PortalActivated)
    //    {
    //        PortalActivationTimer += Time.deltaTime;
    //        if (PortalActivationTimer > 2)
    //        {
    //            Portal.GetComponent<portalController>().SwitchingRoom();
    //            Portal.GetComponent<portalController>().CurrentRoom = (VirtualRoom)Referent; // Leads to change of active virtualroom in preferences.
    //            PortalActivated = true;
    //            StolperwegeHelper.netManagement.networkMenuScript.IsActive = false;
    //        }
    //        ActualizePortalStatus();
    //    }
    //}

    //protected override void OnTriggerExit(Collider other)
    //{
    //    base.OnTriggerExit(other);

    //    if (other.name.Contains("Finger") && Portal != null)
    //    {
    //        Destroy(Portal);
    //        PortalActivated = false;
    //        PortalActivationTimer = 0;
    //    }
    //}

    //private void ActualizePortalStatus()
    //{
    //    if (Portal == null) return;

    //    Vector2 Object2DPos = new Vector2(transform.position.x, transform.position.z);
    //    Vector2 CenterEye2DPos = new Vector2(centerEye.position.x, centerEye.position.z);

    //    Portal.transform.LookAt(new Vector3(CenterEye2DPos.x, Portal.transform.position.y, CenterEye2DPos.y));
    //    Vector2 normalized = (Object2DPos - CenterEye2DPos).normalized * 2;
    //    Portal.transform.position = new Vector3(normalized.x, Portal.transform.position.y, normalized.y);

    //    float transparency = Mathf.Min(PortalActivationTimer / 2, 1);
    //    Portal.GetComponent<portalController>().SetTransparency(transparency);
        
    //}

    //public override bool OnDrop(Collider other)
    //{
    //    bool result = base.OnDrop(other);
    //    if (MessageActive)
    //    {
    //        StolperwegeHelper.
    //        StolperwegeHelper.messageBox.RemoveActualInfoMessage();
    //        MessageActive = false;
    //    }
    //    return result;
    //}
}

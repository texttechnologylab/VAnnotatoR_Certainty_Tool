using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VR = UnityEngine.VR;
using UnityEngine.AI;
using System;
using Valve.VR;

public class PointFingerColliderScript : PointFinger
{
 
    private HandAnimator Hand;

    public override bool IsPointing { get { return Hand != null && Hand.IsPointing; } }

    public override bool LeftHand { get { return tag.Equals("leftIndexFinger"); } }
    public override bool RightHand { get { return tag.Equals("rightIndexFinger"); } }
    public override bool IsClicking { get { return SteamVR_Actions.default_trigger.GetStateDown(StolperwegeHelper.User.PointerHandType); } }

    public override bool IsHoldingClick { get { return SteamVR_Actions.default_trigger.GetState(StolperwegeHelper.User.PointerHandType); } }

    public override bool IsReleasing { get { return SteamVR_Actions.default_trigger.GetStateUp(StolperwegeHelper.User.PointerHandType); } }

    protected override Vector3 PointingDirection { get  { return Quaternion.AngleAxis(45, Hand.transform.right) * Hand.transform.forward; } }

    protected override Vector3 FingerTip { get { return transform.position; } }

    protected void Start()
    {
        Hand = GetComponentInParent<HandAnimator>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        GetComponent<SphereCollider>().enabled = Hand != null && IsPointing;
    }

    public void setHapticFeedback(float strength, int length = 5)
    {
        // TODO
        //InputInterface.setVibration(controller, strength, length);
    }
}

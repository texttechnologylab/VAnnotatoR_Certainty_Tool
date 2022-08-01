using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VR = UnityEngine.VR;
using Valve.VR;


public class DragFingerColliderScript : DragFinger
{
    private HandAnimator Hand;
    public bool colliderOverrided = false;
    public override bool LeftHand { get { return tag.Equals("leftArm"); } }

    public void Awake()
    {
        Hand = GetComponent<HandAnimator>();
        if (tag.Equals("leftArm")) StolperwegeHelper.LeftFist = this;
        if (tag.Equals("rightArm")) StolperwegeHelper.RightFist = this;
        
    }

    public override bool IsGrabbing { get { return Hand.IsGrabbing; } }

    protected override void Update()
    {
        base.Update();
        GetComponent<SphereCollider>().enabled = Hand != null && IsGrabbing;        
    }




    public override void SetHapticFeedback(float strength)
    {
        // TODO
    }

    public override void SetHapticFeedback(float strength, float length)
    {
        // TODO
    }

}

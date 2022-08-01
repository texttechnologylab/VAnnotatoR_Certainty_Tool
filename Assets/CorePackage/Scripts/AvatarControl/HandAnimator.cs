using System.Collections;
using System.Collections.Generic;
using static StolperwegeHelper;
using UnityEngine;
using Valve.VR;

public class HandAnimator : MonoBehaviour
{

    private Animator Animator;
    private SteamVR_Input_Sources Controller;

    public int ActualPose { get; private set; }
    public bool IsHolding { get { return HoldedObject != null; } }
    public bool IsGrabbing { get { return ActualPose == 1; } }
    public bool IsPointing { get { return ActualPose == 0; } }
    public GameObject HoldedObject { get; set; }
    public bool FixPose = false;
    

    // Start is called before the first frame update
    void Start()
    {
        Animator = GetComponent<Animator>();
        Animator.speed = 5;
        ActualPose = 0;
        Controller = tag.Contains("left") ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
    }

    // Update is called once per frame
    void Update()
    {
        if (FixPose) return;
        if (SteamVR_Actions.default_grab.GetStateDown(Controller) && (User.PointerHand == null || 
            User.PointerHand.Hit == null || User.PointerHand.GetComponentInParent<DragFinger>().HasGrabableObjectsInRange || 
            (!User.PointerHand.Hit.SupportsDistanceGrab)))
        {
            Animator.SetBool("Grab", true);
            ActualPose = 1;
        }

        if (SteamVR_Actions.default_grab.GetStateUp(Controller))
        {
            Animator.SetBool("Grab", false);
            ActualPose = 0;
        }
    }
}

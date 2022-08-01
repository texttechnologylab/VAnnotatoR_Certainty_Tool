using HTC.UnityPlugin.StereoRendering;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Place this script on your gazeCamera.
 * ...
 */


public class SimpleGazeCursor : MonoBehaviour {

    public Camera gazeCamera;

    public GameObject HittedObject { get; private set; }
    public InteractiveObject lastHit { get; private set; }
    public InteractiveObject activeParent { get; private set; }
    public Vector3 LookNormal { get; private set; }
    public Ray Ray { get; private set; }
    public RaycastHit Hit;

    public void Start()
    {
        StolperwegeHelper.Gaze = this;
    }

    // Update is called once per frame

    
    void Update () {
        if (StolperwegeHelper.User.PointerHand.IsPointing && StolperwegeHelper.User.PointerHand.Hit != null)
        {
            ResetLastHit();
            return;
        }
            
        Ray = gazeCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        LookNormal = Ray.direction.normalized;

        if (Physics.Raycast(Ray, out Hit, Mathf.Infinity))
        {
            HittedObject = Hit.transform.gameObject;

            if (Hit.collider.GetComponent<InteractiveObject>() != null)
            {               
                if (lastHit != null)
                {
                    if (Hit.collider.gameObject == lastHit.gameObject)
                    {
                        if (lastHit.TriggerOnFocus)
                        {
                            lastHit.OnFocus?.Invoke(Hit.point);                            
                            lastHit.CheckClick();
                        }                        
                        return;
                    }

                    ResetLastHit();

                }
                
                lastHit = Hit.collider.gameObject.GetComponent<InteractiveObject>();
                if (lastHit.TriggerOnFocus)
                {
                    if (lastHit.BlockRotationOnFocus) StolperwegeHelper.User.RotationBlocked = true;
                    //lastHit.Highlight = true;
                    lastHit.CheckClick();
                }
                

            }
            else
            {                
                ResetLastHit();
            }

        }
        else
        {
            ResetLastHit();
            HittedObject = null;
        }
    }
    
    private void ResetLastHit()
    {
        if (lastHit != null)
        {
            if (lastHit.StatusBoxTriggered)
                lastHit.ShutDownStatusBox();
            if (lastHit.BlockRotationOnFocus) StolperwegeHelper.User.RotationBlocked = false;
            lastHit.Highlight = false;
            lastHit = null;
        }
    }

    RaycastHit _boxHit; Vector3 _boxCenter; int layerMask = 1 << 19;
    public RaycastHit BoxCast(Vector3 boxSize, Quaternion boxRotation)
    {
        _boxCenter = transform.position + Vector3.up * boxSize.y;// + LookNormal * 0.5f;
        _boxHit = default;
        if (Physics.BoxCast(_boxCenter, boxSize / 2, LookNormal, out _boxHit, boxRotation, Mathf.Infinity, layerMask))
            return _boxHit;
        return default;
    }

    public bool HitsObject(InteractiveObject io)
    {
        return lastHit != null && lastHit.Equals(io);
    }
}

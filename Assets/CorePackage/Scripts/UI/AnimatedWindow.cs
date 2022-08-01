using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StolperwegeHelper;
using Valve.VR;

public class AnimatedWindow : InteractiveObject {
    
    protected bool _active = false;
    protected bool _animOn;
    protected Vector3 HiddenPos;
    protected Vector3 TargetPos;
    private float _animLerp;
    private Vector3 playerPosOnActivation;
    protected float SpawnDistanceMultiplier = 1;
    private bool _draggedByPointer;

    /// <summary>
    /// Defines if the is visible or not and prepares the animation of (dis)appearance
    /// </summary>
    public virtual bool Active
    {
        get
        {
            return _active;
        }
        set
        {

            if (value == _active) return;

            _active = value;
            _animLerp = 0;

            Vector3 horizontalLookDir = Vector3.Scale(CenterEyeAnchor.transform.forward, new Vector3(1, 0, 1));
            horizontalLookDir.Normalize();

            TargetPos = CenterEyeAnchor.transform.position + horizontalLookDir * SpawnDistanceMultiplier;
            HiddenPos = TargetPos + Vector3.up * 4;

            if (_active)
            {

                SetVisible(true);
                transform.position = HiddenPos;
                transform.LookAt(new Vector3(CenterEyeAnchor.transform.position.x,
                                             transform.position.y,
                                             CenterEyeAnchor.transform.position.z));
                playerPosOnActivation = User.transform.position;
            }
            _animOn = true;
        }
    }

    public bool HideOnMovement;
    private bool _initialized = false;

    // Use this for initialization
    public override void Start ()
    {
        if (_initialized) return;
        SearchForParts = false;
        base.Start();
        LookAtUserOnHold = true;
        KeepYRotation = true;
        Grabable = true;
        Removable = true;
        DestroyOnObjectRemover = true;
        ManipulatedByGravity = false;
        isThrowable = false;
        UseHighlighting = false;
        OnDistanceGrab = ControlWindowPosition;
        _initialized = true;
    }
	
	// Update is called once per frame
	public virtual void Update ()
    {

        if (Active && HideOnMovement && User.transform.position != playerPosOnActivation)
            Active = false;

        if (_animOn)
        {
            _animLerp += Time.deltaTime;
            if (Active) Show();
            else Hide();
        }
    }

    public void SetVisible(bool visible)
    {
        for (int c = 0; c < transform.childCount; c++)
            transform.GetChild(c).gameObject.SetActive(visible);
    }

    /// <summary>
    /// The animation, that defines how the window appears.
    /// This method will be called in the update function until the target position of the window is reached.
    /// </summary>
    private void Show()
    {
        transform.position = Vector3.Slerp(transform.position, TargetPos, _animLerp);
        if (transform.position == TargetPos)
        {
            _animLerp = 0;
            _animOn = false;
        }
    }

    /// <summary>
    /// The animation, that defines how the window disappears.
    /// This method will be called in the update function until the target position of the window is reached.
    /// </summary>
    private void Hide()
    {
        transform.position = Vector3.Slerp(transform.position, HiddenPos, _animLerp);
        if (transform.position == HiddenPos)
        {
            _animLerp = 0;
            _animOn = false;
            SetVisible(false);
        }
    }

    private IEnumerator ControlWindowPosition()
    {
        _draggedByPointer = true;
        transform.SetParent(User.PointerHand.transform);
        transform.position = PointerSphere.transform.position;
        float actualDistance, newDistance, touchInput; Vector3 lookDir;
        while (SteamVR_Actions.default_grab.GetState(User.PointerHandType))
        {
            lookDir = User.CenterEyeAnchor.transform.position;
            lookDir.y = transform.position.y;
            transform.LookAt(lookDir);
            touchInput = SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).y;
            if (touchInput != 0)
            {
                actualDistance = (User.PointerHand.transform.position - transform.position).magnitude;
                newDistance = Mathf.Max(0.5f, Mathf.Min(actualDistance + touchInput, 5));
                transform.position -= PointerPath.transform.up * (newDistance - actualDistance) / 10;
            }
            yield return null;
        }
        transform.SetParent(null);
        _draggedByPointer = false;
    }

    protected override void CheckScrolling(Vector3 hitPoint)
    {
        if (_draggedByPointer) return;
        base.CheckScrolling(hitPoint);
    }

}

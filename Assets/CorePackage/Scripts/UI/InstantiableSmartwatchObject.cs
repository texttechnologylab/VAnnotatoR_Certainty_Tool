using System.Collections;
using static StolperwegeHelper;
using UnityEngine;
using Valve.VR;

public abstract class InstantiableSmartwatchObject : InteractiveObject
{

    public static string PrefabPath;
    public static string Icon { get; protected set; } = "hallo";

    public Vector3 ShrinkedSize { get; protected set; } = Vector3.one * 0.2f;
    public Vector3 ExpandedSize { get; protected set; } = Vector3.one;

    /// <summary>
    /// This will be setted true after the object has reached his normal size (after drag&drop out of the smartwatch). 
    /// </summary>
    private bool Expanded;
    private bool _draggedByPointer;


    public virtual void Initialize()
    {
        OnDistanceGrab = ControlPosition;
    }


    public override bool OnDrop()
    {
        if (!Expanded)
        {
            Expanded = true;
            StartCoroutine(Expand());
        }
        return base.OnDrop();
    }

    /// <summary>
    /// This method defines the expanding animation of the object, after it was dropped for the first time, after pulling it out of the smartwatch.
    /// </summary>
    private float _animLerp;
    protected virtual IEnumerator Expand()
    {
        while (transform.localScale != ExpandedSize)
        {
            _animLerp += Time.deltaTime;
            transform.localScale = Vector3.Lerp(transform.localScale, ExpandedSize, _animLerp);
            yield return null;
        }

        Smartwatch.InstancedObject = null;
    }



    private IEnumerator ControlPosition()
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

using UnityEngine;
using System.Collections.Generic;

public abstract class DragFinger : MonoBehaviour
{
    private readonly float GRAB_DISTANCE = 0.05f;
    public abstract bool IsGrabbing { get; }

    public static float ThrowForceMultiplier = 20;
    public abstract bool LeftHand { get; }

    public SphereCollider Collider { get { return GetComponent<SphereCollider>(); } }

    public Vector3 ColliderPosition 
    {
        get { return Collider.center; }
    }

    public InteractiveObject GrabedObject;
    public bool HasGrabableObjectsInRange { get { return GetNearObjects().Count > 0; } }

    private bool switchedPose = false;
    private float grabTime = 0;


    // Update is called once per frame
    protected virtual void Update()
    {

        if (IsGrabbing)
        {
            if (GrabedObject != null)
            {
                if (GrabedObject.isThrowable) GrabedObject.ActualizeThrowingVariables(this);
                if (GrabedObject is ARDisplay) ((ARDisplay)GrabedObject).CheckRay();
                GrabedObject.OnHold?.Invoke();
            }
                
            if (!switchedPose)
            {
                switchedPose = true;
                CheckGrab();
            }
            
        }
        else
        {
            switchedPose = false;

            if (GrabedObject == null) return;      
            if (Time.time - grabTime < 0.5f) GrabedObject.OnDrop(GetComponent<SphereCollider>());
            else GrabedObject.OnRelease(GetComponent<SphereCollider>());

            GrabedObject.OnDrop();
            StolperwegeHelper.User.DroppedObjects.Add(GrabedObject);

            GrabedObject = null;
        }

    }
    
    private void CheckGrab()
    {

        GetNearObjects();

        if (_interactives.Count > 0)
        {
            _interactives.Sort((a, b) =>
            {
                float dist1 = (a.transform.position - transform.position).magnitude;
                float dist2 = (b.transform.position - transform.position).magnitude;
                return dist1.CompareTo(dist2);
            });
            if (GrabedObject != null) GrabedObject.OnDrop(GetComponent<SphereCollider>());
            GrabedObject = _interactives[0];
            GrabedObject.OnGrab(GetComponent<Collider>());
        }
        grabTime = Time.time;
    }

    List<InteractiveObject> _interactives;
    private List<InteractiveObject> GetNearObjects()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, GRAB_DISTANCE);

        _interactives = new List<InteractiveObject>();

        foreach (Collider c in colliders)
        {
            InteractiveObject io = c.GetComponent<InteractiveObject>();
            bool avatar_in_collider = c.bounds.Contains(StolperwegeHelper.User.Head.transform.position);
            if (io != null && io.Grabable && !avatar_in_collider) _interactives.Add(io);
        }
        return _interactives;
    }

    public abstract void SetHapticFeedback(float strength);

    public abstract void SetHapticFeedback(float strength, float length);
}
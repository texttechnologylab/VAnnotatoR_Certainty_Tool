using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLever : InteractiveObject
{

    private float MinHeight = 0;
    private float MaxHeight = 0.35f;
    private Transform Socket;
    private Tutorial Tutorial;

    public void Init(Tutorial tutorial)
    {
        Tutorial = tutorial;
        Socket = transform.parent;
    }

    public override bool OnDrop()
    {
        base.OnDrop();
        return true;
    }


    protected override void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Arm") && other.GetComponent<DragFinger>().GrabedObject == null)
        {
            other.GetComponent<HandAnimator>().HoldedObject = gameObject;
        }
    }

    private Vector3 inversePos;
    protected override void OnTriggerStay(Collider other)
    {
        if (other.name.Contains("Arm") && other.GetComponent<DragFinger>().GrabedObject == null)
        {
            inversePos = Socket.InverseTransformPoint(other.transform.position);
            SetPosition(inversePos.y);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.name.Contains("Arm") && other.transform.GetComponent<DragFinger>().Equals(GrabHand))
        {
            other.GetComponent<HandAnimator>().HoldedObject = null;
        }
    }

    public void ResetLever()
    {
        transform.localPosition = Vector3.up * MaxHeight;
    }

    public void SetPosition(float y)
    {

        transform.localPosition = Vector3.up * Mathf.Min(MaxHeight, Mathf.Max(MinHeight, y));
        Tutorial.SetDoor2Position(Mathf.Min(MaxHeight, Mathf.Max(MinHeight, y)) / MaxHeight);
    }
}

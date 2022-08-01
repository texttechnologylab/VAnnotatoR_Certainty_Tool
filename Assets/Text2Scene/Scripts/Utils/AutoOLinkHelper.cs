using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AutoOLinkHelper : MonoBehaviour
{
    // Start is called before the first frame update
    BoxCollider _collider;
    Rigidbody _rigidbody;

    Dictionary<InteractiveShapeNetObject, string> contactObjects = new Dictionary<InteractiveShapeNetObject,string>();
    void Start()
    {
        if (gameObject.GetComponent<BoxCollider>() != null)
            _collider = GetComponent<BoxCollider>();
        else
            _collider = gameObject.AddComponent<BoxCollider>();
        _collider.enabled = true;
        _collider.isTrigger = true;

        if (gameObject.GetComponent<Rigidbody>() != null)
            _rigidbody = GetComponent<Rigidbody>();
        else
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.isKinematic = true;

        contactObjects = new Dictionary<InteractiveShapeNetObject, string>();
    }

    private void Update()
    {
                InteractiveShapeNetObject[] keys = contactObjects.Keys.ToArray<InteractiveShapeNetObject>();
        //Ich hasse diese Lösung, aber ich bekomme es beim Besten Willen nicht hin, wenn die objekte stehen .....

        foreach (InteractiveShapeNetObject iso in keys)
        {

            contactObjects[iso] = Rcc8Test.Rcc8(iso.gameObject, gameObject);
        }
    }

    public Dictionary<InteractiveShapeNetObject, string> GetAllConnectedObjects()
    {
        return contactObjects;
    }

    public void ActivateAllConnectedColliderFrames(bool activate)
    {
        foreach(InteractiveShapeNetObject iso in contactObjects.Keys){
            iso.ActivateColliderFrame(activate);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        InteractiveShapeNetObject iso = other.gameObject.GetComponent<InteractiveShapeNetObject>();
        if (iso != null)
        {
            contactObjects.Add(iso, Rcc8Test.Rcc8(other.gameObject, gameObject));
            iso.ActivateColliderFrame(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {

        InteractiveShapeNetObject iso = other.gameObject.GetComponent<InteractiveShapeNetObject>();
        if (iso != null)
        {
            contactObjects[iso] = Rcc8Test.Rcc8(other.gameObject, gameObject);
        }


    }

    private void OnTriggerExit(Collider other)
    {
        InteractiveShapeNetObject iso = other.gameObject.GetComponent<InteractiveShapeNetObject>();
        if (iso != null)
        {
            contactObjects.Remove(iso);
            iso.ActivateColliderFrame(false);
        }
    }

    private void OnDestroy()
    {
        Destroy(_rigidbody);
    }
}

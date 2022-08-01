using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AutoSpatialLinkHelper : MonoBehaviour
{
    // Start is called before the first frame update
    BoxCollider _collider;
    Rigidbody _rigidbody;

    HashSet<InteractiveShapeNetObject> contactObjects = new HashSet<InteractiveShapeNetObject>();

    Dictionary<InteractiveShapeNetObject, string> olinkObjects = new Dictionary<InteractiveShapeNetObject, string>();

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

        contactObjects = new HashSet<InteractiveShapeNetObject>();
        olinkObjects = new Dictionary<InteractiveShapeNetObject, string>();
    }

    private void FixedUpdate()
    {
        ActivateOLinkFrames(false);

        if (((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<ObjectTab>().CheckOLinkAutoCreation)
        {
            BoxCollider figure_collider = gameObject.GetComponent<BoxCollider>();

            ExtDebug.DrawBox(transform.position, figure_collider.bounds.size * 3, gameObject.transform.rotation, Color.magenta);

            Collider[] hitColliders = Physics.OverlapBox(transform.position, figure_collider.bounds.size*4, transform.rotation);
            olinkObjects.Clear();
            ObjectTab tab = ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<ObjectTab>();

            //Select for a relationtype only the object with the lowest distance.
            //Dictionary<string, float> typedistance = new Dictionary<string, float>();

            for (int i = 0; i < hitColliders.Length; i++)
            {
                InteractiveShapeNetObject obj_in_olink_range = hitColliders[i].gameObject.GetComponent<InteractiveShapeNetObject>();


                if (obj_in_olink_range != null && !contactObjects.Contains(obj_in_olink_range) && hitColliders[i].gameObject != gameObject) { 
                    string olinkrel = null;
                    if (tab.frameMode == "absolut")
                        olinkrel = OLinkTest.AbsoluteRelationCheck(gameObject, hitColliders[i].gameObject);
                    else if (tab.frameMode == "intrinsic")
                        olinkrel = OLinkTest.RelationCheck(gameObject, hitColliders[i].gameObject, false);
                    else if (tab.frameMode == "relative" && tab.referencePt != null)
                        olinkrel = OLinkTest.RelationCheck(gameObject, hitColliders[i].gameObject, true, tab.referencePt.Object3D);
                    else if (tab.frameMode == "undefined")
                        olinkrel = OLinkTest.RelationCheck(gameObject, hitColliders[i].gameObject, true, StolperwegeHelper.User.gameObject);

                    if (olinkrel != null)
                    {
                        /*
                        float dist = Vector3.Distance(obj_in_olink_range.Object_center, gameObject.GetComponent<InteractiveShapeNetObject>().Object_center);

                        if (typedistance.ContainsKey(olinkrel))
                        {
                            if(dist < typedistance[olinkrel])
                            {
                                olinkObjects.
                            }
                        }
                        else
                        {
                            olinkObjects[obj_in_olink_range] = olinkrel;
                            typedistance[olinkrel] = dist;
                        }*/
                        olinkObjects[obj_in_olink_range] = olinkrel;
                    }   
                }
            }
            ActivateOLinkFrames(true);
        }
    }


    public HashSet<InteractiveShapeNetObject> GetAllQSConnectedObjects()
    {
        return contactObjects;
    }

    public Dictionary<InteractiveShapeNetObject, string> GetAllOConnectedObjects()
    {
        return olinkObjects;
    }


    public void ActivateAllConnectedColliderFrames(bool activate)
    {
        ActivateOLinkFrames(activate);
        ActivateQSLinkFrames(activate);

    }

    private void ActivateQSLinkFrames(bool activate)
    {
        foreach (InteractiveShapeNetObject iso in contactObjects)
            iso.ActivateColliderFrame(activate);
    }

    private void ActivateOLinkFrames(bool activate)
    {
        foreach (InteractiveShapeNetObject iso in olinkObjects.Keys)
            if(!contactObjects.Contains(iso) || activate)
                iso.ActivateColliderFrame(activate);
    }

    private void OnTriggerEnter(Collider other)
    {
        InteractiveShapeNetObject iso = other.gameObject.GetComponent<InteractiveShapeNetObject>();
        if (iso != null)
        {
            contactObjects.Add(iso);

            if(((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<ObjectTab>().CheckQSLinkAutoCreation)
                iso.ActivateColliderFrame(true);
        }
    }

    /*
    private void OnTriggerStay(Collider other)
    {
        InteractiveShapeNetObject iso = other.gameObject.GetComponent<InteractiveShapeNetObject>();
        if (iso != null)
            Debug.Log(Rcc8Test.Rcc8(gameObject, other.gameObject));
    }
    */

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

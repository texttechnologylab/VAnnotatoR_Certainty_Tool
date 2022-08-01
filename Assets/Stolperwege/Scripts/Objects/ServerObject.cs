using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ServerObject : NetworkObject {

    private bool _colliderLocked;

    public string ModelName;


    // Use this for initialization
    public override void Start () 
    {
        if (gameObject.name == "DummyServerObject")
        {
            ModelName = "https://resources.hucompute.org/download/3908?session=99CBA8F6F6357A2FE512B5E549D9CE06.jvm1";
            Debug.Log("dummy server object model link set to: " + GameObject.Find("DummyServerObject").GetComponent<ServerObject>().ModelName);
        }
        else if (gameObject.name == "DummyServerObject2")
        {
            ModelName = "https://resources.hucompute.org/download/3860?session=99CBA8F6F6357A2FE512B5E549D9CE06.jvm1";
            //model = "https://resources.hucompute.org/download/4037?session=99CBA8F6F6357A2FE512B5E549D9CE06.jvm1";
            Debug.Log("dummy server object2 model link set to: " + GameObject.Find("DummyServerObject2").GetComponent<ServerObject>().ModelName);
        }
        MeshCollider collider = gameObject.AddComponent<MeshCollider>() as MeshCollider;
        collider.convex = true;

        InteractiveObject[] scripts = GetComponents<InteractiveObject>();
        /*
        foreach (InteractiveObject script in scripts)
        {
            if (!(script is ServerObject)) script.enabled = false;
        }
        */
    }

    public override void SetupContent()
    {
        // Does nothing
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other.name.Contains("Finger") && IsGrabbed && !_colliderLocked)
        {
            _colliderLocked = true;
            GameObject instance = Instantiate(gameObject);
            instance.GetComponent<ServerObject>().isThrowable = false;

        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (other.name.Contains("Finger")) _colliderLocked = false;
    }


    //add additional functions here:



}

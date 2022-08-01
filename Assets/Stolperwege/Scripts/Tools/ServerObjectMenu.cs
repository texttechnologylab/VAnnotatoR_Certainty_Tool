using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ServerObjectMenu : NetworkBehaviour {
    public GameObject sphereObject;
    public GameObject plane;
    public bool useDummyData = true;
    public HashSet<GameObject> sphereList = new HashSet<GameObject>();

    // Use this for initialization
    void Start () {
        //get all server objects
        //if(useDummyData == true) fillMenuWithDummies(4);
        allignObjects(sphereList);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void GetAllServerObjects()
    {

    }

    //public void fillMenu(StolperwegeUnityObject[] stolperwegeUnityObjects)
    //{
    //    sphereList.Clear();
    //    foreach(StolperwegeUnityObject suo in stolperwegeUnityObjects)
    //    {
    //        GameObject sphere = createNewSphereObject();
    //        sphere.transform.parent = gameObject.transform;
    //        sphere.GetComponent<ServerSphereObject>().name = suo.value;
    //        sphere.GetComponent<ServerSphereObject>().model = suo.downloadLink;
    //        sphere.GetComponent<ServerSphereObject>().uri = suo.id;
    //        sphereList.Add(sphere);
    //    }
    //}

    //public void fillMenuWithDummies(int count)
    //{
    //    if (sphereList != null) sphereList.Clear();
    //    for(int i = 0; i < count; i++)
    //    {
    //        GameObject sphere = createNewSphereObject();
    //        sphere.transform.parent = plane.transform;
    //        sphere.GetComponent<ServerSphereObject>().name = "leuchtturm240";
    //        sphere.GetComponent<ServerSphereObject>().model = "https://resources.hucompute.org/download/3908?session=9105AA9F7024E46DBC968CDCA3DA2B79.jvm1";
    //        sphere.GetComponent<ServerSphereObject>().uri = "http://app.stolperwege.hucompute.org/unityobject/100";
    //        sphereList.Add(sphere);
    //    }
    //}
    
    public GameObject createNewSphereObject()
    {
        GameObject go = GameObject.Instantiate(sphereObject);
        go.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        return go;
    }

    public void allignObjects(HashSet<GameObject> spheres)
    {
        Vector3 pos = new Vector3(4, 0, -4);
        foreach (GameObject sphere in spheres)
        {
            sphere.transform.localPosition = pos;
            pos.x = pos.x - 2;
            if(pos.x < -4)
            {
                pos.z = pos.z + 2;
                pos.x = 4;
            }
            Debug.Log("new Vector: " + pos);
        }
    }

}

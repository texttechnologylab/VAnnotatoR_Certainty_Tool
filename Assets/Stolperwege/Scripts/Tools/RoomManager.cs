using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


/// <summary>
/// Class that focuses on managing GameObjects and their Unity position in virtualrooms <-> StolperwegeServerRepresentation
/// </summary>
public class RoomManager : MonoBehaviour {
    public VirtualRoom currentRoom;
    public string currentRoomUri { get; set; }
    public string currentRoomData { get; set; }
    public string dummyCurrentRoomUri = "http://app.stolperwege.hucompute.org/virtualroom/2912";
    public Dictionary<string, string> currentRoomDict { get; set; }
    public float updateTimer = 0;
    private int updateNR = 0;
    //private networkMenuTileScript netMenuTile;
    private StolperwegeInterface StolperwegeInterface;

    // Use this for initialization
    void Start () {
        //Create new StolperwegeUnityObject:
        //Debug.Log("Creating temporary local StolperwegeUnityObject...");
        //StolperwegeUnityObject leuchtturm = new StolperwegeUnityObject(null);
        //Debug.Log(leuchtturm.downloadLink);
        //StartCoroutine(LoadObjects());
        //StartCoroutine(this.gameObject.GetComponent<LoadAssetBundleFromFile>().DownloadObject("leuchtturm"));

        StolperwegeInterface = SceneController.GetInterface<StolperwegeInterface>();
        Debug.Log("Created in Unity Version: " + Application.unityVersion);
        //if (StolperwegeHelper.User != null)
        //{
        //    currentRoom = StolperwegeHelper.User.GetComponent<Prefe>().CurrentRoom;
        //    Debug.Log("preferences found");
        //}
        //else
        //{
        //    Debug.Log("no preferences found");
        //}
        Debug.Log("1: "+ currentRoom);
        Debug.Log("currentRoomUri: " + currentRoomUri);
        if(currentRoom == null)
        {
            //no room selected, using dummy data
            Debug.Log("dummy");
            currentRoomUri = dummyCurrentRoomUri;
            Debug.Log(dummyCurrentRoomUri);
            StartCoroutine(StolperwegeInterface.CreatePreference(currentRoomUri, "roomdata", CreateRoomData()));

        }
        else
        {
            Debug.Log("current room uri: " + currentRoom.ID);
            currentRoomUri = (string)currentRoom.ID;
            //LoadObjects();
        }

        //StartCoroutine(LoadCurrentRoom());
        //StartCoroutine(StolperwegeHelper.netManagement.host(currentRoom));
        //StolperwegeHelper.netManagement.networkManager.StartHost(); //test!

    }

    //public IEnumerator AddDummyObjectsToCurrentRoom()
    //{
    //    //Add current roomdata as preference to uri:
    //    StartCoroutine(StolperwegeInterface.CreatePreference(currentRoomUri, "roomdata", ServerObjectManager.createDummyRoom()));
    //    return null;
    //}

    public void Update()
    {
        updateTimer += Time.deltaTime;

        if (updateTimer > 10)
        {
            updateTimer = 0;
            if (SceneManager.GetActiveScene().name == "VirtualRoom")
            {
                //Debug.Log("in Virtual Room");
                UpdateCurrentRoom();
            }
        }
    }
    
    /// <summary>
    /// Method that returns a JSON-Dictionary of a room together with their unity-position in the room.
    /// </summary>
    /// <returns></returns>
    public static string CreateRoomData()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        Dictionary<string, string> gameObjectAndPosition = new Dictionary<string, string>();

        foreach (GameObject go in allObjects)
        {
            if (go.GetComponent<ServerObject>() != null)
            {
                if (go.activeSelf)
                {
                    if (go.GetComponent<ServerObject>() != null) Debug.Log("Server Object found: " + go.name);
                    if (gameObjectAndPosition.ContainsKey(go.GetComponent<ServerObject>().ModelName))
                    {
                        string positionData = go.transform.position.x.ToString() + ", " + go.transform.position.y.ToString()
                            + ", " + go.transform.position.z.ToString();
                        string rotationData = go.transform.eulerAngles.x.ToString() + ", " + go.transform.eulerAngles.y.ToString()
                            + ", " + go.transform.eulerAngles.z.ToString();
                        string positionAndRotationData = positionData + " : " + rotationData;

                        gameObjectAndPosition[go.GetComponent<ServerObject>().ModelName] = positionAndRotationData;
                    }
                    else
                    {
                        string positionData = go.transform.position.x.ToString() + ", " + go.transform.position.y.ToString()
                            + ", " + go.transform.position.z.ToString();
                        string rotationData = go.transform.eulerAngles.x.ToString() + ", " + go.transform.eulerAngles.y.ToString()
                            + ", " + go.transform.eulerAngles.z.ToString();
                        string positionAndRotationData = positionData + " : " + rotationData;

                        gameObjectAndPosition.Add(go.GetComponent<ServerObject>().ModelName, positionAndRotationData);
                    }
                }
            }

        }

        string roomJson = NetworkUtils.DictToJson(gameObjectAndPosition);

        //Debug.Log(roomJson);

        return roomJson;
    }

    /// <summary>
    /// loads the currently entered room(objects) by uri
    /// </summary>
    public IEnumerator LoadCurrentRoom()
    {
        
        Hashtable returnedPreferences = new Hashtable();
        Debug.Log(currentRoomUri);
        if (currentRoomUri == null) yield break;

        //WWWForm form = new WWWForm();

        //form.AddField("uri", uri);
        //WWW www = new WWW(StolperwegeInterface.PREFERENCE, form);
        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(StolperwegeInterface.PREFERENCES + "?uri=" + currentRoomUri);

        yield return webRequest.SendWebRequest();
        
        Debug.Log(webRequest.downloadHandler.text);

        if (webRequest.downloadHandler.text.Length <= 0) yield break;
        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text)[StolperwegeInterface.JSONPARAM_RESULT];

        //JsonData data = JsonMapper.ToObject(www.text)[StolperwegeInterface.JSONPARAM_RESULT]["preferences"];

        //JsonData roomData = JsonMapper.ToObject(www.text)[StolperwegeInterface.JSONPARAM_RESULT]["roomdata"];
        string roomdata = null;
        for (int i = 0; i < data.Count; i++)
        {
            JsonData pref = data[i];
            if (pref["key"].ToString().Length > 0)
                returnedPreferences.Add(pref["key"].ToString(), pref["value"].ToString());
        }
        Debug.Log("preferences for uri " + currentRoomUri + " : ");
        foreach (string key in returnedPreferences.Keys)
        {
            Debug.Log(String.Format("{0}: {1}", key, returnedPreferences[key]));
            roomdata = returnedPreferences[key].ToString();
            Debug.Log("roomdata string: " + roomdata);

            //currentRoomData = roomdata;
            //currentRoomUri = currentRoomUri;
            

            if (GameObject.Find("RoomManager").name != null)
            {
                GameObject.Find("RoomManager").GetComponent<RoomManager>().currentRoomData = roomdata;
                GameObject.Find("RoomManager").GetComponent<RoomManager>().currentRoomUri = currentRoomUri;
            }
            else
            {
                Debug.Log("RoomManager does not exist in the current scene.");
            }
        }

        //Debug.Log(GameObject.Find("RoomManager").GetComponent<RoomManager>().currentRoomData + " , " + GameObject.Find("RoomManager").GetComponent<RoomManager>().currentRoomUri);
        currentRoomDict = NetworkUtils.JsonToDict(roomdata);
        //LoadObject(currentRoomDict);
        //GameObject.Find("RoomManager").GetComponent<RoomManager>().currentRoomDict = NetworkUtils.JsonToDict(roomdata);
        //GameObject.Find("RoomManager").GetComponent<RoomManager>().LoadObject(GameObject.Find("RoomManager").GetComponent<RoomManager>().currentRoomDict);


        //delete dummies:
        GameObject dummy1 = GameObject.Find("DummyServerObject");
        if (dummy1) Destroy(dummy1);
        GameObject dummy2 = GameObject.Find("DummyServerObject2");
        if (dummy2) Destroy(dummy2);

    }


    /// <summary>
    /// Gets preferences of a room by uri
    /// </summary>
    public IEnumerator LoadRoomByUri(string uri)
    {
        Hashtable returnedPreferences = new Hashtable();

        if (uri == null) yield break;

        //WWWForm form = new WWWForm();

        //form.AddField("uri", uri);
        //WWW www = new WWW(StolperwegeInterface.PREFERENCE, form);
        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(StolperwegeInterface.PREFERENCES + "?uri=" + uri);

        yield return webRequest.SendWebRequest();

        Debug.Log(webRequest.downloadHandler.text);

        if (webRequest.downloadHandler.text.Length <= 0) yield break;
        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text)[StolperwegeInterface.JSONPARAM_RESULT];

        //JsonData data = JsonMapper.ToObject(www.text)[StolperwegeInterface.JSONPARAM_RESULT]["preferences"];

        //JsonData roomData = JsonMapper.ToObject(www.text)[StolperwegeInterface.JSONPARAM_RESULT]["roomdata"];

        string roomdata = null;
        for (int i = 0; i < data.Count; i++)
        {
            JsonData pref = data[i];
            if (pref["key"].ToString().Length > 0)
                returnedPreferences.Add(pref["key"].ToString(), pref["value"].ToString());
        }
        Debug.Log("preferences for uri " + uri + " : ");

        foreach (string key in returnedPreferences.Keys)
        {
            Debug.Log(string.Format("{0}: {1}", key, returnedPreferences[key]));
            roomdata = returnedPreferences[key].ToString();
            Debug.Log("roomdata string: " + roomdata);
            if (GameObject.Find("RoomManager").name != null)
            {
                GameObject.Find("RoomManager").GetComponent<RoomManager>().currentRoomData = roomdata;
                GameObject.Find("RoomManager").GetComponent<RoomManager>().currentRoomUri = uri;
            }
            else
            {
                Debug.Log("RoomManager does not exist in the current scene.");
            }
        }

        Debug.Log(GameObject.Find("RoomManager").GetComponent<RoomManager>().currentRoomData + " , " + GameObject.Find("RoomManager").GetComponent<RoomManager>().currentRoomUri);

        GameObject.Find("RoomManager").GetComponent<RoomManager>().currentRoomDict = NetworkUtils.JsonToDict(roomdata);
        GameObject.Find("RoomManager").GetComponent<RoomManager>().LoadObject(GameObject.Find("RoomManager").GetComponent<RoomManager>().currentRoomDict);

    }

    /*
     * Loads all objects from dictionary into room.
     */
    public IEnumerator LoadObjects()
    {

        HashSet<StolperwegeElement> objects = new HashSet<StolperwegeElement>();

        yield return StolperwegeInterface.GetAllElements("UnityObject", false, (HashSet<StolperwegeElement> res) => { objects = res; });

        List<GameObject> listObjects = new List<GameObject>();
        foreach (StolperwegeUnityObject unityObj in objects)
        {
            GameObject obj = null;
            yield return StolperwegeInterface.DownloadObject(unityObj.downloadLink, "0, 0, 0 : 0, 0, 0", (GameObject res) => { obj = res; });
            //obj.AddComponent<ServerObject>();
            if (obj != null ) listObjects.Add(obj);
        }
    }

    public void LoadObject(Dictionary<string, string> dictionary)
    {
        Dictionary<string, string>.KeyCollection keys = dictionary.Keys;

        foreach (string key in keys)
        {
            StartCoroutine(StolperwegeInterface.DownloadObject(key, dictionary[key], (GameObject res) => { }));
        }
    }

    public void UpdateCurrentRoom()
    {
        Debug.Log("Updating roomdata...");
        updateNR++;
        Debug.Log("currently updating room:" + currentRoomUri);
        StartCoroutine(StolperwegeInterface.CreatePreference(currentRoomUri, "roomdata", CreateRoomData()));
    }



    public static Dictionary<string, string> currentRoomJsonToDict()
    {
        string jsonDataString = GameObject.Find("RoomManager").GetComponent<RoomManager>().currentRoomData;

        Dictionary<string, string> roomDataDict = new Dictionary<string, string>();

        roomDataDict = NetworkUtils.JsonToDict(jsonDataString);
        Debug.Log("room data dictionary: " + roomDataDict);
        return roomDataDict;
    }

    public void CoroutineHelper(IEnumerator routine)
    {
        StartCoroutine(routine);
    }


}

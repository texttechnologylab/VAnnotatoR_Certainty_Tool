using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using UnityEngine.Networking;
using System.IO;
using Mirror;
using System.Text;
using System.Net;
using System;

using System.Text.RegularExpressions;


public class StolperwegeInterface : Interface
{

    //public const string WS = "http://localhost:4567/";
    //public const string WS = "http://141.2.108.252:4567/";
    //public const string WS = "http://141.2.108.226:4567/";
    public const string WS = "http://app.stolperwege.hucompute.org/";
    public const string API = WS + "api";
    public const string UNITYPOSITIONS = WS + "unitypositions";
    public const string DISCOURSEREFERENT = WS + "discoursereferent";
    public const string DISCOURSEREFERENTS = WS + "discoursereferents";
    public const string STOLPERWEGEELEMENTS = WS + "stolperwegeelements";
    public const string TYPESYSTEM = WS + "typesystem/";
    public const string LOGIN = WS + "login";
    public const string LOGOUT = WS + "logout";
    public const string PREFERENCE = WS + "preference";
    public const string PREFERENCES = WS + "preferences";
    public const string RELATION = WS + "relation";
    public const string ROOM = WS + "room";
    public const string TOOLCHAIN = WS + "toolchain";
    public const string TOOLCHAINS = WS + "toolchains";
    public const string DELETEELEMENT = WS + "element";
    public const string RELATIONTYPES = WS + "relationtypes";
    public const string RELATIONTYPE = WS + "relationtype";

    public const string PERMISSIONS = WS + "permissions";
    public const string PERMISSION = WS + "permission";
    public const string DELETE = "DELETE";
    public const string GRANT = "GRANT";
    public const string READ = "READ";
    public const string WRITE = "WRITE";

    public const string USER = WS + "user";
    public const string USERS = WS + "users";

    public const string DISCOURSER_TITLE = "title";
    public const string DISCOURSER_DESCRIPTION = "description";

    public const string JSONPARAM_MESSAGE = "message";
    public const string JSONPARAM_RESULT = "result";
    public const string JSONPARAM_SUCCESS = "success";
    public const string JSONPARAM_EQUI = "equivalent";
    public const string JSONPARAM_URI = "uri";
    public const string JSONPARAM_VALUE = "value";
    public const string JSONPARAM_PUBLIC = "public";
    public const string JSONPARAM_TYPE = "type";
    public const string JSONPARAM_PERSON_FIRSTNAME = "firstName";
    public const string JSONPARAM_PERSON_LASTNAME = "lastName";
    public const string JSONPARAM_PERSON_BIRTHDATE = "birthDate";
    public const string JSONPARAM_PERSON_DEATHDATE = "deathDate";
    public const string JSONPARAM_UPOS_ORIENTATION = "orientation";
    public const string JSONPARAM_UPOS_POSITION = "posvector3";
    public const string JSONPARAM_BUILDING_POSITION = "position";
    public const string JSONPARAM_BUILDING_MODEL = "model";
    public const string JSONPARAM_BUILDING_SCENE = "scene";
    public const string JSONPARAM_BUILDING_PREFAB = "prefab";
    public const string JSONPARAM_BUILDING_SCALE = "scale";
    public const string JSONPARAM_TOOLS = "tools";
    public const string JSONPARAM_LANGUAGE = "language";
    public const string JSONPARAM_MODEL = "model";
    public const string JSONPARAM_HOST = "host";
    public const string JSONPARAM_CLIENTS = "clients";
    public const string JSONPARAM_REQUIREDANNOTATIONS = "requiredAnnotations";
    public const string JSONPARAM_UNITYOBJECTIDENTIFIER = "unityObjectIdentifier";
    public const string JSONPARAM_AVATAR = "avatar";
    public const string JSONPARAM_AVATAR_PREVIEW = "preview";
    public const string JSONPARAM_CCITE_HOWTOCITE = "howtocite";

    /// <summary>
    /// Will be used to show the state of the login on UI-elements
    /// </summary>
    public string LoginMessage { get; private set; }
    public Hashtable Referents { get; private set; } = new Hashtable();
    public StolperwegeUser CurrentUser;
    public DiscourseReferent DefaultAvatarConfig;
    public UnityBuilding CurrentBuilding;
    public Cite CurrentCite;
    public VirtualRoom CurrentRoom;
    public OSMManager MapManager;

    //  public static bool OnlineStatus = false;


    public static RelationType EquivalentRelation { get; private set; }
    public static RelationType ContainsRelation { get; private set; }

    /// <summary>
    /// Verlinkt die Klassen des Web-Services mit den Unity-Klassen
    /// </summary>
    public Hashtable TypeClassTable = new Hashtable
      {
          { "org.hucompute.publichistory.datastore.typesystem.Time",typeof(StolperwegeTime) },
          { "org.hucompute.publichistory.datastore.typesystem.UnityBuilding",typeof(UnityBuilding) },
          { "org.hucompute.publichistory.datastore.typesystem.DiscourseReferent",typeof(DiscourseReferent) },
          { "org.hucompute.publichistory.datastore.typesystem.StolperwegeUri",typeof(StolperwegeUri) },
          { "org.hucompute.publichistory.datastore.typesystem.Position",typeof(Position) },
          { "org.hucompute.publichistory.datastore.typesystem.Place",typeof(Place) },
          { "org.hucompute.publichistory.datastore.typesystem.Person",typeof(Person) },
          { "org.hucompute.publichistory.datastore.typesystem.Image",typeof(StolperwegeImage) },
          { "org.hucompute.publichistory.datastore.typesystem.UnityPosition",typeof(UnityPosition) },
          { "org.hucompute.publichistory.datastore.typesystem.Audio",typeof(Audio) },
          { "org.hucompute.publichistory.datastore.typesystem.Event",typeof(StolperwegeEvent) },
          { "org.hucompute.publichistory.datastore.typesystem.Proposition",typeof(Proposition) },
          { "org.hucompute.publichistory.datastore.typesystem.Argument",typeof(Argument) },
          { "org.hucompute.publichistory.datastore.typesystem.ArgumentRole",typeof(ArgumentRole) },
          { "org.hucompute.publichistory.datastore.typesystem.Connector",typeof(Connector) },
          { "org.hucompute.publichistory.datastore.typesystem.Predicate",typeof(Predicate) },
          { "org.hucompute.publichistory.datastore.typesystem.Photogrammetrie",typeof(Photogrammetrie) },
          { "org.hucompute.publichistory.datastore.typesystem.Message",typeof(StolperwegeMessage) },
          { "org.hucompute.publichistory.datastore.typesystem.Preference",typeof(StolperwegePreference) },
          { "org.hucompute.publichistory.datastore.typesystem.TimeProcess",typeof(StolperwegeTimeProcess) },
          { "org.hucompute.publichistory.datastore.typesystem.Video",typeof(StolperwegeVideo) },
          { "org.hucompute.publichistory.datastore.typesystem.TermConnector",typeof(StolperwegeTermConnector) },
          { "org.hucompute.publichistory.datastore.typesystem.Term",typeof(StolperwegeTerm) },
          { "org.hucompute.publichistory.datastore.typesystem.StolperwegeUser",typeof(StolperwegeUser) },
          { "org.hucompute.publichistory.datastore.typesystem.Text",typeof(StolperwegeText) },
          { "org.hucompute.publichistory.datastore.typesystem.StolperwegeElement", typeof(StolperwegeElement) },
          { "org.hucompute.publichistory.datastore.typesystem.StolperwegeSet", typeof(StolperwegeSet) },
          { "org.hucompute.publichistory.datastore.typesystem.VirtualRoom", typeof(VirtualRoom) },
          { "org.hucompute.publichistory.datastore.typesystem.Permission", typeof(StolperwegePermission) },
          { "org.hucompute.publichistory.datastore.typesystem.UnityObject", typeof(StolperwegeUnityObject) },
          { "org.hucompute.publichistory.datastore.typesystem.Cite", typeof(Cite) },
          { "org.hucompute.publichistory.datastore.typesystem.StolperwegeCamera", typeof(StolperwegeCamera) },
          //{ "org.hucompute.publichistory.datastore.typesystem.ToolChain", typeof(ToolChain) },
          //{ "org.hucompute.publichistory.datastore.typesystem.Visualization", typeof(Visualization) },
          { "org.hucompute.publichistory.datastore.typesystem.Waypoint", typeof(Waypoint) },
          { "org.hucompute.publichistory.datastore.typesystem.Avatar", typeof(StolperwegeAvatar) },
          { "uima.cas.String", typeof(string) },
          { "uima.cas.StringArray", typeof(string) },
          { "uima.cas.Double", typeof(double) },
          { "uima.cas.Boolean", typeof(bool) },
          { "uima.cas.Integer", typeof(int) }
      };

    public Hashtable TypeSystemTable { get; private set; } //Beinhaltet f�r jeden Eintrag der typeClassTable alle verf�gbare Relationen

    public Hashtable ApiTable { get; private set; } //Swagger API des Web-Services, wird ben�tigt, um herauszufinden, ob eine Relation notwendig ist

    public bool Ready { get; private set; }

    public bool ParsingCompleted = false;

    public enum NetworkObjectType { User, Friend, Request, ConfirmRemain, Invitation, List, Room, StolperwegeElementObject, None }

    public enum RelType { RELATION, PROPERTY, REST_PARAMETER}

    /// <summary>
    /// Format fuer eine Relation des Datenmodells
    /// </summary>
    public struct RelationType
    {
        public string title;
        public string id;
        public Type from;
        public Type to;
        public bool mandatory;
        public bool multiple;
        public RelType type;
        public bool property;
    }

    protected override IEnumerator InitializeInternal()
    {
        Name = "Stolperwege";
        OnLogin = (loginData, afterLogin) =>
        {
            StartCoroutine(Login(loginData, (success) => { afterLogin(success, LoginMessage); }));
        };
        OnSetupBrowser = SetupBrowser;
        StartCoroutine(ParseTypes());
        yield break;
    }

    //  private void Awake()
    //  {
    //      //GameObject go = new GameObject();
    //      //ObjImporter.loadObj("PPT_figure_small.obj", go);
    //      StolperwegeHelper.stolperwegeInterface = this;
    //      // Load only if the user is logged in
    //      Text2SceneDBInterface.Init();


    //      DontDestroyOnLoad(gameObject);

    //      StartCoroutine(parseTypes());
    //      if (SceneManager.GetActiveScene().name.Contains("Annotation") || SceneManager.GetActiveScene().name.Contains("Homeroom_v3")|| SceneManager.GetActiveScene().name.Contains("Halle"))
    //      {

    //          //StartCoroutine(init());
    //      }
    //  }

    //  private void Start()
    //  {
    //     // StolperwegeRelationExtended.Test();
    //  }

    /// <summary>
    /// ALT
    /// 
    /// L�dt alle sich auf dem Server befifindeten Unity-Positionen und die daran h�ngenden StolperwegeElemente herunter
    /// ParseTypes() muss vor dieser Funktion aufgerufen werden
    /// </summary>
    /// <returns></returns>
    public IEnumerator init()
    {
        while (!ParsingCompleted)
        {
            yield return new WaitForEndOfFrame();
        }

        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(UNITYPOSITIONS);

        yield return webRequest.SendWebRequest();
        Debug.Log(webRequest.downloadHandler.text);

        if (webRequest.downloadHandler.text.Equals("")) yield break;

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text)[JSONPARAM_RESULT];

        Queue<string> idqueue = new Queue<string>();

        for (int i = 0; i < data.Count; i++)
        {
            yield return ParseJSON(data[i]);
            if (data[i].Keys.Contains(JSONPARAM_EQUI))
                EnqueueEquis(idqueue, data[i]);
        }

        yield return LoadArgumentRoles();

        yield return LoadStolperwegeElements(idqueue, true);

        foreach (object o in Referents.Values)
        {
            ((StolperwegeElement)o).Link();
            if (o.GetType() == typeof(DiscourseReferent))
                yield return ((DiscourseReferent)o).GetPreferences();

        }

        foreach (object o in Referents.Values)
            ((StolperwegeElement)o).Draw();




        Ready = true;

        //if (NetworkServer.active)
        //    foreach (StolperwegeElement ele in referents.Values)
        //    {
        //        if (ele.Object3D != null)
        //            NetworkServer.Spawn(ele.Object3D.gameObject);
        //    }

        /*GameObject pictureBox = (GameObject)Instantiate(Resources.Load("StolperwegeElements/ImageBox"));

        print("Images"+getImages().Count);
        pictureBox.GetComponent<ImageBox>().Images = getImages();
        pictureBox.transform.position = new Vector3(2.5f, -2f, 17f);*/
    }

    /// <summary>
    /// L�dt das Typsystem f�r jede Klasse der typeClassTable und f�llt die typeSystemTable mit allen m�glichen Relationen dieser
    /// </summary>
    /// <returns></returns>
    private IEnumerator ParseTypes()
    {
        yield return ParseAPI();
        Debug.Log("Parsing Stolperwege-typesystem...");
        Hashtable allTypes = new Hashtable();
        TypeSystemTable = new Hashtable();

        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(RELATIONTYPES);

        yield return webRequest.SendWebRequest();

        //Debug.Log(www.text);

        if (webRequest.downloadHandler.text.Equals("")) yield break;

        //Debug.Log(typeStr + " " + www.text);

        JsonData customRelsJSON = JsonMapper.ToObject(webRequest.downloadHandler.text)[JSONPARAM_RESULT];

        HashSet<RelationType> customRelations = new HashSet<RelationType>();

        //Debug.Log(wwwRels.text); 

        for (int i = 0; i < customRelsJSON.Count; i++)
        {
            if (!customRelsJSON[i]["Name"].ToString().Contains(":")) continue;

            RelationType rType = new RelationType
            {
                title = customRelsJSON[i]["Name"].ToString().Split(':')[1],
                from = (System.Type)TypeClassTable[customRelsJSON[i]["RangeFrom"].ToString()],
                to = (System.Type)TypeClassTable[customRelsJSON[i]["RangeTo"].ToString().Replace("[]", "")],
                multiple = true,
                mandatory = false,
                id = customRelsJSON[i]["Name"].ToString()
            };

            customRelations.Add(rType);
        }

        foreach (string typeStr in TypeClassTable.Keys)
        {
            webRequest = StolperwegeHelper.CreateWebRequest(TYPESYSTEM + typeStr);

            yield return webRequest.SendWebRequest();

            //Debug.Log(www.text);

            if (webRequest.downloadHandler.text.Equals("")) yield break;

            //Debug.Log(typeStr + " " + www.text);

            JsonData jsnfull = JsonMapper.ToObject(webRequest.downloadHandler.text);

            if (!jsnfull.Keys.Contains(JSONPARAM_RESULT)) continue;

            JsonData data = jsnfull[JSONPARAM_RESULT]["definition"];

            HashSet<RelationType> types = new HashSet<RelationType>();
            HashSet<string> marked = new HashSet<string>();

            for (int i = 0; i < data.Count; i++)
            {
                JsonData type = data[i];

                if (!type["id"].ToString().StartsWith("org.hucompute.publichistory.datastore.typesystem.")) continue;

                RelationType rType = new RelationType
                {
                    title = type["name"].ToString(),
                    from = (Type)TypeClassTable[type["definition"][0]["domain"].ToString()],
                    to = (Type)TypeClassTable[type["definition"][1]["range"].ToString().Replace("[]", "")],
                    multiple = type["definition"][1]["range"].ToString().Contains("[]"),
                    mandatory = false,
                    id = type["id"].ToString()
                };

                if (rType.to == null) continue;

                rType.type = (!(rType.to == typeof(StolperwegeElement) || rType.to.IsSubclassOf(typeof(StolperwegeElement)))) ? RelType.PROPERTY : RelType.RELATION;

                if (ApiTable.Contains(typeStr))
                {
                    Hashtable apiType = (Hashtable)ApiTable[typeStr];

                    if (apiType.Contains(rType.title))
                    {
                        rType.mandatory = (bool)apiType[rType.title];
                        marked.Add(rType.title);
                    }

                }

                if (EquivalentRelation.title == null && rType.title.Equals("equivalent"))
                    EquivalentRelation = rType;

                if (ContainsRelation.title == null && rType.title.Equals("contains"))
                    ContainsRelation = rType;

                //Debug.Log(type.ToJson());

                //Debug.Log(type["definition"][0]["domain"] + " " + type["definition"][1]["range"].ToString().Replace("[]", "") + " " + type["name"]);
                string rel1 = rType.from.Name + rType.to.Name + rType.title;
                if (!allTypes.Contains(rel1))
                {
                    allTypes.Add(rel1, rType);
                    //Debug.Log(rType.from.Name + " " + rType.title + " " + rType.to.Name);
                }

                types.Add((RelationType)allTypes[rel1]);

            }
            if (ApiTable.Contains(typeStr))
            {
                foreach (string key in ((Hashtable)ApiTable[typeStr]).Keys)
                {
                    if (marked.Contains(key)) continue;

                    Hashtable apiType = (Hashtable)ApiTable[typeStr];

                    string toTypeStr = "org.hucompute.publichistory.datastore.typesystem." + key[0].ToString().ToUpper() + key.Substring(1);

                    if (!TypeClassTable.Contains(toTypeStr)) continue;

                    RelationType rType = new RelationType
                    {
                        title = key,
                        from = (System.Type)TypeClassTable[typeStr],
                        to = (System.Type)TypeClassTable[toTypeStr],
                        multiple = false,
                        mandatory = (bool)apiType[key],
                        id = toTypeStr + ":" + key,
                        type = RelType.REST_PARAMETER
                    };

                    types.Add(rType);
                }
            }

            foreach (RelationType relType in customRelations)
            {
                if (relType.from == ((System.Type)TypeClassTable[typeStr]) || ((System.Type)TypeClassTable[typeStr]).IsSubclassOf(relType.from))
                {
                    types.Add(relType);
                }

            }


            TypeSystemTable.Add(typeStr, types);
        }
        Debug.Log("<color=green>Parsing Stolperwege-typesystem done.</color>");
        ParsingCompleted = true;
    }

    /// <summary>
    /// L�dt die Swagger API des Web-Services und speichert zu jeder in dieser angebebenen Relation, ob diese notwendig ist
    /// Die Namen der Parameter der REST-Schnittstelle des Web-Services m�ssen mit den Namen der Relationen des Typsystems �bereinstimmen, damit diese problemlos funktioniert
    /// </summary>
    /// <returns></returns>
    private IEnumerator ParseAPI()
    {
        Debug.Log("Parsing Stolperwege API...");
        ApiTable = new Hashtable();

        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(API);

        yield return webRequest.SendWebRequest();

        Debug.Log(webRequest.downloadHandler.text);
        if (webRequest.downloadHandler.text.Equals("")) yield break;

        Debug.Log(webRequest.downloadHandler.text);
        JsonData jsnfull = JsonMapper.ToObject(webRequest.downloadHandler.text);

        if (!jsnfull.Keys.Contains("paths")) yield break;

        JsonData data = jsnfull["paths"];

        Hashtable typeApiTable;
        foreach (string key in data.Keys)
        {
            JsonData typeJSON = data[key];

            string type = "org.hucompute.publichistory.datastore.typesystem." + key.Replace("/", "");

            string realType = "";
            foreach (string typeID in TypeClassTable.Keys)
                if (typeID.ToLower().Equals(type.ToLower()))
                    realType = typeID;

            if (realType.Length <= 0 || !typeJSON.Keys.Contains("post") || !typeJSON["post"].Keys.Contains("parameters")) continue;

            typeJSON = typeJSON["post"]["parameters"];

            typeApiTable = new Hashtable();

            for (int i = 0; i < typeJSON.Count; i++)
            {
                // uri w�re mehr mals eingef�gt worden, kann sp�ter wahrscheinlich gel�scht werden
                if (typeApiTable.ContainsKey(typeJSON[i]["name"].ToString())) continue;
                typeApiTable.Add(typeJSON[i]["name"].ToString(), (bool)typeJSON[i]["required"]);
            }

            if (!ApiTable.Contains(realType))
                ApiTable.Add(realType, typeApiTable);
        }
        Debug.Log("<color=green>Parsing Stolperwege API done.</color>");
    }

    /// <summary>
    /// L�dt alle auf dem Web-Service vordefinierten ArgumentRoles
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadArgumentRoles()
    {
        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(WS + "roles");

        yield return webRequest.SendWebRequest();
        Debug.Log(webRequest.downloadHandler.text);

        if (webRequest.downloadHandler.text.Equals("")) yield break;

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text)[JSONPARAM_RESULT];

        for (int i = 0; i < data.Count; i++)
        {
            yield return ParseJSON(data[i]);
        }
    }

    //Loads all Images from the server
    IEnumerator LoadImages()
    {
        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(WS + "images");

        yield return webRequest.SendWebRequest();
        Debug.Log(webRequest.downloadHandler.text);

        if (webRequest.downloadHandler.text.Equals("")) yield break;

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text)[JSONPARAM_RESULT];

        print(data.Count);
        for (int i = 0; i < data.Count; i++)
        {
            yield return ParseJSON(data[i]);
        }
    }

    //Returns all avaiable Images
    public List<StolperwegeImage> GetImages()
    {
        List<StolperwegeImage> images = new List<StolperwegeImage>();

        foreach (string key in Referents.Keys)
        {
            if (Referents[key] is StolperwegeImage)
                images.Add((StolperwegeImage)Referents[key]);
        }
        return images;
    }

    /// <summary>
    /// Entfernt, falls vorhanden, das n vor der ID
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static string FormatURI(string uri)
    {
        string[] arr = uri.Split('/');
        return arr[arr.Length - 1].Replace("n", "");
    }

    /// <summary>
    /// Gibt das StolperwegeElement mit der �bergebenen ID zur�ck, falls es bereits geladen wurde
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public StolperwegeElement GetStolperwegeElement(string id)
    {
        string realId = FormatURI(id);
        if (Referents.ContainsKey(realId)) return ((StolperwegeElement)Referents[realId]);

        return null;
    }

    public IEnumerator LoadStolperwegeElements(Queue<string> idqueue, bool loadAll)
    {
        int depth = loadAll ? 4 : 1;
        while (idqueue.Count > 0)
        {
            string id = idqueue.Dequeue();


            yield return LoadStolperwegeElements(id, depth);
        }
    }

    //loads all Elements, which are connected to the Elements in the idqueue
    public IEnumerator LoadStolperwegeElements(string id, int depth)
    {
        if (!Referents.ContainsKey(FormatURI(id)))
        {
            UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(DISCOURSEREFERENT + "?uri=" + id);

            yield return webRequest.SendWebRequest();

            if (!webRequest.downloadHandler.text.Contains(JSONPARAM_RESULT))
            {
                yield break;
            }

            JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text)[JSONPARAM_RESULT];

            ParseJSON(data);

            if (depth > 0)
            {
                JsonData referentData = ((StolperwegeElement)Referents[FormatURI(id)]).Data;

                HashSet<string> ids = GetRelatedElementIDs(referentData);
                foreach (string nID in ids)
                    yield return LoadStolperwegeElements(nID, depth - 1);
            }
        }
    }

    /// 
    /// Enqueue or parse all Elements in eqdata
    public void EnqueueEquis(Queue<string> idqueue, JsonData eqData)
    {
        foreach (string key in eqData.Keys)
        {
            if (eqData[key].IsArray)
            {
                JsonData eqArray = eqData[key];
                if ((eqArray.Count > 0 && !eqArray[0].ToJson().Contains("http://app.stolperwege.hucompute.org/"))) continue;
                for (int i = 0; i < eqArray.Count; i++)
                {
                    if (eqArray[i].IsString)
                    {
                        string nid = eqArray[i].ToString();
                        if (!idqueue.Contains(nid) && !Referents.Contains(FormatURI(nid)))
                            idqueue.Enqueue(nid);
                    }
                    else
                    {
                        ParseJSON(eqArray[i]);
                        //Debug.Log(eqArray.ToString());
                        EnqueueEquis(idqueue, eqArray[i]);
                    }
                }
            }
        }
    }

    public static HashSet<string> GetRelatedElementIDs(JsonData eqData)
    {
        HashSet<string> ids = new HashSet<string>();

        foreach (string key in eqData.Keys)
        {
            if (eqData[key].IsArray)
            {
                JsonData eqArray = eqData[key];
                if ((eqArray.Count > 0 && !eqArray[0].ToJson().Contains("http://app.stolperwege.hucompute.org/"))) continue;
                for (int i = 0; i < eqArray.Count; i++)
                {
                    if (eqArray[i].IsString)
                    {
                        string nid = eqArray[i].ToJson().Replace("\"", "");
                        ids.Add(nid);
                    }
                    else
                    {
                        //parseJSON(eqArray[i]);

                        if (eqArray[i].Keys.Contains("uri"))
                            ids.Add(eqArray[i]["uri"].ToJson().Replace("\"", ""));
                    }
                }
            }
            else if (eqData[key].IsString && !key.Equals("uri") && eqData[key].ToJson().StartsWith("http://app.stolperwege.hucompute.org/"))
            {
                ids.Add(eqData[key].ToJson().Replace("\"", ""));
            }
        }

        return ids;
    }

    /// <summary>
    /// Erh�lt ein JSON eines StolperwegeElementes und gibt ein daraus erzeugtes Element zur�ck
    /// </summary>
    /// <param name="jsonObject"></param>
    /// <returns></returns>
    public StolperwegeElement ParseJSON(JsonData jsonObject)
    {
        if (!jsonObject.Keys.Contains(JSONPARAM_TYPE))
            return null;

        if (!TypeClassTable.ContainsKey(jsonObject[JSONPARAM_TYPE].ToString())) return null;
        StolperwegeElement element = (StolperwegeElement)Activator.CreateInstance((Type)TypeClassTable[jsonObject[JSONPARAM_TYPE].ToString()], new object[] { jsonObject });

        string id = FormatURI((string)element.ID);

        if (Referents.ContainsKey(id))
            return (StolperwegeElement)Referents[id];

        Referents.Add(id, element);

        return element;
    }

    //  /// <summary>
    //  /// Compares our online-state with that logged on the StolperwegeServer and checks if our user isn't logged in from another ip.
    //  /// </summary>
    //  /// <returns></returns>
    //  public static IEnumerator AWO()
    //  {
    //      if (CurrentUser.Status)
    //      {
    //          WWWForm form = new WWWForm();
    //          form.AddField("target", CurrentUser.id);
    //          WWW www = new WWW(USER, form);

    //          yield return www;
    //          JsonData data = JsonMapper.ToObject(www.text);

    //          if (!data.Keys.Contains(JSONPARAM_RESULT))
    //          {
    //              Debug.Log("Couldn't get information about our online-state.");
    //              yield break;
    //          }

    //          data = data[JSONPARAM_RESULT];

    //          StolperwegeElement element = parseJSON(data);

    //          if (element != null && element is StolperwegeUser)
    //          {
    //              StolperwegeUser user = (StolperwegeUser)element;
    //              if (user.Status != CurrentUser.Status)
    //              {
    //                  Debug.Log("We were logged out by the StolperwegeServer!");
    //                  CurrentUser = user;
    //              } else
    //              {
    //                if (!user.Preferences["ip"].Equals(GameObject.Find("NetworkManager").GetComponent<NetworkManagement>().ExtIp)) // Comparing the IP-Preference with our own IP.
    //                  {

    //                      Debug.Log("Our StolperwegeUser was logged in from another location."); 
    //                      // Online Status ist noch nicht konsistent mit den Funktionen. (Wird an anderer Stelle idR noch nicht verwendet.)
    //                      CurrentUser.Status = false;
    //                  }
    //              }

    //          }
    //      }  

    //  }

    public delegate void ExcecuteOnLogin(bool success);



    /// <summary>
    /// Logs the specified user/password pair as "online" at the StolperwegeServer.    
    /// </summary>
    /// <param name="login"></param>Instanz der LoginData-Klasse mit username und password
    /// <param name="excecute">Wird aufgerufen, wenn der Login abgeschlossen wurde, mit Parameter, ob der Versuch erfolgreich war.</param>
    /// <returns></returns>

    public IEnumerator Login(LoginData login, ExcecuteOnLogin excecute)
    {
        WWWForm form = new WWWForm();

        form.AddField("username", login.Username);
        form.AddField("password", login.Password);

        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(LOGIN, form);
        yield return webRequest.SendWebRequest();

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text);

        Debug.Log(data.ToJson());

        if (!data.Keys.Contains(JSONPARAM_RESULT))
        {
            excecute(false);
            yield break;
        }

        data = data[JSONPARAM_RESULT];

        StolperwegeElement element = ParseJSON(data);

        if (element != null && element is StolperwegeUser)
        {
            StolperwegeUser user = (StolperwegeUser)element;
            CurrentUser = user;
            //yield return user.GetPreferences(user);

            Queue<string> idqueue = new Queue<string>();

            if (data.Keys.Contains(JSONPARAM_EQUI))
                //Debug.Log(data.ToString());
                EnqueueEquis(idqueue, data);

            yield return LoadStolperwegeElements(idqueue, true);

            user.Link();

            Hashtable r = new Hashtable(Referents);

            foreach (object o in r.Values)
            {
                ((StolperwegeElement)o).Link();
                if (o.GetType() == typeof(DiscourseReferent))
                {
                    yield return ((DiscourseReferent)o).GetPreferences();
                }
            }


            foreach (object o in r.Values)
                if (((StolperwegeElement)o).StolperwegeObject == null)
                    ((StolperwegeElement)o).Draw();


            LoginMessage = "Login successfull.";
            excecute(true);
            
            yield break;
        }


        LoginMessage = "Login failed.";
        excecute(false);

    }

    /// <summary>
    /// Logs the current user as "offline" at the StolperwegeServer.
    /// </summary>
    /// <param name="excecute">Wird aufgerufen, wenn der Login abgeschlossen wurde, mit Parameter, ob der Versuch erfolgreich war.</param>
    /// <returns></returns>
    public IEnumerator Logout(ExcecuteOnLogin excecute)
    {
        WWWForm form = new WWWForm();

        form.AddField("target", (string)CurrentUser.ID);

        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(LOGOUT, form);

        yield return webRequest.SendWebRequest();

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text);

        Debug.Log(webRequest.downloadHandler.text);

        if (!data.Keys.Contains(JSONPARAM_SUCCESS))
        {
            excecute(false);
            Debug.Log("Logout failed!");
            yield break;
        }
        if (data[JSONPARAM_SUCCESS].ToString().Contains("true"))
        {
            excecute(true);
            Debug.Log("Logout completed");
            yield break;
        }
        Debug.Log("Logout not in Form!");
        excecute(false);

    }

    public delegate void OnPostExcecute(HashSet<StolperwegeElement> result);

    /// <summary>
    /// Durchsucht alle DiscourseReferenten nach ihrer Value
    /// </summary>
    /// <param name="query">Suchwort</param>
    /// <param name="type">Filterung nach einem bestimmten Typ</param>
    /// <param name="excecute">Wird mit den gefundenen Elementen aufgerufen</param>
    /// <returns></returns>
    public IEnumerator SearchReferents(string url, string query, string type, bool drawObjects, OnPostExcecute excecute)
    {
        if (query.Length > 0) url += "?query=" + query;

        Debug.Log(url);
        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(url);

        type = "org.hucompute.publichistory.datastore.typesystem." + type;

        yield return webRequest.SendWebRequest();
        Debug.Log(webRequest.downloadHandler.text);

        if (webRequest.downloadHandler.text.Equals("")) yield break;

        Debug.Log(webRequest.downloadHandler.text);

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text)[JSONPARAM_RESULT];

        HashSet<StolperwegeElement> result = new HashSet<StolperwegeElement>();

        Queue<string> idqueue = new Queue<string>();

        for (int i = 0; i < data.Count; i++)
        {
            StolperwegeElement element = ParseJSON(data[i]);
            if (data[i].Keys.Contains(JSONPARAM_EQUI))
                EnqueueEquis(idqueue, data[i]);
            if (element.GetType() == (Type)TypeClassTable[type])
                result.Add(element);

            yield return element;
        }

        //yield return loadStolperwegeElements(idqueue, false);

        foreach (object o in Referents.Values)
        {
            ((StolperwegeElement)o).Link();
            if (o.GetType() == typeof(DiscourseReferent))
            {
                yield return ((DiscourseReferent)o).GetPreferences();
            }
        }


        foreach (object o in Referents.Values)
            if (drawObjects && ((StolperwegeElement)o).StolperwegeObject == null)
                ((StolperwegeElement)o).Draw();

        excecute(result);
    }

    public IEnumerator SearchReferents(string url, string query, bool drawObjects, OnPostExcecute excecute)
    {
        if (query.Length > 0) url += "?query=" + query;

        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(url);

        yield return webRequest.SendWebRequest();

        if (webRequest.downloadHandler.text.Equals("")) yield break;

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text)[JSONPARAM_RESULT];

        HashSet<StolperwegeElement> result = new HashSet<StolperwegeElement>();

        Queue<string> idqueue = new Queue<string>();

        StolperwegeElement element;
        for (int i = 0; i < data.Count; i++)
        {
            element = ParseJSON(data[i]);
            if (element == null) continue;
            result.Add(element);

            //if (i > 0 && i % 99 == 0) yield return null;
        }

        //yield return loadStolperwegeElements(idqueue, false);

        // TODO link & draw on drag
        //foreach (object o in referents.Values)
        //{
        //    ((StolperwegeElement)o).link();
        //    if (o.GetType() == typeof(DiscourseReferent))
        //    {
        //        yield return ((DiscourseReferent)o).GetPreferences();
        //    }
        //}


        //foreach (object o in referents.Values)
        //    if (drawObjects && ((StolperwegeElement)o).Object3D == null)
        //        ((StolperwegeElement)o).draw();

        excecute(result);
    }

    public IEnumerator GetAllElements(string type, bool drawObjects, OnPostExcecute excecute)
    {
        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(WS + type.ToLower() + "s");

        type = "org.hucompute.publichistory.datastore.typesystem." + type;

        yield return webRequest.SendWebRequest();
        //Debug.Log(www.text);

        if (webRequest.downloadHandler.text.Equals("")) yield break;

        //Debug.Log(www.text);

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text)[JSONPARAM_RESULT];

        HashSet<StolperwegeElement> result = new HashSet<StolperwegeElement>();

        Queue<string> idqueue = new Queue<string>();

        for (int i = 0; i < data.Count; i++)
        {
            StolperwegeElement element = ParseJSON(data[i]);
            //if (data[i].Keys.Contains(JSONPARAM_EQUI))
            EnqueueEquis(idqueue, data[i]);
            if (element.GetType() == (System.Type)TypeClassTable[type])
            {

                result.Add(element);
            }
            yield return element;
        }

        yield return LoadStolperwegeElements(idqueue, false);

        Hashtable r = new Hashtable(Referents);

        foreach (object o in r.Values)
        {
            ((StolperwegeElement)o).Link();

            if (o.GetType() == typeof(DiscourseReferent))
            {
                yield return ((DiscourseReferent)o).GetPreferences();
            }
        }


        foreach (object o in r.Values)
            if (drawObjects && ((StolperwegeElement)o).StolperwegeObject == null)
                ((StolperwegeElement)o).Draw();

        excecute(result);
    }

    public IEnumerator GetPositions(float lat, float lon, bool drawObjects, bool includeAllBuildings, OnPostExcecute excecute)
    {
        string type;
        string url = WS + "positions?latitude=" + ("" + lat).Replace(",", ".") + "&longitude=" + ("" + lon).Replace(",", ".") + "&distance=0.4";
        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(url);

        type = "org.hucompute.publichistory.datastore.typesystem.Position";

        Debug.Log(webRequest.url);
        yield return webRequest.SendWebRequest();
        Debug.Log(webRequest.downloadHandler.text);

        if (webRequest.downloadHandler.text.Equals("")) yield break;

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text)[JSONPARAM_RESULT];

        HashSet<StolperwegeElement> result = new HashSet<StolperwegeElement>();

        Queue<string> idqueue = new Queue<string>();

        for (int i = 0; i < data.Count; i++)
        {
            StolperwegeElement element = ParseJSON(data[i]);
            EnqueueEquis(idqueue, data[i]);
            if (element.GetType() == (System.Type)TypeClassTable[type])
            {
                result.Add(element);
            }
            yield return element;
        }

        yield return LoadStolperwegeElements(idqueue, false);

        foreach (object o in Referents.Values)
            ((StolperwegeElement)o).Link();

        foreach (object o in Referents.Values)
            if (drawObjects && ((StolperwegeElement)o).StolperwegeObject == null)
                ((StolperwegeElement)o).Draw();

        if (includeAllBuildings)
        {
            yield return GetAllElements("UnityBuilding", false, (HashSet<StolperwegeElement> buildings) => { result.UnionWith(buildings); });
        }

        excecute(result);
    }

    public IEnumerator GetElement(string type, string id, bool draw, OnCreated excecutor)
    {
        type = type.Replace("org.hucompute.publichistory.datastore.typesystem.", "").ToLower();

        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(WS + type + "\\" + id);

        yield return webRequest.SendWebRequest();
        Debug.Log(webRequest.downloadHandler.text);

        if (webRequest.downloadHandler.text.Equals("")) yield break;

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text);

        if (!data.Keys.Contains(JSONPARAM_RESULT)) yield break;

        data = data[JSONPARAM_RESULT];

        StolperwegeElement element = ParseJSON(data);

        Queue<string> idqueue = new Queue<string>();
        HashSet<StolperwegeElement> result = new HashSet<StolperwegeElement>();

        EnqueueEquis(idqueue, data);
        if (element.GetType() == (System.Type)TypeClassTable[type])
        {
            result.Add(element);
        }
        yield return element;

        yield return LoadStolperwegeElements(idqueue, true);

        foreach (object o in Referents.Values)
            ((StolperwegeElement)o).Link();

        /*
        foreach (object o in referents.Values)
            if (draw && ((StolperwegeElement)o).Object3D == null)
                ((StolperwegeElement)o).draw();*/

        Debug.Log(element.GetType());
        Debug.Log(ParseJSON(data).GetType());

        element.Link();
        if (draw) element.Draw();
        if (element.StolperwegeObject != null) element.StolperwegeObject.gameObject.SetActive(true);

        excecutor?.Invoke(element);
    }

    public delegate void OnCreatedType(RelationType type);

    public IEnumerator CreateRelationType(string name, string to, string from, OnCreatedType onCreated)
    {
        WWWForm postForm = new WWWForm();
        postForm.AddField("name", name);
        postForm.AddField("rangeto", to);
        postForm.AddField("rangefrom", from);

        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(RELATIONTYPE, postForm);

        yield return webRequest.SendWebRequest();

        if (webRequest.isNetworkError) yield break;

        Debug.Log(webRequest.downloadHandler.text);

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text)[JSONPARAM_RESULT];

        RelationType rType = new RelationType
        {
            title = data["Name"].ToString().Split(':')[1],
            from = (Type)TypeClassTable[data["RangeFrom"].ToString()],
            to = (Type)TypeClassTable[data["RangeTo"].ToString().Replace("[]", "")],
            multiple = true,
            mandatory = false,
            id = data["Name"].ToString()
        };

        foreach (String typeStr in TypeClassTable.Keys)
        {
            Type currentType = (System.Type)TypeClassTable[typeStr];

            if (rType.from == currentType || currentType.IsSubclassOf(rType.from))
            {
                ((HashSet<RelationType>)TypeSystemTable[typeStr]).Add(rType);
            }
        }

        onCreated?.Invoke(rType);
    }

    public IEnumerator DeleteElement(string id)
    {
        string uri = DELETEELEMENT + "?id=" + id;

        UnityWebRequest delete = UnityWebRequest.Delete(uri);

        yield return delete.SendWebRequest();

        if (delete.isNetworkError) yield break;

        if (Referents.Contains(id))
        {
            StolperwegeElement se = (StolperwegeElement)Referents[id];

            se.RmvAllRelations();

            Referents.Remove(id);
        }
    }

    public IEnumerator GetUnityPositionsForElement(string elementId, OnPostExcecute excecute)
    {

        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(UNITYPOSITIONS + "?building" + elementId);

        yield return webRequest.SendWebRequest();
        Debug.Log(webRequest.downloadHandler.text);

        if (webRequest.downloadHandler.text.Equals("")) yield break;

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text)[JSONPARAM_RESULT];

        HashSet<StolperwegeElement> result = new HashSet<StolperwegeElement>();

        Queue<string> idqueue = new Queue<string>();

        for (int i = 0; i < data.Count; i++)
        {
            StolperwegeElement element = ParseJSON(data[i]);
            EnqueueEquis(idqueue, data[i]);
            result.Add(element);

            yield return element;
        }

        yield return LoadStolperwegeElements(idqueue, false);

        foreach (object o in Referents.Values)
            ((StolperwegeElement)o).Link();

        excecute?.Invoke(result);
    }




    public delegate void OnCreated(StolperwegeElement element);

    /// <summary>
    /// Erstellt ein Element eines bestimmten Types
    /// </summary>
    /// <param name="type"></param>
    /// <param name="paramsTable">Key-Value Paare, welche mit an die REST Funktion des Web-Services �bergeben werden</param>
    /// <param name="excecutor">Wird mit dem erstellten Element aufgerufen</param>
    /// <returns></returns>
    public IEnumerator CreateElement(string type, Hashtable paramsTable, OnCreated excecutor, bool draw = true)
    {
        WWWForm form = new WWWForm();

        string keys = "";

        //Debug.Log(paramsTable.Count);

        foreach (string key in paramsTable.Keys)
        {
            form.AddField(key, paramsTable[key].ToString());

            keys += key + " " + paramsTable[key].ToString() + "; ";
            Debug.Log(keys);
        }


        //if (type.Contains("unityposition") && CurrentBuilding != null)
        //    form.AddField("building", CurrentBuilding.id);

        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(WS + type, form);

        Debug.Log(WS + type);
        Debug.Log(webRequest.url);

        yield return webRequest.SendWebRequest();

        Debug.Log(webRequest.downloadHandler.text);

        Debug.Log("create " + keys + "  " + webRequest.downloadHandler.text);

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text);

        if (!data.Keys.Contains(JSONPARAM_RESULT)) yield break;

        data = data[JSONPARAM_RESULT];

        StolperwegeElement element = ParseJSON(data);

        Queue<string> idqueue = new Queue<string>();
        HashSet<StolperwegeElement> result = new HashSet<StolperwegeElement>();

        EnqueueEquis(idqueue, data);
        if (element.GetType() == (System.Type)TypeClassTable[type])
        {
            result.Add(element);
        }
        yield return element;

        yield return LoadStolperwegeElements(idqueue, true);

        foreach (object o in Referents.Values)
            ((StolperwegeElement)o).Link();

        element.Link();
        if (draw && element.StolperwegeObject == null) element.Draw();
        if (element.StolperwegeObject != null) element.StolperwegeObject.gameObject.SetActive(true);

        excecutor?.Invoke(element);
    }

    public IEnumerator CreateElement(string type, Hashtable paramsTable, OnCreated excecutor)
    {
        yield return CreateElement(type, paramsTable, excecutor, false);
    }


    /// <summary>
    /// Erstellt ein Element aus einer Element-Maske
    /// </summary>
    /// <param name="newElement"></param>
    /// <param name="excecutor"></param>
    /// <returns></returns>
    public IEnumerator CreateElement(NewElement newElement, OnCreated excecutor)
    {
        string type = newElement.Type.Replace("org.hucompute.publichistory.datastore.typesystem.", "").ToLower();

        Hashtable table = new Hashtable();

        foreach (ConnectionPoint cp in newElement.GetComponentsInChildren<ConnectionPoint>())
        {
            if (cp.Value != null && !(cp.Value is GameObject))
                table.Add(cp.Type.title, cp.Value.ToString());
        }

        StolperwegeWordObject[] words = newElement.ConnectedWords;

        if (words[0] != null)
        {
            table.Add("begin", words[0].Range.x);

            if (words[1] != null)
                table.Add("end", words[1].Range.y);
            else
                table.Add("end", words[0].Range.y);

            Debug.Log(table["end"]);
        }



        yield return CreateElement(type, table, excecutor, true);
    }

    public IEnumerator CreateElements(HashSet<NewElement> newElements, OnCreated excecutor)
    {
        foreach (NewElement element in newElements)
            yield return CreateElement(element, excecutor);
    }

    public GameObject CreateElementDummy(string type)
    {
        if (!TypeSystemTable.Contains(type)) return null;

        Debug.Log("create" + type);

        GameObject dummy = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/NewElement")));

        dummy.GetComponent<NewElement>().Init(type, (HashSet<RelationType>)TypeSystemTable[type]);

        return dummy;
    }


    public IEnumerator RemoveElement(string type, StolperwegeElement element)
    {
        // remove all relations
        //Debug.Log(element);
        //foreach (StolperwegeElement rel in element.RelatedElements)
        //{
        //    rel.RmvStolperwegeRelation(element, EquivalentRelation);
        //    element.RmvStolperwegeRelation(rel, EquivalentRelation);

        //}

        //element.RmvAllRelations();

        string uri = WS + type + "?uri=" + element.ID;

        Debug.Log(uri);


        using (UnityWebRequest del = UnityWebRequest.Delete(uri))
        {

            del.downloadHandler = new DownloadHandlerBuffer();
            yield return del.SendWebRequest();

            if (del.isNetworkError || del.isHttpError)
            {
                Debug.Log(del.error);
            }
            else
            {
                Debug.Log(del.downloadHandler.text);
            }
        }

        if (element.StolperwegeObject != null) Destroy(element.StolperwegeObject.gameObject);
    }

    public GameObject CreateElementDummy(Type type)
    {
        return CreateElementDummy(TypeToString(type));
    }

    public string TypeToString(Type type)
    {
        return "org.hucompute.publichistory.datastore.typesystem." + type.Name.Replace("Stolperwege", "");
    }

    public List<Type> GetSubtypes(Type type)
    {
        List<Type> types = new List<Type>();

        foreach (Type t in TypeClassTable.Values)
        {
            if (t.IsSubclassOf(type))
            {
                types.Add(t);
            }

        }

        return types;
    }

    /// <summary>
    /// Downloads an object and instantiates it at the given posiotion 
    /// + adds ServerObject script to instantiated
    /// </summary>
    public delegate void OnDownloaded(GameObject unityObj);
    public IEnumerator DownloadObject(string BundleURL, string position, OnDownloaded onDownload, string AssetName = "")
    {
        UnityWebRequest webRequest = UnityWebRequestAssetBundle.GetAssetBundle(BundleURL);
        webRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return webRequest.SendWebRequest();
        Debug.Log(webRequest.downloadHandler.text);

        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.Log(webRequest.error);
        }
        else
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(webRequest);
            Debug.Log("Downloaded AssetBundle: " + bundle.name);
            //Debug.Log(bundle.isStreamedSceneAssetBundle);
            Debug.Log(bundle.GetAllAssetNames());
            bundle.LoadAllAssets();
            Debug.Log(AssetBundle.GetAllLoadedAssetBundles());


            var allAssets = bundle.LoadAllAssets();
            foreach (var asset in allAssets)
            {
                //TODO:
                string[] splitPositionData = position.Split(':');


                var go = Instantiate(asset, NetworkUtils.StringToVector3(splitPositionData[0]), Quaternion.Euler(NetworkUtils.StringToVector3(splitPositionData[1]))) as GameObject;

                go.AddComponent<MeshCollider>();
                go.GetComponent<MeshCollider>().convex = true;
                go.GetComponent<MeshCollider>().isTrigger = true;
                Debug.Log("colliders added to downloaded object.");

                ServerObject script = go.AddComponent<ServerObject>();
                script.enabled = true;
                script.ModelName = BundleURL;
                Debug.Log("serverobject script added.");

                onDownload(go);
                Debug.Log(go.GetComponent<ServerObject>().ModelName);
                NetworkServer.Spawn(go);


            }

            bundle.Unload(false);
            //bundle.GetAllAssetNames();
        }
    }





    public string createRoom(string value, bool isPublic, string description = "")
    {
        string returnedURI = null;

        string roomJson = RoomManager.CreateRoomData();

        //create VirtualRoom on server:

        //string www = "http://app.stolperwege.hucompute.org/room?value=testroom&description=test&public=true";
        string www = ROOM + "?value=" + value + "&description=" + description + "&public=" + isPublic;


        try
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(www);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(roomJson);
                streamWriter.Flush();
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                Debug.Log(responseText);

                string pattern = @"http:.*(?=\"",)";
                string input = responseText;


                foreach (Match m in Regex.Matches(input, pattern))
                {
                    Debug.Log(m.Value);
                    Debug.Log("Created Room: uri = " + m.Value);
                    returnedURI = m.Value;
                }

            }
        }
        catch (WebException e)
        {
            Debug.Log(e.Message);
        }


        //TODO:
        //add json as preference

        CreatePreference(returnedURI, "roomObjectData", roomJson); //not working?








        //TODO: add base permission
        //give max. permission to current user:






        if (returnedURI != null)
        {
            Debug.Log("uri: " + returnedURI);
            return returnedURI;
        }
        else
        {
            return "error";
        }

    }




    public void updateRoom()
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

        string json = NetworkUtils.DictToJson(gameObjectAndPosition);

        Debug.Log(json);

        //send updated Data to server:
        //TODO:


    }


    /// <summary>
    /// Converts Vector3 to string
    /// </summary>
    public string SerializeVector3Array(Vector3[] aVectors)
    {
        StringBuilder sb = new StringBuilder();
        foreach (Vector3 v in aVectors)
        {
            sb.Append(v.x).Append(" ").Append(v.y).Append(" ").Append(v.z).Append("|");
        }
        if (sb.Length > 0) // remove last "|"
            sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }
    public IEnumerator CreatePreference(DiscourseReferent discourseReferent, string key, string value)
    {
        yield return CreatePreference((string)discourseReferent.ID, key, value, (success) => { discourseReferent.Link(); });
    }

    public delegate void preferenceCreated(bool success);
    public IEnumerator CreatePreference(string uri, string key, string value, preferenceCreated preferenceCreated = null)
    {
        Debug.Log("Preference!");
        WWWForm form = new WWWForm();
        form.AddField("uri", uri);
        form.AddField("key", key);
        form.AddField("value", value);

        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(PREFERENCE, form);

        yield return webRequest.SendWebRequest();
        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text);

        if (data.Keys.Contains("success") && !bool.Parse(data["success"].ToString())) Debug.Log(uri + " " + key + " type: " + value);

        Debug.Log("addRel" + uri + " " + key + " type: " + value);
        if (preferenceCreated == null) preferenceCreated(true);
    }

    public IEnumerator CreateRelation(StolperwegeElement start, StolperwegeElement end, RelationType type)
    {
        WWWForm form = new WWWForm();

        form.AddField("start", (string)start.ID);
        form.AddField("end", (string)end.ID);
        form.AddField("type", type.id);

        UnityWebRequest webRequest = StolperwegeHelper.CreateWebRequest(RELATION, form);

        yield return webRequest.SendWebRequest();

        JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text);

        Debug.Log(data.ToJson());

        if (data.Keys.Contains("success"))
        {
            if (!bool.Parse(data["success"].ToString())) Debug.Log(end.ID + " " + start.ID + " type: " + type.title);
            else
            {
                if (type.Equals(EquivalentRelation))
                {
                    if (start is DiscourseReferent) ((DiscourseReferent)start).Equivalents.Add(end);
                    if (end is DiscourseReferent) ((DiscourseReferent)end).Equivalents.Add(start);
                }

                Debug.Log("addRel" + start.ID + " " + end.ID + " " + type.id + " " + webRequest.downloadHandler.text);
            }
        }
    }

    public delegate void OnPermissionCreated(StolperwegePermission permission);

    public IEnumerator CreatePermission(StolperwegeUser user, StolperwegeElement element, string permissionType, OnPermissionCreated onCreated)
    {

        if (user == null || element == null)
        {
            Debug.Log("User and the other StolperwegeElement must be both defined.");
            yield break;
        }
        HashSet<StolperwegeElement> relatedElems = element.GetRelatedElementsByType(EquivalentRelation);

        StolperwegePermission permission = null;

        // proof if the permission on the target object eventually already exists

        foreach (StolperwegeElement relatedElem in relatedElems)
        {
            if (relatedElem is StolperwegePermission)
            {
                StolperwegePermission relatedPermission = (StolperwegePermission)relatedElem;
                if (!relatedPermission.Linked) relatedPermission.Link();
                if (relatedPermission.permissionType.Equals(permissionType))
                {
                    permission = relatedPermission;
                }
                else
                {
                    if (user.getRelationsTypesTo(relatedPermission).Count > 0)
                    {
                        user.RmvStolperwegeRelation(relatedPermission, StolperwegeInterface.EquivalentRelation);
                        //yield return RmvRelation(user, relatedPermission, EquivalentRelation);
                    }
                }

            }
        }

        // create a new permission and the relation to the target if not

        if (permission == null)
        {

            Hashtable parameter = new Hashtable();
            parameter.Add("value", permissionType);

            yield return CreateElement("permission", parameter, (StolperwegeElement result) => { permission = (StolperwegePermission)result; });

            Debug.Log(permission);

            yield return permission.AddStolperwegeRelationAsynch(element, EquivalentRelation);
        }

        onCreated(permission);

        yield return permission.AddStolperwegeRelationAsynch(user, EquivalentRelation);

        // link the user to the permission
    }

    public IEnumerator RemoveRelation(StolperwegeElement start, StolperwegeElement end, RelationType type)
    {
        string uri = RELATION + "?start=" + start.ID + "&end=" + end.ID + "&type=" + type.id;

        Debug.Log(uri);

        using (UnityWebRequest del = UnityWebRequest.Delete(uri))
        {

            del.downloadHandler = new DownloadHandlerBuffer();
            yield return del.SendWebRequest();

            if (del.isNetworkError || del.isHttpError)
            {
                Debug.Log(del.error);
            }
            else
            {
                Debug.Log(del.downloadHandler.text);
            }
        }

    }

    public static string lastTimer = "";

    public RelationType GetRelationTypeFromName(string type, string name)
    {

        if (!type.Contains("org.hucompute.publichistory.datastore.typesystem."))
            type = "org.hucompute.publichistory.datastore.typesystem." + type;

        HashSet<RelationType> types = (HashSet<RelationType>)TypeSystemTable[type];

        if (types == null) return new RelationType();

        foreach (RelationType t in types)
            if (t.title.Equals(name))
                return t;

        return new RelationType();
    }

    public IEnumerator SetupBrowser(DataBrowser browser)
    {
        // Close any panels animated
        browser.DataPanel.SetComponentStatus(false);
        if (browser.FilterPanel.IsActive)
        {
            if (browser.SearchPanel.IsActive) StartCoroutine(browser.FilterPanel.Activate(false));
            else yield return StartCoroutine(browser.FilterPanel.Activate(false));
        }
        if (browser.SearchPanel.IsActive) yield return StartCoroutine(browser.SearchPanel.Activate(false));

        // ============================= FILTER PANEL SETUP ============================

        // Set filters
        if (!browser.DataSpaceFilterMap.ContainsKey(Name))
        {
            Dictionary<string, InteractiveCheckbox.CheckboxStatus> filters = new Dictionary<string, InteractiveCheckbox.CheckboxStatus>();
            foreach (string type in TypeClassTable.Keys)
            {
                if (((Type)TypeClassTable[type]).IsSubclassOf(typeof(DiscourseReferent)) ||
                TypeClassTable[type].Equals(typeof(DiscourseReferent)))
                {
                    name = type.Substring(type.LastIndexOf('.') + 1);
                    filters.Add(name, InteractiveCheckbox.CheckboxStatus.AllChecked);
                }
            }
            browser.DataSpaceFilterMap.Add(Name, filters);
        }

        // Define filter update event
        browser.FilterPanel.FilterUpdater = () =>
        {
            for (int i = 0; i < browser.FilterPanel.Checkboxes.Length; i++)
            {
                browser.FilterPanel.Checkboxes[i].gameObject.SetActive((browser.FilterPanel.TypePointer + i) < browser.FilterPanel.TypeList.Count);
                if (browser.FilterPanel.Checkboxes[i].gameObject.activeInHierarchy)
                {
                    browser.FilterPanel.Checkboxes[i].ButtonValue = browser.FilterPanel.TypeList[browser.FilterPanel.TypePointer + i];
                    browser.FilterPanel.Checkboxes[i].Status = browser.FilterPanel.Types[browser.FilterPanel.TypeList[browser.FilterPanel.TypePointer + i]];
                    browser.FilterPanel.Openers[i].gameObject.SetActive(false);
                }
            }
        };

        // Set event for changing checkboxes
        browser.FilterPanel.CheckboxUpdater = (type, status) => { browser.FilterPanel.Types[type] = status; };

        // Initialize filter panel
        browser.FilterPanel.Init("Discourse Referent Types", browser.DataSpaceFilterMap[Name]);

        // ============================= DATA PANEL SETUP ============================
        // Root button functionality
        browser.DataPanel.Root.gameObject.SetActive(false);

        // Parent button functionality
        browser.DataPanel.ParentDir.gameObject.SetActive(false);

        // Define browser update event
        browser.BrowserUpdater = () =>
        {
            browser.DataPanel.Init("Loading discourse referents...");
            StartCoroutine(SearchReferents(DISCOURSEREFERENTS, "", false, (res) =>
            {
                if (browser.SelectedInterface.Equals(this))
                {
                    List<DiscourseReferent> datas = new List<DiscourseReferent>();
                    foreach (DiscourseReferent element in res)
                    {
                        if (browser.FilterPanel.Types.ContainsKey(element.PrettyType) &&
                            browser.FilterPanel.Types[element.PrettyType] == InteractiveCheckbox.CheckboxStatus.AllChecked &&
                            CheckPatternMatch(element.Value, browser.SearchPanel.SearchPattern))
                            datas.Add(element);
                    }
                    browser.DataPanel.Init("Discourse referents", datas);
                }
                
            }));
        };

        // Define datacontainer events
        foreach (DataContainer dc in browser.DataPanel.DataContainers)
            dc.OnClick = null;

        // ============================= LOADING LAST STATE ============================ 

        StartCoroutine(browser.FilterPanel.Activate(true));
        StartCoroutine(browser.SearchPanel.Activate(true));
        browser.BrowserUpdater();
    }

    private bool CheckPatternMatch(string value, string pattern)
    {
        if (pattern == "") return true;
        return value.ToLower().Contains(pattern.ToLower());
    }

}
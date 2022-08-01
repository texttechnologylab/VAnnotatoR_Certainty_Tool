using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AvatarObject : InteractiveObject
{
    private const string MENU_TILE = "StolperwegeElements/AvatarOptions";
    private bool shrinked = false;
    public ChangablePartScript currentlyChangeing = null;
    private GameObject _menuTile = null;
    private GameObject menuTile

    {
        get 
        {
            return _menuTile;
        }
        set
        {
            if (value == null) Destroy(_menuTile);
            _menuTile = value;
        }
    }
    private DiscourseReferent _config = null;
    public DiscourseReferent config
    {
        get
        {
            return _config;
        }
        set
        {
            if (value == _config) return;
            _config = value;
            colorize();
        }
    }
    private bool _inDresser = true;
    private bool InDresser
    {
        get
        {
            return _inDresser;
        }
        set
        {
            if (value == _inDresser) return;

            _inDresser = value;
            ManipulatedByGravity = !value;
        }
    }
    private bool inEditMode = false;
    public DressingMenu parentScript = null;
    private StolperwegeInterface StolperwegeInterface;

    public override void Start()
    {
        base.Start();

        Grabable = true;
        ManipulatedByGravity = false;
        IsCollectable = false;
        if (!IsDefaultConfig())
        {
            foreach (string s in GetChangeableBodyParts())
            {
                transform.Find(s).gameObject.AddComponent<ChangablePartScript>();
                transform.Find(s).GetComponent<ChangablePartScript>().allowedColors = new HashSet<string>(AllowedHexColors(s));
            }
        }
        GetComponent<SphereCollider>().isTrigger = true;
        StolperwegeInterface = SceneController.GetInterface<StolperwegeInterface>();
    }

    public void Update()
    {
        if (IsGrabbed)
        {
            //Testing only

            if (Input.GetKey(KeyCode.C))
            {
                if (IsDefaultConfig())
                {
                    Debug.Log("You can't change the default config"); 
                    return;
                }
                foreach (string s in GetChangeableBodyParts())
                {
                    Debug.Log(s + " " + AllowedColors(s).ToString());
                    ChangeColor(s, AllowedColors(s)[0]);
                    ChangeConfigColor(s, AllowedHexColors(s)[0]); ;
                }
                
            }
            if (Input.GetKey(KeyCode.D))
            {
                DeleteAvatar();
            }
            if (Input.GetKey(KeyCode.S))
            {
                Save();
            }
            //
        }
        if (shrinked && !IsGrabbed)
        {
            //transform.localScale = Vector3.one;
            transform.localScale = new Vector3(2.040816f, 2.040816f, 2.040816f);
            shrinked = false;
        }
        if (!IsGrabbed && menuTile != null) menuTile = null;
        if (!IsGrabbed && inEditMode)
        {
            inEditMode = false;
            menuTile = null;
        }
        }

    public void resetParts()
    {
        if (!IsDefaultConfig())
        {
            foreach (ChangablePartScript cps in GetComponentsInChildren<ChangablePartScript>())
            {
                cps.DestroyColorBalls();
                cps.resetColor();
            }
            currentlyChangeing = null;
        }
    } 

    public override GameObject OnGrab(Collider other)
    {

        resetParts();
        GameObject result = gameObject;
        //gameObject.GetComponent<Animator>().enabled = false;
        Debug.Log("Grabbed: " + config.ID);
        float scale = 0.1f;
        float reverseScale = 1f - scale;
        if (!shrinked) gameObject.transform.localScale -= new Vector3(gameObject.transform.localScale.x * reverseScale, gameObject.transform.localScale.y * reverseScale, gameObject.transform.localScale.z * reverseScale);
        shrinked = true;
        //Debug.Log(gameObject.transform.localScale.ToString());
        //InDresser = false;
        //gameObject.GetComponent<Animator>().enabled = true;
        base.OnGrab(other);
        if (other.tag.Contains("left"))
        {
            result.transform.localPosition = new Vector3(-0.0067f, 0.0062f, -0.0022f);
            result.transform.localRotation = Quaternion.Euler(12.368f, -18.36f, -46.159f);
        }
        else
        {
            result.transform.localPosition = new Vector3(0.0072f, 0.0058f, -0.0008f);
            result.transform.localRotation = Quaternion.Euler(4.679f, 38f, 55.7f);
        }
        
        return result;
    }


    protected override void OnTriggerEnter(Collider other)
    {
        
       
        if (other.tag.Equals("leftIndexFinger") || other.tag.Equals("rightIndexFinger"))
        {
            if (IsGrabbed)
            {
                if (menuTile == null)
                {
                    menuTile = (GameObject)Instantiate(Resources.Load(MENU_TILE));
                    menuTile.name = "AvatarOptions";
                    menuTile.transform.SetParent(transform);
                    menuTile.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    menuTile.transform.localPosition = new Vector3(2f, 2f, 2f);
                }
                else
                {
                    menuTile = null;
                }
            }
        }
        if (other.name.Equals("head") && IsGrabbed)
        {
            //Debug.Log("enter");
            //Debug.Log(config.id + " configID");
            //Debug.Log((StolperwegeInterface.CurrentUser.activeConfig.id + " ActiveConfigID"));
            //if (!(StolperwegeInterface.CurrentUser.activeConfig.id.Equals(config.id)))
            //{
                Debug.Log(config.ID);
                StolperwegeInterface.CurrentUser.activeConfig = config;
                StartCoroutine(StolperwegeInterface.CurrentUser.AddPreference("activeConfig", (string)config.ID));
                GetComponentInParent<AvatarColorManager>().colorFromActiveConfig();
            //}

            
        }
        base.OnTriggerEnter(other);

    }

    public void DeleteAvatar()
    {
        if (!IsDefaultConfig())
        {
            if(config.ID.Equals(StolperwegeInterface.CurrentUser.activeConfig.ID))
            {
                StolperwegeInterface.CurrentUser.activeConfig = StolperwegeInterface.DefaultAvatarConfig;
                GetComponentInParent<AvatarColorManager>().colorFromActiveConfig();
            }
            StartCoroutine(StolperwegeInterface.RemoveElement("discoursereferent", config));
            foreach (DiscourseReferent c in parentScript.configs)
            {
                if (config.ID.Equals(c.ID)) parentScript.configs.Remove(c);
            }
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Can't delete default config");
        }
    }

    //public override StolperwegeElement Referent
    //{

    //    get
    //    {
    //        return base.Referent;
    //    }

    //    set
    //    {
    //        base.Referent = value;
    //    }
    //}

    public void colorize()
    {
        //Debug.Log(string.Format("Colorful started\nConfigID: {0}\nCount: {1}", config.id, config.Preferences.Count));
        foreach (DictionaryEntry e in config.Preferences)
        {
           // Debug.Log(e.Key.ToString());
           // Debug.Log(e.Value.ToString());
            bool hasPart = false;
            for (int j = 0; j < transform.childCount; j++)
            {
                if (transform.GetChild(j).GetComponent<SkinnedMeshRenderer>() == null) continue;
                if (transform.GetChild(j).name.Equals((string)e.Key))
                {
                    hasPart = true;
                    break;
                }
            }
            //   Debug.Log(hasPart);
            if (hasPart)
            {
                ChangeColor(transform.gameObject, (string)e.Key, StolperwegeHelper.ConvertHexToColor((string)e.Value));
            }
        }
    }

    public void ChangeConfigColor(string part, string color)
    {
        config.AddPreference(part, color);
        foreach (DiscourseReferent aconfig in StolperwegeInterface.CurrentUser.avatarConfigs)
        {
            if ((config.ID).Equals(aconfig.ID))
            {
                StartCoroutine(aconfig.AddPreference(part, color));
                return;    
            } 
        }
        Debug.Log("AConfig not found");
    }

    /// <summary>Changes the color of any subpart of a Gameobject, that has a SkinnedMeshRenderer component.</summary>
    /// <param name="anchor">The Gameobject</param>
    /// <param name="partName">The subpart</param>
    /// <param name="color">The desired color</param>
    private void ChangeColor(GameObject anchor, string partName, Color color)
    {
        anchor.transform.Find(partName).GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", color);
    }

    /// <summary>
    /// Changes the color of a subpart of a Gameobject wiht the Avatar Tag, that has a SkinnedMeshRenderer component.</summary>
    /// <param name="part">The subpart</param>
    /// <param name="color">The desired color</param>
    public new void ChangeColor(string part, Color color)
    {
        ChangeColor(gameObject, part, color);
    }

    /// <summary>Converts rgb 0-255 to Unity Color.</summary>
    /// <param name="r">Red (0-255)</param>
    /// <param name="g">Green (0-255)</param>
    /// <param name="b">Blue (0-255)</param>
    public Color ConvertColor(int r, int g, int b)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
    }

    public List<Color> AllowedColors(string bodyPart)
    {
        List<Color> colors = new List<Color>();
        foreach (DictionaryEntry e in config.Preferences)
        {
            if (e.Key.Equals("Color_" + bodyPart))
            {
                string value = e.Value.ToString();
                //Debug.Log("Liste erlaubter Farben für " + e.Key.ToString() + ": " + value);
                foreach (string s in value.Split(','))
                {

                    colors.Add(StolperwegeHelper.ConvertHexToColor(s));

                }
                return colors;
            }

        }
        return colors;
    }
    public List<string> AllowedHexColors(string bodyPart)
    {
        List<string> colors = new List<string>();
        foreach (DictionaryEntry e in config.Preferences)
        {
            if (e.Key.Equals("Color_" + bodyPart))
            {
                string value = e.Value.ToString();
                //Debug.Log("Liste erlaubter Farben für " + e.Key.ToString() + ": " + value);
                foreach (string s in value.Split(','))
                {

                    colors.Add(s);

                }
                return colors;
            }

        }
        return colors;
    }



    public void Save()
    {
        if (IsDefaultConfig())
        {
            Debug.Log("You can't change the default config"); 
            return;
        }
        for (int j = 0; j < transform.childCount; j++)
        {
            if (transform.GetChild(j).GetComponent<SkinnedMeshRenderer>() == null) continue; 

            if (AllowedColors(transform.GetChild(j).name).Contains(transform.GetChild(j).GetComponent<SkinnedMeshRenderer>().material.color))
            {
                ChangeConfigColor(transform.GetChild(j).name, ColorUtility.ToHtmlStringRGB(transform.GetChild(j).GetComponent<SkinnedMeshRenderer>().material.color));
                StartCoroutine(StolperwegeInterface.CreatePreference((string)config.ID, transform.GetChild(j).name, ColorUtility.ToHtmlStringRGB(transform.GetChild(j).GetComponent<SkinnedMeshRenderer>().material.color)));
                Debug.Log("Saved color: " + transform.GetChild(j).GetComponent<SkinnedMeshRenderer>().material.color + " hex: " + ColorUtility.ToHtmlStringRGB(transform.GetChild(j).GetComponent<SkinnedMeshRenderer>().material.color) + " for boypart: " + transform.GetChild(j).name + " in: " + config.ID);
            }   
            else
            {
                Debug.Log("Couldn't save color: " + transform.GetChild(j).GetComponent<SkinnedMeshRenderer>().material.color + " hex: " + ColorUtility.ToHtmlStringRGB(transform.GetChild(j).GetComponent<SkinnedMeshRenderer>().material.color) + " for boypart: " + transform.GetChild(j).name + " in: " + config.ID);
            }
        }        
    }



    private List<string> GetChangeableBodyParts()
    {
        List<string> bodyParts = new List<string>();

        for (int j = 0; j < transform.childCount; j++)
        {
            if (transform.GetChild(j).GetComponent<SkinnedMeshRenderer>() == null) continue;
            if (AllowedColors(transform.GetChild(j).name).Count > 0) bodyParts.Add(transform.GetChild(j).name);
        }
        if (bodyParts.Count == 0) Debug.Log("No colorable bodyparts found");
        return bodyParts;
    }
    public bool IsDefaultConfig()
    {
        foreach (DictionaryEntry e in config.Preferences)
        {
            if (e.Key.ToString().Equals("default") && e.Value.ToString().Equals("true"))
            {
                return true;
            }
        }
        return false;
    }




    //private bool _inflated = false;
    //public bool Inflated
    //{
    //    get
    //    {
    //        return _inflated;
    //    }

    //    set
    //    {
    //        if (_inflated == value) return;

    //        _inflated = value;

    //        if (_inflated)
    //        {
    //            transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
    //            GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cube);

    //            stand.transform.rotation = transform.rotation;
    //            stand.transform.localScale = transform.localScale;
    //            stand.name = "stand";
    //            stand.GetComponent<MeshRenderer>().material = Resources.Load("materials/StolperwegeMaterials/StolpersteinStandMaterial") as Material;
    //            stand.GetComponent<Collider>().enabled = false;
    //            GetComponent<BoxCollider>().center = new Vector3(0, -4f, 0);
    //            GetComponent<BoxCollider>().size = new Vector3(1, 10f, 1);
    //            stand.transform.parent = transform;
    //            stand.transform.localPosition = new Vector3(0, -1, 0);

    //            Mesh mesh = stand.GetComponent<MeshFilter>().mesh;

    //            Vector3[] verts = mesh.vertices;
    //            Vector3[] newverts = new Vector3[verts.Length];

    //            for (int i = 0; i < verts.Length; i++)
    //            {
    //                Vector3 v = verts[i];
    //                if (v.y == -0.5) newverts[i] = new Vector3(v.x, v.y - 10, v.z);
    //                else if (v.y == -10.5) newverts[i] = new Vector3(v.x, v.y + 10, v.z);
    //                else newverts[i] = v;
    //            }

    //            mesh.vertices = newverts;
    //            mesh.RecalculateBounds();

    //            ShowSideImages(true);
    //        }
    //        else
    //        {
    //            transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
    //            GetComponent<BoxCollider>().center = new Vector3(0, 0, 0);
    //            GetComponent<BoxCollider>().size = new Vector3(1, 1, 1);
    //            Destroy(transform.Find("stand").gameObject);
    //            ShowSideImages(false);
    //        }


    //    }

    //}

    //private bool infClick = false;

    //public override void onPointerEnter(Collider other)
    //{
    //    base.onPointerEnter(other);

    //    infClick = false;
    //}

    //public override void Update()
    //{
    //    base.Update();

    //    if (pointertriggered && !infClick && InputInterface.getButtonDown(InputInterface.ControllerType.LHAND, InputInterface.ButtonType.STICK))
    //    {
    //        Inflated = !Inflated;
    //        infClick = true;
    //    }
    //}
}

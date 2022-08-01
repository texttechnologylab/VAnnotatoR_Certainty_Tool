using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Menu(PrefabPath + "StandardMenuItem", true)]
public class DressingMenu : MenuScript
{
    //private bool creatingAvatar = false;
    public bool dresser = true;
    public bool loadLocal = true;
    //private GameObject pedestals;
    //private int configIndex = 0;
    //private int editIndex;
    //private DressingMenu parentMenu = null;
    public List<DiscourseReferent> configs;
    //private StolperwegeInterface _stolperwegeInterface;

    public override void Start()
    {
        base.Start();
        SetNameAndIcon("Avatar", "\xf4ff");
        //_stolperwegeInterface = SceneController.GetInterface<StolperwegeInterface>();
        // TODO
        //switch (gameObject.name)
        //{        

        //    case "NewAvatarIcon":

        //        OnClick = CreateNewAvatar;
        //        parentMenu = GetComponentInParent<DressingMenu>();
        //        break;

        //    case "ScrollLeft":
                
        //        OnClick = ScrollLeft;
        //        parentMenu = GetComponentInParent<DressingMenu>();
        //        break;

        //    case "ScrollRight":

        //        OnClick = ScrollRight;
        //        parentMenu = GetComponentInParent<DressingMenu>();
        //        break;

        //    case "DressingRoomIcon":

                
        //        _proofStatus = () => { Active = _stolperwegeInterface.CurrentUser != null; };
        //        OnClick = DressingRoom;
        //        parentMenu = this;

        //        //postActivation = () =>
        //        //{
        //        //    Debug.Log("postActivation");        
        //        //    if (!dresser) StartCoroutine(ActivateSubMenu());
        //        //};

        //        break;

        //    default:

        //        _proofStatus = () => { Active = true; };
        //        OnClick = SubmenuController;
        //        break;

        //}
        //base.Start();
    }

//    public AvatarObject[] GetAvatarObjects()
//    {
        
//        return pedestals.GetComponentsInChildren<AvatarObject>();
//    }

//    private void getValues()
//    {
//        pedestals = transform.parent.GetComponentInParent<DressingMenu>().pedestals;
//        configIndex = transform.parent.GetComponentInParent<DressingMenu>().configIndex;
//        editIndex = transform.parent.GetComponentInParent<DressingMenu>().editIndex;
//        configs = transform.parent.GetComponentInParent<DressingMenu>().configs;
//    }

//    //private void Save(int index)
//    //{
//    //    foreach(KeyValuePair<string, string> e in pedestals.transform.GetChild(index).GetComponent<Pedestal>().coloring)
//    //    {
//    //        StolperwegeInterface.CreatePreference(pedestals.transform.GetChild(index).GetComponent<Pedestal>().config.id, e.Key, e.Value);
//    //    }
//    //}

//    private HashSet<string> getPaintableBodyparts(DiscourseReferent config)
//    {
//        HashSet<string> parts = new HashSet<string>();
//        foreach (DictionaryEntry preference in config.Preferences)
//        {
//            if (preference.Key.ToString().StartsWith("Color_"))
//            {

//            }
//        }
//        return parts;
//    }
//    public delegate void OnCreated(bool created);
//    private IEnumerator CreateNewHelp(DiscourseReferent discourseReferent, DiscourseReferent dr, OnCreated created)
//    {
//        yield return(_stolperwegeInterface.CreateRelation(dr, _stolperwegeInterface.CurrentUser, StolperwegeInterface.EquivalentRelation));
//        yield return(_stolperwegeInterface.CreateRelation(dr, discourseReferent.avatar, StolperwegeInterface.EquivalentRelation));

//        //   ((DiscourseReferent)dr),(StolperwegeInterface.CurrentUser);
//        //   ((DiscourseReferent)dr).addEquivalent(discourseReferent.avatar);

//        foreach (DictionaryEntry preference in discourseReferent.Preferences)
//        {
//            if (!((string)preference.Key).Equals("default"))
//                yield return(_stolperwegeInterface.CreatePreference(dr, (string)preference.Key, (string)preference.Value));
//                StartCoroutine(dr.AddPreference((string)preference.Key, (string)preference.Value));
//            //else yield return(StolperwegeInterface.CreatePreference(dr, "default", "false", (success) => { }));
//        }
//        yield return (_stolperwegeInterface.CreatePreference(dr, "default", "false"));
//        StartCoroutine(dr.AddPreference("default", "false"));
//        _stolperwegeInterface.CurrentUser.avatarConfigs.Add(dr);
//        configs = _stolperwegeInterface.CurrentUser.avatarConfigs;
//        created(true);
//    }

//    private void CreateNewAvatar(DiscourseReferent discourseReferent)
//    {
//        if (creatingAvatar) return;
//        creatingAvatar = true;
//        getValues();
//        //dresser = GetComponentInParent<DressingRoomMenuIconScript>().dresser;
//        //configs = GetComponentInParent<DressingRoomMenuIconScript>().configs;
//        //pedestals = GetComponentInParent<DressingRoomMenuIconScript>().pedestals;
//        //configIndex = GetComponentInParent<DressingRoomMenuIconScript>().configIndex;
//        Hashtable hs = new Hashtable();
//        hs.Add("value", "newAvatar");
//        hs.Add("description", "newDes");
//        StartCoroutine(_stolperwegeInterface.CreateElement("discoursereferent", hs, (dr) => 
//        {
//            StartCoroutine(CreateNewHelp(discourseReferent, (DiscourseReferent)dr, (created) => {
//                getValues();
//                int configIndex = configs.Count / pedestals.transform.childCount;
//                parentMenu.configIndex = configIndex;
//                loadConfigs(configIndex);
//                creatingAvatar = false;
                
//            }));
            


//        } ));
        
        
//        // Copy Default config and link to current user
//    }
//    private void CreateNewAvatar()
//    {
    
//        if (StolperwegeHelper.defAvatarConfig == null)
//        {
//            Debug.Log("Keine default Avatarconfig gesetzt.");
//            return;
//        }
//        CreateNewAvatar(StolperwegeHelper.defAvatarConfig);
//    }

//private void ScrollLeft()
//    {
//        getValues();
//        Debug.Log("scroll links");
//        scroll("left");

//    }

//    private void ScrollRight()
//    {
//        getValues();
//        Debug.Log("scroll rechts");
//        scroll("right");

//    }

//    private void DressingRoom()
//    {

//        if (dresser)
//        {
//            dresser = false;
//            Debug.Log(_stolperwegeInterface.CurrentUser.ID);
//            Debug.Log("Any configs: " + _stolperwegeInterface.CurrentUser.avatarConfigs.Count);
//            configs = _stolperwegeInterface.CurrentUser.avatarConfigs;
//            drawPedestals();
//            loadConfigs(configIndex);
            
           

//            StartCoroutine(ActivateSubMenu());
//        }
//        else
//        {
//            dresser = true;
//            CloseDressingRoom(GameObject.Find("Pedestals(Clone)"), null);            
//            StartCoroutine(HideSubMenu());
//        }
//    }

//    private void CloseDressingRoom(GameObject gameObject, StolperwegeUser stolperwegeUser)
//    {
//        if(stolperwegeUser != null)
//        {
//            // TODO: update configs
//        }
//        //foreach (Child of Pedestal)
//        //{
//        //    Destroy(Child.assignedObject);
//        //}
//        Destroy(gameObject);
//    }

    

//    private void scroll(string direction)
//    {
//        getValues();
//        int maxIndex = configs.Count / pedestals.transform.childCount;
//        if (maxIndex < 0) maxIndex = 0;
//        if (direction == "left") configIndex--;
//        else if (direction == "right") configIndex++;
//        else
//        {
//            Debug.Log("Unknown scroll direction: " + direction);
//            return;
//        }
//        if (configIndex < 0) configIndex = 0;
//        else if (configIndex > maxIndex) configIndex = maxIndex;

//        Debug.Log("maxIndex: " + maxIndex);
//        transform.parent.GetComponentInParent<DressingMenu>().configIndex = configIndex;
//        transform.parent.GetComponentInParent<DressingMenu>().loadConfigs(configIndex);
//    }

//    private void loadConfigs(int index)
//    {
//        Debug.Log("load configs: index = " + index);
//        Debug.Log(configs.Count);
//        int pedestalCount = pedestals.transform.childCount;

//        for (int i = 0; i < pedestalCount; i++)
//        {
//            pedestals.transform.GetChild(i).GetComponent<Pedestal>().UnAssign();
//            if (i < (configs.Count - pedestalCount * index))
//            {
//                int j = i;
//                if (loadLocal)
//                {
//                    pedestals.transform.GetChild(j).GetComponent<Pedestal>().LoadLocalAvatar("Avatar/avatar1.5.2", configs[i], parentMenu);

//                    //addUtilities(j, configs[index * pedestalCount + j]);
//                    //colorful(j, configs[index * pedestalCount + j]);
                        
//                }
//                else
//                {
//                    StartCoroutine(pedestals.transform.GetChild(i).GetComponent<Pedestal>().LoadAvatar(configs[pedestalCount * index + i].avatar.ID, (success) =>
//                    {
//                        if (success)
//                        {
//                            pedestals.transform.GetChild(i).GetComponentInChildren<AvatarObject>().config = configs[index * pedestalCount + j];
//                    //addUtilities(j, configs[index * pedestalCount + j]);
//                    //colorful(j, configs[index * pedestalCount + j]);

//                    //pedestals.transform.GetChild(j).GetComponent<Pedestal>().config = configs[j]; //added to addUtilities
//                        }
//                    }));
//                }             
//            }            
//        }
//    }
//    private void drawPedestals()
//    {
//        pedestals = (GameObject)GameObject.Instantiate(Resources.Load("StolperwegeElements/Pedestals"));
//        Transform cameraAnchor = StolperwegeHelper.findParentWithTag(gameObject.transform, "MainCamera").transform;
//        Transform pedestalAnchor = cameraAnchor.Find("PedestalAnchor");
//        //pedestals.transform.SetParent(pedestalAnchor, false);
//        //pedestals.transform.SetParent(new GameObject("pedestals").transform, true);
//        pedestals.transform.position = new Vector3(pedestalAnchor.position.x, 0, pedestalAnchor.position.z);
//        pedestals.transform.eulerAngles = new Vector3(0, pedestalAnchor.eulerAngles.y, 0);
//    }
    
    //public static GameObject findParentWithTag(GameObject childObject, string tag)
    //{
    //    Transform t = childObject.transform;
    //    while (t.parent != null)
    //    {
    //        if (t.parent.tag == tag)
    //        {
    //            return t.parent.gameObject;
    //        }
    //        t = t.parent.transform;
    //    }
    //    return null; // Could not find a parent with given tag.

    //private void addUtilities(int pedestalIndex, DiscourseReferent config)
    //{
    //pedestals.transform.GetChild(pedestalIndex).GetComponent<Pedestal>().config = config;

    //foreach (Transform child in pedestals.transform.GetChild(pedestalIndex).GetComponent<Pedestal>().getDisplayedObject().transform)
    //{
    //    if (child.name != "Game_Engine")
    //    {
    //        //List<Color> list = allowedColors(config, child.name);
    //        //              Debug.Log(pedestalIndex + " " + child.name + " " + list.Count);
    //        //if (list.Count > 0) pedestals.transform.GetChild(pedestalIndex).GetComponent<Pedestal>().allowedColors.Add(child.name, list);

    //    }
    //}
    //}

    //private static List<Color> allowedColors(DiscourseReferent config, string bodyPart)
    //{
    //    List<Color> colors = new List<Color>();
    //    foreach (DictionaryEntry e in config.Preferences)
    //    {
    //        if (e.Key.Equals("Color_" + bodyPart))
    //        {
    //            string value = e.Value.ToString();
    //            //Debug.Log("Liste erlaubter Farben für " + e.Key.ToString() + ": " + value);
    //            foreach (string s in value.Split(','))
    //            {

    //                colors.Add(ConvertColor(s));

    //            }
    //            return colors;
    //        }

    //    }
    //return colors;
    //}

    //private void colorful(int pedestalIndex, DiscourseReferent config)
    //{
    //    Debug.Log(string.Format("Colorful started\nPedestalindex: {0}\nConfigID: {1}\nCount: {2}", pedestalIndex, config.id, config.Preferences.Count));

    //    foreach (DictionaryEntry e in config.Preferences)
    //    {
    //        Debug.Log(e.Key.ToString());
    //        Debug.Log(e.Value.ToString());
    //        bool hasPart = false;
    //        for(int j = 0; j < pedestals.transform.GetChild(pedestalIndex).GetComponent<Pedestal>().getDisplayedObject().transform.childCount; j++)
    //        {
    //            if (pedestals.transform.GetChild(pedestalIndex).GetComponent<Pedestal>().getDisplayedObject().transform.GetChild(j).name.Equals((string)e.Key))
    //            {
    //                hasPart = true;
    //                break;
    //            }
    //        }
    //     //   Debug.Log(hasPart);
    //        if(hasPart)ChangeColor(pedestals.transform.GetChild(pedestalIndex).GetComponent<Pedestal>().getDisplayedObject().transform.gameObject ,(string) e.Key, ConvertColor((string)e.Value));
    //    }
    //}

    ///// <summary>Converts rgb 0-255 to Unity Color.</summary>
    ///// <param name="r">Red (0-255)</param>
    ///// <param name="g">Green (0-255)</param>
    ///// <param name="b">Blue (0-255)</param>
    //public static Color ConvertColor(int r, int g, int b)
    //{
    //    return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
    //}

    ///// <summary>Converts rgb 0-255 to Unity Color.</summary>
    ///// <param name="r">Red (0-255)</param>
    ///// <param name="g">Green (0-255)</param>
    ///// 
    ///// <param name="b">Blue (0-255)</param>
    //public static Color ConvertColor(string hex)
    //{
    //    return ConvertColor(int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber), int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber), int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber));
    //}

    ///// <summary>Changes the color of any subpart of a Gameobject, that has a SkinnedMeshRenderer component.</summary>
    ///// <param name="anchor">The Gameobject</param>
    ///// <param name="partName">The subpart</param>
    ///// <param name="color">The desired color</param>
    //private void ChangeColor(GameObject anchor, string partName, Color color)
    //{
    //    anchor.transform.Find(partName).GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", color);
    //}


    // }




}

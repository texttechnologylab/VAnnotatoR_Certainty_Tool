using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.VFX;
using TMPro;

public class SceneController : MonoBehaviour {

    public static List<Interface> Interfaces { get; private set; }
    private static Dictionary<Type, Interface> interfaceDict;
    public static Scene ActiveScene { get { return SceneManager.GetActiveScene(); } }
    public static SceneScript ActiveSceneScript { get; private set; }
    public static bool Initialized;

    // Use this for initialization
    void Awake() 
    {
        StolperwegeHelper.SceneController = this;
        StolperwegeHelper.InitFileExtensionTypeMap();
        InitializeInterfaces();
        SetScenescript();
        if (GameObject.Find("VRWriter") == null)
            Instantiate(Resources.Load("Prefabs/VRWriter/VRWriter"));
        GameObject avatar = (GameObject)Instantiate(Resources.Load("Prefabs/Avatar/Avatar"));
        avatar.name = "Avatar";
        DontDestroyOnLoad(avatar);
        avatar.AddComponent<AvatarController>();
        DontDestroyOnLoad(gameObject);
        Initialized = true;
    }

    public static TInterface GetInterface<TInterface>()
        where TInterface : Interface
    {
        return (TInterface)interfaceDict[typeof(TInterface)];
    }

    private void InitializeInterfaces()
    {
        GameObject serverInterfaceContainer = new GameObject("ServerInterfaceContainer");
        DontDestroyOnLoad(serverInterfaceContainer);
        IEnumerable<Type> interfaceTypes = StolperwegeHelper.GetTypesDerivingFrom<Interface>();
        Interfaces = new List<Interface>();
        interfaceDict = new Dictionary<Type, Interface>();
        
        foreach (var interfaceType in interfaceTypes)
        {
            var prefabInterface = interfaceType.GetCustomAttribute<PrefabInterfaceAttribute>();
            
            Interface initializedInterface;
            if (prefabInterface != null) //Check if Interface is a Prefab Interface
            {
                // Instantiate Interface-Prefab
                var prefab = Resources.Load<GameObject>(prefabInterface.PrefabPath);
                if (prefab == null)
                {
                    Debug.LogError($"Prefab of '{interfaceType.Name}' interface is not present at [{prefabInterface.PrefabPath}]!");
                    continue;
                }

                var gameObject = Instantiate(prefab);
                initializedInterface = gameObject.GetComponent<Interface>();
                if (initializedInterface == null)
                {
                    Debug.LogError($"Prefab of '{interfaceType.Name}' interface does not contain a Interface component!");
                    continue;
                }
            }
            else
            {
                // Add the interface-script to the container-gameobject
                initializedInterface = (Interface)serverInterfaceContainer.AddComponent(interfaceType);
            }
            
            Interfaces.Add(initializedInterface);
            interfaceDict.Add(interfaceType, initializedInterface);
            StartCoroutine(initializedInterface.Initialize());
        }
    }

    private void SetScenescript()
    {
        if (gameObject.GetComponent<SceneScript>() != null) Destroy(gameObject.GetComponent<SceneScript>());
        ActiveSceneScript = null;
        string sceneName = ActiveScene.name;
        IEnumerable<Type> sceneTypes = new List<Type>(StolperwegeHelper.GetTypesDerivingFrom<SceneScript>());
        foreach (Type type in sceneTypes)
        {
            if (type.FullName.Contains(sceneName))
            {
                ActiveSceneScript = (SceneScript)gameObject.AddComponent(type);
                ActiveSceneScript.Initialize();
            }
        }
    }

    public void LoadScene(string sceneName)
    {
        //if(StolperwegeHelper.netManagement.networkMenuScript != null) StolperwegeHelper.netManagement.networkMenuScript.IsActive = false;
        SceneManager.LoadScene(sceneName);
    }
	public void LoadScene(int sceneName)
    {
        //if(StolperwegeHelper.netManagement.networkMenuScript != null) StolperwegeHelper.netManagement.networkMenuScript.IsActive = false;
        SceneManager.LoadScene(sceneName);

    }

}

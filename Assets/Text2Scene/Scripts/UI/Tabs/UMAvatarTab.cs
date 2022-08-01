using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Valve.VR;
using UMA;
using UMA.CharacterSystem;

[PrefabInterface(PrefabPath + "UMAvatarTab")]
public class UMAvatarTab : BuilderTab
{
    private TextMeshPro ModelSlot;
    private MeshRenderer ModelThumbnail;
    private InteractiveButton ModelAdd;
    private TextMeshPro ModelAddDisplay;

    private readonly string PARENT_GAMEOBJECT_NAME = "AvatarModelEditorUtilities";
        
    public ShapeNetModel AvatarObject;
    /// <summary>
    /// Initializes the tab.
    /// </summary>
    public override void Initialize(SceneBuilder sceneBuilder)
    {
        base.Initialize(sceneBuilder);

        FindGameObjects();
        Name = "Avatar";
        ShowOnToolbar = true;

        ModelAddDisplay.text = "\U0000f067";

        ModelAdd.OnClick = () => {
            if (SceneBuilderSceneScript.WaitingForResponse) return;
            SceneBuilderSceneScript.WaitingForResponse = true;
            Vector3 spawn = new Vector3(ModelAdd.transform.position.x - 1f, 0.01f, ModelAdd.transform.position.z - 1f);
            CreateAvatar(spawn, Quaternion.identity, Vector3.one);
            ModelAdd.Active = false;
        };
    }

    /// <summary>
    /// Finds all the needed GameObjects and Components
    /// </summary>
    private void FindGameObjects()
    {
        try
        {
            ModelSlot = gameObject.transform.Find(PARENT_GAMEOBJECT_NAME + "/ModelSlot").GetComponent<TextMeshPro>();
            ModelThumbnail = gameObject.transform.Find(PARENT_GAMEOBJECT_NAME + "/ModelThumbnail").GetComponent<MeshRenderer>();
            ModelAdd = gameObject.transform.Find(PARENT_GAMEOBJECT_NAME + "/ModelAdd").GetComponent<InteractiveButton>();
            ModelAddDisplay = gameObject.transform.Find(PARENT_GAMEOBJECT_NAME + "/ModelAdd/Tag").GetComponent<TextMeshPro>();
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    private GameObject _objectInstance;
    private void CreateAvatar(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        _objectInstance = (GameObject) Instantiate(Resources.Load("Prefabs/Avatar/ShapeNetUMA"));
        StartCoroutine(_objectInstance.GetComponent<UMAISOObject>().Init(_objectInstance.GetComponent<UMAController>(), (root) =>
        {
            _objectInstance.GetComponent<InteractiveShapeNetObject>().Init(root, true, true);
            _objectInstance.layer = 19;
            _objectInstance.transform.position = root.Position.Vector;
            _objectInstance.transform.rotation = root.Rotation.Quaternion;
            _objectInstance.transform.localScale = root.Scale.Vector;
            _objectInstance.transform.SetParent(Builder.SceneBuilderControl.ObjectContainer.transform, true);
            if (AvatarObject != null && ((string)AvatarObject.ID).Equals(root.Object_ID))
            {
                AvatarObject = null;
                StolperwegeHelper.User.ActionBlocked = false;
            }
            SceneBuilderSceneScript.WaitingForResponse = false;
            ModelAdd.Active = true;
        }, pos, rot, scale));
    }

    public override void ResetTab()
    {
        // Do nothing
    }
}

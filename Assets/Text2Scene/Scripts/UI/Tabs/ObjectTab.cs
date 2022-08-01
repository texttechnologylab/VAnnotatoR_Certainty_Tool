using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;
using MathHelper;
using LitJson;

[PrefabInterface(PrefabPath + "ObjectTab")]
public class ObjectTab : BuilderTab
{

    public const string OBJECT_GROUP = "objectgroup";
    public const string ABSTRACT_VOLUME = "abstractvolume";
    public const string ABSTRACT_LINE = "abstractline";
    public const string ABSTRACT_AREA = "abstractarea";
    public const string ABSTRACT_POINT = "abstractpoint";
    public const string ABSTRACT_CYLINDER = "abstractcylinder";
    public static HashSet<string> ABSTRACT_TYPES = new HashSet<string>
    { OBJECT_GROUP , ABSTRACT_VOLUME, ABSTRACT_POINT,
      ABSTRACT_LINE, ABSTRACT_AREA, ABSTRACT_CYLINDER };

    // =========================== UI Elements ============================
    private TextMeshPro ModelSlot;
    private MeshRenderer ModelThumbnail;
    private TextMeshPro ModelLoading;
    private InteractiveButton PreviousBtn;
    private InteractiveButton NextBtn;
    private InteractiveButton ModelRemoveBtn;
    private InteractiveButton AnchorBtn;


    private InteractiveButton VolumeCreateBtn;
    private InteractiveButton AreaCreateBtn;
    private InteractiveButton LineCreateBtn;
    private InteractiveButton PointCreateBtn;

    private InteractiveCheckbox AbstractAutoCreation;
    private bool _checkAbstractAutoCreation;
    public bool CheckAbstractAutoCreation
    {
        get { return _checkAbstractAutoCreation; }
        set
        {
            _checkAbstractAutoCreation = value;
            AbstractAutoCreation.Status = (_checkAbstractAutoCreation) ? InteractiveCheckbox.CheckboxStatus.AllChecked : InteractiveCheckbox.CheckboxStatus.NoneChecked;
        }
    }

    private InteractiveCheckbox GrabLinkAutoCreation;
    private bool _checkGrabLinkAutoCreation;
    public bool CheckGrabLinkAutoCreation
    {
        get { return _checkGrabLinkAutoCreation; }
        set
        {
            _checkGrabLinkAutoCreation = value;
            GrabLinkAutoCreation.Status = (_checkGrabLinkAutoCreation) ? InteractiveCheckbox.CheckboxStatus.AllChecked : InteractiveCheckbox.CheckboxStatus.NoneChecked;
        }
    }

    private InteractiveCheckbox QSLinkAutoCreation;
    private bool _checkQSLinkAutoCreation;
    public bool CheckQSLinkAutoCreation
    {
        get { return _checkQSLinkAutoCreation; }
        set
        {
            _checkQSLinkAutoCreation = value;
            QSLinkAutoCreation.Status = (_checkQSLinkAutoCreation) ? InteractiveCheckbox.CheckboxStatus.AllChecked : InteractiveCheckbox.CheckboxStatus.NoneChecked;
            GrabLinkAutoCreation.Active = value || CheckOLinkAutoCreation;
        }
    }


    public string frameMode = null;
    public IsoEntity referencePt = null;
    private InteractiveButton OLinkAbsolutButton;
    private InteractiveButton OLinkIntrinsicButton;
    private InteractiveButton OLinkRelativeButton;
    private InteractiveButton OLinkUndefinedButton;
    private InteractiveButton OLinkUReferencePtButton;
    private InteractiveButton OLinkReferencePtDeleteButton;

    private InteractiveCheckbox OLinkAutoCreation;
    private bool _checkOLinkAutoCreation;
    public bool CheckOLinkAutoCreation
    {
        get { return _checkOLinkAutoCreation; }
        set
        {
            _checkOLinkAutoCreation = value;
            OLinkAutoCreation.Status = (_checkOLinkAutoCreation) ? InteractiveCheckbox.CheckboxStatus.AllChecked : InteractiveCheckbox.CheckboxStatus.NoneChecked;

            OLinkAbsolutButton.Active = _checkOLinkAutoCreation;
            OLinkIntrinsicButton.Active = _checkOLinkAutoCreation;
            OLinkRelativeButton.Active = _checkOLinkAutoCreation;
            OLinkUndefinedButton.Active = _checkOLinkAutoCreation;

            OLinkUReferencePtButton.Active = _checkOLinkAutoCreation && frameMode.Equals("");
            OLinkReferencePtDeleteButton.Active = _checkOLinkAutoCreation && frameMode.Equals("");

            GrabLinkAutoCreation.Active = value || CheckQSLinkAutoCreation;
        }
    }


    private InteractiveButton CreateEventPathButton;
    private InteractiveButton CreateEventPathCancelButton;
    private bool CancelEventPath = false;


    // ===================================== Properties
    private GameObject _ghostDoor;
    public GameObject GhostDoor
    {
        get
        {
            if (_ghostDoor == null)
            {
                _ghostDoor = (GameObject)Instantiate(Resources.Load("Text2Scene/Prefabs/Door"));
                Collider[] colliders = GhostDoor.GetComponentsInChildren<Collider>();
                for (int i = 0; i < colliders.Length; i++)
                    colliders[i].enabled = false;
                MakeGhostObject(_ghostDoor);
            }
            return _ghostDoor;
        }
    }

    private bool _doorEditorMode;
    public bool DoorEditorMode
    {
        get { return _doorEditorMode; }
        set
        {
            _doorEditorMode = value;
            //DoorEditor.ButtonOn = _doorEditorMode;
            if (_doorEditorMode) WindowEditorMode = false;

        }
    }

    private GameObject _ghostWindow;
    public GameObject GhostWindow
    {
        get
        {
            if (_ghostWindow == null)
            {
                _ghostWindow = (GameObject)Instantiate(Resources.Load("Text2Scene/Prefabs/Window"));
                _ghostWindow.GetComponent<Collider>().enabled = false;
                MakeGhostObject(_ghostWindow);
            }
            return _ghostWindow;
        }
    }


    private bool _windowEditorMode;
    public bool WindowEditorMode
    {
        get { return _windowEditorMode; }
        set
        {
            _windowEditorMode = value;
            //WindowEditor.ButtonOn = _windowEditorMode;
            if (_windowEditorMode) DoorEditorMode = false;

        }
    }

    private ShapeNetModel _shapeNetObject;
    private GameObject GhostObject;
    public IsoEntity QuickLinkEntity;
    public ShapeNetModel ShapeNetObject
    {
        get { return _shapeNetObject; }
        set
        {
            if (_shapeNetObject != null && value != null &&
                value.Equals(_shapeNetObject)) return;
            _shapeNetObject = value;
            if (_shapeNetObject != null)
            {
                DoorEditorMode = false;
                WindowEditorMode = false;
                if (GhostObject != null) GhostObject.SetActive(false);
                if (Builder.SceneBuilderControl.LoadedModels.ContainsKey((string)_shapeNetObject.ID))
                {
                    GhostObject = Builder.SceneBuilderControl.LoadedModels[(string)_shapeNetObject.ID][0];
                    OnModelLoaded();
                }
                else
                {
                    LoadModel((string)_shapeNetObject.ID, true);
                }

                StartCoroutine(FreeObjectPlacement((string)_shapeNetObject.ID));

                ModelThumbnail.material.SetTexture("_MainTex", _shapeNetObject.Thumbnail);
                ModelThumbnail.material.shader = Shader.Find("Unlit/Texture");
                ModelRemoveBtn.Active = true;
                AnchorBtn.Active = true;
                AnchorOnGround = true;
            }
            else
            {
                //StolperwegeHelper.RadialMenu.Show(false);
                if (GhostObject != null)
                {
                    GhostObject.SetActive(false);
                    GhostObject = null;
                }
                ModelThumbnail.material = StolperwegeHelper.ThumbnailMaterial;
                ModelRemoveBtn.Active = false;
                AnchorBtn.Active = false;
            }
            QuickLinkEntity = null;
        }
    }

    private Dictionary<string, int> _loadedObjectIDMap;
    private List<string> _loadedObjectIDQueue;
    private int _loadedObjectIDPointer = -1;
    public int LoadedObjectIDPointer
    {
        get { return _loadedObjectIDPointer; }
        set
        {
            _loadedObjectIDPointer = value;
            PreviousBtn.Active = _loadedObjectIDPointer > 0;
            NextBtn.Active = _loadedObjectIDPointer < _loadedObjectIDQueue.Count - 1;
            ShapeNetObject = SceneController.GetInterface<ShapeNetInterface>().ShapeNetModels[_loadedObjectIDQueue[_loadedObjectIDPointer]];
        }
    }

    private bool _anchorOnGround;
    public bool AnchorOnGround
    {
        get { return _anchorOnGround; }
        set
        {
            _anchorOnGround = value;
            AnchorBtn.ButtonOn = _anchorOnGround;
        }
    }



    public bool TriggerLocked { get; private set; }
    public bool ModelIsLoading { get; private set; }
    public bool ObjectPlacingMode { get; private set; }

    // ================================= Methods =============================================
    public override void Initialize(SceneBuilder builder)
    {
        base.Initialize(builder);

        Name = "Objects";

        ShowOnToolbar = true;
        _loadedObjectIDMap = new Dictionary<string, int>();
        _loadedObjectIDQueue = new List<string>();

        ModelSlot = transform.Find("ShapeNetObjectLoader/ObjectSlot").GetComponent<TextMeshPro>();
        ModelThumbnail = transform.Find("ShapeNetObjectLoader/Thumbnail").GetComponent<MeshRenderer>();
        ModelLoading = transform.Find("ShapeNetObjectLoader/ObjectLoading").GetComponent<TextMeshPro>();
        ModelLoading.text = "";
        ModelRemoveBtn = transform.Find("ShapeNetObjectLoader/RemoveButton").GetComponent<InteractiveButton>();
        ModelRemoveBtn.OnClick = () =>
        {
            ShapeNetObject = null;
            NextBtn.Active = false;
            PreviousBtn.Active = true;
            _loadedObjectIDPointer = _loadedObjectIDPointer + 1;
        };
        PreviousBtn = transform.Find("ShapeNetObjectLoader/PreviousButton").GetComponent<InteractiveButton>();
        PreviousBtn.OnClick = () => { LoadedObjectIDPointer -= 1; };
        PreviousBtn.Active = false;
        NextBtn = transform.Find("ShapeNetObjectLoader/NextButton").GetComponent<InteractiveButton>();
        NextBtn.OnClick = () => { LoadedObjectIDPointer += 1; };
        NextBtn.Active = false;
        AnchorBtn = transform.Find("ShapeNetObjectLoader/AnchorButton").GetComponent<InteractiveButton>();
        AnchorBtn.OnClick = () => { AnchorOnGround = !AnchorOnGround; };


        QSLinkAutoCreation = transform.Find("AutoLinkCreator/AutoQSLinks/Checkbox").GetComponent<InteractiveCheckbox>();
        QSLinkAutoCreation.Start();
        QSLinkAutoCreation.AllChecked = "On";
        QSLinkAutoCreation.NoneChecked = "Off";
        
        QSLinkAutoCreation.OnClick = () => { CheckQSLinkAutoCreation = !CheckQSLinkAutoCreation; };

        OLinkAutoCreation = transform.Find("AutoLinkCreator/AutoOLinks/Checkbox").GetComponent<InteractiveCheckbox>();
        OLinkAutoCreation.Start();
        OLinkAutoCreation.AllChecked = "On";
        OLinkAutoCreation.NoneChecked = "Off";
        OLinkAutoCreation.OnClick = () => { CheckOLinkAutoCreation = !CheckOLinkAutoCreation; };

        GrabLinkAutoCreation = transform.Find("AutoLinkCreator/AutoGrabLinks/Checkbox").GetComponent<InteractiveCheckbox>();
        GrabLinkAutoCreation.Start();
        GrabLinkAutoCreation.AllChecked = "On";
        GrabLinkAutoCreation.NoneChecked = "Off";
        GrabLinkAutoCreation.OnClick = () => { CheckGrabLinkAutoCreation = !CheckGrabLinkAutoCreation; };


        ////////////////////////////////////////////////

        OLinkAbsolutButton = transform.Find("AutoLinkCreator/OLinkFrameType/absolut").GetComponent<InteractiveButton>();
        OLinkIntrinsicButton = transform.Find("AutoLinkCreator/OLinkFrameType/intrinsic").GetComponent<InteractiveButton>();
        OLinkRelativeButton = transform.Find("AutoLinkCreator/OLinkFrameType/relative").GetComponent<InteractiveButton>();
        OLinkUndefinedButton = transform.Find("AutoLinkCreator/OLinkFrameType/undefined").GetComponent<InteractiveButton>();
        OLinkUReferencePtButton = transform.Find("AutoLinkCreator/ReferencePoint/Button").GetComponent<InteractiveButton>();
        OLinkReferencePtDeleteButton = transform.Find("AutoLinkCreator/ReferencePoint/Remove").GetComponent<InteractiveButton>();


        frameMode = "intrinsic";
        referencePt = null;

        CheckQSLinkAutoCreation = true;
        CheckOLinkAutoCreation = true;
        CheckGrabLinkAutoCreation = false;

        OLinkAbsolutButton.ButtonOn = false;
        OLinkIntrinsicButton.ButtonOn = true;
        OLinkRelativeButton.ButtonOn = false;
        OLinkUndefinedButton.ButtonOn = false;
        OLinkUReferencePtButton.ChangeText("[Ground]");
        OLinkUReferencePtButton.Active = false;
        OLinkReferencePtDeleteButton.Active = false;

        OLinkAbsolutButton.OnClick = () => {
            frameMode = "absolut";
            OLinkAbsolutButton.ButtonOn = true;
            OLinkIntrinsicButton.ButtonOn = false;
            OLinkRelativeButton.ButtonOn = false;
            OLinkUndefinedButton.ButtonOn = false;
            referencePt = null;
            OLinkUReferencePtButton.ChangeText("[Center]");
            OLinkUReferencePtButton.Active = false;
            OLinkReferencePtDeleteButton.Active = false;
        };

        OLinkIntrinsicButton.OnClick = () => { 
            frameMode = "intrinsic";
            OLinkAbsolutButton.ButtonOn = false;
            OLinkIntrinsicButton.ButtonOn = true;
            OLinkRelativeButton.ButtonOn = false;
            OLinkUndefinedButton.ButtonOn = false;
            referencePt = null;
            OLinkUReferencePtButton.ChangeText("[Ground]");
            OLinkUReferencePtButton.Active = false;
            OLinkReferencePtDeleteButton.Active = false;
        };

        OLinkRelativeButton.OnClick = () => {
            frameMode = "relative";
            OLinkAbsolutButton.ButtonOn = false;
            OLinkIntrinsicButton.ButtonOn = false;
            OLinkRelativeButton.ButtonOn = true;
            OLinkUndefinedButton.ButtonOn = false;
            OLinkUReferencePtButton.ChangeText(" - ");
            OLinkUReferencePtButton.Active = true;
            OLinkReferencePtDeleteButton.Active = true;
        };


        OLinkUndefinedButton.OnClick = () => {
            frameMode = "undefined";
            OLinkAbsolutButton.ButtonOn = false;
            OLinkIntrinsicButton.ButtonOn = false;
            OLinkRelativeButton.ButtonOn = false;
            OLinkUndefinedButton.ButtonOn = true;
            referencePt = null;
            OLinkUReferencePtButton.ChangeText("[User]");
            OLinkUReferencePtButton.Active = false;
            OLinkReferencePtDeleteButton.Active = false;
        };

        //OLinkUReferencePtButton.Active = false;
        OLinkUReferencePtButton.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(OLinkUReferencePtButton));
        };

        //OLinkReferencePtDeleteButton.Active = false;
        OLinkReferencePtDeleteButton.OnClick = () =>
        {
            referencePt = null;
            OLinkUReferencePtButton.ChangeText(" - ");
        };


        ///////////////////////////////////////////
        CreateEventPathButton = transform.Find("CreateEventPath/CreateEventPath/Button").GetComponent<InteractiveButton>();
        CreateEventPathButton.OnClick = () => {
            Debug.Log("Create Event Path Registered");

            StartCoroutine(CreateEventPath()); 
        };
        CreateEventPathButton.Active = true;
        CreateEventPathButton.ChangeText("Create");

        CreateEventPathCancelButton = transform.Find("CreateEventPath/CreateEventPath/Remove").GetComponent<InteractiveButton>();
        CreateEventPathCancelButton.Active = false;
        CreateEventPathCancelButton.OnClick = () => { CancelEventPath = true; };

        ///////////////////////////////////////////////////////
        AbstractAutoCreation = transform.Find("AbstractObjectCreator/AbstractObjectAutoCreation/Checkbox").GetComponent<InteractiveCheckbox>();
        AbstractAutoCreation.Start();
        AbstractAutoCreation.AllChecked = "On";
        AbstractAutoCreation.NoneChecked = "Off";
        CheckAbstractAutoCreation = true;
        AbstractAutoCreation.OnClick = () => { CheckAbstractAutoCreation = !CheckAbstractAutoCreation; };


        VolumeCreateBtn = transform.Find("AbstractObjectCreator/CreateAbstractVolumeButton").GetComponent<InteractiveButton>();
        VolumeCreateBtn.Start();
        VolumeCreateBtn.ButtonValue = "AddVolumeObject";
        VolumeCreateBtn.OnClick = () =>
        {
            if (SceneBuilderSceneScript.WaitingForResponse) return;
            SceneBuilderSceneScript.WaitingForResponse = true;
            Builder.TextAnnotatorInterface.OnElemCreated = (entity) =>
            {
                SceneBuilderSceneScript.WaitingForResponse = false;
                CreateObject((IsoSpatialEntity)entity);
                VolumeCreateBtn.Active = true;
            };
            Builder.TextAnnotatorInterface.SendSpatialEnityCreatingRequest("abstractvolume", VolumeCreateBtn.transform.position + transform.forward * 0.2f, Quaternion.identity, Vector3.one);
            VolumeCreateBtn.Active = false;
        };

        AreaCreateBtn = transform.Find("AbstractObjectCreator/CreateAbstractAreaButton").GetComponent<InteractiveButton>();
        AreaCreateBtn.Start();
        AreaCreateBtn.ButtonValue = "AddAreaObject";
        AreaCreateBtn.OnClick = () =>
        {
            if (SceneBuilderSceneScript.WaitingForResponse) return;
            SceneBuilderSceneScript.WaitingForResponse = true;
            Builder.TextAnnotatorInterface.OnElemCreated = (entity) =>
            {
                SceneBuilderSceneScript.WaitingForResponse = false;
                CreateObject((IsoSpatialEntity)entity);
                AreaCreateBtn.Active = true;
            };

            Builder.TextAnnotatorInterface.SendSpatialEnityCreatingRequest("abstractarea", AreaCreateBtn.transform.position + transform.forward * 0.2f, Quaternion.identity, Vector3.one);
            AreaCreateBtn.Active = false;
        };

        LineCreateBtn = transform.Find("AbstractObjectCreator/CreateAbstractLineButton").GetComponent<InteractiveButton>();
        LineCreateBtn.Start();
        LineCreateBtn.ButtonValue = "AddLineObject";
        LineCreateBtn.OnClick = () =>
        {
            if (SceneBuilderSceneScript.WaitingForResponse) return;
            SceneBuilderSceneScript.WaitingForResponse = true;
            Builder.TextAnnotatorInterface.OnElemCreated = (entity) =>
            {
                SceneBuilderSceneScript.WaitingForResponse = false;
                CreateObject((IsoSpatialEntity)entity);
                LineCreateBtn.Active = true;
            };
            Builder.TextAnnotatorInterface.SendSpatialEnityCreatingRequest("abstractline", LineCreateBtn.transform.position + transform.forward * 0.2f, Quaternion.identity, Vector3.one);
            LineCreateBtn.Active = false;
        };

        PointCreateBtn = transform.Find("AbstractObjectCreator/CreateAbstractPointButton").GetComponent<InteractiveButton>();
        PointCreateBtn.Start();
        PointCreateBtn.ButtonValue = "AddPointObject";
        PointCreateBtn.OnClick = () =>
        {
            if (SceneBuilderSceneScript.WaitingForResponse) return;
            SceneBuilderSceneScript.WaitingForResponse = true;
            Builder.TextAnnotatorInterface.OnElemCreated = (entity) =>
            {
                SceneBuilderSceneScript.WaitingForResponse = false;
                CreateObject((IsoSpatialEntity)entity);
                PointCreateBtn.Active = true;
            };
            Builder.TextAnnotatorInterface.SendSpatialEnityCreatingRequest(ObjectTab.ABSTRACT_POINT, PointCreateBtn.transform.position + transform.forward * 0.2f, Quaternion.identity, Vector3.one);
            PointCreateBtn.Active = false;
        };
    }

    private IEnumerator CreateEventPath()
    {
        if (SceneBuilderSceneScript.WaitingForResponse) yield break;
        SceneBuilderSceneScript.WaitingForResponse = true;
        Builder.TextAnnotatorInterface.OnElemCreated = (entity) =>
        {
            SceneBuilderSceneScript.WaitingForResponse = false;
            CreateObject((IsoEventPath)entity);
            CreateEventPathButton.Active = false;
        };


        Debug.Log("CreateEventPath()");
        StolperwegeHelper.BlockInteractiveObjClick = true;
        CreateEventPathButton.ChangeText("Start");
        CreateEventPathCancelButton.Active = true;




        IsoEntity start = null;
        List<IsoEntity> mids = new List<IsoEntity>();
        IsoEntity end = null;
        int mode = 0; //Where to save the IsoEntities
        Debug.Log("Create Event Path");
        while (!CancelEventPath)
        {
           
            _hit = StolperwegeHelper.User.PointerHand.IsPointing ? StolperwegeHelper.User.PointerHand.Hit : null;
            if (SteamVR_Actions.default_trigger.GetStateDown(StolperwegeHelper.User.PointerHandType))
            {
                Debug.Log("Click registered");
                if (_hit == null)
                {
                    mode++;
                    if(mode == 1)
                        CreateEventPathButton.ChangeText("Mids");
                    else if (mode == 2)
                        CreateEventPathButton.ChangeText("End");
                }

                if (_hit is TokenObject || _hit is InteractiveShapeNetObject)
                {
                    IsoEntity ent = _hit is TokenObject ? ((TokenObject)_hit).Entity : ((InteractiveShapeNetObject)_hit).Entity;

                    if (mode == 0)
                    {
                        start = ent;
                        mode++;
                        CreateEventPathButton.ChangeText("Mids");
                    }
                    else if(mode == 1)
                    {
                        mids.Add(ent);
                    }
                    else if (mode == 2)
                    {
                        end = ent;
                        mode++;
                    }
                }

                if(mode == 3)
                {
                    Vector3 pos = StolperwegeHelper.CenterEyeAnchor.transform.position + StolperwegeHelper.CenterEyeAnchor.transform.forward * 0.4f; ;
                    Vector3 scale = Vector3.one;
                    Quaternion rot = Quaternion.identity;
                    Builder.TextAnnotatorInterface.SendEventPathCreatingRequest(ObjectTab.ABSTRACT_VOLUME, pos, rot, scale, beginID: start, endID: end, mids: mids);
                    break;
                }
            }

            yield return null;

        }

        Debug.Log("Create Event Path Finished");
        StolperwegeHelper.BlockInteractiveObjClick = false;
        CreateEventPathButton.ChangeText("Create");
        CreateEventPathCancelButton.Active = false;
        CreateEventPathButton.Active = false;
        yield break;
    }


    private void Update()
    {
        CheckHands();

    }

    GameObject _objectInstance;
    public InteractiveShapeNetObject CreateObject(IsoEntity spatialEntity)
    {
        Debug.Log("Create Object: " + spatialEntity.ID);
        if (spatialEntity.Object_ID == null)
        {
            Debug.LogWarning("Entity: " + spatialEntity.ID + " doeas not has an Object_ID!");
            return null;
        }

        Color basecolor = GetColorToEntity(spatialEntity);
        if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_VOLUME))
        {
            _objectInstance = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractCube")));
            _objectInstance.SetActive(true);
            _objectInstance.AddComponent<InteractiveShapeNetObject>().Init(spatialEntity, true, false);
            //_objectInstance.GetComponent<InteractiveShapeNetObject>().SetObjColor(basecolor);
        }
        else if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_AREA))
        {
            _objectInstance = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractArea")));
            _objectInstance.SetActive(true);
            _objectInstance.AddComponent<InteractiveShapeNetObject>().Init(spatialEntity, true, false);
            //_objectInstance.GetComponent<InteractiveShapeNetObject>().SetObjColor(basecolor);
        }
        else if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_LINE))
        {
            _objectInstance = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractLine")));
            _objectInstance.SetActive(true);
            _objectInstance.AddComponent<InteractiveShapeNetObject>().Init(spatialEntity, true, false);
            //_objectInstance.GetComponent<InteractiveShapeNetObject>().SetObjColor(basecolor);
        }
        else if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_POINT))
        {
            _objectInstance = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractPoint")));
            _objectInstance.SetActive(true);
            _objectInstance.AddComponent<InteractiveShapeNetObject>().Init(spatialEntity, true, false);
            //_objectInstance.GetComponent<InteractiveShapeNetObject>().SetObjColor(basecolor);
        }
        else if (spatialEntity.Object_ID.Equals(ObjectTab.ABSTRACT_CYLINDER))
        {
            _objectInstance = (GameObject)(Instantiate(Resources.Load("Prefabs/AbstractObjects/AbstractCylinder")));
            _objectInstance.SetActive(true);
            _objectInstance.AddComponent<InteractiveShapeNetObject>().Init(spatialEntity, true, false);
            //_objectInstance.GetComponent<InteractiveShapeNetObject>().SetObjColor(basecolor);
        }
        else if (UMAISOEntity.AVATAR_TYPE_NAMES.Contains(spatialEntity.Object_ID))
        {
            //TODO -.-
        }
        else
        {
            _objectInstance = Instantiate(Builder.SceneBuilderControl.LoadedModels["" + spatialEntity.Object_ID][1]);
            _objectInstance.SetActive(true);
            _objectInstance.AddComponent<InteractiveShapeNetObject>().Init(spatialEntity, true, true);
            //_objectInstance.GetComponent<InteractiveShapeNetObject>().SetObjColor(basecolor);
        }

        _objectInstance.layer = 19;
        _objectInstance.transform.position = spatialEntity.Position.Vector;
        _objectInstance.transform.rotation = spatialEntity.Rotation.Quaternion;
        _objectInstance.transform.localScale = spatialEntity.Scale.Vector;
        _objectInstance.transform.SetParent(Builder.SceneBuilderControl.ObjectContainer.transform, true);

        if (ShapeNetObject != null && ((string)ShapeNetObject.ID).Equals(spatialEntity.Object_ID))
        {
            ShapeNetObject = null;
            StolperwegeHelper.User.ActionBlocked = false;
        }

        return _objectInstance.GetComponent<InteractiveShapeNetObject>();
    }

    public static void MakeGhostObject(GameObject go)
    {
        MeshRenderer[] _renderers = go.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i].GetComponent<TextMeshPro>() != null ||
                _renderers[i].GetComponent<TMP_SubMesh>() != null ||
                !_renderers[i].material.HasProperty("_BaseColor")) continue;
            Color _color = _renderers[i].material.GetColor("_BaseColor");
            _color.a = 0.6f;
            _renderers[i].material.SetColor("_BaseColor", _color);
        }
    }


    private Color GetColorToEntity(IsoEntity entity)
    {
        if (entity.GetType() == typeof(IsoEventPath))
            return IsoEventPath.ClassColor;
        else if (entity.GetType() == typeof(IsoLocationPath))
            return IsoLocationPath.ClassColor;
        else if (entity.GetType() == typeof(IsoLocationPlace))
            return IsoLocationPlace.ClassColor;
        else if (entity.GetType() == typeof(IsoLocation))
            return IsoLocation.ClassColor;
        else if (entity.GetType() == typeof(IsoSpatialEntity))
            return IsoSpatialEntity.ClassColor;
        else if (entity.GetType() == typeof(IsoMotion))
            return IsoMotion.ClassColor;
        else if (entity.GetType() == typeof(IsoNonMotionEvent))
            return IsoNonMotionEvent.ClassColor;
        else if (entity.GetType() == typeof(IsoSRelation))
            return IsoSRelation.ClassColor;
        else if (entity.GetType() == typeof(IsoMeasure))
            return IsoMeasure.ClassColor;
        else if (entity.GetType() == typeof(IsoMRelation))
            return IsoMRelation.ClassColor;
        else
            return Color.black;
    }

    public void LoadModel(string id, bool ghostActive)
    {
        ModelIsLoading = true;
        GhostObject = null;
        try
        {
            _shapeNetObject = SceneController.GetInterface<ShapeNetInterface>().ShapeNetModels[id];
        }
        catch (System.Exception e)
        {
            Debug.Log(id);
            throw e;
        }
        StartCoroutine(SceneController.GetInterface<ShapeNetInterface>().GetModel((string)ShapeNetObject.ID, (path) =>
        {
            GameObject _object = ObjectLoader.LoadObject(path + "\\" + _shapeNetObject.ID + ".obj", path + "\\" + _shapeNetObject.ID + ".mtl");
            GhostObject = ObjectLoader.Reorientate_Obj(_object, _shapeNetObject.Up, _shapeNetObject.Front, _shapeNetObject.Unit);
            GhostObject.transform.parent = Builder.SceneBuilderControl.ObjectContainer.transform;

            BoxCollider _collider = GhostObject.AddComponent<BoxCollider>();
            _collider.size = ShapeNetObject.AlignedDimensions / 100;
            //_collider.center = Vector3.up * _collider.size.y / 2;

            LineRenderer lines = GhostObject.AddComponent<LineRenderer>();
            lines.enabled = false;

            GameObject colliderDisplay = (GameObject)(Instantiate(Resources.Load("Prefabs/Frames/FrameCube")));
            colliderDisplay.transform.parent = GhostObject.transform;
            colliderDisplay.transform.localScale = _collider.size + new Vector3(0.001f, 0.001f, 0.001f); //Prevents Flickering
            colliderDisplay.transform.position = _collider.center;// + new Vector3(0f, 0.001f, 0f);
            colliderDisplay.SetActive(true);
            colliderDisplay.name = "FrameCube";

            Builder.SceneBuilderControl.LoadedModels.Add((string)ShapeNetObject.ID, new GameObject[2]);
            Builder.SceneBuilderControl.LoadedModels[(string)ShapeNetObject.ID][0] = GhostObject;
            Builder.SceneBuilderControl.LoadedModels[(string)ShapeNetObject.ID][1] = Instantiate(GhostObject);
            Builder.SceneBuilderControl.LoadedModels[(string)ShapeNetObject.ID][1].SetActive(false);
            MakeGhostObject(GhostObject);
            GhostObject.SetActive(ghostActive);
            _collider.enabled = false;
            OnModelLoaded();
        }));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!TriggerLocked && other.GetComponent<DataBrowserResource>() != null &&
            other.GetComponent<DataBrowserResource>().Data != null)
        {
            VRData data = other.GetComponent<DataBrowserResource>().Data;
            if (data is ShapeNetModel) AddLoadedObject((string)data.ID);
        }
    }

    public void AddLoadedObject(string id)
    {
        if (!_loadedObjectIDMap.ContainsKey(id))
        {
            TriggerLocked = true;
            StartCoroutine(Builder.LoadingAnimation(ModelLoading));
            _loadedObjectIDMap.Add(id, _loadedObjectIDMap.Count);
            _loadedObjectIDQueue.Add(id);
            LoadedObjectIDPointer = _loadedObjectIDMap.Count - 1;
        }
        else
        {
            LoadedObjectIDPointer = _loadedObjectIDMap[id];
        }
    }

    private void OnModelLoaded()
    {
        ModelLoading.text = "";
        TriggerLocked = false;
        ModelIsLoading = false;
        Builder.InterruptLoadingAnimation = true;
    }

    protected override void CheckHands()
    {
        base.CheckHands();

        if (ObjectInLeftHand != null && (ObjectInLeftHand.Data is ShapeNetModel))
        {
            localPos = Builder.transform.InverseTransformPoint(ObjectInLeftHand.transform.position);
            Builder.Effect.SetVector3("Start", localPos);
            Builder.Effect.SetVector3("Target", ModelSlot.transform.localPosition);
            Builder.Effect.SetFloat("StartRadius", ObjectInLeftHand.GetComponent<BoxCollider>().bounds.size.x);
            Builder.Effect.SetFloat("TargetRadius", ModelSlot.rectTransform.rect.width);
            if (!Builder.EffectActive) Builder.SetEffectStatus(true);
        }
        else if (ObjectInRightHand != null && (ObjectInRightHand.Data is ShapeNetModel))
        {
            localPos = Builder.transform.InverseTransformPoint(ObjectInRightHand.transform.position);
            Builder.Effect.SetVector3("Start", localPos);
            Builder.Effect.SetVector3("Target", ModelSlot.transform.localPosition);
            Builder.Effect.SetFloat("StartRadius", ObjectInRightHand.GetComponent<BoxCollider>().bounds.size.x);
            Builder.Effect.SetFloat("TargetRadius", ModelSlot.rectTransform.rect.width);
            if (!Builder.EffectActive) Builder.SetEffectStatus(true);
        }
        else if (Builder.EffectActive) Builder.SetEffectStatus(false);
    }

    public override void ResetTab()
    {
        DoorEditorMode = false;
        WindowEditorMode = false;
        ShapeNetObject = null;
    }

    protected override void UpdateTab()
    {
        base.UpdateTab();

    }

    private enum PlaceMode { Distance, Scale, Rotation }
    private float x_buffer = 0.5f;  //When to react on rotation change
    private IEnumerator FreeObjectPlacement(string obj_id)
    {
        while (GhostObject == null)
        {
            yield return null;
        }

        ObjectPlacingMode = true;
        PlaceMode currentMode = PlaceMode.Distance;
        StolperwegeHelper.User.TeleportBlocked = true;
        bool blockrotationchange = false;


        //CircleRenderer for Rotation Visualisation
        LineRenderer rotationLine = GhostObject.GetComponent<LineRenderer>();
        if (rotationLine == null)
            rotationLine = GhostObject.AddComponent<LineRenderer>();
        rotationLine.enabled = false;

        //For AutoOlinks

        AutoSpatialLinkHelper olinkhelper = GhostObject.AddComponent<AutoSpatialLinkHelper>();

        //AutoOLinkHelper garanties BoxCollider
        BoxCollider collider = GhostObject.GetComponent<BoxCollider>();

        //Object to Pointer Position
        GhostObject.transform.position = StolperwegeHelper.User.PointerHand.transform.position; // - new Vector3(0, collider.bounds.size.y * 0.5f, 0);
        GhostObject.transform.position -= StolperwegeHelper.PointerPath.transform.up * 5; //Distance

        //Rotate Object to User
        Vector3 lookDir = StolperwegeHelper.User.CenterEyeAnchor.transform.position;
        lookDir.y = GhostObject.transform.position.y;
        GhostObject.transform.LookAt(lookDir);

        //Update Radialmenu für SpatialEntities
        StolperwegeHelper.RadialMenu.UpdateSection(RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.SpatialEntity], 0);




        float distanceUpdate = 0f;  //DistanceChange to User
        Vector3 newPosition;
        int rot_axis = 0;  //Rotation Modus

        //Do until Trigger Down
        while (!SteamVR_Actions.default_trigger.GetStateDown(StolperwegeHelper.User.PointerHandType))
        {
            if (GhostObject == null) break;

            //As Long User is Pointing -> Change Location, Scale and Rotation
            if (StolperwegeHelper.User.PointerHand.IsPointing)
            {
                StolperwegeHelper.RadialMenu.Show(false);

                //Mode Selection for Moving, Scaling, Rotation
                if (SteamVR_Actions.default_click.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    if (currentMode == PlaceMode.Distance)
                    {
                        //Scaling
                        StolperwegeHelper.User.RotationBlocked = true;
                        currentMode = PlaceMode.Scale;
                        rotationLine.enabled = false;
                        PointFinger.SetPointerEmissionColor(StolperwegeHelper.GUCOLOR.SONNENGELB);
                        PointFinger.SetPointerColor(StolperwegeHelper.GUCOLOR.SENFGELB);
                    }
                    else if (currentMode == PlaceMode.Scale)
                    {
                        //Rotating
                        StolperwegeHelper.User.RotationBlocked = true;
                        rot_axis = 0;
                        currentMode = PlaceMode.Rotation;
                        LineRendererHelper.LineToCircle(GhostObject, rot_axis);
                        rotationLine.enabled = true;
                        PointFinger.SetPointerEmissionColor(StolperwegeHelper.GUCOLOR.ORANGE);
                        PointFinger.SetPointerColor(StolperwegeHelper.GUCOLOR.EMOROT);
                    }
                    else if (currentMode == PlaceMode.Rotation)
                    {
                        //Moving
                        StolperwegeHelper.User.RotationBlocked = false;
                        currentMode = PlaceMode.Distance;
                        rotationLine.enabled = false;
                        PointFinger.SetPointerEmissionColor(StolperwegeHelper.GUCOLOR.GOETHEBLAU);
                        PointFinger.SetPointerColor(Color.black);
                    }
                }

                //Update Object Position to Pointermovements
                newPosition = StolperwegeHelper.User.PointerHand.transform.position; // - new Vector3(0, collider.bounds.size.y * 0.5f, 0);

                //Touchposition on Touchpad/Thumbstick
                float touchInput = SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).y;

                if (currentMode == PlaceMode.Distance)
                {
                    if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x < x_buffer && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x > -x_buffer)
                    {
                        //Update Distance to User
                        distanceUpdate += touchInput;
                    }
                }
                else if (currentMode == PlaceMode.Scale)
                {
                    if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x < x_buffer && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x > -x_buffer)
                    {
                        //Update Scale between 0.05f and 10f
                        Vector3 newScale = GhostObject.transform.localScale + new Vector3(touchInput, touchInput, touchInput) / 10;
                        GhostObject.transform.localScale = Vector3.Min(new Vector3(10f, 10f, 10f), Vector3.Max(newScale, new Vector3(0.05f, 0.05f, 0.05f)));
                    }
                }
                else if (currentMode == PlaceMode.Rotation)
                {
                    //Only move once, when moveing trigger on x-Achsis
                    if (!blockrotationchange)
                    {

                        //Change Rotaton Axis
                        if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x > 0.5)
                        {
                            rot_axis = (rot_axis + 1) % 3;
                            blockrotationchange = true;
                            LineRendererHelper.LineToCircle(GhostObject, rot_axis);
                        }
                        else if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x < -0.5)
                        {
                            rot_axis = Mathf.Abs((rot_axis - 1) % 3);
                            blockrotationchange = true;
                            LineRendererHelper.LineToCircle(GhostObject, rot_axis);
                        }
                    }
                    else
                    {
                        //Unblock Rotationchange
                        if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x > -0.1 & SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x < 0.1)
                        {
                            blockrotationchange = false;
                        }
                    }


                    if (rot_axis == 0)
                    {
                        //Rotate around y-Achsis
                        if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x < x_buffer && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x > -x_buffer)
                        {
                            GhostObject.transform.Rotate(Vector3.up * touchInput);
                        }
                    }
                    if (rot_axis == 1)
                    {
                        if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x < x_buffer && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x > -x_buffer)
                        {
                            GhostObject.transform.Rotate(Vector3.right * touchInput);
                        }
                    }
                    if (rot_axis == 2)
                    {
                        if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x < x_buffer && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x > -x_buffer)
                        {
                            GhostObject.transform.Rotate(Vector3.forward * touchInput);
                        }
                    }

                }

                //Update ObjectPosition to Pointer
                newPosition -= StolperwegeHelper.PointerPath.transform.up * Mathf.Min(20, Mathf.Max(5 + distanceUpdate / 6, 0.1f));

                GhostObject.transform.position = newPosition;
                GhostObject.SetActive(true);
            }
            else
            {
                //Show RadialMenu und deatctive object when not pointing. (e.g. air grabbing)
                StolperwegeHelper.RadialMenu.Show(true);
                GhostObject.SetActive(false);
            }
            yield return null;
            continue;
        }

        //When Trigger down -> Reset alle Functions
        PointFinger.SetPointerEmissionColor(StolperwegeHelper.GUCOLOR.GOETHEBLAU);
        PointFinger.SetPointerColor(Color.black);
        StolperwegeHelper.User.TeleportBlocked = false;
        StolperwegeHelper.User.RotationBlocked = false;
        rotationLine.enabled = false;
        ObjectPlacingMode = false;

        olinkhelper.ActivateAllConnectedColliderFrames(false);

        InteractiveShapeNetObject figureobj = null;

        SceneBuilderSceneScript.WaitingForResponse = true;

        if (QuickLinkEntity == null)
        {
            Builder.TextAnnotatorInterface.OnElemCreated = (entity) =>
            {
                figureobj = CreateObject((IsoSpatialEntity)entity);
                SceneBuilderSceneScript.WaitingForResponse = false;
            };
            if (StolperwegeHelper.RadialMenu.GetSelectedSection() == null)
                Builder.TextAnnotatorInterface.SendSpatialEnityCreatingRequest(obj_id, GhostObject.transform.position, GhostObject.transform.rotation, GhostObject.transform.localScale);
            else if (StolperwegeHelper.RadialMenu.GetSelectedSection().Value.Equals(AnnotationTypes.SPATIAL_ENTITY))
                Builder.TextAnnotatorInterface.SendSpatialEnityCreatingRequest(obj_id, GhostObject.transform.position, GhostObject.transform.rotation, GhostObject.transform.localScale);
            else if (StolperwegeHelper.RadialMenu.GetSelectedSection().Value.Equals(AnnotationTypes.PATH))
                Builder.TextAnnotatorInterface.SendLocationPathCreatingRequest(obj_id, GhostObject.transform.position, GhostObject.transform.rotation, GhostObject.transform.localScale);
            else if (StolperwegeHelper.RadialMenu.GetSelectedSection().Value.Equals(AnnotationTypes.PLACE))
                Builder.TextAnnotatorInterface.SendLocationPlaceCreatingRequest(obj_id, GhostObject.transform.position, GhostObject.transform.rotation, GhostObject.transform.localScale);
            else if (StolperwegeHelper.RadialMenu.GetSelectedSection().Value.Equals(AnnotationTypes.EVENT_PATH))
                Builder.TextAnnotatorInterface.SendEventPathCreatingRequest(obj_id, GhostObject.transform.position, GhostObject.transform.rotation, GhostObject.transform.localScale);
        }
        else
        {
            Builder.TextAnnotatorInterface.ChangeEventMap.Add((int)QuickLinkEntity.ID, (update) =>
            {
                IsoEntity entity = (IsoEntity)update;
                CreateObject(entity);
                SceneBuilderSceneScript.WaitingForResponse = false;
            });
            Dictionary<string, object> updateMap = new Dictionary<string, object>();
            updateMap.Add("object_id", obj_id);

            #region position
            JsonData position = new JsonData();
            position["_type"] = AnnotationTypes.VEC3;
            JsonData posMapJSON = new JsonData();
            posMapJSON["x"] = GhostObject.transform.position.x;
            posMapJSON["y"] = GhostObject.transform.position.y;
            posMapJSON["z"] = GhostObject.transform.position.z;
            position["features"] = posMapJSON;
            updateMap.Add("position", position);
            #endregion

            #region rotation
            JsonData rotation = new JsonData();
            rotation["_type"] = AnnotationTypes.VEC4;
            JsonData rotMapJSON = new JsonData();
            rotMapJSON["x"] = GhostObject.transform.rotation.x;
            rotMapJSON["y"] = GhostObject.transform.rotation.y;
            rotMapJSON["z"] = GhostObject.transform.rotation.z;
            rotMapJSON["w"] = GhostObject.transform.rotation.w;
            rotation["features"] = rotMapJSON;
            updateMap.Add("rotation", rotation);
            #endregion

            #region scale
            JsonData scaleVector = new JsonData();
            scaleVector["_type"] = AnnotationTypes.VEC3;
            JsonData scaleMapJSON = new JsonData();
            scaleMapJSON["x"] = GhostObject.transform.localScale.x;
            scaleMapJSON["y"] = GhostObject.transform.localScale.y;
            scaleMapJSON["z"] = GhostObject.transform.localScale.z;
            scaleVector["features"] = scaleMapJSON;
            updateMap.Add("scale", scaleVector);
            #endregion
            Builder.TextAnnotatorInterface.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + QuickLinkEntity.ID, updateMap } }, null);
        }        
        
        HashSet<InteractiveShapeNetObject> all_qs_links = olinkhelper.GetAllQSConnectedObjects();
        Dictionary<InteractiveShapeNetObject, string> all_o_links = olinkhelper.GetAllOConnectedObjects();

        Destroy(olinkhelper);

        if (CheckQSLinkAutoCreation)
        {
            foreach (InteractiveShapeNetObject iso in all_qs_links)
            {
                while (SceneBuilderSceneScript.WaitingForResponse)
                    yield return null;

                foreach (IsoLink link in figureobj.Entity.LinkedVia.Keys)
                    if (link.Figure == iso.Entity || link.Ground == iso.Entity)
                        continue;

                foreach (IsoEntity ent in iso.Entity.AllRelatedQSLinkFigures())
                    if (ent.InteractiveShapeNetObject != null && all_qs_links.Contains(ent.InteractiveShapeNetObject))
                        continue;

                String rcc8type = Rcc8Test.Rcc8(gameObject, iso.gameObject);
                if (rcc8type == "TPPc")
                    iso.Entity.SendLinkRequest(AnnotationTypes.QSLINK, figureobj.Entity, "TTP", "auto generated");
                else if (rcc8type == "NTTPc")
                    iso.Entity.SendLinkRequest(AnnotationTypes.QSLINK, figureobj.Entity, "IN", "auto generated");
                else
                    figureobj.Entity.SendLinkRequest(AnnotationTypes.QSLINK, iso.Entity, rcc8type, "auto generated");
            }
        }

        if (CheckOLinkAutoCreation)
        {
            foreach (InteractiveShapeNetObject iso in all_o_links.Keys)
            {
                while (SceneBuilderSceneScript.WaitingForResponse)
                    yield return null;

                String relType = all_o_links[iso];
                foreach (IsoLink link in figureobj.Entity.LinkedVia.Keys)
                    if ((link.Figure == iso.Entity || link.Ground == iso.Entity) && link.Rel_Type.Equals(relType))
                        continue;

                
                figureobj.Entity.SendOLinkRequest(iso.Entity, frameMode, referencePt, relType, "auto generated");
            }
        }
        Debug.Log("::::::::::::::::::::::::::::::::::");
        //yield break;
    }

    private LineRenderer Arrow;
    private GameObject Cone;
    InteractiveObject _hit; 
    bool _match;
    Vector3[] _points; Vector3 _start, _end, _middle; Color _color;
    private IEnumerator ActualizeConnector(InteractiveButton button)
    {
        if (Arrow == null)
        {
            Arrow = StolperwegeHelper.CenterEyeAnchor.gameObject.GetComponent<LineRenderer>();
            if (Arrow == null)
                Arrow = StolperwegeHelper.CenterEyeAnchor.gameObject.AddComponent<LineRenderer>();
        }
        Arrow.positionCount = 9;
        _points = new Vector3[9];
        Arrow.SetPositions(_points);
        Arrow.material = (Material)Instantiate(Resources.Load("Materials/UI/Pointer"));
        Arrow.widthMultiplier = 0.005f;
        Arrow.useWorldSpace = true;
        Arrow.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Arrow.receiveShadows = false;
        Cone = (GameObject)(Instantiate(Resources.Load("Prefabs/UI/Cone")));
        Cone.GetComponent<MeshRenderer>().material = (Material)Instantiate(Resources.Load("Materials/UI/Pointer"));
        Cone.transform.localScale *= 2;

        while (!SteamVR_Actions.default_trigger.GetStateUp(StolperwegeHelper.User.PointerHandType))
        {
            // get the hit
            _hit = StolperwegeHelper.User.PointerHand.IsPointing ? StolperwegeHelper.User.PointerHand.Hit : null;
            Cone.SetActive(_hit != null);
            Arrow.enabled = _hit != null;
            if (_hit == null)
            {
                yield return null;
                continue;
            }

            // check matches
            _match = false;

            if (_hit != null)
            {
                if (_hit is TokenObject)
                {
                    if (((TokenObject)_hit).HasEntity)
                        _match = true;
                }

                if (_hit is InteractiveShapeNetObject)
                {
                    _match = true;
                }
            }

            // actualize arrow
            _start = button.transform.position;

            _end = _match ? _hit.transform.position : StolperwegeHelper.User.PointerHand.RaySphere.transform.position;
            _middle = (_start + _end) / 2;
            _middle = transform.InverseTransformPoint(_middle);
            _middle.z = 0.5f;
            _middle = transform.TransformPoint(_middle);
            _color = _match ? Color.green : Color.red;
            Arrow.material.SetColor("_Color", _color);
            Arrow.material.SetColor("_EmissionColor", _color * 5);
            _points = BezierCurve.CalculateCurvePoints(_start, _middle, _end, _points);
            Cone.GetComponent<Renderer>().material.SetColor("_Color", _color);
            Cone.GetComponent<Renderer>().material.SetColor("_EmissionColor", _color * 5);
            Cone.transform.forward = _points[_points.Length - 1] - _points[_points.Length - 2];
            Cone.transform.position = _points[_points.Length - 1] - Cone.transform.forward * Cone.GetComponent<Renderer>().bounds.size.z / 2;
            Cone.transform.forward = _points[_points.Length - 1] - _points[_points.Length - 2];
            _points[_points.Length - 1] = Cone.transform.position;
            Arrow.SetPositions(_points);

            yield return null;
        }
        Destroy(Arrow);
        Destroy(Cone);
        if (_hit != null)
        {
            if (_match)
            {
                IsoEntity hit = null;
                if (_hit is TokenObject)
                    hit = ((TokenObject)_hit).GetEntity();
                else if (_hit is InteractiveShapeNetObject)
                    hit = ((InteractiveShapeNetObject)_hit).Entity;

                if(hit != null)
                {
                    referencePt = hit;
                    OLinkUReferencePtButton.ChangeText(hit.TextContent);
                }
            }

        }
    }


}

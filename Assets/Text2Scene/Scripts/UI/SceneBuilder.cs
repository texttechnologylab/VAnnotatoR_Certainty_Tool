using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using LitJson;
using MathHelper;
using System;
using Valve.VR;
using System.Reflection;
using UnityEngine.VFX;

public class SceneBuilder : AnimatedWindow
{

    private bool _initDone;

    /**************************************
     *      REMINDER WINDOW
     **************************************/

    private GameObject _reminder;
    private TextMeshPro _textField;
    private InteractiveButton _acceptButton;
    private InteractiveButton _declineButton;
    private InteractiveButton _closeButton;
    public delegate void ButtonEvent();
    public GameObject Reminder
    {
        get
        {
            if (_reminder == null)
            {
                _reminder = ((GameObject)Instantiate(Resources.Load("Prefabs/Reminder")));
                _reminder.transform.SetParent(transform);
                _reminder.transform.localRotation = Quaternion.identity;
                _reminder.transform.localPosition = Vector3.forward * 0.05f;
                _textField = _reminder.transform.Find("Text").GetComponent<TextMeshPro>();
                _acceptButton = _reminder.transform.Find("AcceptButton").GetComponent<InteractiveButton>();
                _declineButton = _reminder.transform.Find("DeclineButton").GetComponent<InteractiveButton>();
                _closeButton = _reminder.transform.Find("CloseButton").GetComponent<InteractiveButton>();
            }
            return _reminder;
        }
    }

    // **************************************
    // *      TAB HANDLING
    // **************************************/
    private InteractiveButton PreviousTabs;
    private List<InteractiveButton> TabButtons;
    private InteractiveButton NextTabs;

    public Dictionary<Type, BuilderTab> BuilderTabMap;
    public List<BuilderTab> BuilderTabs { get; private set; }
    private int _tabIndex;
    public int TabIndex
    {
        get { return _tabIndex; }
        set
        {
            if (value < 0 || value > BuilderTabs.Count) return;
            _tabIndex = value;
            PreviousTabs.gameObject.SetActive(_tabIndex > 0);
            NextTabs.gameObject.SetActive(_tabIndex + TabButtons.Count < BuilderTabs.Count);
            for (int i=0; i<TabButtons.Count; i++)
            {
                TabButtons[i].gameObject.SetActive(_tabIndex + i < BuilderTabs.Count);
                if (TabButtons[i].gameObject.activeInHierarchy)
                {
                    TabButtons[i].ChangeText(BuilderTabs[_tabIndex + i].Name);
                    TabButtons[i].ButtonValue = BuilderTabs[_tabIndex + i];
                    BuilderTabs[_tabIndex + i].ControlButton = TabButtons[i];
                    BuilderTabs[_tabIndex + i].ActualizeControlButtonStatus();
                }
                
            }
        }
    }

    ///**************************************
    // *      SURFACE SETTING TAB 
    // **************************************/
    //private GameObject OthersTab;
    //private KeyboardEditText SurfaceSizeField;

    ///**************************************
    // *      ROOM SETTING TAB 
    // **************************************/
    //private GameObject RoomsTab;
    //private InteractiveButton PreviousRoomBtn;
    //public KeyboardEditText RoomDisplay { get; private set; }
    //private InteractiveButton NextRoomBtn;
    //private InteractiveButton CreateRoomBtn;
    //private InteractiveButton DeleteRoomBtn;

    //private TextMeshPro RoomEditorLabel;
    //private InteractiveCheckbox RoomEditorCheckbox;

    //private TextMeshPro WallHeightLabel;
    //public KeyboardEditText WallHeightField { get; private set; }

    //private TextMeshPro TextureSlotIcon;
    //private MeshRenderer TextureThumbnail;
    //private TextMeshPro TextureLoading;
    //private InteractiveButton TextureAcceptBtn;
    //private InteractiveButton TextureRemoveBtn;
    //private TextMeshPro TextureSpaceLabel;
    //private InteractiveButton TextureSpaceBtn;
    //private TextMeshPro TilingXLabel;
    //private KeyboardEditText TilingXField;
    //private TextMeshPro TilingYLabel;
    //private KeyboardEditText TilingYField;
    //private TextMeshPro OffsetXLabel;
    //private KeyboardEditText OffsetXField;
    //private TextMeshPro OffsetYLabel;
    //private KeyboardEditText OffsetYField;

    //private TextMeshPro CornerLabel;
    //private InteractiveButton CornerModeChanger;
    //private TextMeshPro WallLengthLabel;
    //private InteractiveCheckbox WallLengthCheckbox;

    // VARIABLES, PROPERTIES

    //private GameObject _dummyContainer;
    //private GameObject _cornerContainer;
    //public Dictionary<Vector3, InteractiveCorner> Corners;
    //public Vector3 ActualEditorPoint { get; private set; }

    //public bool ShowGrid;
    //public Terrain Ground;
    //private InteractiveObject InteractiveGround;

    //private Material _defaultWallMaterial;
    //public Material DefaultWallMaterial
    //{
    //    get
    //    {
    //        if (_defaultWallMaterial == null)
    //            _defaultWallMaterial = (Material)Instantiate(Resources.Load("Text2Scene/DefaultWallMaterial"));
    //        return _defaultWallMaterial;
    //    }
    //}

    //private int _surfaceSize;
    //private int _newSize;
    //public int SurfaceSize
    //{
    //    get { return _surfaceSize; }
    //    set
    //    {
    //        _newSize = Mathf.Min(Mathf.Max(50, value), 200);
    //        if (_newSize != _surfaceSize)
    //        {
    //            _surfaceSize = _newSize;
    //            SetupEnvironment();
    //            //TODO adjust room positions
    //        }
    //        SurfaceSizeField.Text = "" + _surfaceSize;
    //    }
    //}

    //private bool _roomEditor;
    //public bool RoomEditor
    //{
    //    get { return _roomEditor; }
    //    set
    //    {
    //        if (CheckRoomsForCrossingWalls()) return;
    //        _roomEditor = value;
    //        //if (_ghostCorner0 != null) _ghostCorner0.SetActive(_roomEditor);
    //        if (_roomEditor)
    //        {
    //            DoorEditorMode = false;
    //            WindowEditorMode = false;
    //        }
    //        UpdateRoomTabGUI();
    //        UpdateCorners();
    //        if (!_roomWallsUpdated) UpdateRoomWalls();
    //    }
    //}

    //public enum Mode { Create, Move, Delete }
    //private Mode _cornerMode;
    //public Mode CornerMode
    //{
    //    get { return _cornerMode; }
    //    set
    //    {
    //        _cornerMode = value;
    //        CornerModeChanger.ChangeText(_cornerMode == Mode.Create ? "\xf067" : (_cornerMode == Mode.Move) ? "\xf0b2" : "\xf2ed");
    //        foreach (InteractiveCorner corner in Corners.Values)
    //            corner.GetComponent<Collider>().enabled = _cornerMode != Mode.Create;
    //    }
    //}

    //public enum BuilderTab { Document, Rooms, Objects, Other, Reconnecter }
    //private BuilderTab _activeTab;
    //public BuilderTab ActiveTab
    //{
    //    get { return _activeTab; }
    //    set
    //    {
    //        _activeTab = value;
    //        OtherBtn.ButtonOn = _activeTab == BuilderTab.Other;
    //        RoomsBtn.ButtonOn = _activeTab == BuilderTab.Rooms;
    //        ObjectsBtn.ButtonOn = _activeTab == BuilderTab.Objects;
    //        DocumentBtn.ButtonOn = _activeTab == BuilderTab.Document;
    //        OthersTab.SetActive(_activeTab == BuilderTab.Other);
    //        RoomsTab.SetActive(_activeTab == BuilderTab.Rooms);
    //        ObjectsTab.SetActive(_activeTab == BuilderTab.Objects);
    //        DocumentTab.SetActive(_activeTab == BuilderTab.Document);
    //        ReconnecterTab.SetActive(_activeTab == BuilderTab.Reconnecter);
    //        if (_activeTab == BuilderTab.Objects)
    //            ModelRemoveBtn.Active = ShapeNetObject != null;
    //        if (_activeTab == BuilderTab.Document)
    //            SaveDocumentBtn.Active = LoadedDocument != null && LoadedDocument.HasChanges;
    //        if (_activeTab != BuilderTab.Rooms && RoomEditor)
    //        {
    //            _roomWallsUpdated = false;
    //            RoomEditor = false;
    //        }
    //        if (_activeTab == BuilderTab.Reconnecter)
    //        {
    //            OtherBtn.Active = false;
    //            RoomsBtn.Active = false;
    //            ObjectsBtn.Active = false;
    //            DocumentBtn.Active = false;
    //        }
    //    }
    //}

    //public bool AnnotatorActive { get { return QuickAnnotatorField.activeInHierarchy; } }

    //private bool _WallLength;
    //public bool WallLength
    //{
    //    get { return _WallLength; }
    //    set
    //    {
    //        _WallLength = value;
    //        WallLengthCheckbox.ButtonOn = _WallLength;
    //        UpdateRoomWalls();
    //    }
    //}

    //public List<InteractiveRoom> Rooms { get; private set; }

    //public InteractiveRoom SelectedRoom
    //{
    //    get
    //    {
    //        if (Rooms.Count == 0 || SelectedRoomIndex == -1) return null;
    //        return Rooms[SelectedRoomIndex];
    //    }
    //}

    //private int _selectedRoomIndex;
    //public int SelectedRoomIndex
    //{
    //    get { return _selectedRoomIndex; }
    //    set
    //    {
    //        _selectedRoomIndex = value;            
    //        PreviousRoomBtn.Active = _selectedRoomIndex > 0;
    //        NextRoomBtn.Active = _selectedRoomIndex > -1 && _selectedRoomIndex < Rooms.Count - 1;
    //        RoomDisplay.Text = (_selectedRoomIndex == -1) ? RoomDisplay.Description : SelectedRoom.Name;
    //        RoomDisplay.inputField.color = (_selectedRoomIndex == -1) ? Color.gray : Color.white;
    //        RoomDisplay.GetComponent<Collider>().enabled = _selectedRoomIndex > -1;
    //        DeleteRoomBtn.Active = _selectedRoomIndex > -1;
    //        WallHeightField.Text = (_selectedRoomIndex == -1) ? WallHeightField.Description : SelectedRoom.WallHeight.ToString();
    //        ObjectsBtn.Active = _selectedRoomIndex > -1;
    //        for (int i = 0; i < Rooms.Count; i++) Rooms[i].SetRoomStatus();
    //        if (_selectedRoomIndex > -1)
    //        {
    //            TextureSlotIcon.color = Color.white;
    //            TextureSpaceLabel.color = Color.white;
    //            TextureSpaceBtn.Active = true;
    //            TextureSpaceBtn.ButtonText.color = Color.white;
    //            TextureSpaceIndex = 0;
    //            //SelectedRoom.UpdateRoom();
    //        } else
    //        {
    //            TextureSlotIcon.color = Color.gray;
    //            TextureSpaceLabel.color = Color.gray;
    //            TextureSpaceBtn.Active = false;
    //            TextureSpaceBtn.ButtonText.color = Color.gray;
    //            TextureSlot = null;
    //        }
    //        if (!_roomWallsUpdated) UpdateRoomWalls();
    //    }
    //}

    //public Vector2 Tiling
    //{
    //    get
    //    {
    //        if (TextureSlot == null || TilingXField.Text == "-") return Vector2.one;
    //        return new Vector2(float.Parse(TilingXField.Text.Replace('.', ',')), float.Parse(TilingYField.Text.Replace('.', ',')));
    //    }
    //}

    //public Vector2 Offset
    //{
    //    get
    //    {
    //        if (TextureSlot == null || OffsetXField.Text == "-") return Vector2.zero;
    //        return new Vector2(float.Parse(OffsetXField.Text.Replace('.', ',')), float.Parse(OffsetYField.Text.Replace('.', ',')));
    //    }
    //}



    //public enum TextureSpace { Ground, Inner_Walls, Outer_Walls, Top }
    //private List<TextureSpace> Spaces = new List<TextureSpace>() { TextureSpace.Ground, TextureSpace.Inner_Walls, TextureSpace.Outer_Walls, TextureSpace.Top };
    //private int _tSpaceIndex;
    //public TextureSpace ActualSpace { get { return Spaces[TextureSpaceIndex]; } }
    //public int TextureSpaceIndex
    //{
    //    get { return _tSpaceIndex; }
    //    set
    //    {
    //        _tSpaceIndex = value;
    //        TextureSpaceBtn.ChangeText(ActualSpace.ToString().Replace("_", " "));
    //        if (SelectedRoom != null)
    //        {
    //            switch (ActualSpace)
    //            {
    //                case TextureSpace.Ground:
    //                    TextureSlot = (SelectedRoom.GroundTexture != null) ? 
    //                        SelectedRoom.GroundTexture.GetShapeNetTexture() : null;
    //                    break;
    //                case TextureSpace.Inner_Walls:
    //                    TextureSlot = (SelectedRoom.InnerWallTexture != null) ?
    //                        SelectedRoom.InnerWallTexture.GetShapeNetTexture() : null;
    //                    break;
    //                case TextureSpace.Outer_Walls:
    //                    TextureSlot = (SelectedRoom.OuterWallTexture != null) ?
    //                        SelectedRoom.OuterWallTexture.GetShapeNetTexture() : null;
    //                    break;
    //                default:
    //                    TextureSlot = (SelectedRoom.TopTexture != null) ?
    //                        SelectedRoom.TopTexture.GetShapeNetTexture() : null;
    //                    break;
    //            }
    //        }
    //        else
    //            TextureSlot = null;
    //    }
    //}

    //private ShapeNetTexture _textureSlot;
    //public ShapeNetTexture TextureSlot
    //{
    //    get { return _textureSlot; }
    //    set
    //    {
    //        _textureSlot = value;
    //        if (_textureSlot != null && !_textureSlot.IsDownloaded)
    //            ShapeNetInterface.RequestTexture(_textureSlot.ID, (path) => { OnTextureLoaded(); });
    //        else OnTextureLoaded();
    //    }
    //}

    //public bool ObjectRequestPending; <=========================== ERSETZEN DURCH WaitingForResponse!!!!
    public bool InterruptLoadingAnimation;
    public SimpleGazeCursor Gaze { get { return StolperwegeHelper.Gaze; } }
    public SceneBuilderSceneScript SceneBuilderControl 
    { 
        get 
        {
            if (SceneController.ActiveSceneScript == null ||
                !SceneController.ActiveSceneScript.GetType().Equals(typeof(SceneBuilderSceneScript)))
                return null;
            return (SceneBuilderSceneScript)SceneController.ActiveSceneScript; 
        } 
    }
    public TextAnnotatorInterface TextAnnotatorInterface { get { return global::SceneController.GetInterface<TextAnnotatorInterface>();  } }
    public VisualEffect Effect { get; private set; }
    public bool EffectActive;


    //public HashSet<Vector3> CornersToCreate;
    //public HashSet<string> RequestedOpeningIDs;
    //public bool CreatingMultipleCorners;
    //public bool CornerDraggingInterrupted;

    //public InteractiveCorner MovedCorner;
    //private LineRenderer Arrow; 
    //private GameObject Cone; 
    //private Vector3[] points; 
    //private Vector3 middle;

    //private delegate void OnClosingWallCreated();


    public void Initialize()
    {
        Grabable = true;
        UseHighlighting = false;
        SearchForParts = false;

        base.Start();
        LookAtUserOnHold = true;
        KeepYRotation = true;
        Removable = false;
        DestroyOnObjectRemover = false;

        // ==================================== Initializing tabs and buttons =======================================

        // Tabs and buttons
        TabButtons = new List<InteractiveButton>();
        InteractiveButton[] buttons = GetComponentsInChildren<InteractiveButton>();

        InitializeTabs();
        
        for (int i=0; i<buttons.Length; i++)
        {
            if (buttons[i].name.Equals("PreviousTabs"))
                PreviousTabs = buttons[i];
            else if (buttons[i].name.Equals("NextTabs"))
                NextTabs = buttons[i];
            else
            {
                InteractiveButton tabButton = buttons[i];
                TabButtons.Add(tabButton);
                tabButton.OnClick = () =>
                {
                    foreach (InteractiveButton tButton in TabButtons)
                    {
                        bool clickedBtn = tButton.ButtonText.Equals(tabButton.ButtonText);
                        if (tButton.ButtonValue != null &&
                            tButton.ButtonValue is BuilderTab)
                            ((BuilderTab)tButton.ButtonValue).Active = clickedBtn;
                    }
                };
            }                
        }
        PreviousTabs.OnClick = () => { TabIndex -= TabButtons.Count; };
        NextTabs.OnClick = () => { TabIndex += TabButtons.Count; };        

        //ActualizeTabButtons();

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].tag = "UI";

        // Adding effect
        Effect = gameObject.AddComponent<VisualEffect>();
        Effect.visualEffectAsset = Resources.Load<VisualEffectAsset>("Effects/Particles");

        ResetBuilder();


        //    Rooms = new List<InteractiveRoom>();
        //    Corners = new Dictionary<Vector3, InteractiveCorner>();
        //    CornersToCreate = new HashSet<Vector3>();
        //    _generatedObjects = new Dictionary<string, GameObject[]>();
        //    _loadedObjectIDQueue = new List<string>();
        //    _loadedObjectIDMap = new Dictionary<string, int>();
        //    RequestedOpeningIDs = new HashSet<string>();
        //    _dummyWalls = new List<GameObject>();
        //    _dummyWallLabels = new List<TextMeshPro>();

        //    // Surface section

        //    SurfaceSizeField = OthersTab.transform.Find("SurfaceSize/InputBar").GetComponent<KeyboardEditText>();
        //    SurfaceSizeField.OnCommit = (text, go) => { SurfaceSize = int.Parse(text); };

        //    // Room settings section

        //    PreviousRoomBtn = RoomsTab.transform.Find("RoomSettings/RoomHandler/Previous").GetComponent<InteractiveButton>();
        //    PreviousRoomBtn.executeOnClick = () => {
        //        StartCoroutine(CheckClosingWall(() => {
        //            _roomWallsUpdated = false;
        //            SelectedRoomIndex -= 1;
        //        }));
        //    };

        //    RoomDisplay = RoomsTab.transform.Find("RoomSettings/RoomHandler/RoomDisplay").GetComponent<KeyboardEditText>();
        //    RoomDisplay.Description = "No rooms found.";
        //    RoomDisplay.MaxChars = 25;
        //    RoomDisplay.ChangeTextOnCommit = false;
        //    RoomDisplay.OnCommit = (name, go) => 
        //    {
        //        SelectedRoom.SendRoomNameUpdatingRequest(name);
        //    };

        //    NextRoomBtn = RoomsTab.transform.Find("RoomSettings/RoomHandler/Next").GetComponent<InteractiveButton>();
        //    NextRoomBtn.executeOnClick = () => {
        //        StartCoroutine(CheckClosingWall(() => {
        //            _roomWallsUpdated = false;
        //            SelectedRoomIndex += 1;
        //        }));
        //    };

        //    CreateRoomBtn = RoomsTab.transform.Find("RoomSettings/RoomHandler/CreateRoom").GetComponent<InteractiveButton>();
        //    CreateRoomBtn.executeOnClick = () =>
        //    {
        //        if (CheckRoomsForCrossingWalls()) return;
        //        CreateRoomBtn.Active = false;
        //        DeleteRoomBtn.Active = false;
        //        StartCoroutine(CheckClosingWall(SendRoomCreatingRequest));
        //    };

        //    DeleteRoomBtn = RoomsTab.transform.Find("RoomSettings/RoomHandler/DeleteRoom").GetComponent<InteractiveButton>();
        //    DeleteRoomBtn.executeOnLongClick = () => { if (SelectedRoom != null) SendRoomDeletingRequest(SelectedRoom.Room); };
        //    DeleteRoomBtn.LoadingText = "Deleting room...";

        //    // TODO

        //    RoomEditorLabel = RoomsTab.transform.Find("RoomEditorOptions/RoomEditor/Description").GetComponent<TextMeshPro>();
        //    RoomEditorCheckbox = RoomsTab.transform.Find("RoomEditorOptions/RoomEditor/Checkbox").GetComponent<InteractiveCheckbox>();
        //    RoomEditorCheckbox.Start();
        //    RoomEditorCheckbox.AllChecked = "On";
        //    RoomEditorCheckbox.NoneChecked = "Off";
        //    RoomEditorCheckbox.executeOnClick = () => {
        //        _roomWallsUpdated = false;
        //        RoomEditor = !RoomEditorCheckbox.ButtonOn;
        //    };

        //    WallHeightLabel = RoomsTab.transform.Find("RoomEditorOptions/WallHeight/Description").GetComponent<TextMeshPro>();
        //    WallHeightField = RoomsTab.transform.Find("RoomEditorOptions/WallHeight/InputBar").GetComponent<KeyboardEditText>();
        //    WallHeightField.ChangeTextOnCommit = false;
        //    WallHeightField.OnCommit = (text, go) => 
        //    {
        //        if (!WallHeightField.InputChanged) return;
        //        SelectedRoom.SendWallHeightUpdatedingRequest(float.Parse(text.Replace('.', ',')));

        //    };
        //    WallHeightField.IsNumberField = true;
        //    WallHeightField.EnableFloatingPoint = true;
        //    WallHeightField.EnableNegativeNumber = false;
        //    WallHeightField.Description = "-";

        //    TextureSpaceLabel = RoomsTab.transform.Find("RoomTextures/TextureEditor/Space/Description").GetComponent<TextMeshPro>();
        //    TextureSpaceBtn = RoomsTab.transform.Find("RoomTextures/TextureEditor/Space/Button").GetComponent<InteractiveButton>();
        //    TextureSpaceBtn.executeOnClick = () => { TextureSpaceIndex = (TextureSpaceIndex + 1) % Spaces.Count; };

        //    TextureSlotIcon = RoomsTab.transform.Find("RoomTextures/TextureEditor/TextureSlot").GetComponent<TextMeshPro>();
        //    TextureThumbnail = RoomsTab.transform.Find("RoomTextures/TextureEditor/Thumbnail").GetComponent<MeshRenderer>();
        //    TextureLoading = RoomsTab.transform.Find("RoomTextures/TextureEditor/TextureLoading").GetComponent<TextMeshPro>();
        //    TextureLoading.text = "";
        //    TextureRemoveBtn = RoomsTab.transform.Find("RoomTextures/TextureEditor/RemoveButton").GetComponent<InteractiveButton>();
        //    TextureRemoveBtn.executeOnClick = ActualizeTexture;
        //    TextureAcceptBtn = RoomsTab.transform.Find("RoomTextures/TextureEditor/AcceptButton").GetComponent<InteractiveButton>();
        //    TextureAcceptBtn.executeOnClick = ActualizeTexture;
        //    TilingXLabel = RoomsTab.transform.Find("RoomTextures/TextureEditor/TilingX/Description").GetComponent<TextMeshPro>();
        //    TilingXField = RoomsTab.transform.Find("RoomTextures/TextureEditor/TilingX/InputBar").GetComponent<KeyboardEditText>();
        //    TilingXField.OnCommit = (text, go) => 
        //    {
        //        if (!TilingXField.InputChanged) return;
        //        TilingXField.Text = "" + Mathf.Min(Mathf.Max(0.1f, NumericHelper.ParseFloat(text)), 100);
        //        TextureAcceptBtn.Active = true;
        //    };
        //    TilingXField.IsNumberField = true;
        //    TilingXField.EnableFloatingPoint = true;
        //    TilingXField.EnableNegativeNumber = false;
        //    TilingXField.Description = "-";

        //    TilingYLabel = RoomsTab.transform.Find("RoomTextures/TextureEditor/TilingY/Description").GetComponent<TextMeshPro>();
        //    TilingYField = RoomsTab.transform.Find("RoomTextures/TextureEditor/TilingY/InputBar").GetComponent<KeyboardEditText>();
        //    TilingYField.OnCommit = (text, go) =>  
        //    {
        //        if (!TilingYField.InputChanged) return;
        //        TilingYField.Text = "" + Mathf.Min(Mathf.Max(0.1f, NumericHelper.ParseFloat(text)), 100);
        //        TextureAcceptBtn.Active = true;
        //    };
        //    TilingYField.IsNumberField = true;
        //    TilingYField.EnableFloatingPoint = true;
        //    TilingYField.EnableNegativeNumber = false;
        //    TilingYField.Description = "-";

        //    OffsetXLabel = RoomsTab.transform.Find("RoomTextures/TextureEditor/OffsetX/Description").GetComponent<TextMeshPro>();
        //    OffsetXField = RoomsTab.transform.Find("RoomTextures/TextureEditor/OffsetX/InputBar").GetComponent<KeyboardEditText>();
        //    OffsetXField.OnCommit = (text, go) => 
        //    {
        //        if (!OffsetXField.InputChanged) return;
        //        OffsetXField.Text = "" + Mathf.Min(Mathf.Max(0.1f, NumericHelper.ParseFloat(text)), 100);
        //        TextureAcceptBtn.Active = true;
        //    };
        //    OffsetXField.IsNumberField = true;
        //    OffsetXField.EnableFloatingPoint = true;
        //    OffsetXField.EnableNegativeNumber = true;
        //    OffsetXField.Description = "-";

        //    OffsetYLabel = RoomsTab.transform.Find("RoomTextures/TextureEditor/OffsetY/Description").GetComponent<TextMeshPro>();
        //    OffsetYField = RoomsTab.transform.Find("RoomTextures/TextureEditor/OffsetY/InputBar").GetComponent<KeyboardEditText>();
        //    OffsetYField.OnCommit = (text, go) => 
        //    {
        //        if (!OffsetYField.InputChanged) return;
        //        OffsetYField.Text = "" + Mathf.Min(Mathf.Max(0.1f, NumericHelper.ParseFloat(text)), 100);
        //        TextureAcceptBtn.Active = true;
        //    };
        //    OffsetYField.IsNumberField = true;
        //    OffsetYField.EnableFloatingPoint = true;
        //    OffsetYField.EnableNegativeNumber = true;
        //    OffsetYField.Description = "-";
        //    // Room editor options section

        //    CornerLabel = RoomsTab.transform.Find("RoomEditorOptions/CornerMode/Description").GetComponent<TextMeshPro>();
        //    CornerModeChanger = RoomsTab.transform.Find("RoomEditorOptions/CornerMode/ModeChanger").GetComponent<InteractiveButton>();
        //    CornerModeChanger.Start();
        //    CornerModeChanger.executeOnClick = () => {
        //        StartCoroutine(CheckClosingWall(() =>
        //        {
        //            if (CornerMode == Mode.Create && SelectedRoom.Corners.Count > 0) CornerMode = Mode.Move;
        //            else if (CornerMode == Mode.Move) CornerMode = Mode.Delete;
        //            else if (CornerMode == Mode.Delete) CornerMode = Mode.Create;

        //        }));
        //    };

        //    WallLengthLabel = RoomsTab.transform.Find("RoomEditorOptions/WallLength/Description").GetComponent<TextMeshPro>();
        //    WallLengthCheckbox = RoomsTab.transform.Find("RoomEditorOptions/WallLength/Checkbox").GetComponent<InteractiveCheckbox>();
        //    WallLengthCheckbox.Start();
        //    WallLengthCheckbox.AllChecked = "On";
        //    WallLengthCheckbox.NoneChecked = "Off";
        //    WallLengthCheckbox.executeOnClick = () => { WallLength = !WallLengthCheckbox.ButtonOn; };


        //    _lastEditorPoint = Vector3.one * 10000;
        _initDone = true;
    }

    private void InitializeTabs()
    {
        BuilderTabs = new List<BuilderTab>();
        BuilderTabMap = new Dictionary<Type, BuilderTab>();
        IEnumerable<Type> tabs = StolperwegeHelper.GetTypesDerivingFrom<BuilderTab>();        

        foreach (Type tab in tabs)
        {

            PrefabInterfaceAttribute prefabInterface = tab.GetCustomAttribute<PrefabInterfaceAttribute>();

            BuilderTab builderTab = null;
            if (prefabInterface != null) //Check if the tab is a Prefab Interface
            {
                // Instantiate Interface-Prefab
                GameObject prefab = Resources.Load<GameObject>(prefabInterface.PrefabPath);
                if (prefab == null)
                {
                    Debug.LogError($"Prefab of '{tab.Name}' interface is not present at [{prefabInterface.PrefabPath}]!");
                    continue;
                }

                GameObject gameObject = Instantiate(prefab);
                gameObject.transform.SetParent(transform);
                gameObject.transform.localPosition = Vector3.zero;
                builderTab = gameObject.GetComponent<BuilderTab>();
                if (builderTab == null)
                {
                    Debug.LogError($"Prefab of '{tab.Name}' interface does not contain a Interface component!");
                    continue;
                }
            }

            if (builderTab != null)
            {
                builderTab.Initialize(this);
                if (builderTab.ShowOnToolbar) BuilderTabs.Add(builderTab);
                BuilderTabMap.Add(tab, builderTab);
            }
        }
    }
    
    public Tab GetTab<Tab>()
        where Tab : BuilderTab
    {
        return (Tab)BuilderTabMap[typeof(Tab)];
    }

    public void DisableTabButtons()
    {
        foreach (InteractiveButton tabButton in TabButtons)
            tabButton.Active = false;
    }
    // *************************************************************************************************************
    // *      BUILDER METHODS
    // *************************************************************************************************************/
    //bool firstTranslationSetted; float xPos, zPos; int xRounder, zRounder; InteractiveRoom _room;
    //Vector3 _translation, _point0, _point1, _point2, _point3, _lastEditorPoint;     
    //GameObject _roomObject, _dummyCorner0, _dummyCorner1, _dummyCorner2, _dummyCorner3;
    //List<GameObject> _dummyWalls; List<TextMeshPro> _dummyWallLabels; GameObject _dummyWall; TextMeshPro _dummyWallLabel;
    //bool _activateDummyWall0, _activateDummyWall1, _hasWrongWall, _overlapping; Axis RoomDirection;
    //int _crossingStatus; int[] _crossingStates; Vector2 start, target, end, intersection; 
    //List<Vector3> _starts, _ends; List<Vector2> _starts2D; HashSet<Vector3> _startSet;

    /// <summary>
    /// Initializes a popup window for a reminder message
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="acceptText"></param>
    /// <param name="accept"></param>
    /// <param name="closeText"></param>
    /// <param name="close"></param>
    public void InitReminder(string msg, ButtonEvent accept, ButtonEvent decline=null, ButtonEvent close=null)
    {
        Reminder.SetActive(true);
        _textField.text = msg;

        _acceptButton.gameObject.SetActive(true);
        _acceptButton.OnClick = () => { accept?.Invoke(); };

        if (decline != null)
        {
            _declineButton.gameObject.SetActive(true);
            _declineButton.OnClick = decline.Invoke;
            Vector3 localPos = _acceptButton.transform.localPosition;
            localPos.x = 0.1f;
            _acceptButton.transform.localPosition = localPos;
            localPos = _declineButton.transform.localPosition;
            localPos.x = -0.1f;
            _declineButton.transform.localPosition = localPos;
        }
        else
        {
            _declineButton.gameObject.SetActive(false);
            Vector3 localPos = _acceptButton.transform.localPosition;
            localPos.x = 0;
            _acceptButton.transform.localPosition = localPos;
        }

        _closeButton.gameObject.SetActive(close != null);
        if (close != null) _closeButton.OnClick = close.Invoke;
    }

    public void SetEffectStatus(bool active)
    {
        EffectActive = active;
        Effect.enabled = EffectActive;
        if (EffectActive) Effect.Play();
        else Effect.Stop();
    }

    public void SetInitialPosition(Vector3 translation)
    {
        transform.position = StolperwegeHelper.User.transform.position + translation;
    }

    //private Vector3 _translation;
    //public void Update()
    //{
        

        //if (!_initDone) return;

        //    if (!InteractiveGround.Highlight)
        //    {
        //        _dummyCorner0.SetActive(false);
        //        TurnOffCornerTools();
        //        TurnOffArrow();
        //        if (SelectedRoom != null && SelectedRoom.Corners.Count > 2 && RoomEditor)
        //        {
        //            if (SelectedRoom.GetClosingWall() != null)
        //                SelectedRoom.GetClosingWall().WallBaseline.SetActive(true);
        //            else
        //            {
        //                SetupDummyWalls(new List<Vector3>() { SelectedRoom.GetLastCorner().Position }, 
        //                                new List<Vector3>() { SelectedRoom.GetFirstCorner().Position },
        //                                new Color[] { StolperwegeHelper.GUCOLOR.LICHTBLAU }, false);
        //            }

        //        }
        //    }        

        //    if (ActiveTab == BuilderTab.Objects && DoorEditorMode) EditDoor();
        //    else GhostDoor.SetActive(false);
        //    if (ActiveTab == BuilderTab.Objects && WindowEditorMode) EditWindow();
        //    else GhostWindow.SetActive(false);

        //    if (ActiveTab == BuilderTab.Objects && !DoorEditorMode && !WindowEditorMode &&
        //        (ShapeNetObject != null)) PlaceObject();
        //    else if (GhostObject != null) GhostObject.SetActive(false);

        //    if (StolperwegeHelper.LeftHand != null && StolperwegeHelper.RightHand != null &&
        //        (ActiveTab == BuilderTab.Objects || (ActiveTab == BuilderTab.Rooms && SelectedRoomIndex > -1) ||
        //        ActiveTab == BuilderTab.Document)) CheckHands();
        //    if (_snapRotationLocked && InputInterface.getAxis(InputInterface.ControllerType.RHAND).x == 0) _snapRotationLocked = false;
        //    if (_snapScalingLocked && InputInterface.getAxis(InputInterface.ControllerType.RHAND).y == 0) _snapScalingLocked = false;

        //    if (Input.GetKeyDown(KeyCode.Alpha1)) StolperwegeHelper.User.transform.position = Vector3.up;

        //    if (SceneController.GetInterface<ResourceManagerInterface>().SessionID != null && LoginButton.gameObject.activeInHierarchy) LoadExamples();
        //    if (StolperwegeHelper.RightHandAnim.IsPointing && Input.GetKeyDown(KeyCode.F))
        //        StolperwegeHelper.RightHandAnim.FixPose = !StolperwegeHelper.RightHandAnim.FixPose;
        //    if (StolperwegeHelper.LeftHandAnim.IsPointing && Input.GetKeyDown(KeyCode.F))
        //        StolperwegeHelper.LeftHandAnim.FixPose = !StolperwegeHelper.RightHandAnim.FixPose;
    //}


    //private void CheckPoint(Vector3 hit)
    //{
    //    if (!InteractiveGround.enabled || CreatingMultipleCorners) return;
    //    xPos = (int)(hit.x * 10);
    //    xRounder = (Mathf.Abs(xPos) % 10 <= 2) ? 0 : (Mathf.Abs(xPos) % 10 <= 7) ? 5 : 10;
    //    if (xPos < 0) xRounder *= -1;
    //    xPos = (xPos - (xPos % 10) + xRounder) / 10f;

    //    zPos = (int)(hit.z * 10);
    //    zRounder = (Mathf.Abs(zPos) % 10 <= 2) ? 0 : (Mathf.Abs(zPos) % 10 <= 7) ? 5 : 10;
    //    if (zPos < 0) zRounder *= -1;
    //    zPos = (zPos - (zPos % 10) + zRounder) / 10f;

    //    ActualEditorPoint = new Vector3(xPos, 0, zPos);

    //    if (CornerMode != Mode.Delete)
    //    {

    //        // if one of the trigger will be pressed, stop creating corners or interrupt the moving of a corner
    //        if (SteamVR_Actions.default_trigger.GetStateDown(SteamVR_Input_Sources.LeftHand) || 
    //            SteamVR_Actions.default_trigger.GetStateDown(SteamVR_Input_Sources.RightHand))
    //        {
    //            if (MovedCorner != null) SetMovedWallStatus(MovedCorner, true);
    //            MovedCorner = null;
    //            CornerDraggingInterrupted = true;
    //            _dummyCorner0.SetActive(false);
    //            TurnOffCornerTools();
    //            TurnOffArrow();
    //        }

    //        if (ActualEditorPoint != _lastEditorPoint)
    //        {
    //            _hasWrongWall = false;
    //            // Check if the selected room already has a corner at the actual hit point or
    //            // the move-mode is activated and the stick is not pressed
    //            // if so turn of the first corner-pointer
    //            // if we are not in movement-mode and the room has already more then 2 corners, blend the last wall in

    //            if ((Corners.ContainsKey(ActualEditorPoint) && Corners[ActualEditorPoint].Rooms.Contains(SelectedRoom)) ||
    //                (CornerMode == Mode.Move && !SteamVR_Actions.default_click.GetStateUp(SteamVR_Input_Sources.RightHand) &&
    //                !SteamVR_Actions.default_click.GetState(SteamVR_Input_Sources.RightHand)))
    //            {
    //                _dummyCorner0.SetActive(false);
    //                if (MovedCorner != null) SetMovedWallStatus(MovedCorner, true);
    //                TurnOffCornerTools();
    //                TurnOffArrow();
    //                if (SelectedRoom.Corners.Count > 2)
    //                {
    //                    if (SelectedRoom.GetClosingWall() != null)
    //                        SelectedRoom.GetClosingWall().WallBaseline.SetActive(true);
    //                    else
    //                        SetupDummyWalls(new List<Vector3>() { SelectedRoom.GetLastCorner().Position }, 
    //                                        new List<Vector3>() { SelectedRoom.GetFirstCorner().Position },
    //                                        new Color[] { StolperwegeHelper.GUCOLOR.LICHTBLAU }, false);
    //                }
    //                return;
    //            }

    //            // in create-mode when the stick IS NOT pressed OR in move-mode when the stick IS pressed
    //            if ((CornerMode == Mode.Create && !SteamVR_Actions.default_click.GetState(SteamVR_Input_Sources.RightHand) &&
    //                !SteamVR_Actions.default_click.GetStateUp(SteamVR_Input_Sources.RightHand)) ||
    //                (CornerMode == Mode.Move && MovedCorner != null && SteamVR_Actions.default_click.GetState(SteamVR_Input_Sources.RightHand)))
    //            {
    //                _dummyCorner0.SetActive(true);


    //                if (CornerMode == Mode.Create)
    //                {
    //                    if (SelectedRoom.GetClosingWall() != null)
    //                        SelectedRoom.GetClosingWall().WallBaseline.SetActive(false);

    //                    // first dummy wall is active <=> we are in create-mode && there is at least 1 corner in the room
    //                    // second dummy wall is active <=> we are in create-mode && there are at least 2 corners in the room                        
    //                    _activateDummyWall0 = SelectedRoom.Corners.Count > 0;
    //                    _activateDummyWall1 = SelectedRoom.Corners.Count > 1;

    //                    // if the second dummy wall is active => the next corner is the first corner of the room
    //                    if (_activateDummyWall1) _nextCorner = SelectedRoom.GetFirstCorner();
    //                    if (_activateDummyWall0)
    //                    {
    //                        // the previous corner in the array is the last corner of the room
    //                        _prevCorner = SelectedRoom.GetLastCorner();

    //                        // set the point-array sizes depending on if the second dummy wall is active
    //                        if (_starts == null) _starts = new List<Vector3>();
    //                        else _starts.Clear();

    //                        if (_ends == null) _ends = new List<Vector3>();
    //                        else _ends.Clear();

    //                        _pos = _activateDummyWall1 ? _nextCorner.Position : Vector3.one * -1000;

    //                        // now check if the dummy walls are crossing or overlapping other walls
    //                        // and set their colors:
    //                        // => red: the wall can't be placed at the actual location
    //                        // => orange: the wall can be placed, but the room still needs other corners, because the wall crosses an another
    //                        // => blue: the wall can be placed
    //                        _colors = CheckWallsOnCreateCorner(_prevCorner.Position, ActualEditorPoint, _pos);
    //                        if (_hasWrongWall) SetDummyCorner(_dummyCorner0, SelectedRoom.Corners.Count, ActualEditorPoint, Color.red);
    //                        else SetDummyCorner(_dummyCorner0, SelectedRoom.Corners.Count, ActualEditorPoint, StolperwegeHelper.GUCOLOR.LICHTBLAU);

    //                        _starts.Add(_prevCorner.Position);
    //                        _ends.Add(ActualEditorPoint);
    //                        if (_activateDummyWall1)
    //                        {
    //                            _starts.Add(ActualEditorPoint);
    //                            _ends.Add(_nextCorner.Position);
    //                        }
    //                        SetupDummyWalls(_starts, _ends, _colors, true);
    //                    }
    //                    else
    //                    {
    //                        SetupDummyWalls(null, null, null, false);
    //                        SetDummyCorner(_dummyCorner0, SelectedRoom.Corners.Count, ActualEditorPoint, StolperwegeHelper.GUCOLOR.LICHTBLAU);
    //                    }

    //                }
    //                else
    //                {
    //                    SetMovedWallStatus(MovedCorner, ActualEditorPoint == MovedCorner.Position);
    //                    if (_starts == null) _starts = new List<Vector3>();
    //                    else _starts.Clear();

    //                    if (_ends == null) _ends = new List<Vector3>();
    //                    else _ends.Clear();

    //                    if (_starts2D == null) _starts2D = new List<Vector2>();
    //                    else _starts2D.Clear();

    //                    if (_startSet == null) _startSet = new HashSet<Vector3>();
    //                    else _startSet.Clear();

    //                    // collect all corners adjacent to the corner that should be moved
    //                    // add their 3D and 2D Positions in an array
    //                    // avoid putting overlapping walls twice in the array
    //                    foreach (InteractiveCorner c in MovedCorner.CornerWallMap.Keys)
    //                    {
    //                        if (!_startSet.Contains(c.Position))
    //                        {
    //                            _startSet.Add(c.Position);
    //                            _starts.Add(c.Position);
    //                            _starts2D.Add(c.Position2D);
    //                            _ends.Add(ActualEditorPoint);
    //                        }
    //                    }

    //                    // check if the dummy walls are crossing other walls
    //                    _colors = CheckWallsOnMoveCorner(_starts2D, new Vector2(ActualEditorPoint.x, ActualEditorPoint.z), MovedCorner);
    //                    SetDummyCorner(_dummyCorner0, MovedCorner.GetIndex(SelectedRoom), ActualEditorPoint, (_hasWrongWall) ? Color.red : StolperwegeHelper.GUCOLOR.LICHTBLAU);
    //                    SetupDummyWalls(_starts, _ends, _colors, true);
    //                }

    //            }

    //            if (SteamVR_Actions.default_click.GetState(SteamVR_Input_Sources.RightHand) &&
    //                !CornerDraggingInterrupted && SelectedRoom.Corners.Count == 0)
    //            {
    //                _dummyCorner0.SetActive(true);

    //                _dummyCorner0.transform.GetChild(0).gameObject.SetActive(true);
    //                _point2 = ActualEditorPoint;
    //                if (_point0.x == _point2.x) RoomDirection = Axis.Z;
    //                if (_point0.z == _point2.z) RoomDirection = Axis.X;
    //                _point1 = (RoomDirection == Axis.X) ? new Vector3(_point2.x, 0, _point0.z) : new Vector3(_point0.x, 0, _point2.z);
    //                _point3 = (RoomDirection == Axis.X) ? new Vector3(_point0.x, 0, _point2.z) : new Vector3(_point2.x, 0, _point0.z);

    //                _dummyCorner1.SetActive(_point2 != _point0);
    //                _dummyCorner2.SetActive(_point2.x != _point0.x && _point2.z != _point0.z);
    //                _dummyCorner3.SetActive(_point2.x != _point0.x && _point2.z != _point0.z);

    //                _activateDummyWall0 = _dummyCorner1.activeInHierarchy;
    //                _activateDummyWall1 = _activateDummyWall0 && _dummyCorner2.activeInHierarchy;

    //                if (_activateDummyWall0)
    //                {
    //                    if (!_activateDummyWall1)
    //                    {
    //                        _color = CheckWallCrossing(_point0, _point2);
    //                        SetDummyCorner(_dummyCorner0, 0, _point0, (_hasWrongWall) ? Color.red : StolperwegeHelper.GUCOLOR.LICHTBLAU);
    //                        SetDummyCorner(_dummyCorner1, 1, _point2, (_hasWrongWall) ? Color.red : StolperwegeHelper.GUCOLOR.LICHTBLAU);
    //                        SetupDummyWalls(new List<Vector3>() { _point0 }, new List<Vector3>() { _point2 }, new Color[] { _color }, true);
    //                    }
    //                    else
    //                    {
    //                        if (_starts == null) _starts = new List<Vector3>();
    //                        else _starts.Clear();

    //                        _starts.Add(_point0);
    //                        _starts.Add(_point1);
    //                        _starts.Add(_point2);
    //                        _starts.Add(_point3);

    //                        if (_ends == null) _ends = new List<Vector3>();
    //                        else _ends.Clear();

    //                        _ends.Add(_point1);
    //                        _ends.Add(_point2);
    //                        _ends.Add(_point3);
    //                        _ends.Add(_point0);



    //                        _colors = CheckBoxCrossing(_point0, _point1, _point2, _point3);
    //                        SetDummyCorner(_dummyCorner0, 0, _point0, (_colors[3] == Color.red || _colors[0] == Color.red) ? Color.red : StolperwegeHelper.GUCOLOR.LICHTBLAU);
    //                        SetDummyCorner(_dummyCorner1, 1, _point1, (_colors[0] == Color.red || _colors[1] == Color.red) ? Color.red : StolperwegeHelper.GUCOLOR.LICHTBLAU);
    //                        SetDummyCorner(_dummyCorner2, 2, _point2, (_colors[1] == Color.red || _colors[2] == Color.red) ? Color.red : StolperwegeHelper.GUCOLOR.LICHTBLAU);
    //                        SetDummyCorner(_dummyCorner3, 3, _point3, (_colors[2] == Color.red || _colors[3] == Color.red) ? Color.red : StolperwegeHelper.GUCOLOR.LICHTBLAU);
    //                        SetupDummyWalls(_starts, _ends, _colors, true);
    //                    }
    //                }
    //                else TurnOffCornerTools();
    //            }
    //        }

    //        if (SteamVR_Actions.default_click.GetStateDown(SteamVR_Input_Sources.RightHand))
    //        {
    //            CornerDraggingInterrupted = false;
    //            _point0 = ActualEditorPoint;
    //            _dummyCorner0.SetActive(true);
    //            _dummyCorner0.transform.position = _point0;
    //        }

    //        //if (InputInterface.getButtonUp(InputInterface.ControllerType.RHAND, InputInterface.ButtonType.STICK))
    //        if (SteamVR_Actions.default_click.GetStateUp(SteamVR_Input_Sources.RightHand))
    //        {
    //            if (!CornerDraggingInterrupted && !_hasWrongWall)
    //            {
    //                if (CornerMode == Mode.Create)
    //                {
    //                    if (_dummyCorner1.activeInHierarchy)
    //                    {
    //                        if (_dummyCorner2.activeInHierarchy)
    //                            StartCoroutine(CreateMultipleCorners(new Vector3[] { _point0, _point1, _point2, _point3 }));
    //                        else
    //                            StartCoroutine(CreateMultipleCorners(new Vector3[] { _point0, _point2 }));
    //                    }
    //                    else
    //                    {

    //                        if (Corners.ContainsKey(_point0)) ShareCorner(Corners[_point0], SelectedRoom);
    //                        else SendCornerCreatingRequest(new List<Vector3>() { _point0 });
    //                    }
    //                } else
    //                {
    //                    SetMovedWallStatus(MovedCorner, true);
    //                    MovedCorner.SendCornerUpdateRequest(ActualEditorPoint);
    //                    MovedCorner = null;
    //                    TurnOffCornerTools();
    //                    TurnOffArrow();
    //                }

    //            }

    //            if (!CreatingMultipleCorners) TurnOffCornerTools();
    //            _point1 = _point2 = _point3 = _point0;
    //        }            
    //    }

    //    _lastEditorPoint = ActualEditorPoint;
    //}

    //private void DrawArrow(Vector3 start, Vector3 end, Color color)
    //{
    //    if (Arrow == null)
    //    {
    //        Arrow = gameObject.AddComponent<LineRenderer>();
    //        SetupArrow(Arrow);
    //    }
    //    if (Cone == null)
    //    {
    //        Cone = (GameObject)(Instantiate(Resources.Load("Surface2Info/prefabs/Cone")));
    //        Cone.transform.localScale *= 2;
    //    }

    //    middle = (start + end) / 2;
    //    middle.y = Mathf.Max(end.y, start.y) + 0.5f;
    //    points = new Vector3[9];
    //    points = BezierCurve.CalculateCurvePoints(start, middle, end, points);
    //    Cone.GetComponent<Renderer>().material.SetColor("_Color", color);
    //    Cone.transform.forward = points[points.Length - 1] - points[points.Length - 2];
    //    Cone.transform.position = points[points.Length - 1] - Cone.transform.forward * Cone.GetComponent<Renderer>().bounds.size.z / 2;
    //    Cone.transform.forward = points[points.Length - 1] - points[points.Length - 2];
    //    points[points.Length - 1] = Cone.transform.position;
    //    Arrow.material.SetColor("_Color", color);
    //    Arrow.SetPositions(points);
    //}

    //private void SetupArrow(LineRenderer arrow)
    //{
    //    arrow.positionCount = 9;
    //    arrow.material = (Material)(Instantiate(Resources.Load("Surface2Info/materials/SurfaceGridMaterial")));
    //    arrow.widthMultiplier = 0.01f;
    //    arrow.useWorldSpace = true;
    //    arrow.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    //    arrow.receiveShadows = false;                
    //}

    //private Color[] CheckWallsOnCreateCorner(Vector3 s, Vector3 t, Vector3 e)
    //{
    //    TurnOffArrow();
    //    //start = new Vector2(s.x, s.z);
    //    //target = new Vector2(t.x, t.z);
    //    //end = new Vector2(e.x, e.z);
    //    //if (e != Vector3.one * -1000 && 
    //    //    LineCalculations.LineLineIntersection(start, target, target, end, out intersection, out _overlapping))
    //    //    if (_overlapping) return new Color[2] { Color.red, Color.red };
    //    //_colors = new Color[2];
    //    //_wallColor0 = StolperwegeHelper.GUCOLOR.LICHTBLAU;
    //    //_wallColor1 = StolperwegeHelper.GUCOLOR.LICHTBLAU;
    //    //for (int i = 0; i < Rooms.Count; i++)
    //    //{
    //    //    _crossingStates = Rooms[i].CheckWallsOnCreateCorner(start, target, end, SelectedRoom);
    //    //    if (_crossingStates[0] == -1) _wallColor0 = Color.red;
    //    //    if (_crossingStates[1] == -1) _wallColor1 = Color.red;
    //    //    else if (_crossingStates[1] == 0)
    //    //    {
    //    //        if (_crossingStates[0] != -1) _wallColor1 = new Color(1, 0.5f, 0, 1);
    //    //        else _wallColor1 = Color.red;
    //    //    }
    //    //}
    //    _colors = new Color[] { StolperwegeHelper.GUCOLOR.LICHTBLAU, StolperwegeHelper.GUCOLOR.LICHTBLAU };
    //    //_colors[0] = _wallColor0;
    //    //_colors[1] = _wallColor1;
    //    //_hasWrongWall = _crossingStates[0] == -1 || _crossingStates[1] == -1;
    //    return _colors;
    //}

    //private Color[] CheckWallsOnMoveCorner(List<Vector2> starts, Vector2 target, InteractiveCorner corner)
    //{
    //    _colors = new Color[corner.CornerWallMap.Count];
    //    for (int i = 0; i < _colors.Length; i++)
    //    {

    //        if (corner.CornerWallMap.ContainsKey(Corners[new Vector3(starts[i].x, 0, starts[i].y)]))
    //            foreach (InteractiveWall wall in corner.CornerWallMap[Corners[new Vector3(starts[i].x, 0, starts[i].y)]])
    //                if (wall.Room.Equals(SelectedRoom))
    //                    _colors[i] = StolperwegeHelper.GUCOLOR.LICHTBLAU;
    //        else _colors[i] = Color.gray;

    //    }


    //    for (int i = 0; i < Rooms.Count; i++)
    //    {
    //        _crossingStates = Rooms[i].CheckWallsOnMoveCorner(starts, target, corner);
    //        for (int j = 0; j < _crossingStates.Length; j++)
    //            if (_crossingStates[j] == -1)
    //            {
    //                _hasWrongWall = true;
    //                _colors[j] = Color.red;
    //            }
    //    }

    //    DrawArrow(MovedCorner.transform.position, ActualEditorPoint, (_hasWrongWall) ? Color.red : StolperwegeHelper.GUCOLOR.LICHTBLAU);
    //    return _colors;
    //}

    //private Color CheckWallCrossing(Vector3 s, Vector3 e)
    //{
    //    TurnOffArrow();
    //    //Vector2 start = new Vector2(s.x, s.z);
    //    //Vector2 end = new Vector2(e.x, e.z);
    //    //_crossingStatus = 1;
    //    //for (int i=0; i<Rooms.Count; i++)
    //    //{
    //    //    _crossingStatus = Mathf.Min(_crossingStatus, Rooms[i].CheckWallCrossing(start, end));
    //    //    if (_crossingStatus == -1)
    //    //    {
    //    //        _hasWrongWall = true;
    //    //        return Color.red;
    //    //    }
    //    //}
    //    //return _crossingStatus == 0 ? new Color(1, 0.5f, 0, 1) : StolperwegeHelper.GUCOLOR.LICHTBLAU;
    //    return StolperwegeHelper.GUCOLOR.LICHTBLAU;
    //}

    //private Color[] CheckBoxCrossing(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    //{
    //    TurnOffArrow();
    //    //for (int i=0; i<Rooms.Count; i++)
    //    //{
    //    //    if (Rooms[i].Positions.Contains(p0) &&
    //    //        Rooms[i].Positions.Contains(p1) &&
    //    //        Rooms[i].Positions.Contains(p2) &&
    //    //        Rooms[i].Positions.Contains(p3) &&
    //    //        Rooms[i].CheckPointLocation((p0 + p1 + p2 + p3) / 4f) == 0)
    //    //        return new Color[4] { Color.red, Color.red, Color.red, Color.red };
    //    //}
    //    _colors = new Color[4];
    //    //_colors[0] = CheckWallCrossing(p0, p1);
    //    //_colors[1] = CheckWallCrossing(p1, p2);
    //    //_colors[2] = CheckWallCrossing(p2, p3);
    //    //_colors[3] = CheckWallCrossing(p3, p0);
    //    _colors[0] = StolperwegeHelper.GUCOLOR.LICHTBLAU;
    //    _colors[1] = StolperwegeHelper.GUCOLOR.LICHTBLAU;
    //    _colors[2] = StolperwegeHelper.GUCOLOR.LICHTBLAU;
    //    _colors[3] = StolperwegeHelper.GUCOLOR.LICHTBLAU;
    //    return _colors;
    //}

    //private void SetMovedWallStatus(InteractiveCorner corner, bool status)
    //{
    //    Debug.Log(corner);
    //    foreach (HashSet<InteractiveWall> walls in corner.CornerWallMap.Values)
    //        foreach (InteractiveWall wall in walls)
    //            wall.WallBaseline.SetActive(status);
    //}

    //InteractiveRoom _roomWithCrossingWall;
    //private bool CheckRoomsForCrossingWalls()
    //{
    //    //_roomWithCrossingWall = null;
    //    //for (int i=0; i<Rooms.Count; i++)
    //    //{
    //    //    if (Rooms[i].HasCrossingWalls)
    //    //    {
    //    //        _roomWithCrossingWall = Rooms[i];
    //    //        InitReminder(1);
    //    //        return true;
    //    //    }
    //    //}
    //    return false;
    //}

    //private void SetDummyCorner(GameObject corner, int index, Vector3 pos, Color color)
    //{
    //    corner.transform.position = pos;
    //    corner.transform.GetChild(0).GetComponent<TextMeshPro>().text = "" + index;
    //    corner.transform.GetChild(0).LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
    //    corner.transform.GetChild(0).Rotate(Vector3.up * 180, Space.Self);
    //    corner.transform.localScale = Vector3.one * 0.12f;
    //    color.a = 0.6f;
    //    corner.GetComponent<MeshRenderer>().material.color = color;
    //}

    //private void CreateDummyWall()
    //{
    //    _dummyWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //    _dummyWall.name = "DummyWall" + _dummyWalls.Count;
    //    MakeGhostObject(_dummyWall);
    //    _dummyWall.transform.parent = _dummyContainer.transform;
    //    DestroyImmediate(_dummyWall.GetComponent<Collider>());
    //    _dummyWalls.Add(_dummyWall);


    //    GameObject label = new GameObject("DummyWallLabel" + _dummyWallLabels.Count);
    //    _dummyWallLabel = label.AddComponent<TextMeshPro>();
    //    _dummyWallLabel.fontSize = 1;
    //    _dummyWallLabel.alignment = TextAlignmentOptions.Center;
    //    _dummyWallLabels.Add(_dummyWallLabel);

    //}

    //private void SetupDummyWalls(List<Vector3> starts, List<Vector3> ends, Color[] colors, bool showLabel)
    //{
    //    if (starts != null)
    //    {
    //        for (int i = 0; i < starts.Count; i++)
    //        {
    //            if (i >= _dummyWalls.Count) CreateDummyWall();
    //            _dummyWall = _dummyWalls[i];
    //            _dummyWall.SetActive(true);
    //            _dummyWall.transform.position = (starts[i] + ends[i]) / 2;
    //            _dummyWall.transform.LookAt(starts[i]);
    //            _dummyWall.transform.localScale = new Vector3(0.06f, 0.06f, (starts[i] - ends[i]).magnitude);
    //            _dummyWall.transform.position += Vector3.up * 0.03f;
    //            _color = colors[i];
    //            _color.a = 0.7f;
    //            _dummyWall.GetComponent<MeshRenderer>().material.color = _color;

    //            _dummyWallLabel = _dummyWallLabels[i];
    //            _dummyWallLabel.gameObject.SetActive(showLabel);
    //            _dummyWallLabel.transform.position = _dummyWall.transform.position + Vector3.up * 0.15f;
    //            _dummyWallLabel.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
    //            _dummyWallLabel.transform.Rotate(Vector3.up * 180, Space.Self);
    //            _dummyWallLabel.text = "" + ((int)((starts[i] - ends[i]).magnitude * 100) / 100f) + "m";
    //        }
    //    }

    //    if (starts == null)
    //    {
    //        for (int i = 0; i < _dummyWalls.Count; i++)
    //        {
    //            _dummyWalls[i].SetActive(false);
    //            _dummyWallLabels[i].gameObject.SetActive(false);
    //        }
    //    }
    //    else if (starts.Count < _dummyWalls.Count)
    //    {
    //        for (int i=starts.Count; i<_dummyWalls.Count; i++)
    //        {
    //            _dummyWalls[i].SetActive(false);
    //            _dummyWallLabels[i].gameObject.SetActive(false);
    //        }

    //    }

    //}

    //private void TurnOffCornerTools()
    //{
    //    _dummyCorner1.SetActive(false);
    //    _dummyCorner2.SetActive(false);
    //    _dummyCorner3.SetActive(false);
    //    SetupDummyWalls(null, null, null, false);       
    //}

    //private void TurnOffArrow()
    //{
    //    if (Arrow != null) DestroyImmediate(Arrow);
    //    if (Cone != null) DestroyImmediate(Cone);
    //}

    private void CleanUp()
    {
/*        foreach (InteractiveRoom room in Rooms)
            Destroy(room.gameObject);
        Rooms.Clear();

        foreach (InteractiveCorner corner in Corners.Values)
            Destroy(corner.gameObject);
        Corners.Clear();*/

        for (int i = 0; i < SceneBuilderControl.ObjectContainer.transform.childCount; i++)
        {
            if (SceneBuilderControl.ObjectContainer.transform.GetChild(i).GetComponent<InteractiveShapeNetObject>() != null &&
                SceneBuilderControl.ObjectContainer.transform.GetChild(i).GetComponent<InteractiveShapeNetObject>().Entity.Panel != null)
                Destroy(SceneBuilderControl.ObjectContainer.transform.GetChild(i).GetComponent<InteractiveShapeNetObject>().Entity.Panel.gameObject);
            Destroy(SceneBuilderControl.ObjectContainer.transform.GetChild(i).gameObject);
        }
    }

    public void ResetBuilder()
    {
        SetEffectStatus(false);
        foreach (BuilderTab tab in BuilderTabs)
            tab.ResetTab();
        TabIndex = 0;
        if (BuilderTabs.Count > 0)
            TabButtons[0].OnClick();
        CleanUp();
    //    _roomWallsUpdated = false;
    //    CornerMode = Mode.Create;
    //    SelectedRoomIndex = (Rooms != null && Rooms.Count > 0) ? Rooms.Count - 1 : -1;
    //    RoomEditor = false;
    //    WallLength = false;
    
    //    TextureSpaceIndex = 0;
    }

    //public void SetupEnvironment()
    //{

    //    // set terrain size
    //    if (Ground == null)
    //    {
    //        Ground = GameObject.Find("Terrain").GetComponent<Terrain>();
    //        Ground.gameObject.layer = 19;
    //        _cornerContainer = new GameObject("CornerContainer");
    //        _cornerContainer.transform.parent = Ground.transform;
    //        _objectContainer = new GameObject("ObjectContainer");
    //        _objectContainer.transform.parent = Ground.transform;
    //        _dummyContainer = new GameObject("DummyContainer");
    //        _dummyContainer.transform.parent = Ground.transform;
    //        InteractiveGround = Ground.gameObject.AddComponent<InteractiveObject>();
    //        InteractiveGround.OnFocus = CheckPoint;
    //        InteractiveGround.OnPointer = CheckPoint;

    //        _dummyCorner0 = (GameObject)Instantiate(Resources.Load("Text2Scene/Prefabs/Corner"));
    //        _dummyCorner0.GetComponent<Renderer>().material.color = StolperwegeHelper.GUCOLOR.LICHTBLAU;
    //        MakeGhostObject(_dummyCorner0);
    //        DestroyImmediate(_dummyCorner0.GetComponent<InteractiveCorner>());
    //        DestroyImmediate(_dummyCorner0.GetComponent<Collider>());            
    //        _dummyCorner1 = Instantiate(_dummyCorner0);
    //        _dummyCorner2 = Instantiate(_dummyCorner0);
    //        _dummyCorner3 = Instantiate(_dummyCorner0);
    //        _dummyCorner0.transform.GetChild(0).GetComponent<TextMeshPro>().text = "0";
    //        _dummyCorner1.transform.GetChild(0).GetComponent<TextMeshPro>().text = "1";
    //        _dummyCorner2.transform.GetChild(0).GetComponent<TextMeshPro>().text = "2";
    //        _dummyCorner3.transform.GetChild(0).GetComponent<TextMeshPro>().text = "3";
    //        _dummyCorner0.SetActive(false);
    //        _dummyCorner1.SetActive(false);
    //        _dummyCorner2.SetActive(false);
    //        _dummyCorner3.SetActive(false);

    //    }
    //    Ground.terrainData.size = new Vector3(SurfaceSize, 0, SurfaceSize);
    //    Ground.transform.position = new Vector3(-SurfaceSize / 2f, 0, -SurfaceSize / 2f);
    //}

    //private void UpdateRoomTabGUI()
    //{
    //    RoomEditorLabel.color = (Rooms.Count > 0) ? Color.white : Color.gray;
    //    RoomEditorCheckbox.Active = Rooms.Count > 0;
    //    RoomEditorCheckbox.ButtonOn = RoomEditor;
    //    InteractiveGround.enabled = Rooms.Count > 0 && RoomEditor;
    //    CornerLabel.color = (Rooms.Count > 0 && RoomEditor) ? Color.white : Color.gray;
    //    CornerModeChanger.Active = Rooms.Count > 0 && RoomEditor;
    //    WallLengthLabel.color = (Rooms.Count > 0 && RoomEditor) ? Color.white : Color.gray;
    //    WallLengthCheckbox.Active = Rooms.Count > 0 && RoomEditor;
    //    WallHeightLabel.color = (SelectedRoom != null) ? Color.white : Color.gray;
    //    WallHeightField.inputField.color = (SelectedRoom != null) ? Color.white : Color.gray;
    //    WallHeightField.GetComponent<Collider>().enabled = SelectedRoom != null;
    //}

    //private IEnumerator LoadRooms()
    //{
    //    CleanUp();
    //    float height = 2.5f;        
    //    foreach (AnnotationRoomObject aRoom in LoadedDocument.RoomSet)
    //    {
    //        if (walls == null) walls = new List<AnnotationRoomWall>();
    //        else walls.Clear();
    //        LoadRoom(aRoom);
    //        roomToLoad = aRoom.Object3D.GetComponent<InteractiveRoom>();
    //        foreach (int id in aRoom.ObjectFeatures)
    //        {
    //            if (!LoadedDocument.Text_ID_Map.ContainsKey(id))
    //            {
    //                Debug.Log("ID not existing: " + id);
    //                continue;
    //            }
    //            data = LoadedDocument.Text_ID_Map[id];
    //            if (data is AnnotationRoomObjectAttribute)
    //            {
    //                attr = (AnnotationRoomObjectAttribute)data;
    //                if (attr.Key.Contains("Texture"))
    //                {
    //                    while (!SceneController.GetInterface<ShapeNetInterface>().Initialized) yield return null;
    //                    if (attr.Key.Equals("GroundTexture")) roomToLoad.LoadGroundTexture(attr);
    //                    if (attr.Key.Equals("WallTexture")) roomToLoad.LoadInnerWallTexture(attr);
    //                    if (attr.Key.Equals("OuterWallTexture")) roomToLoad.LoadOuterWallTexture(attr);
    //                    if (attr.Key.Equals("TopTexture")) roomToLoad.LoadTopTexture(attr);
    //                }
    //            }
    //            if (data is AnnotationRoomWall)
    //            {
    //                Debug.Log(data.ID);
    //                wall = (AnnotationRoomWall)data;
    //                walls.Add(wall);
    //                height = (float)wall.WallHeight;
    //            }                
    //        }
    //        walls.Sort(new AnnotationWallComparer());
    //        corners = new List<int>();
    //        for (int i=0; i<walls.Count; i++)
    //        {
    //            wall = walls[i];
    //            if (i == 0)
    //            {
    //                nextWall = walls[i + 1];
    //                if (wall.VectorList[1] == nextWall.VectorList[0] ||
    //                    wall.VectorList[1] == nextWall.VectorList[1])
    //                {
    //                    corners.Add(wall.VectorList[0]);
    //                    corners.Add(wall.VectorList[1]);
    //                }
    //                else
    //                {
    //                    corners.Add(wall.VectorList[1]);
    //                    corners.Add(wall.VectorList[0]);
    //                }
    //            }
    //            else
    //            {
    //                if (corners[0] == wall.VectorList[1])
    //                    corners.Add(wall.VectorList[0]);
    //                else
    //                    corners.Add(wall.VectorList[1]);
    //            }
    //        }
    //        for (int i = 0; i < corners.Count - 1; i++)
    //        {
    //            wall = walls[i];
    //            e1 = LoadCorner((AnnotationVector)LoadedDocument.Text_ID_Map[corners[i]], roomToLoad);
    //            e2 = LoadCorner((AnnotationVector)LoadedDocument.Text_ID_Map[corners[i + 1]], roomToLoad);
    //            iWall = (GameObject)Instantiate(Resources.Load("Text2Scene/Prefabs/Wall"));
    //            if (wall.VectorList[0] == e1.Vector.ID)
    //                iWall.GetComponent<InteractiveWall>().Initialize(e1, e2, wall, roomToLoad);
    //            else
    //                iWall.GetComponent<InteractiveWall>().Initialize(e2, e1, wall, roomToLoad);
    //            roomToLoad.AddWallToRoom(iWall.GetComponent<InteractiveWall>());
    //            foreach (int featureID in wall.Openings)
    //            {
    //                attr = (AnnotationRoomObjectAttribute)LoadedDocument.Text_ID_Map[featureID];
    //                if (attr.Key.Equals("Door") || attr.Key.Equals("Window"))
    //                    iWall.GetComponent<InteractiveWall>().LoadOpening(attr);
    //            }
    //        }
    //        roomToLoad.WallHeight = height;
    //    }
    //    foreach (AnnotationRoomObject rObject in LoadedDocument.ObjectSet)
    //    {
    //        if (!rObject.ShapeNetID.Equals("objectgroup"))
    //        {
    //            if (SceneController.GetInterface<ShapeNetInterface>().ShapeNetModels.ContainsKey(rObject.ShapeNetID))
    //            {
    //                if (!_generatedObjects.ContainsKey(rObject.ShapeNetID))
    //                    LoadModel(rObject.ShapeNetID, false);
    //                while (!_generatedObjects.ContainsKey(rObject.ShapeNetID))
    //                    yield return null;
    //            }
    //            else
    //            {
    //                Debug.Log("ERROR - Could not load object: " + rObject.ShapeNetID);
    //                continue;
    //            }
    //        }            
    //        CreateObject(rObject);
    //    }
    //    Debug.Log("========================================================");
    //    ResetConsole();
    //}

    public IEnumerator LoadingAnimation(TextMeshPro textField)
    {
        InterruptLoadingAnimation = false;
        textField.text = "\xf254";
        while (!InterruptLoadingAnimation)
        {
            textField.transform.Rotate(Vector3.forward * 2, Space.Self);
            yield return null;
        }
        textField.transform.localEulerAngles = Vector3.up * 180;
    }

    ///*************************************************************************************************************
    // *      ROOM HANDLING METHODS
    // *************************************************************************************************************/
    //Dictionary<string, object> roomFeatures; Dictionary<string, List<Dictionary<string, object>>> features;
    //List<string> _idsToRemove; bool _roomWallsUpdated;

    //private IEnumerator CheckClosingWall(OnClosingWallCreated onClosingWallCreated)
    //{
    //    if (SelectedRoom != null && SelectedRoom.GetClosingWall() == null && SelectedRoom.Corners.Count > 2)
    //    {
    //        SelectedRoom.CreateClosingWall();

    //        while (SelectedRoom.GetClosingWall() == null)
    //            yield return null;
    //    }
    //    onClosingWallCreated?.Invoke();
    //}

    //int rIndex = 0;
    //private void SendRoomCreatingRequest()
    //{            

    //    features = new Dictionary<string, List<Dictionary<string, object>>>();
    //    roomFeatures = new Dictionary<string, object>();

    //    roomFeatures.Add("name", "CustomRoom" + rIndex++);
    //    roomFeatures.Add("scale", 1);

    //    features.Add(TextAnnotatorClient.ROOM_OBJECT_TYPE, new List<Dictionary<string, object>>() { roomFeatures });
    //    SceneController.GetInterface<TextAnnotatorClient>().FireWorkBatchCommand(null, features, null, null);
    //}

    //public void CreateRoom(AnnotationRoomObject aRoom, bool setAsActive)
    //{
    //    _roomObject = new GameObject();
    //    _roomObject.transform.parent = Ground.transform;
    //    _room = _roomObject.AddComponent<InteractiveRoom>();
    //    _room.Initialize(this, aRoom);
    //    Rooms.Add(_room);
    //    _roomWallsUpdated = false;
    //    if (setAsActive)
    //    {
    //        if (!RoomEditor) RoomEditor = true;
    //        SelectedRoomIndex = Rooms.Count - 1;
    //        CornerMode = Mode.Create;
    //        CreateRoomBtn.Active = true;
    //        DeleteRoomBtn.Active = true;
    //    } else if (SelectedRoom != null)
    //        SelectedRoomIndex = Rooms.IndexOf(SelectedRoom);

    //}

    //private void LoadRoom(AnnotationRoomObject aRoom)
    //{
    //    _roomObject = new GameObject();
    //    _roomObject.transform.parent = Ground.transform;
    //    _room = _roomObject.AddComponent<InteractiveRoom>();
    //    _room.Initialize(this, aRoom);
    //    Rooms.Add(_room);
    //}

    //public void SendRoomDeletingRequest(AnnotationRoomObject room)
    //{

    //    if (room.Object3D != null) _room = room.Object3D.GetComponent<InteractiveRoom>();

    //    if (_idsToRemove == null) _idsToRemove = new List<string>();
    //    else _idsToRemove.Clear();
    //    _idsToRemove.Add("" + room.ID);

    //    foreach (InteractiveCorner corner in _room.Corners)
    //        if (corner.Rooms.Count == 1)
    //            _idsToRemove.Add("" + corner.Vector.ID);

    //    foreach (int attrID in room.ObjectFeatures)
    //        _idsToRemove.Add("" + attrID);

    //    foreach (InteractiveWall wall in _room.Walls)
    //    {
    //        foreach (int attrID in wall.Wall.Openings)
    //            _idsToRemove.Add("" + attrID);                
    //        _idsToRemove.Add("" + wall.Wall.ID);
    //    }

    //    if (LoadedDocument.Type_Map.ContainsKey(TextAnnotatorClient.ROOM_OBJECT_TYPE))
    //    {
    //        foreach (AnnotationRoomObject ro in LoadedDocument.Type_Map[TextAnnotatorClient.ROOM_OBJECT_TYPE])
    //        {
    //            if (ro.RoomID.Equals(room.ID))
    //                _idsToRemove.Add("" + ro.ID);
    //        }

    //    }

    //    SceneController.GetInterface<TextAnnotatorClient>().FireWorkBatchCommand(_idsToRemove, null, null, null);
    //}

    //public void RemoveRoom(InteractiveRoom room)
    //{
    //    _roomWallsUpdated = false;
    //    Rooms.Remove(room);
    //    SelectedRoomIndex = (SelectedRoomIndex == Rooms.Count) ? SelectedRoomIndex - 1 : SelectedRoomIndex;
    //    CreateRoomBtn.Active = true;
    //    DeleteRoomBtn.Active = SelectedRoomIndex != -1;
    //    if (SelectedRoomIndex == -1) UpdateRoomTabGUI();
    //}

    //public void UpdateRoomWalls()
    //{
    //    for (int i = 0; i < Rooms.Count; i++)
    //        StartCoroutine(Rooms[i].UpdateWalls());
    //    _roomWallsUpdated = true;

    //}

    //public void UpdateRooms()
    //{
    //    for (int i = 0; i < Rooms.Count; i++)
    //        Rooms[i].UpdateRoom();
    //}

    ///*************************************************************************************************************
    // *      CORNER HANDLING METHODS
    // *************************************************************************************************************/
    //GameObject _cornerObject; InteractiveCorner _corner, _prevCorner, _nextCorner; InteractiveCorner[] _corners; int index; 
    //Dictionary<string, object> _attrFeatures; List<Vector3> _cornersToCreate;

    //private IEnumerator CreateMultipleCorners(Vector3[] posArray)
    //{
    //    CreatingMultipleCorners = true;
    //    _corners = new InteractiveCorner[posArray.Length];
    //    _cornersToCreate = new List<Vector3>();
    //    for (int i=0; i<posArray.Length; i++)
    //        if (!Corners.ContainsKey(posArray[i]))
    //            _cornersToCreate.Add(posArray[i]);

    //    if (_cornersToCreate.Count > 0)
    //    {
    //        SendCornerCreatingRequest(_cornersToCreate);
    //        while (CornersToCreate.Count > 0)
    //            yield return null;
    //    }        

    //    for (int i = 0; i < posArray.Length; i++)
    //        _corners[i] = Corners[posArray[i]];

    //    SelectedRoom.AddCorners(_corners);
    //    CreatingMultipleCorners = false;
    //    TurnOffCornerTools();
    //}

    //private void SendCornerCreatingRequest(List<Vector3> positions)
    //{
    //    if (CornersToCreate.Count > 0) return;
    //    features = new Dictionary<string, List<Dictionary<string, object>>>();
    //    features.Add(TextAnnotatorClient.VECTOR_TYPE, new List<Dictionary<string, object>>());
    //    for (int i=0; i<positions.Count; i++)
    //    {
    //        CornersToCreate.Add(positions[i]);
    //        _attrFeatures = new Dictionary<string, object>();
    //        _attrFeatures.Add("x", (double)positions[i].x);
    //        _attrFeatures.Add("y", (double)positions[i].y);
    //        _attrFeatures.Add("z", (double)positions[i].z);
    //        features[TextAnnotatorClient.VECTOR_TYPE].Add(_attrFeatures);
    //    }

    //    SceneController.GetInterface<TextAnnotatorClient>().FireWorkBatchCommand(null, features, null, null);
    //}

    //public void CreateCorner(AnnotationVector vec, InteractiveRoom room = null)
    //{
    //    if (room == null) room = SelectedRoom;
    //    for (int i = 0; i < Rooms.Count; i++)
    //    {
    //        if (Rooms[i].Equals(room)) continue;
    //        foreach (InteractiveWall wall in Rooms[i].Walls)
    //            if (wall.IsCornerInWall(vec.Vector3D, out index))
    //            {
    //                InsertCorner(vec, Rooms[i], index);
    //                return;
    //            }
    //    }
    //    _cornerObject = (GameObject)Instantiate(Resources.Load("Text2Scene/Prefabs/Corner"));
    //    _corner = _cornerObject.GetComponent<InteractiveCorner>();
    //    _corner.Initialize(vec, room);
    //    Corners.Add(vec.Vector3D, _corner);
    //    _cornerObject.transform.SetParent(_cornerContainer.transform, true);        
    //    _cornerObject.GetComponent<Collider>().enabled = CornerMode != Mode.Create;
    //    if (!CreatingMultipleCorners) room.AddCorner(_corner);
    //    if (CornersToCreate.Contains(vec.Vector3D)) CornersToCreate.Remove(vec.Vector3D);
    //}

    //public InteractiveCorner LoadCorner(AnnotationVector vec, InteractiveRoom room)
    //{
    //    bool cornerInserted = false;
    //    _corner = null;
    //    foreach (InteractiveCorner e in Corners.Values)
    //        if (e.Vector.Equals(vec))
    //        {
    //            _corner = e;
    //            break;
    //        }

    //    if (_corner == null)
    //    {
    //        _cornerObject = (GameObject)Instantiate(Resources.Load("Text2Scene/Prefabs/Corner"));
    //        _corner = _cornerObject.GetComponent<InteractiveCorner>();
    //        for (int i = 0; i < Rooms.Count; i++)
    //        {
    //            if (Rooms[i].Equals(room)) continue;
    //            foreach (InteractiveWall wall in Rooms[i].Walls)
    //                if (wall.IsCornerInWall(vec.Vector3D, out index))
    //                {
    //                    _corner.Initialize(vec, Rooms[i], index);
    //                    _corner.AddToRoom(room, -1, false);
    //                    cornerInserted = true;
    //                }
    //        }
    //        if (!cornerInserted)
    //        {
    //            _corner.Initialize(vec, room);
    //        }
    //        Corners.Add(vec.Vector3D, _corner);
    //        _cornerObject.transform.SetParent(_cornerContainer.transform, true);
    //    } else
    //    {
    //        _corner.AddToRoom(room, -1, false);
    //    }
    //    if (!room.Corners.Contains(_corner))
    //    {
    //        room.Corners.Add(_corner);
    //        room.Positions.Add(_corner.Position);
    //    }
    //    return _corner;
    //}

    //public void ShareCorner(InteractiveCorner corner, InteractiveRoom room)
    //{
    //    // TODO meta message for sharing corner on other clients!!!!
    //    corner.AddToRoom(room, -1, true);
    //    SelectedRoom.AddCorner(corner);        
    //}

    //public void InsertCorner(AnnotationVector vector, InteractiveRoom room, int index)
    //{
    //    _cornerObject = (GameObject)Instantiate(Resources.Load("Text2Scene/Prefabs/Corner"));
    //    _corner = _cornerObject.GetComponent<InteractiveCorner>();
    //    _corner.Initialize(vector, room, index);
    //    Corners.Add(vector.Vector3D, _corner);
    //    _cornerObject.transform.SetParent(_cornerContainer.transform, true);
    //    ShareCorner(_corner, room);
    //}

    //public void RemoveCorner(InteractiveCorner corner, InteractiveRoom room)
    //{
    //    if (room != null)
    //    {
    //        corner.RemoveFromRoom(room);            
    //        if (!SceneController.GetInterface<TextAnnotatorClient>().MarkedForRemoving.Contains(room.Room.ID)) 
    //            room.RemoveCorner(corner);
    //        if (corner.Rooms.Count == 0)
    //        {
    //            CornerMode = Mode.Create;
    //            Corners.Remove(corner.Vector.Vector3D);
    //            Destroy(corner.gameObject);
    //        }
    //        else
    //            corner.ActualizeIndex();
    //    } else
    //    {
    //        Corners.Remove(corner.Vector.Vector3D);
    //        Destroy(corner.gameObject);
    //    }
    //}

    //public void UpdateCorners()
    //{
    //    for (int e = 0; e < _cornerContainer.transform.childCount; e++)
    //        _cornerContainer.transform.GetChild(e).gameObject.SetActive(RoomEditor);
    //}

    //private void OnTextureLoaded()
    //{
    //    TextureLoading.text = "";
    //    TextureLoading.transform.localRotation = Quaternion.identity;
    //    _triggerLocked = false;
    //    _interruptLoadingAnimation = true;
    //    ActualizeTextureControlFields();
    //}

    ///*************************************************************************************************************
    // *      ROOM-OBJECT HANDLING METHODS
    // *************************************************************************************************************/
    //PointFinger _pointingHand; SteamVR_Input_Sources _controlHand; SimpleGazeCursor _gaze; Vector3 _ghostObjectPos, _wallPosition, _dir, _rotAround, _pos;
    //Transform _hittedWall; float _wallWidth, _wallDepth, _objectWidth, _time; RaycastHit _hit; GameObject _objectInstance;
    //bool _snapRotationLocked, _snapScalingLocked, _left, _right, _triggerLocked; DataBrowserResource _data, _leftObject, _rightObject; 
    //string _id; TextMeshPro _activeSlot; DragFinger _leftHand, _rightHand; static MeshRenderer[] _renderers; static Color _color;
    //Color[] _colors; Color _wallColor0, _wallColor1; BoxCollider _collider;
    //public AnnotationVector _annoObjLocation, _annoObjRotation; public Vector3 _requestedLocation; public Quaternion _requestedRotation; 


    //private void EditDoor()
    //{
    //    _gaze = StolperwegeHelper.Gaze;
    //    _pointingHand = StolperwegeHelper.GetHandWithPointer();
    //    GhostDoor.SetActive(true);
    //    if (_pointingHand != null)
    //    {
    //        if (_pointingHand.HittedObject != null && _pointingHand.HittedObject.transform.IsChildOf(Ground.transform) && 
    //            _pointingHand.HittedObject.name.Contains("Wall") && ObjectFitsOnSurface(GhostDoor, _pointingHand.HittedObject))
    //        {
    //            SetOpeningPosition(GhostDoor, _pointingHand.Hit, 0);
    //            if (SteamVR_Actions.default_click.GetStateDown(SteamVR_Input_Sources.RightHand) && !ObjectRequestPending)
    //                _pointingHand.HittedObject.GetComponentInParent<InteractiveWall>().SendOpeningCreatingRequest("Door", _wallPosition, GhostDoor.transform.rotation);

    //        }
    //        else
    //            GhostDoor.SetActive(false);
    //        if (_pointingHand.HittedObject != null && _pointingHand.HittedObject.name.Contains("Door") && !ObjectRequestPending &&
    //            SteamVR_Actions.default_click.GetStateDown(SteamVR_Input_Sources.RightHand))
    //            _pointingHand.HittedObject.GetComponentInParent<InteractiveWall>().SendOpeningDeletingRequest(_pointingHand.HittedObject.transform.parent.gameObject);
    //    }
    //    else if (_gaze.HittedObject != null && _gaze.HittedObject.transform.IsChildOf(Ground.transform))
    //    {
    //        if (_gaze.HittedObject.name.Contains("Wall") && ObjectFitsOnSurface(GhostDoor, _gaze.HittedObject))
    //        {
    //            SetOpeningPosition(GhostDoor, _gaze.Hit, 0);
    //            if (SteamVR_Actions.default_click.GetStateDown(SteamVR_Input_Sources.RightHand) && !ObjectRequestPending)
    //                _gaze.HittedObject.GetComponentInParent<InteractiveWall>().SendOpeningCreatingRequest("Door", _wallPosition, GhostDoor.transform.rotation);
    //        }
    //        else
    //            GhostDoor.SetActive(false);
    //        if (_gaze.HittedObject.name.Contains("Door") && !ObjectRequestPending &&
    //            SteamVR_Actions.default_click.GetStateDown(SteamVR_Input_Sources.RightHand))
    //            _gaze.HittedObject.GetComponentInParent<InteractiveWall>().SendOpeningDeletingRequest(_gaze.HittedObject.transform.parent.gameObject);

    //    }
    //    else
    //        GhostDoor.SetActive(false);
    //}

    //private void EditWindow()
    //{
    //    _gaze = StolperwegeHelper.Gaze;
    //    _pointingHand = StolperwegeHelper.GetHandWithPointer();
    //    GhostWindow.SetActive(true);
    //    if (_pointingHand != null)
    //    {
    //        if (_pointingHand.HittedObject != null && _pointingHand.HittedObject.transform.IsChildOf(Ground.transform) &&
    //            _pointingHand.HittedObject.name.Contains("Wall") && ObjectFitsOnSurface(GhostWindow, _pointingHand.HittedObject))
    //        {
    //            SetOpeningPosition(GhostWindow, _pointingHand.Hit, 1);
    //            if (SteamVR_Actions.default_click.GetStateDown(SteamVR_Input_Sources.RightHand) && !ObjectRequestPending)
    //                _pointingHand.HittedObject.GetComponentInParent<InteractiveWall>().SendOpeningCreatingRequest("Window", _wallPosition, GhostWindow.transform.rotation);

    //        }
    //        else
    //            GhostWindow.SetActive(false);

    //        if (_pointingHand.HittedObject != null && _pointingHand.HittedObject.name.Contains("Window") && !ObjectRequestPending &&
    //            SteamVR_Actions.default_click.GetStateDown(SteamVR_Input_Sources.RightHand))
    //            _pointingHand.HittedObject.GetComponentInParent<InteractiveWall>().SendOpeningDeletingRequest(_pointingHand.HittedObject);           

    //    }
    //    else if (_gaze.HittedObject != null && _gaze.HittedObject.transform.IsChildOf(Ground.transform))
    //    {
    //        if (_gaze.HittedObject.name.Contains("Wall") && ObjectFitsOnSurface(GhostWindow, _gaze.HittedObject))
    //        {
    //            SetOpeningPosition(GhostWindow, _gaze.Hit, 1);
    //            if (SteamVR_Actions.default_click.GetStateDown(SteamVR_Input_Sources.RightHand) && !ObjectRequestPending)
    //                _gaze.HittedObject.GetComponentInParent<InteractiveWall>().SendOpeningCreatingRequest("Window", _wallPosition, GhostWindow.transform.rotation);

    //        }
    //        else
    //            GhostWindow.SetActive(false);

    //        if (_gaze.HittedObject.name.Contains("Window") && !ObjectRequestPending &&
    //           SteamVR_Actions.default_click.GetStateDown(SteamVR_Input_Sources.RightHand))
    //            _gaze.HittedObject.GetComponentInParent<InteractiveWall>().SendOpeningDeletingRequest(_gaze.HittedObject);

    //    }
    //    else
    //        GhostWindow.SetActive(false);
    //}

    //private void SetOpeningPosition(GameObject gameObject, RaycastHit hit, float height)
    //{
    //    _ghostObjectPos = hit.point;
    //    _hittedWall = hit.transform;
    //    _ghostObjectPos.y = _hittedWall.position.y;
    //    _wallWidth = Mathf.Max(_hittedWall.localScale.x, _hittedWall.localScale.z);
    //    _wallDepth = Mathf.Min(_hittedWall.localScale.x, _hittedWall.localScale.z);
    //    _objectWidth = gameObject.GetComponent<BoxCollider>().size.x;
    //    if (((_ghostObjectPos - _hittedWall.position).magnitude + _objectWidth / 2) > _wallWidth / 2)
    //    {
    //        _dir = (_ghostObjectPos - _hittedWall.position).normalized;
    //        _ghostObjectPos = _hittedWall.position + _dir * (_wallWidth / 2 - _objectWidth / 2);

    //    }
    //    _wallPosition = _hittedWall.parent.InverseTransformPoint(_ghostObjectPos);
    //    _wallPosition.y = SelectedRoom.transform.position.y + height;
    //    _wallPosition.z = 0;
    //    _ghostObjectPos.z += _wallDepth / 2;
    //    _ghostObjectPos.y = SelectedRoom.transform.position.y + height;
    //    gameObject.transform.forward = hit.normal;
    //    gameObject.transform.position = _ghostObjectPos;


    //}

    //private bool ObjectFitsOnSurface(GameObject objectToFit, GameObject wall)
    //{
    //    if (wall.transform.localScale.x > wall.transform.localScale.z)
    //        return objectToFit.GetComponent<BoxCollider>().size.x < wall.transform.localScale.x &&
    //               objectToFit.GetComponent<BoxCollider>().size.y < wall.transform.localScale.y;
    //    else
    //        return objectToFit.GetComponent<BoxCollider>().size.x < wall.transform.localScale.z &&
    //               objectToFit.GetComponent<BoxCollider>().size.y < wall.transform.localScale.y;
    //}

    //protected override void OnTriggerEnter(Collider other)
    //{
    //    base.OnTriggerEnter(other);
    //    if (!_triggerLocked && other.GetComponent<DataBrowserResource>() != null)
    //    {
    //        _data = other.GetComponent<DataBrowserResource>();
    //        if (_data.Data != null && _data.Data is ShapeNetObject)
    //        {
    //            ShapeNetObject shapeNetObject = (ShapeNetObject)_data.Data;
    //            _id = shapeNetObject.ID;
    //            if (ActiveTab == BuilderTab.Rooms && SelectedRoomIndex > -1 && shapeNetObject is ShapeNetTexture)
    //            {
    //                _triggerLocked = true;
    //                StartCoroutine(LoadingAnimation(TextureLoading));
    //                TextureSlot = (ShapeNetTexture)shapeNetObject;
    //                if (_data.transform.parent != null && _data.transform.parent.GetComponent<DragFinger>() != null)
    //                    _data.transform.parent.GetComponent<DragFinger>().setHapticFeedback(0.2f, 5);
    //            }
    //        }
    //    }
    //}

    ///*************************************************************************************************************
    // *      TEXTURE HANDLING METHODS
    // *************************************************************************************************************/

    //AnnotationRoomObjectAttribute _actualRoomTexture; Vector2 _actualOffset, _actualTiling; List<AnnotationBase> attributes; string attrName;
    //private void ActualizeTextureControlFields()
    //{
    //    //TextureThumbnail.material = (TextureSlot == null) ? StolperwegeHelper.ThumbnailMaterial : TextureSlot.Material;
    //    if (TextureSlot == null) TextureThumbnail.material = StolperwegeHelper.ThumbnailMaterial;
    //    else
    //    {
    //        if (TextureSlot.Material != null) TextureThumbnail.material = TextureSlot.Material;
    //        else StartCoroutine(TextureSlot.GetMaterial(this, (mat) => { TextureThumbnail.material = mat; }));
    //    }

    //    if (TextureSlot != null && TextureSlot.DataContainer != null) TextureSlot.DataContainer.ActualizeThumbnail();

    //    GetActualTextureData();
    //    TextureRemoveBtn.Active = SelectedRoom != null && TextureSlot != null && _actualRoomTexture != null && TextureSlot.ID.Equals(_actualRoomTexture.TextureID);
    //    TextureAcceptBtn.Active = SelectedRoom != null && (_actualRoomTexture == null && TextureSlot != null) || (_actualRoomTexture != null && TextureSlot != null && !TextureSlot.ID.Equals(_actualRoomTexture.ID)) || (_actualRoomTexture != null && TextureSlot == null);

    //    // tiling fields
    //    TilingXLabel.color = (TextureSlot != null) ? Color.white : Color.gray;
    //    TilingXField.GetComponent<Collider>().enabled = TextureSlot != null;
    //    TilingXField.Text = (TextureSlot != null) ? "" + _actualTiling.x : TilingXField.Description;
    //    TilingXField.inputField.color = (TextureSlot != null) ? Color.white : Color.gray;
    //    TilingYLabel.color = (TextureSlot != null) ? Color.white : Color.gray;
    //    TilingYField.GetComponent<Collider>().enabled = TextureSlot != null;
    //    TilingYField.Text = (TextureSlot != null) ? "" + _actualTiling.y : TilingYField.Description;
    //    TilingYField.inputField.color = (TextureSlot != null) ? Color.white : Color.gray;

    //    // offset fields
    //    OffsetXLabel.color = (TextureSlot != null) ? Color.white : Color.gray;
    //    OffsetXField.GetComponent<Collider>().enabled = TextureSlot != null;
    //    OffsetXField.Text = (TextureSlot != null) ? "" + _actualOffset.x : OffsetXField.Description;
    //    OffsetXField.inputField.color = (TextureSlot != null) ? Color.white : Color.gray;
    //    OffsetYLabel.color = (TextureSlot != null) ? Color.white : Color.gray;
    //    OffsetYField.GetComponent<Collider>().enabled = TextureSlot != null;
    //    OffsetYField.Text = (TextureSlot != null) ? "" + _actualOffset.y : OffsetYField.Description;
    //    OffsetYField.inputField.color = (TextureSlot != null) ? Color.white : Color.gray;
    //}

    //private void GetActualTextureData()
    //{
    //    _actualRoomTexture = (SelectedRoom == null) ? null : 
    //                         (ActualSpace == TextureSpace.Ground) ? SelectedRoom.GroundTexture : 
    //                         (ActualSpace == TextureSpace.Top) ? SelectedRoom.TopTexture : 
    //                         (ActualSpace == TextureSpace.Inner_Walls) ? SelectedRoom.InnerWallTexture : SelectedRoom.OuterWallTexture;
    //    _actualTiling = (SelectedRoom == null) ? Vector2.one : 
    //                    (ActualSpace == TextureSpace.Ground) ? SelectedRoom.GroundTextureTiling : 
    //                    (ActualSpace == TextureSpace.Top) ? SelectedRoom.TopTextureTiling :
    //                    (ActualSpace == TextureSpace.Inner_Walls) ? SelectedRoom.InnerWallTextureTiling : SelectedRoom.OuterWallTextureTiling;
    //    _actualOffset = (SelectedRoom == null) ? Vector2.zero : 
    //                    (ActualSpace == TextureSpace.Ground) ? SelectedRoom.GroundTextureOffset : 
    //                    (ActualSpace == TextureSpace.Top) ? SelectedRoom.TopTextureOffset :
    //                    (ActualSpace == TextureSpace.Inner_Walls) ? SelectedRoom.InnerWallTextureOffset : SelectedRoom.OuterWallTextureOffset;
    //}

    //private void ActualizeTexture()
    //{
    //    TextureAcceptBtn.Active = false;
    //    TextureRemoveBtn.Active = false;
    //    if (SelectedRoom == null) return;
    //    if (ActualSpace == TextureSpace.Ground) SelectedRoom.ChangeGroundTexture(TextureSlot, Tiling, Offset);
    //    if (ActualSpace == TextureSpace.Inner_Walls) SelectedRoom.ChangeInnerWallTexture(TextureSlot, Tiling, Offset);
    //    if (ActualSpace == TextureSpace.Outer_Walls) SelectedRoom.ChangeOuterWallTexture(TextureSlot, Tiling, Offset);
    //    if (ActualSpace == TextureSpace.Top) SelectedRoom.ChangeTopTexture(TextureSlot, Tiling, Offset);
    //    ActualizeTextureControlFields();
    //}

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StolperwegeHelper;
using Valve.VR;

public class SceneBuilderSceneScript : SceneScript
{

    private readonly int SERVER_RESPONSE_TIMEOUT = 10;

    private GameObject Plane;    

    private BoxCollider GroundCollider;
    private InteractiveObject InteractiveGround;

    public SceneBuilder SceneBuilder { get; private set; }

    public Dictionary<string, GameObject[]> LoadedModels { get; private set; }
    public GameObject ObjectContainer { get; private set; }

    private float _surfaceSize = 100;
    public float SurfaceSize
    {
        get { return _surfaceSize; }
        set
        {
            float newValue = Mathf.Min(1000, Mathf.Max(1, value));
            _surfaceSize = newValue;
            Plane.gameObject.transform.localScale = new Vector3(_surfaceSize / 10f, 0, _surfaceSize / 10f);
            GroundCollider.size = new Vector3(_surfaceSize, 0.01f, _surfaceSize);
            Plane.GetComponent<MeshRenderer>().material.mainTextureScale = Vector2.one * _surfaceSize;
        }
    }

    private Vector3 _lastEditorGridPoint;
    public Vector3 ActualEditorGridPoint { get; private set; }

    private static float _requestTimer;
    private static bool _waitingForResponse;
    public static bool WaitingForResponse
    {
        get { return _waitingForResponse; }
        set
        {
            _waitingForResponse = value;
            if (_waitingForResponse) _requestTimer = 0;
        }
    }

    public override void Initialize()
    {
        LoadedModels = new Dictionary<string, GameObject[]>();



        // create gridded ground
        GameObject ground = new GameObject("SceneBuilderGrid");
        ground.gameObject.layer = 19;
        GroundCollider = ground.AddComponent<BoxCollider>();
        GroundCollider.isTrigger = true;
        Plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Destroy(Plane.GetComponent<Collider>());
        Plane.transform.SetParent(ground.transform);
        Plane.transform.localPosition = Vector3.zero;
        //Plane.GetComponent<Renderer>().material = Instantiate(Resources.Load<Material>("Materials/SceneBuilderGrid"));
        Plane.GetComponent<Renderer>().material = Instantiate(Resources.Load<Material>("Materials/SceneBuilderGridDark"));

        InteractiveGround = ground.AddComponent<InteractiveObject>();
        InteractiveGround.UseHighlighting = false;
        InteractiveGround.SearchForParts = false;
        InteractiveGround.OnFocus = CheckPoint;
        InteractiveGround.OnPointer = CheckPoint;

        // set ground size
        SurfaceSize = _surfaceSize;

        // create container object for models
        ObjectContainer = new GameObject("ObjectContainer");
        ObjectContainer.transform.parent = InteractiveGround.transform;

        StartCoroutine(SetupPlayer());

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) || (SteamVR_Actions.default_right_action1.activeBinding &&
            SteamVR_Actions.default_right_action1.GetStateDown(SteamVR_Input_Sources.RightHand)))
            StartCoroutine(SceneController.GetInterface<TextAnnotatorInterface>().AutoLogin());
        if (WaitingForResponse)
        {
            _requestTimer += Time.deltaTime;
            if (_requestTimer >= SERVER_RESPONSE_TIMEOUT)
            {
                WaitingForResponse = false;
                StatusBox.Active = true;
                StatusBox.Components.LoadingBar.SetActive(false);
                StartCoroutine(StatusBox.SetInfoText("Server response timeout (" + SERVER_RESPONSE_TIMEOUT + "s)", true, 1.5f));
            }
                
        }
    }

    private IEnumerator SetupPlayer()
    {
        while (!SceneController.Initialized) yield return null;
        User.transform.position = Vector3.zero;
        while (OpenVR.System.GetTrackedDeviceActivityLevel(0) != 
            EDeviceActivityLevel.k_EDeviceActivityLevel_UserInteraction) 
            yield return null;
        SceneBuilder = ((GameObject)Instantiate(Resources.Load("Prefabs/SceneBuilder"))).GetComponent<SceneBuilder>();
        SceneBuilder.Initialize();

        Vector3 positionmodifier = CenterEyeAnchor.transform.forward;
        positionmodifier.y = 0f;
        SceneBuilder.transform.position = CenterEyeAnchor.transform.position + positionmodifier;
        SceneBuilder.transform.LookAt(CenterEyeAnchor.transform);
        SceneBuilder.SetInitialPosition(SceneBuilder.transform.position - User.transform.position);
    }

    private void CheckPoint(Vector3 hit)
    {

    //    if (!InteractiveGround.enabled || CreatingMultipleCorners) return;
        float xPos = (int)(hit.x * 10);
        int xRounder = (Mathf.Abs(xPos) % 10 <= 2) ? 0 : (Mathf.Abs(xPos) % 10 <= 7) ? 5 : 10;
        if (xPos < 0) xRounder *= -1;
        xPos = (xPos - (xPos % 10) + xRounder) / 10f;

        float zPos = (int)(hit.z * 10);
        int zRounder = (Mathf.Abs(zPos) % 10 <= 2) ? 0 : (Mathf.Abs(zPos) % 10 <= 7) ? 5 : 10;
        if (zPos < 0) zRounder *= -1;
        zPos = (zPos - (zPos % 10) + zRounder) / 10f;

        ActualEditorGridPoint = new Vector3(xPos, 0, zPos);

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

        _lastEditorGridPoint = ActualEditorGridPoint;
    }
}
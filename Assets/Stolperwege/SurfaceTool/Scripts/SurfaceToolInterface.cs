using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using TMPro;

public class SurfaceToolInterface : Interface
{
    private const float SURFACE_THICKNESS = 0.001f;
    private const float MIN_DRAW_SIZE = 0.02f;
    public SurfaceHUDMenu DrawUI { get; private set; }
    public SurfaceRecognizer SurfaceRecognizer { get; private set; }

    public enum SurfaceMode { None, Drawer, Recognizer, Connector };
    private SurfaceMode _surfaceTool = SurfaceMode.None;
    public SurfaceMode SurfaceTool
    {
        get { return _surfaceTool; }
        set
        {
            if (value == _surfaceTool) return;
            _surfaceTool = value;
        }
    }

    public bool CanDraw { get { return Active && SurfaceTool == SurfaceMode.Drawer && DrawMode != _DrawMode.None; } }
    public bool CanRecognizeSurface { get { return Active && SurfaceTool == SurfaceMode.Recognizer; } }
    public bool CanConnectSurface { get { return Active && SurfaceTool == SurfaceMode.Connector; } }

    //private bool _isDrawing;
    //public bool IsDrawing
    //{
    //    get { return _isDrawing; }
    //    set
    //    {
    //        _isDrawing = value;
    //        StolperwegeHelper.User.LeftTriggerBlocked = _isDrawing;
    //        StolperwegeHelper.User.RightTriggerBlocked = _isDrawing;
    //    }
    //}

    public enum _DrawMode { None, Finger, Pointer }
    private _DrawMode _drawMode;
    private bool _isDrawing;
    public _DrawMode DrawMode
    {
        get { return _drawMode; }
        set
        {
            _drawMode = value;
            DrawUI.ActualizeDrawerButtons();
            StolperwegeHelper.User.PointerSwitchBlocked = _drawMode != _DrawMode.None;
            StolperwegeHelper.User.LeftTriggerBlocked = _drawMode != _DrawMode.None;
            StolperwegeHelper.User.RightTriggerBlocked = _drawMode != _DrawMode.None;
            StolperwegeHelper.LeftFinger.DisableRay = _drawMode == _DrawMode.Finger;
            StolperwegeHelper.RightFinger.DisableRay = _drawMode == _DrawMode.Finger;
        }
    }

    //public enum _DrawType { Unknown, Free, Shape, Angular, Rectangular, Square, Circle };
    public enum _DrawType { Rectangular, Square, Ellipse, Circle };
    private _DrawType _drawType;
    public _DrawType DrawType
    {
        get { return _drawType; }
        set
        {
            _drawType = value;
            DrawUI.ActualizeDrawerButtons();
        }
    }

    private static Material _defaultObjectMaterial;
    public static Material DefaultObjectMaterial
    {
        get
        {
            if (_defaultObjectMaterial == null)
            {
                _defaultObjectMaterial = Instantiate(Resources.Load<Material>("Materials/DefaultObjectMaterial"));
                _defaultObjectMaterial.SetColor("_EmissionColor", InteractiveObject.EmissionColor * InteractiveObject.HighlightPower);                
            }                
            return _defaultObjectMaterial;
        }
    }

    private GameObject _squareDummy;
    private GameObject _circleDummy;
    private GameObject _actualObject;
    private Vector3 _drawStartPoint;
    private Vector3 _drawEndPoint;
    public InteractiveObject lastHit { get; private set; }
    public GameObject HittedObject { get; private set; }
    public Ray Ray { get; private set; }
    public RaycastHit Hit;

    private SteamVR_Input_Sources _drawHand;
    private SteamVR_Input_Sources DrawHand
    {
        get { return _drawHand; }
        set
        {
            _drawHand = value;
            ControlHand = _drawHand == SteamVR_Input_Sources.LeftHand ?
                SteamVR_Input_Sources.RightHand : SteamVR_Input_Sources.LeftHand;
        }
    }

    private SteamVR_Input_Sources ControlHand;

    //private FloatingStage _stageToMove;
    //private Vector3 stageMovDir;
    //private Vector3 stagePos;
    //private float stageMovSpeed;
    //public FloatingStage StageToMove
    //{
    //    get { return _stageToMove; }
    //    set
    //    {
    //        if (_stageToMove != null && (value == null ||
    //            _stageToMove != value))
    //            _stageToMove.Activate(false);

    //        if (_stageToMove == value) return;

    //        _stageToMove = value;
    //    }
    //}


    private bool _active;
    public bool Active
    {
        get { return _active; }
        set
        {
            if (value == _active) return;
            _active = value;
            DrawUI.gameObject.SetActive(_active);
            if (_active) DrawUI.transform.position = StolperwegeHelper.CenterEyeAnchor.transform.position + StolperwegeHelper.CenterEyeAnchor.transform.forward * 0.4f + Vector3.down * 0.2f;
        }
    }

    public GameObject SurfaceUI;
    protected override IEnumerator InitializeInternal()
    {
        Name = "SurfaceTool";
        CreateDummyObjects();

        yield return new WaitUntil(() => { return StolperwegeHelper.CenterEyeAnchor != null; });
        DrawUI = Instantiate(Resources.Load<GameObject>("Prefabs/SurfaceMenu")).GetComponent<SurfaceHUDMenu>();
        SurfaceRecognizer = DrawUI.GetComponentInChildren<SurfaceRecognizer>();
        DrawUI.Init();        
        DrawUI.transform.SetParent(StolperwegeHelper.User.transform, true);


        DrawUI.ActivateDrawerSubmenu(false);
        DrawUI.ActivateRecognizerSubmenu(false);
    
        DrawUI.gameObject.SetActive(_active);
    }

    void Update()
    {
        if (StolperwegeHelper.CenterEyeAnchor != null && SteamVR_Actions.default_right_action2.activeBinding &&
            SteamVR_Actions.default_right_action2.GetStateDown(SteamVR_Input_Sources.RightHand)) 
        {
            Active = !Active;
        }

        if (Active) DrawUI.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);

        if (CanDraw && !_isDrawing)
        {
            if (DrawMode == _DrawMode.Finger)
            {
                if (SteamVR_Actions.default_trigger.GetStateDown(SteamVR_Input_Sources.LeftHand))
                {
                    DrawHand = SteamVR_Input_Sources.LeftHand;
                    _drawStartPoint = StolperwegeHelper.LeftFinger.transform.position;
                    StartCoroutine(HandleDrawing());
                }
                else if (SteamVR_Actions.default_trigger.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    DrawHand = SteamVR_Input_Sources.RightHand;
                    _drawStartPoint = StolperwegeHelper.RightFinger.transform.position;
                    StartCoroutine(HandleDrawing());
                }
            }
            else if (DrawMode == _DrawMode.Pointer && StolperwegeHelper.User.PointerHand.HittedObject != null &&
                     SteamVR_Actions.default_trigger.GetStateDown(StolperwegeHelper.User.PointerHandType))
            {
                DrawHand = StolperwegeHelper.User.PointerHandType;
                _drawStartPoint = StolperwegeHelper.User.PointerHand.RaySphere.transform.position;
                StartCoroutine(HandleDrawing());
            }
        }
    }

    private IEnumerator HandleDrawing()
    {
        _isDrawing = true;
        _actualObject = (DrawType == _DrawType.Circle || DrawType == _DrawType.Ellipse) ? Instantiate(_circleDummy) : Instantiate(_squareDummy);
        _actualObject.GetComponentInChildren<Renderer>().material.EnableKeyword("_EMISSION");

        // For the purposes of the Certaintytool the object gets an identification script
        AnnotationObject anObj = _actualObject.transform.GetChild(0).gameObject?.AddComponent<AnnotationObject>();
        anObj.Type = (DrawType == _DrawType.Circle || DrawType == _DrawType.Ellipse) ? "Circle" : "Rect";

        Vector3 firstDirection = Vector3.zero, drawDirection, secondPoint;
        bool minSizeReached, firstDirLocked = false; Plane plane = new Plane();
        while (SteamVR_Actions.default_trigger.GetState(DrawHand))
        {
            if (SteamVR_Actions.default_grab.GetStateDown(DrawHand) ||
                SteamVR_Actions.default_grab.GetStateDown(ControlHand))
            {
                if (_actualObject != null)
                {
                    Destroy(_actualObject);
                    _actualObject = null;
                }
                break;
            }
            _drawEndPoint = DrawMode == _DrawMode.Pointer ? StolperwegeHelper.User.PointerHand.RaySphere.transform.position :
                            DrawHand == SteamVR_Input_Sources.LeftHand ? StolperwegeHelper.LeftFinger.transform.position : StolperwegeHelper.RightFinger.transform.position;
            drawDirection = _drawEndPoint - _drawStartPoint;
            minSizeReached = drawDirection.magnitude >= MIN_DRAW_SIZE;
            if (firstDirLocked && !minSizeReached) firstDirLocked = false;
            if (!firstDirLocked) firstDirection = drawDirection.normalized;
            if (!firstDirLocked && minSizeReached && SteamVR_Actions.default_trigger.GetStateDown(ControlHand))
                firstDirLocked = true;

            _actualObject.SetActive(minSizeReached && (DrawMode != _DrawMode.Pointer || StolperwegeHelper.User.PointerHand.HittedObject != null));
            if (_actualObject.activeInHierarchy)
            {
                Vector3 AB = _drawStartPoint + firstDirection * 1000;
                secondPoint = _drawStartPoint + Vector3.Dot(drawDirection, AB) / Vector3.Dot(AB, AB) * AB;
                if (DrawType == _DrawType.Circle || DrawType == _DrawType.Square)
                {
                    float min = Mathf.Min((secondPoint - _drawStartPoint).magnitude, (_drawEndPoint - secondPoint).magnitude);
                    _actualObject.transform.localScale = firstDirLocked ?
                        new Vector3(min, min, 1) :
                        new Vector3(drawDirection.magnitude, SURFACE_THICKNESS * 10, 1);

                    //if (DrawType == _DrawType.Circle)
                    //{
                    //    _actualObject.transform.localScale = firstDirLocked ?
                    //    new Vector3(min, 1, min) :
                    //    new Vector3(drawDirection.magnitude, SURFACE_THICKNESS * 10, 1);
                    //}
                    //else if (DrawType == _DrawType.Square)
                    //{
                    //    _actualObject.transform.localScale = firstDirLocked ?
                    //    new Vector3(min, min, 1) :
                    //    new Vector3(drawDirection.magnitude, SURFACE_THICKNESS * 10, 1);
                    //}
                }
                else
                {
                    _actualObject.transform.localScale = firstDirLocked ?
                        new Vector3((secondPoint - _drawStartPoint).magnitude, (_drawEndPoint - secondPoint).magnitude, 1) :
                        new Vector3(drawDirection.magnitude, SURFACE_THICKNESS * 10, 1);
                }
                Vector3 forward;
                if (DrawMode == _DrawMode.Finger)
                {
                    if (!firstDirLocked)
                        forward = (StolperwegeHelper.CenterEyeAnchor.transform.position - _drawStartPoint + drawDirection / 2).normalized;
                    else
                    {
                        plane.Set3Points(_drawStartPoint, secondPoint, _drawEndPoint);
                        forward = plane.normal;
                    }
                }
                else
                    forward = StolperwegeHelper.User.PointerHand.RayCastHit.normal;
                _actualObject.transform.position = _drawStartPoint + drawDirection / 2;
                _actualObject.transform.right = drawDirection;
                if (firstDirLocked) _actualObject.transform.forward = forward;
            }
            yield return null;
        }
        _isDrawing = false;
        DrawMode = _DrawMode.None;
        if (_actualObject != null) _actualObject.GetComponentInChildren<Renderer>().material.DisableKeyword("_EMISSION");

        // Connects created Annotations automatically
        Debug.LogWarning("at auto connect");
        CertaintyToolInterface CTInterface = GameObject.Find("CertaintyTool")?.GetComponent<CertaintyToolInterface>();
        if (CTInterface != null && (CTInterface.AutoConnectAnnos || true))
        {
            CTInterface.AutoConnectAnno(anObj);
            Debug.LogWarning("auto connected");
        }

    }

    public static void SetPointerColor(Vector4 color)
    {
        StolperwegeHelper.PointerPath.GetComponent<Renderer>().material.color = color;
        StolperwegeHelper.PointerSphere.GetComponent<Renderer>().material.color = color;
    }

    public bool HitsObject(InteractiveObject io)
    {
        return lastHit != null && lastHit.Equals(io);
    }

    private void CreateDummyObjects()
    {
        #region Circle

        //if (_circleDummy != null) Destroy(_circleDummy);
        //_circleDummy = new GameObject("CircleDummy");
        //GameObject subObject = new GameObject("CircleMesh");
        //subObject.transform.SetParent(_circleDummy.transform);
        //subObject.transform.localPosition = Vector3.zero;
        //MeshRenderer renderer = subObject.AddComponent<MeshRenderer>();
        //MeshFilter filter = subObject.AddComponent<MeshFilter>();
        //Mesh mesh = new Mesh();
        //mesh.name = "CircleMesh";
        //int slices = 32;

        //Vector3[] vertices = new Vector3[slices * 4 + 2];
        //vertices[0] = Vector3.forward * SURFACE_THICKNESS / 2;
        //vertices[slices + 1] = Vector3.back * SURFACE_THICKNESS / 2;
        //Vector3[] normals = new Vector3[vertices.Length];
        //normals[0] = Vector3.forward;
        //normals[slices + 1] = Vector3.back;
        //Vector2[] uvs = new Vector2[slices * 4 + 2];
        //uvs[0] = uvs[slices + 1] = Vector2.one * 0.5f;
        //Vector2 startUV = Vector2.up;
        //for (int i = 1; i < slices + 1; i++)
        //{
        //    vertices[i] = Quaternion.Euler(0, 0, (i - 1) * 360f / slices) * (vertices[0] + Vector3.up * 0.5f);
        //    vertices[i + slices + 1] = Quaternion.Euler(0, 0, (i - 1) * 360f / slices) * (vertices[slices + 1] + Vector3.up * 0.5f);
        //    vertices[i + slices * 2 + 1] = vertices[i];
        //    vertices[i + slices * 3 + 1] = vertices[i + slices + 1];
        //    normals[i] = Vector3.forward;
        //    normals[i + slices + 1] = Vector3.back;
        //    normals[i + slices * 2 + 1] = Vector3.up;
        //    normals[i + slices * 3 + 1] = Quaternion.Euler(0, 0, (i - 1) * 360f / slices) * Vector3.up;
        //    uvs[i] = (Quaternion.Euler(0, 0, -(i - 1) * 360f / slices) * startUV + Vector3.one) / 2;
        //    uvs[i + slices + 1] = (Quaternion.Euler(0, 0, (i - 1) * 360f / slices) * startUV + Vector3.one) / 2;
        //}

        //int[] triangles = new int[slices * 12];
        //int index = 0;
        //for (int i = 0; i < slices; i++)
        //{
        //    triangles[index] = 0;
        //    triangles[index + 1] = i + 1;
        //    triangles[index + 2] = (i + 2) > slices ? (i + 2) % slices : (i + 2);
        //    triangles[index + 3 * slices] = slices + 1;
        //    triangles[index + 3 * slices + 1] = triangles[index + 2] + (slices + 1);
        //    triangles[index + 3 * slices + 2] = triangles[index + 1] + (slices + 1);
        //    index += 3;
        //}

        //index = slices * 6;
        //for (int i = 1; i <= slices; i++)
        //{
        //    triangles[index] = i + slices * 2 + 1;
        //    triangles[index + 1] = i + slices * 3 + 1;
        //    triangles[index + 2] = i % slices + slices * 2 + 2;
        //    triangles[index + 3] = i % slices + slices * 2 + 2;
        //    triangles[index + 4] = i + slices * 3 + 1;
        //    triangles[index + 5] = i % slices + slices * 3 + 2;
        //    index += 6;
        //}

        //mesh.vertices = vertices;
        //mesh.normals = normals;
        //mesh.triangles = triangles;
        //mesh.uv = uvs;
        //filter.mesh = mesh;
        //renderer.material = Instantiate(DefaultObjectMaterial);
        //subObject.AddComponent<MeshCollider>();
        //_circleDummy.SetActive(false);

        if (_circleDummy != null) Destroy(_circleDummy);
        _circleDummy = new GameObject("CircleDummy");
        GameObject subObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        subObject.name = "CircleMesh";
        subObject.transform.SetParent(_circleDummy.transform);
        subObject.transform.localScale = new Vector3(1, SURFACE_THICKNESS, 1);
        subObject.transform.localPosition = Vector3.zero;
        subObject.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
        subObject.GetComponent<MeshRenderer>().material = Instantiate(DefaultObjectMaterial);
        Destroy(subObject.GetComponent<CapsuleCollider>());
        subObject.AddComponent<MeshCollider>();
        _circleDummy.SetActive(false);

        #endregion

        #region Rectangle

        if (_squareDummy != null) Destroy(_squareDummy);
        _squareDummy = new GameObject("SquareDummy");
        subObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        subObject.name = "SquareMesh";
        subObject.transform.SetParent(_squareDummy.transform);
        subObject.transform.localScale = new Vector3(1, 1, SURFACE_THICKNESS);
        subObject.transform.localPosition = Vector3.zero;
        subObject.GetComponent<MeshRenderer>().material = Instantiate(DefaultObjectMaterial);
        _squareDummy.SetActive(false);

        #endregion
    }
}
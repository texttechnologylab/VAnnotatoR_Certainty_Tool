using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using Valve.VR;
using UnityEngine.AI;

public class AvatarController : MonoBehaviour
{

    private ARDisplay ARDisplay;
    private bool IsDraggingARDisplay;

    private int _ltClickCounter; 
    private float _ltDoubleClickTimer;
    private LoginWindow _login;
    public LoginWindow Login
    {
        get
        {
            if (_login == null)
            {
                //_login = ((GameObject)Instantiate(Resources.Load("Prefabs/UI/Login"))).GetComponent<LoginWindow>();
                //_login.name = "Login";
                //_login.Init();
            }
            return _login;
        }
    }

    [SerializeField]
    public GameObject CenterEyeAnchor { get; private set; }

    [SerializeField]
    public GameObject LeftHand { get; private set; }

    [SerializeField]
    public GameObject RightHand { get; private set; }

    [HideInInspector]
    public bool LeftTriggerBlocked;

    [HideInInspector]
    public bool RightTriggerBlocked;

    [HideInInspector]
    public bool TeleportBlocked;

    [HideInInspector]
    public bool MovementBlocked;

    [HideInInspector]
    public bool RotationBlocked;

    [HideInInspector]
    public bool PointerSwitchBlocked;

    [HideInInspector]
    public bool ActionBlocked;

    [HideInInspector]
    public GameObject Head { get; private set; }

    public HashSet<InteractiveObject> DroppedObjects { get; private set; }

    private CapsuleCollider Collider;
    private float FallSpeed = 0.0f;
    private float MaxSlope { get { return Collider.height / 2 * 1.2f; } }

    /// <summary>
    /// The speed of the character.
    /// </summary>
    [SerializeField]
    public float Speed = 3f;

    /// <summary>
    /// The rate of damping on movement.
    /// </summary>
    [SerializeField]
    public float Damping = 0.3f;

    /// <summary>
    /// The rate of additional damping when moving sideways or backwards.
    /// </summary>
    [SerializeField]
    public float BackAndSideDampen = 0.5f;

    /// <summary>
    /// The time for seconds that switching the pointer between the hands should take.
    /// </summary>
    [SerializeField]
    public float PointerActivationTime = 1f;

    /// <summary>
    /// The minimum step size (m).
    /// </summary>
    [SerializeField]
    public float StepOffset = 1.5f;


    /// <summary>
    /// Modifies the strength of gravity.
    /// </summary>
    [SerializeField]
    public float GravityModifier = 0.379f;

    public PointFinger PointerHand;
    
    public delegate void StepTrigger(GameObject collider);
    public StepTrigger OnStepIntoObject;


    public SteamVR_Input_Sources PointerHandType
    { 
        get { return PointerHand.LeftHand ? 
                SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand; } 
    }

    public SteamVR_Input_Sources ControlHandType
    {
        get
        {
            return PointerHand.RightHand ?
              SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
        }
    }

    // Use this for initialization
    void Awake()
    {
        Collider = GetComponent<CapsuleCollider>();
        SetupAvatar();
        DroppedObjects = new HashSet<InteractiveObject>();
        //userPref = StolperwegeHelper.player.gameObject.GetComponent<preferences>();

    }

    public void SetupAvatar()
    {
        CenterEyeAnchor = transform.Find("CenterEyeAnchor").gameObject;        
        LeftHand = transform.Find("LeftHand").gameObject;
        RightHand = transform.Find("RightHand").gameObject;
        StolperwegeHelper.User = this;
        StolperwegeHelper.CenterEyeAnchor = CenterEyeAnchor;
        StolperwegeHelper.LeftHand = LeftHand;
        StolperwegeHelper.RightHand = RightHand;
        StolperwegeHelper.LeftHandAnim = LeftHand.GetComponent<HandAnimator>();
        StolperwegeHelper.RightHandAnim = RightHand.GetComponent<HandAnimator>();
        StolperwegeHelper.LeftFinger = LeftHand.GetComponentInChildren<PointFinger>();
        StolperwegeHelper.RightFinger = RightHand.GetComponentInChildren<PointFinger>();
        StolperwegeHelper.PointerPath = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        StolperwegeHelper.PointerPath.name = "PointerPath";
        StolperwegeHelper.PointerPath.GetComponent<Collider>().enabled = false;
        StolperwegeHelper.PointerPath.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        StolperwegeHelper.PointerPath.GetComponent<MeshRenderer>().material = (Material)Instantiate(Resources.Load("Materials/UI/Pointer"));
        StolperwegeHelper.PointerPath.gameObject.SetActive(false);
        StolperwegeHelper.PointerPath.transform.parent = transform;
        StolperwegeHelper.PointerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        StolperwegeHelper.PointerSphere.name = "PointerSphere";
        StolperwegeHelper.PointerSphere.transform.localScale = Vector3.one * 0.026f;
        StolperwegeHelper.PointerSphere.layer = 2;
        StolperwegeHelper.PointerSphere.GetComponent<Collider>().isTrigger = true;
        StolperwegeHelper.PointerSphere.AddComponent<Rigidbody>();
        StolperwegeHelper.PointerSphere.GetComponent<Rigidbody>().isKinematic = true;
        StolperwegeHelper.PointerSphere.GetComponent<Rigidbody>().useGravity = false;
        StolperwegeHelper.PointerSphere.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        StolperwegeHelper.PointerSphere.GetComponent<MeshRenderer>().material = (Material)Instantiate(Resources.Load("Materials/UI/Pointer"));
        StolperwegeHelper.PointerSphere.gameObject.SetActive(false);
        StolperwegeHelper.PointerSphere.transform.parent = transform;
        PointerHand = (LocalCacheHandler.GetDefaultPointerSide() == SteamVR_Input_Sources.RightHand) ? StolperwegeHelper.RightFinger : StolperwegeHelper.LeftFinger;
        Head = CenterEyeAnchor.transform.Find("Head").gameObject;
        Head.SetActive(false);
    }

    private float _rtPointerSwitchTime, _ltPointerSwitchTime;
    // Update is called once per frame
    void Update()
    {
        if (DroppedObjects != null && DroppedObjects.Count > 0) HandleDroppedObjects();

        UpdateMovement();
        UpdateOrientation();


        // Handle Login Window
        if (SteamVR_Actions.default_left_action2.activeBinding)
        {
            if (SteamVR_Actions.default_left_action2.GetStateDown(SteamVR_Input_Sources.LeftHand))
                Login.Active = !Login.Active;
        }
        else
        {
            if (!RightTriggerBlocked && StolperwegeHelper.RightFist.GrabedObject == null &&
                 SteamVR_Actions.default_trigger.GetStateDown(SteamVR_Input_Sources.RightHand)) 
            {
                if (Login.InterfaceCount > 0)
                {
                    _ltClickCounter += 1;
                    if (_ltClickCounter == 2 && _ltDoubleClickTimer < 0.5f)
                    {
                        Login.Active = !Login.Active;
                        _ltClickCounter = 0;
                        _ltDoubleClickTimer = 0;
                    }
                }
                else Debug.Log("No login space");
            }

            if (_ltClickCounter == 1)
            {
                _ltDoubleClickTimer += Time.deltaTime;
                if (_ltDoubleClickTimer >= 0.5f)
                {
                    _ltClickCounter = 0;
                    _ltDoubleClickTimer = 0;
                }
            }

        }

        // Handle pointer

        // empty loading bar if the pointer was not fully changed to the right and the right-trigger is not pressed
        if (_rtPointerSwitchTime > 0 && !SteamVR_Actions.default_trigger.GetState(SteamVR_Input_Sources.RightHand))
        {
            _rtPointerSwitchTime -= Time.deltaTime;
            StolperwegeHelper.StatusBox.SetLoadingStatus(_rtPointerSwitchTime, PointerActivationTime);
            if (_rtPointerSwitchTime <= 0)
            {
                _rtPointerSwitchTime = 0;
                StolperwegeHelper.StatusBox.Reset();
            }
        }

        // empty loading bar if the pointer was not fully changed to the left and the left-trigger is not pressed
        if (_ltPointerSwitchTime > 0 && !SteamVR_Actions.default_trigger.GetState(SteamVR_Input_Sources.LeftHand))
        {
            _ltPointerSwitchTime -= Time.deltaTime;
            StolperwegeHelper.StatusBox.SetLoadingStatus(_ltPointerSwitchTime, PointerActivationTime);
            if (_ltPointerSwitchTime <= 0)
            {
                _ltPointerSwitchTime = 0;
                StolperwegeHelper.StatusBox.Reset();
            }
        }

        if (!PointerSwitchBlocked)
        {
            if (SteamVR_Actions.default_trigger.GetState(SteamVR_Input_Sources.RightHand) &&
            PointerHand.LeftHand && StolperwegeHelper.RightHandAnim.IsPointing)
            {
                _rtPointerSwitchTime += Time.deltaTime;
                if (!StolperwegeHelper.StatusBox.Active)
                {
                    StolperwegeHelper.StatusBox.Active = true;
                    StartCoroutine(StolperwegeHelper.StatusBox.SetInfoText("Switching pointer to right...", false));
                }
                StolperwegeHelper.StatusBox.SetLoadingStatus(_rtPointerSwitchTime, PointerActivationTime);
                if (_rtPointerSwitchTime >= PointerActivationTime)
                {
                    _rtPointerSwitchTime = 0;
                    PointerHand = StolperwegeHelper.RightFinger;
                    StolperwegeHelper.Smartwatch.SwitchToControlHand();
                    StolperwegeHelper.StatusBox.Reset();
                }
            }

            if (SteamVR_Actions.default_trigger.GetState(SteamVR_Input_Sources.LeftHand) &&
                PointerHand.RightHand && StolperwegeHelper.LeftHandAnim.IsPointing)
            {
                _ltPointerSwitchTime += Time.deltaTime;
                if (!StolperwegeHelper.StatusBox.Active)
                {
                    StolperwegeHelper.StatusBox.Active = true;
                    StartCoroutine(StolperwegeHelper.StatusBox.SetInfoText("Switching pointer to left...", false));
                }
                StolperwegeHelper.StatusBox.SetLoadingStatus(_ltPointerSwitchTime, PointerActivationTime);
                if (_ltPointerSwitchTime >= PointerActivationTime)
                {
                    _ltPointerSwitchTime = 0;
                    PointerHand = StolperwegeHelper.LeftFinger;
                    StolperwegeHelper.Smartwatch.SwitchToControlHand();
                    StolperwegeHelper.StatusBox.Reset();
                }
            }
        }
        

        if (IsDraggingARDisplay)
        {
            if (!StolperwegeHelper.RightFinger.IsPointing ||
                !StolperwegeHelper.LeftFinger.IsPointing)
            {
                if (ARDisplay != null) ARDisplay.Resize(Mathf.Max(ARDisplay.MinWidth, ARDisplay.Width),
                                                        Mathf.Max(ARDisplay.MinHeight, ARDisplay.Height));
                ARDisplay = null;
                IsDraggingARDisplay = false;
            }
            else
            {
                Vector3 point1 = leftFinger.transform.position;
                point1.y = rightFinger.transform.position.y;
                ARDisplay.Resize((rightFinger.transform.position - point1).magnitude, (leftFinger.transform.position - point1).magnitude);
                ARDisplay.transform.position = (leftFinger.transform.position + rightFinger.transform.position) / 2;
                ARDisplay.transform.forward = new Plane(leftFinger.transform.position, point1, rightFinger.transform.position).normal;
            }

        }

    }

    private bool _rightcenter;
    private void UpdateOrientation()
    {
        if (StolperwegeHelper.RadialMenu.Visible ||
            (StolperwegeHelper.User.PointerHand.Hit != null && !StolperwegeHelper.User.PointerHand.Hit.name.Equals("SceneBuilderGrid")) ||
            RotationBlocked)
            return;
        Vector2 primaryAxis = SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand);
        if (Vector2.Distance(primaryAxis, Vector2.zero) < 0.1f) _rightcenter = true;
        if (_rightcenter)
        {
            
            if (primaryAxis.x >= 0.5f)
            {
                _rightcenter = false;
                transform.RotateAround(transform.position, transform.up, 22.5f);
            }
            if (primaryAxis.x <= -0.5f)
            {
                _rightcenter = false;
                transform.RotateAround(transform.position, transform.up, -22.5f);
            }
        }
    }

    private void UpdateMovement()
    {
        Vector2 primaryAxis = SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.LeftHand);
        Quaternion headRot = StolperwegeHelper.CenterEyeAnchor.transform.rotation;

        Vector3 movement = Vector3.zero;

        FallSpeed = Physics.gravity.y * GravityModifier;

        if (!MovementBlocked)
        {
            if (IsGrounded())
            {
                bool moved = primaryAxis.y != 0 || primaryAxis.x != 0;
                if (primaryAxis.y != 0)
                    movement += headRot * Vector3.forward * primaryAxis.y * (Speed * Time.deltaTime);
                if (primaryAxis.x != 0)
                    movement += headRot * Vector3.right * primaryAxis.x * (Speed * Time.deltaTime) * BackAndSideDampen;
                float speedScaler = 1;
                if (moved && movement.magnitude < (StepOffset * Time.deltaTime))
                    speedScaler = (StepOffset * Time.deltaTime) / movement.magnitude;
                movement = Vector3.Scale(movement, new Vector3(speedScaler, 0, speedScaler));
            }
            else
                movement.y += FallSpeed * Time.deltaTime;
            transform.position += movement;
        }
        

        
    }

    List<InteractiveObject> NotDropping = new List<InteractiveObject>();
    private void HandleDroppedObjects()
    {
        foreach (InteractiveObject dropped in DroppedObjects)
        {
            if (dropped.isThrowable)
            {
                dropped.InTheAir += Time.deltaTime;
                if (dropped.IsFlying)
                    dropped.CalculateFlying();


                if (!dropped.IsGrabbed && dropped.InTheAir > 0)
                    dropped.CalculateHovering();
                else
                {
                    dropped.InTheAir = 0;
                    NotDropping.Add(dropped);
                }
            }
            else
                NotDropping.Add(dropped);
        }

        foreach (InteractiveObject notDrop in NotDropping)
            DroppedObjects.Remove(notDrop);

        NotDropping.Clear();
    }

    public void SetTriggerBlockerStatus(bool status)
    {
        LeftTriggerBlocked = status;
        RightTriggerBlocked = status;
    }

    public void CallLogin(Interface iFace)
    {
        Login.Init(iFace);
        if (!Login.Active) Login.Active = true;
    }

    public void Teleport(Vector3 pos, float maxDistance)
    {
        NavMeshHit navHit;
        if (!TeleportBlocked && NavMesh.SamplePosition(pos, out navHit, maxDistance, NavMesh.AllAreas))
            transform.position += pos - transform.position;
    }

    private PointFinger leftFinger, rightFinger;
    public void CreateARDisplay()
    {
        if (ARDisplay != null || IsDraggingARDisplay) return;
        if (leftFinger == null) leftFinger = StolperwegeHelper.LeftFinger;
        if (rightFinger == null) rightFinger = StolperwegeHelper.RightFinger;
        ARDisplay = new GameObject().AddComponent<ARDisplay>();
        ARDisplay.Initialize();
        IsDraggingARDisplay = true;
    }
    
    GameObject _lastGroundObject;
    public bool IsGrounded()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position + Vector3.up * Collider.height / 2, Vector3.down);
        Physics.Raycast(ray, out hit, Collider.height / 2 * MaxSlope);
        if (hit.collider != null && hit.collider.gameObject != null)
        {
            if (!hit.collider.gameObject.Equals(_lastGroundObject))
                OnStepIntoObject?.Invoke(hit.collider.gameObject);
            _lastGroundObject = hit.collider.gameObject;
        }
        return hit.collider != null;
    }
}

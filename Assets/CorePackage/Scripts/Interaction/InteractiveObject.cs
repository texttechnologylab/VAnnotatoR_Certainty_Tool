using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static StolperwegeHelper;
using Valve.VR;
/*  Fügt Objekten die Interaktionsmöglichkeiten Greifen und Pointer-Aktionen hinzu
* 
* */

public class InteractiveObject : MonoBehaviour {

    /************************************************************************************
    ENUMS
    ************************************************************************************/
    public enum CustomType { None, UserDefined, Portal, POBox, InfoSurface, FloatingStage }

    /************************************************************************************
    EDITABLE VARIABLES
    ************************************************************************************/
    
    public float LongClickTime = 2f;
    public float ClickTime = 0.3f;
    public static float HighlightPower = 0.75f;
    public bool Grabable;
    public bool ManipulatedByGravity = true;
    public bool Removable;
    public bool DestroyOnObjectRemover;
    public bool isThrowable;
    public Vector4 mainColor;
    public Vector3 OriginalLocalPosition;
    public Quaternion OriginalLocalRotation;
    public string InfoText;    
    public string LoadingText;
    public bool UseHighlighting = true;
    public bool TriggerOnFocus = true;
    public bool LookAtUserOnHold;
    public bool BlockRotationOnFocus;
    public bool BlockRotationOnPointer;

    /************************************************************************************
    NON-EDITABLE VARIABLES
    ************************************************************************************/

    [HideInInspector]
    public bool SearchForParts = true;
    [HideInInspector]
    public List<Renderer> PartsToHighlight = new List<Renderer>();
    public static Color EmissionColor = new Color(0, 0.627451f, 5.992157f) * 3;
    protected bool KeepXRotation = false;
    protected bool KeepYRotation = false;
    protected bool KeepZRotation = false;

    public bool SupportsLongClick { get { return OnLongClick != null; } }
    public bool SupportsDistanceGrab { get { return OnDistanceGrab != null; } }
    public bool HasInfoText { get { return InfoText != null && InfoText != ""; } }  
    public GameObject ParentObject { get; private set; }
    [HideInInspector]
    public float InTheAir;
    public bool Released { get; protected set; }
    public bool IsFlying { get; protected set; }
    public bool InteractsWithTrashcan { get; protected set; }
    protected float LongClickTimer = 0;
    public bool PointerTriggered { get; protected set; } = false;
    [HideInInspector]
    protected Collider pointercollider;
    [HideInInspector]
    public CustomType Type;
    [HideInInspector]
    public bool StatusBoxTriggered;
    [HideInInspector]
    public Orientation ObjectOrientation;
    public readonly Blackboard Blackboard = new Blackboard();


    /************************************************************************************
   FOR INTERN CALCULATIONS
   ************************************************************************************/

    // Throwing
    protected Vector3 _normalScale;
    private Vector3 _throwingVector;
    private Vector3 _throwingDirection;
    private float _throwingEnergy = 0;
    private Vector3 _handPositionLastFrame;

    private bool _clicked = false;
    private bool _longClicked = false;

    private bool _inBackpack = false;
    private bool _comparisonDone;

    protected int _lastScrollDir = 0;

    /************************************************************************************
    DELEGATES
    ************************************************************************************/

    public delegate void ActionOnClick();
    public ActionOnClick OnClick;

    public delegate bool ActionBeforeLongClick();
    public ActionBeforeLongClick ExecuteBeforeLongClick;

    public delegate void ActionOnLongClick();
    public ActionOnLongClick OnLongClick;

    public delegate IEnumerator AsyncActionOnClick();
    public AsyncActionOnClick AsyncClick;

    public delegate IEnumerator AsyncActionOnLongClick();
    public AsyncActionOnLongClick AsyncLongClick;

    public delegate IEnumerator ActionOnDistanceGrab();
    public ActionOnDistanceGrab OnDistanceGrab;

    public delegate void ActionOnScroll(int _scrollDir);
    public ActionOnScroll OnVerticalScroll;
    public ActionOnScroll OnHorizontalScroll;

    public delegate void ActionOnFocus(Vector3 hitPoint);
    public ActionOnFocus OnFocus;

    public delegate void ActionOnPointer(Vector3 hitPoint);
    public ActionOnPointer OnPointer;

    public delegate void ActionOnHold();
    public ActionOnHold OnHold;

    protected delegate void ActionOnRemove();
    protected ActionOnRemove OnRemove;

    public delegate void InteractiveObjectEvent(InteractiveObject obj);
    public event InteractiveObjectEvent OnGrabed;    

    /************************************************************************************
    PROPERTIES
    ************************************************************************************/

  
    private ArmColliderArray _grabbing;
    public ArmColliderArray Grabbing
    {
        get
        {
            if (_grabbing == null)
                _grabbing = new ArmColliderArray();
            return _grabbing;
        }
    }

    public bool IsCollectable = false;
    protected Vector3 _scaleMultiplier = Vector3.one;
    public virtual Vector3 ScaleMultiplier
    {
        get
        {
            return _scaleMultiplier;
        }

        set
        {
            _scaleMultiplier = value;

            //Vector3 scale = (Grabbing.Count == 1 && SnapHand) ? UniqueScale : new Vector3(NormalScale.x, NormalScale.y, NormalScale.z);
            Vector3 scale = _normalScale;
            scale.x *= _scaleMultiplier.x;
            scale.z *= _scaleMultiplier.z;
            scale.y *= _scaleMultiplier.y;
            transform.localScale = scale;
        }
    }

    public bool IsGrabbed
    {
        get
        {
            return (Grabbing.Get != null);
        }
    }

    protected bool _highlight = false;
    public virtual bool Highlight
    {
        get
        {
            return _highlight;
        }
        set
        {            
            if (value == _highlight) return;
            _highlight = value;
            if (UseHighlighting) SetHighlight();
        }
    }

    private bool _transparent = false;
    public bool Transparent
    {
        get
        {
            return _transparent;
        }

        set
        {
            _transparent = value;
            
            string mode = (_transparent) ? "Fade" : "Opaque";
            float alpha = (_transparent) ? 0.3f : 1f;

            MeshRenderer[] renderer = GetComponents<MeshRenderer>();

            foreach (MeshRenderer mr in renderer)
            {
                foreach (Material m in mr.materials)
                {
                    Color c = m.color;
                    c.a = alpha;
                    m.color = c;

                    ChangeBlendMode(m, mode);
                }
            }
        }
    }

    protected Transform Player
    {
        get { return User.transform; }
    }

    protected InventoryController Backpack
    {
        get { return Inventory; }
    }

    public DragFinger GrabHand
    {
        get
        {
            if (Grabbing.Count != 1) return null;
            return Grabbing.Get.GetComponent<DragFinger>();
        }
    }
    

    /************************************************************************************
    STRUCTS AND CLASSES
    ************************************************************************************/

    /// <summary>
    /// Manager für ??
    /// </summary>
    public class ArmColliderArray
    {
        public Collider LeftArm { get; private set; }
        public Collider RightArm { get; private set; }

        public ArmColliderArray()
        {
            LeftArm = null;
            RightArm = null;
        }

        public Collider Get
        {
            get
            {
                return LeftArm ?? RightArm;
            }
        }

        public void Add(Collider other)
        {
            if (other.tag.Contains("left")) LeftArm = other;
            else RightArm = other;
        }

        public void Remove(Collider other)
        {
            if (other.tag.Contains("left")) LeftArm = null;
            else RightArm = null;
        }

        public int Count
        {
            get
            {
                int i = (LeftArm == null) ? 0 : 1;
                if (RightArm != null) i += 1;
                return i;
            }

        }

        public void Clear()
        {
            LeftArm = null;
            RightArm = null;
        }
    }

    public class Orientation
    {
        public InteractiveObject Object { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        public Orientation(InteractiveObject obj, Vector3 pos, Quaternion rot)
        {
            Object = obj;
            Position = pos;
            Rotation = rot;
        }

        public void ResetOrientation()
        {
            Object.transform.position = Position;
            Object.transform.rotation = Rotation;
        }

        public bool OrientationChanged()
        {
            return Object.transform.position != Position ||
                   Object.transform.rotation != Rotation;
        }
    }

    /************************************************************************************
    METHODS
    ************************************************************************************/

    public virtual void Awake()
    {
        _normalScale = transform.localScale;
        OriginalLocalRotation = (OriginalLocalRotation.x == 0 && OriginalLocalRotation.y == 0 && OriginalLocalRotation.z == 0 && OriginalLocalRotation.w == 0) ? transform.localRotation : OriginalLocalRotation;
        OriginalLocalPosition = (OriginalLocalPosition.x == 0 && OriginalLocalPosition.y == 0 && OriginalLocalPosition.z == 0) ? transform.localPosition : OriginalLocalPosition;
        OnFocus = CheckScrolling;
        OnPointer = CheckScrolling;
        OnHold = LookAtUser;
    }

    // Use this for initialization
    public virtual void Start() {
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().useGravity = ManipulatedByGravity;
            GetComponent<Rigidbody>().isKinematic = true;
        }
        
        if (SearchForParts)
        {            
            Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
            PartsToHighlight = new List<Renderer>();
            foreach (Renderer r in renderers)
                if (r.gameObject.GetComponent<TextMeshPro>() == null &&
                    r.gameObject.GetComponent<TMP_SubMesh>() == null)
                    PartsToHighlight.Add(r);
        }

        ChangeColor("_EmissionColor", EmissionColor * HighlightPower);
        SetHighlight();
    }

    protected PointFinger _pointerHand;
    public virtual void CheckClick()
    {
        _pointerHand = User.PointerHand;
        if (!_pointerHand.IsPointing || User.ActionBlocked)
            return;
        
        if (!SupportsDistanceGrab || !SteamVR_Actions.default_grab.GetState(User.PointerHandType))
        {
            //Long Click
            if (SupportsLongClick && (ExecuteBeforeLongClick == null || ExecuteBeforeLongClick()))
            {
                if (SteamVR_Actions.default_trigger.GetState(User.PointerHandType))
                {
                    LongClickTimer += Time.deltaTime;
                    if (LongClickTimer > ClickTime)
                    {
                        if (!StatusBox.Active)
                        {
                            StatusBox.Active = true;
                            StartCoroutine(StatusBox.SetInfoText(LoadingText, false));
                            StatusBoxTriggered = true;
                        }
                        StatusBox.SetLoadingStatus(LongClickTimer, LongClickTime);
                    }
                    if (LongClickTimer >= LongClickTime)
                    {
                        _longClicked = true;
                        ShutDownStatusBox();
                        OnPointerClick();
                    }
                }

                if (!SteamVR_Actions.default_trigger.GetState(User.PointerHandType) &&
                    LongClickTimer > 0 && StatusBox.Active)
                {
                    LongClickTimer -= Time.deltaTime;
                    StatusBox.SetLoadingStatus(LongClickTimer, LongClickTime);
                    if (LongClickTimer <= 0 && StatusBox.Active)
                        ShutDownStatusBox();
                }


                if (!_clicked && SteamVR_Actions.default_trigger.GetStateUp(User.PointerHandType))
                {
                    _longClicked = LongClickTime <= LongClickTimer;
                    _clicked = LongClickTimer <= ClickTime;
                    if (_clicked || _longClicked) OnPointerClick();
                }

                if ((_clicked || _longClicked) && !SteamVR_Actions.default_trigger.GetState(User.PointerHandType))
                {
                    _clicked = false;
                    _longClicked = false;
                }

            }
            else
            {
                if (!_clicked && SteamVR_Actions.default_trigger.GetStateDown(User.PointerHandType))
                {
                    OnPointerClick();
                    _clicked = true;
                }

                if (_clicked && !SteamVR_Actions.default_trigger.GetStateDown(User.PointerHandType))
                    _clicked = false;
            }
        }

        //Distance Grab
        if (SteamVR_Actions.default_grab.GetStateDown(User.PointerHandType))
        {
            if (SupportsDistanceGrab  && !SteamVR_Actions.default_trigger.GetState(User.PointerHandType))
            {
                StartCoroutine(OnDistanceGrab());
            }
        }
        

    }



    bool verticalScrolled; bool horizontalScrolled;
    protected virtual void CheckScrolling(Vector3 hitPoint)
    {
        if (OnVerticalScroll == null &&
            OnHorizontalScroll == null) return;
        
        if (verticalScrolled)
        {
            if ((_lastScrollDir == 1 && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).y >= 0) ||
                (_lastScrollDir == -1 && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).y <= 0))
            {
                _lastScrollDir = 0;
                verticalScrolled = false;
            }
        }

        if (horizontalScrolled)
        {
            if ((_lastScrollDir == 1 && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x <= 0) ||
                (_lastScrollDir == -1 && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x >= 0))
            {
                _lastScrollDir = 0;
                horizontalScrolled = false;
            }
        }

        if (!verticalScrolled)
        {
            if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).y >= 0.8f)
            {
                _lastScrollDir = -1;
                OnVerticalScroll?.Invoke(_lastScrollDir);
                verticalScrolled = true;
            }
            if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).y <= -0.8f)
            {
                _lastScrollDir = 1;
                OnVerticalScroll?.Invoke(_lastScrollDir);
                verticalScrolled = true;
            }            
        }
        if (!horizontalScrolled)
        {
            if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x <= -0.8f)
            {
                _lastScrollDir = -1;
                OnHorizontalScroll?.Invoke(_lastScrollDir);
                horizontalScrolled = true;
            }
            if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x >= 0.8f)
            {
                _lastScrollDir = 1;
                OnHorizontalScroll?.Invoke(_lastScrollDir);
                horizontalScrolled = true;
            }
        }
    }

    public virtual void ResetObject()
    {
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Rigidbody>().useGravity = false;
        }
        if (ParentObject != null)
        {
            transform.parent = ParentObject.transform;
            transform.position = ParentObject.transform.position;
            transform.rotation = ParentObject.transform.rotation;
            transform.localPosition = OriginalLocalPosition;
            transform.localRotation = OriginalLocalRotation;
        }
        if (ObjectOrientation != null)
            ObjectOrientation.ResetOrientation();

        IsFlying = false;
        Released = false;
        resetScale();
        _handPositionLastFrame = Vector3.zero;
        InTheAir = 0;
        _throwingEnergy = 0;
    }

    protected virtual void SetHighlight()
    {
        
        if (PartsToHighlight == null) return;
        CleanMissingComponents();
        for (int i=0; i<PartsToHighlight.Count; i++)
        {
            if (Highlight)
            {                
                for (int m=0; m<PartsToHighlight[i].materials.Length; m++)
                    PartsToHighlight[i].materials[m].EnableKeyword("_EMISSION_ON");
            }
            else
            {
                for (int m = 0; m < PartsToHighlight[i].materials.Length; m++)
                    PartsToHighlight[i].materials[m].DisableKeyword("_EMISSION_ON");
            }
        }       
    }

    List<Renderer> _missingRenderer;
    protected virtual void ChangeColor(string colorSpace, Color color)
    {
        CleanMissingComponents();
        for (int i = 0; i < PartsToHighlight.Count; i++)
        {
            if (!PartsToHighlight[i].material.HasProperty(colorSpace)) continue;
            if (colorSpace.Equals("_EmissionColor") && color == default)
            {
                EmissionColor = PartsToHighlight[i].material.GetColor(colorSpace) * 3;
                color = EmissionColor;
            }
            for (int m = 0; m < PartsToHighlight[i].materials.Length; m++)
                PartsToHighlight[i].materials[m].SetColor(colorSpace, color);
        }
    }

    private void CleanMissingComponents()
    {
        if (_missingRenderer == null) _missingRenderer = new List<Renderer>();
        else _missingRenderer.Clear();
        for (int i = 0; i < PartsToHighlight.Count; i++)
            if (PartsToHighlight[i] == null) _missingRenderer.Add(PartsToHighlight[i]);
        for (int i = 0; i < _missingRenderer.Count; i++)
            PartsToHighlight.Remove(_missingRenderer[i]);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (Removable && other.name.Equals("StolperwegeObjectRemover") && Grabbing.Count == 1)
        {
            OnRemove?.Invoke();
            if (!DestroyOnObjectRemover) ResetObject();
            else Destroy(gameObject);
        }
    }

    public void ShutDownStatusBox()
    {
        StatusBox.Reset();
        StatusBoxTriggered = false;
        LongClickTimer = 0;
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (IsCollectable && !GrabHand.LeftHand &&
            other.GetComponent<SmartWatchController>()!= null) OnPack();
    }

    protected virtual void LookAtUser()
    {
        if (!LookAtUserOnHold || GrabHand == null || Grabbing.Count != 1) return;
        Vector3 lookTarget = CenterEyeAnchor.transform.position;
        if (KeepXRotation) lookTarget.x = transform.position.x;
        if (KeepYRotation) lookTarget.y = transform.position.y;
        if (KeepZRotation) lookTarget.z = transform.position.z;
        transform.LookAt(lookTarget);
    }

    public virtual void OnPack()
    {
        if (!_comparisonDone)
        {
            _comparisonDone = true;
        }
        ScaleMultiplier = new Vector3(ScaleMultiplier.x - 0.01f, ScaleMultiplier.y - 0.01f, ScaleMultiplier.z - 0.01f);
        if (ScaleMultiplier.x < 0.3f)
        {
            Backpack.AddItem(this);
            _inBackpack = true;
            _comparisonDone = false;
        }
    }

    float lastLongClickTime = 0;
    /// <summary>
    /// Wird aufgerufen, wenn der Pointer auf das Element gerichtet und die Aktionstaste betätigt wird
    /// </summary>
    /// <returns></returns>
    public virtual bool OnPointerClick()
    {
        if (!_longClicked)
        {
            if((Time.time - lastLongClickTime > 0.5))
            {
               
                OnClick?.Invoke();

                if (AsyncClick != null)
                    StartCoroutine(AsyncClick());
            }
            
        } else
        {
         

            lastLongClickTime = Time.time;
            OnLongClick?.Invoke();

            if (AsyncLongClick != null)
                StartCoroutine(AsyncLongClick());
        }

        if (!PointerTriggered) return false;
        return true;
    }

    /// <summary>
    /// Wird aufgerufen, sobald der Pointer auf das Element gerichtet wird
    /// </summary>
    /// <param name="other">Collider des Pointers</param>
    public virtual void OnPointerEnter(Collider other)
    {
        PointerTriggered = true;
        Highlight = true;
    }

    /// <summary>
    /// Wird aufgerufen, sobald der Pointer das Element verlässt
    /// </summary>
    public virtual void OnPointerExit()
    {
        PointerTriggered = false;
        _longClicked = false;
        LongClickTimer = 0;
        Highlight = false;
        if (StatusBox != null && StatusBox.Active)
        {
            StatusBox.Reset();
            StatusBoxTriggered = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool OnDrop(Collider other)
    {
        Grabbing.Remove(other);

        if (Grabbing.Count > 0)
        {
            if (Grabbing.Get != null)
                OnGrab(Grabbing.Get);
            return false;
        }
        if (transform.parent != null && transform.parent.tag != null && 
            (transform.parent.tag.Contains("Arm") || transform.parent.tag.Contains("arm")))
        {
            GetComponent<Collider>().enabled = false;
            SetupDrop();
            transform.parent = ParentObject?.transform;
            GetComponent<Collider>().enabled = true;
        }
        


        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual bool OnDrop()
    {
        Grabbing.Clear();
        _comparisonDone = false;
        if (transform.parent != null && ((transform.parent.tag.Contains("Arm") || transform.parent.tag.Contains("arm"))))
        {
            SetupDrop();
            transform.parent = ParentObject?.transform;
        }

        return true;
    }

    private void SetupDrop()
    {
        if (GetComponentInParent<DragFingerColliderScript>() != null)
            GetComponentInParent<DragFingerColliderScript>().GetComponent<HandAnimator>().HoldedObject = null;
        if (ManipulatedByGravity && GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().useGravity = true;
        }
        IsFlying = true;
        Released = true;
    }

    public virtual void OnRelease(Collider other)
    {
        Grabbing.Remove(other);
    }

    public virtual GameObject OnGrab(Collider other)
    {
        if (!Grabable) return null;
        Released = false;
        IsFlying = false;
        _handPositionLastFrame = other.transform.position;
        _throwingEnergy = 0;
        InTheAir = 0;
        if (ManipulatedByGravity && GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Rigidbody>().useGravity = false;
        }
        if (other.GetComponent<DragFingerColliderScript>() != null)
            other.GetComponent<HandAnimator>().HoldedObject = gameObject;
        
        Grabbing.Add(other);

        transform.parent = other.transform;
        
        if (LookAtUserOnHold)
            transform.localPosition = other.GetComponent<DragFinger>().ColliderPosition;

        if (_inBackpack)
        {
            Backpack.RemoveItem(this);
            _inBackpack = false;
        }
        other.GetComponent<DragFinger>().GrabedObject = this;
        gameObject.SetActive(true);

        OnGrabed?.Invoke(this);
        return gameObject;
        
}

    public void ChangeBlendMode(Material material, string blendMode)
    {
        switch (blendMode)
        {
            case "Opaque":
                material.SetFloat("_Mode", 0f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;
            case "Fade":
                material.SetFloat("_Mode", 2f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        IsFlying = false;
    }

    public virtual Vector3 CalculateRandomRespawnPosition()
    {
        float xPos = UnityEngine.Random.Range(15, 31);
        if (UnityEngine.Random.Range(0, 2) == 1) xPos *= -1;
        float yPos = UnityEngine.Random.Range(15, 31);
        if (UnityEngine.Random.Range(0, 2) == 1) yPos *= -1;
        float zPos = (xPos >= 22 && yPos >= 22) ? UnityEngine.Random.Range(45, 51) : UnityEngine.Random.Range(40, 45);
        
        IsFlying = false;
        _throwingEnergy = 0;
        _throwingVector = Vector3.zero;
        ManipulatedByGravity = false;

        return new Vector3(xPos, yPos, zPos) * 0.01f;
    }

    protected void resetScale()
    {
        ScaleMultiplier = Vector3.one;
    }

    public virtual IEnumerator Delete()
    {
        Destroy(gameObject);
        yield return null;
    }

    public virtual void SetParentObject(GameObject parentObject)
    {
        this.ParentObject = parentObject;
    }

    public void ActualizeThrowingVariables(DragFinger hand)
    {
        _throwingDirection = hand.transform.position - _handPositionLastFrame;
        _throwingEnergy = _throwingDirection.magnitude * DragFinger.ThrowForceMultiplier;
        _handPositionLastFrame = hand.transform.position;
    }

    public void CalculateFlying()
    {
        _throwingVector = _throwingDirection * _throwingEnergy;
        _throwingEnergy -= _throwingEnergy / 100;
        transform.position += _throwingVector;
        if (_throwingEnergy <= 0.0001f)
        {
            _throwingEnergy = 0;
            IsFlying = false;
        }
    }

    public void CalculateHovering()
    {
        transform.Rotate(Vector3.right, 0.05f);
        transform.Rotate(Vector3.up, 0.05f);
        transform.Rotate(Vector3.forward, 0.05f);
    }


}

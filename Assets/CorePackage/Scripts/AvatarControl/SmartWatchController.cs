using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Reflection;
using Valve.VR;

public class SmartWatchController : InteractiveObject
{

    private Vector3 DefaultLocalPosition;
    private Vector3 DefaultLocalRotation;
    private const string ParentPath = "Armature/Main";
    public TextMeshPro Menu;
    public TextMeshPro Timer;

    public MainMenuScript MainMenu;

    public InstantiableSmartwatchObject InstancedObject;
    private SphereCollider _pointerHandCollider;
    private bool _isPointerHandNear;
    private BoxCollider Collider;

    private bool _animOn;
    private float _animLerp;
    private Vector3 _defaultPos = new Vector3(0, -0.005f, 0.002f);
    private Vector3 _raisedPos = new Vector3(0, -0.005f, 0.05f);
    private int _mode;
    private List<Type> InstantiableObjects;
    private TextMeshPro LeftArrow;
    private TextMeshPro RightArrow;
    public int Mode
    {
        get { return _mode; }
        set
        {
            if (value < 0 || value > InstantiableObjects.Count) return;
            _mode = value;
            if (_mode > 0 && MainMenu.IsSubMenuActive)
                StartCoroutine(MainMenu.HideSubMenu());
            LeftArrow.gameObject.SetActive(_mode > 0);
            RightArrow.gameObject.SetActive(_mode < InstantiableObjects.Count);
            Menu.text = _mode > 0 ? (string)InstantiableObjects[_mode - 1].GetField("Icon").GetValue(null) : "\xf58d";
            SetHighlight();
        }
    }


    public override void Start()
    {
        base.Start();
        StolperwegeHelper.Smartwatch = this;
        DefaultLocalPosition = transform.localPosition;
        DefaultLocalRotation = transform.localEulerAngles;
        MainMenu = transform.Find("MainMenu").GetComponent<MainMenuScript>();
        Menu = transform.Find("Menu").GetComponent<TextMeshPro>();
        LeftArrow = Menu.transform.Find("LeftArrow").GetComponent<TextMeshPro>();        
        RightArrow = Menu.transform.Find("RightArrow").GetComponent<TextMeshPro>();
        LeftArrow.gameObject.SetActive(false);
        RightArrow.gameObject.SetActive(false);
        Timer = transform.Find("Timer").GetComponent<TextMeshPro>();
        OnClick = OpenMenu;
        InstantiableObjects = new List<Type>(StolperwegeHelper.GetTypesDerivingFrom<InstantiableSmartwatchObject>());
        InstantiableObjects.Sort((a, b) => { return a.GetType().Name.CompareTo(b.GetType().Name); });
        Collider = GetComponent<BoxCollider>();
        OnHorizontalScroll = (dir) => { Mode += dir; };                
        Mode = 0;
        SwitchToControlHand();
    }

    public void Update()
    {
        ActualizeTime();
        ActualizeMenu();
    }

    private string time;
    private void ActualizeTime()
    {
        if (DateTime.Now.ToString().Replace(" ", "\n").Equals(time)) return;
        time = DateTime.Now.ToString().Replace(" ", "\n");
        Timer.text = time;
        Timer.ForceMeshUpdate();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (Mode > 0 && other.GetComponent<DragFinger>() != null && 
            other.GetComponent<DragFinger>().GrabedObject == null)
        {
            if (InstancedObject == null)
            {
                string prefabPath = (string)InstantiableObjects[Mode - 1].GetField("PrefabPath").GetValue(null);
                InstancedObject = ((GameObject)Instantiate(Resources.Load(prefabPath))).GetComponent<InstantiableSmartwatchObject>();
                InstancedObject.Initialize();                
                InstancedObject.transform.localScale = InstancedObject.ShrinkedSize;
                InstancedObject.transform.position = other.transform.position;
                InstancedObject.transform.rotation = transform.rotation;
                InstancedObject.OnGrab(other);
            }
        }
        if (other.GetComponent<InteractiveObject>() != null && other.GetComponent<InteractiveObject>().IsCollectable)
        {
            if (StolperwegeHelper.Inventory != null && !StolperwegeHelper.Inventory.isActive)
                StolperwegeHelper.Inventory.isActive = true;
            if (MainMenu.IsSubMenuActive)
                StartCoroutine(MainMenu.HideSubMenu());
        }
        if (other.GetComponent<PointFinger>() != null && other.GetComponent<PointFinger>().RightHand)
            OnClick();

    }

    

    private void OpenMenu()
    {
        if (Mode > 0) return;
        if (MainMenu.IsSubMenuActive)
            StartCoroutine(MainMenu.HideSubMenu());
        else
            StartCoroutine(MainMenu.ActivateSubMenu());
    }

    protected override void SetHighlight()
    {
        if (Menu == null) return;
        Menu.color = Highlight && Mode == 0 ? StolperwegeHelper.GUCOLOR.GOETHEBLAU : Color.white;
        LeftArrow.color = Menu.color;
        RightArrow.color = Menu.color;
    }

    bool _raised;
    private void ActualizeMenu()
    {
        _pointerHand = StolperwegeHelper.User.PointerHand;
        _pointerHandCollider = _pointerHand.GetComponent<SphereCollider>();

        _isPointerHandNear = (_pointerHand.transform.TransformPoint(_pointerHandCollider.center) - transform.position).magnitude <= 0.2f;

        if ((_isPointerHandNear && !_raised) || (!_isPointerHandNear && _raised) && !_animOn)
        {
            _animOn = true;
            _animLerp = 0;
        }

        if (_animOn)
        {
            _animLerp += Time.deltaTime * 2;
            if (_isPointerHandNear)
            {
                _raised = true;
                Menu.transform.localPosition = Vector3.Lerp(Menu.transform.localPosition, _raisedPos, _animLerp);
                Menu.transform.localScale = Vector3.Lerp(Menu.transform.localScale, Vector3.one * 3, _animLerp);
                Menu.color = Vector4.Lerp(Menu.color, StolperwegeHelper.GUCOLOR.GOETHEBLAU, _animLerp);
                LeftArrow.color = Menu.color;
                RightArrow.color = Menu.color;
                Collider.center = Vector3.Lerp(Collider.center, Vector3.zero, _animLerp);
                if (Menu.transform.localPosition == _raisedPos)
                {
                    _animOn = false;
                    _animLerp = 0;
                }

            }
            else
            {
                Menu.transform.localPosition = Vector3.Lerp(Menu.transform.localPosition, _defaultPos, _animLerp);
                Menu.transform.localScale = Vector3.Lerp(Menu.transform.localScale, Vector3.one, _animLerp);
                Menu.color = Vector4.Lerp(Menu.color, Color.white, _animLerp);
                LeftArrow.color = Menu.color;
                RightArrow.color = Menu.color;
                Collider.center = Vector3.Lerp(Collider.center, Vector3.back * 0.0125f, _animLerp);
                if (Menu.transform.localPosition == _defaultPos)
                {
                    _raised = false;
                    _animOn = false;
                    _animLerp = 0;
                }

            }
        }

        if (_raised && Mode == 0) SetDefaultMenuState();
    }

    private void SetDefaultMenuState()
    {
        _raised = false;
        Menu.transform.localPosition = _defaultPos;
        Menu.transform.localScale = Vector3.one;
        Menu.color = Color.white;
        LeftArrow.color = Menu.color;
        RightArrow.color = Menu.color;
        Collider.center = Vector3.back * 0.0125f;
    }

    public void SwitchToControlHand()
    {
        if (StolperwegeHelper.User.ControlHandType == SteamVR_Input_Sources.LeftHand)
        {
            transform.SetParent(StolperwegeHelper.LeftHand.transform.Find(ParentPath));
            transform.localEulerAngles = DefaultLocalRotation;
        } else
        {
            transform.SetParent(StolperwegeHelper.RightHand.transform.Find(ParentPath));
            transform.localEulerAngles = DefaultLocalRotation;
            transform.Rotate(Vector3.right, -180, Space.Self);
        }
        transform.localPosition = DefaultLocalPosition;
    }
}

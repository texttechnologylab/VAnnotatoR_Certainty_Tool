using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

//[PrefabInterface("Prefabs/VRWriter/VRWriter")]
public class VRWriter : AnimatedWindow 
{

    public VRWriterInterface Interface { get; private set; }

    public float ActivationTime = 2;
    public override bool Active
    {
        set
        {
            if (_active == value) return;
            base.Active = value;
            Interface.gameObject.SetActive(_active);
            if (_active)
            {
                string textToWrite = "";
                if (Inputfield != null)
                {
                    textToWrite = Inputfield.Text;
                    if (Inputfield.IsNumberField)
                    {
                        Interface.ActiveKeyboardLayoutType = KeyboardLayouts.Layout.Numerical;
                        Interface.Keyboard.EnableFloatingNumber = Inputfield.EnableFloatingPoint;
                        Interface.Keyboard.EnableNegativeNumber = Inputfield.EnableNegativeNumber;
                        Interface.Editor.CheckNumericalButtonStatus();
                    }
                    else
                        Interface.ActiveKeyboardLayoutType = KeyboardLayouts.Layout.German;
                    
                } else { Interface.ActiveKeyboardLayoutType = KeyboardLayouts.Layout.German; }
                if (textToWrite != "") StartCoroutine(Interface.Keyboard.AutoWriteText(textToWrite));
            } 
            else _inputfield = null;
            VRWriterActiveChanged?.Invoke(value);
        }
    }
    
    public event Action<bool> VRWriterActiveChanged;

    public bool TextRecordingOn { get { return Interface.TextRecordingOn; } }

    private KeyboardEditText _inputfield;
    public KeyboardEditText Inputfield
    {
        get
        {
            return _inputfield;
        }
        set
        {
            _inputfield = value;
            Active = true;
        }
    }

    public override void Start()
    {
        base.Start();
        SpawnDistanceMultiplier = 1.5f;
        DestroyOnObjectRemover = false;
        OnRemove = () => { Active = false; };
        StolperwegeHelper.VRWriter = this;
        Interface = transform.Find("Interface").GetComponent<VRWriterInterface>();
        Interface.Init();
        Interface.gameObject.SetActive(false);
        DontDestroyOnLoad(gameObject);
    }

    private float _timeElapsed = 0;
    private bool _triggerReleaseAfterHolding = true;
    public override void Update()
    {
        if (SteamVR_Actions.default_left_action1.activeBinding)
        {
            if (SteamVR_Actions.default_left_action1.GetStateDown(SteamVR_Input_Sources.LeftHand))
                Active = !Active;
        }
        else
        {
            if (!StolperwegeHelper.User.LeftTriggerBlocked && StolperwegeHelper.LeftFist.GrabedObject == null &&
                 SteamVR_Actions.default_trigger.GetState(SteamVR_Input_Sources.LeftHand) && _triggerReleaseAfterHolding)
            {
                _timeElapsed += Time.deltaTime;
                if (!StolperwegeHelper.StatusBox.Active)
                {
                    StolperwegeHelper.StatusBox.Active = true;
                    StartCoroutine(StolperwegeHelper.StatusBox.SetInfoText("Activating keyboard...", false));
                }

                StolperwegeHelper.StatusBox.SetLoadingStatus(_timeElapsed, ActivationTime);
                if (_timeElapsed >= ActivationTime)
                {
                    Active = !Active;
                    _triggerReleaseAfterHolding = false;
                    _timeElapsed = 0;
                    StolperwegeHelper.StatusBox.Reset();
                }
            }

            if (!_triggerReleaseAfterHolding && !SteamVR_Actions.default_trigger.GetState(SteamVR_Input_Sources.LeftHand))
                _triggerReleaseAfterHolding = true;

            if (_timeElapsed > 0 && !SteamVR_Actions.default_trigger.GetState(SteamVR_Input_Sources.LeftHand))
            {
                _timeElapsed -= Time.deltaTime;
                StartCoroutine(StolperwegeHelper.StatusBox.SetInfoText("Deactivating keyboard...", false));
                StolperwegeHelper.StatusBox.SetLoadingStatus(_timeElapsed, ActivationTime);
                if (_timeElapsed <= 0)
                {
                    _timeElapsed = 0;
                    StolperwegeHelper.StatusBox.Reset();
                }
            }
        }
        
        base.Update();  
    }

    //private float _radius;
    //private Vector2 _actual2DPos;
    //public void SetRadius()
    //{
    //    if (CabinMesh == null) CabinMesh = Cabin.GetComponent<MeshFilter>().sharedMesh;
    //    Vector3[] verts = CabinMesh.vertices;
    //    if (DefaultSizevertices == null)
    //    {
    //        DefaultSizevertices = CabinMesh.vertices;
    //        _radius = DefaultSizevertices[0].magnitude;
    //        _radiusMult = DEFAULT_RADIUS_MULTIPLIER;
    //        Interface.transform.localPosition = Vector3.forward;
    //    }

    //    for (int i = 0; i < verts.Length; i++)
    //            verts[i] = new Vector3(DefaultSizevertices[i].x * RadiusMultiplier, DefaultSizevertices[i].y * RadiusMultiplier, DefaultSizevertices[i].z);

    //    CabinMesh.vertices = verts;
    //    _actual2DPos = new Vector2(Interface.transform.localPosition.x, Interface.transform.localPosition.z).normalized;
    //    Interface.transform.localPosition = new Vector3(_actual2DPos.x * _radius * RadiusMultiplier, Interface.transform.localPosition.y, _actual2DPos.y * _radius * RadiusMultiplier);
    //    Cabin.GetComponent<MeshFilter>().mesh = CabinMesh;        
    //}
    
    //private void SetComponentStatus(bool status)
    //{
    //    Cabin.SetActive(status);
    //    Interface.gameObject.SetActive(status);
    //    Ground.SetActive(status);
    //}
}

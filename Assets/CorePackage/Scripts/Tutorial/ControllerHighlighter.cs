using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using TMPro;
using Valve.VR;

public class ControllerHighlighter : MonoBehaviour {

    private GameObject Trigger;
    private GameObject Grab;

    public bool MovementInfo { set { if (Movement != null) Movement.SetActive(value); } }
    private GameObject Movement;
    private GameObject MovementSign;
    private TextMeshPro MovementText;

    public bool PointingInfo { set { if (Pointing != null) Pointing.SetActive(value); } }
    private GameObject Pointing;
    private GameObject PointingSign;
    private TextMeshPro PointingText;

    public bool GrabbingInfo { set { if (Grabbing != null) Grabbing.SetActive(value); } }
    private GameObject Grabbing;
    private GameObject GrabbingSign;
    private TextMeshPro GrabbingText;

    public bool ClickingInfo { set { if (Clicking != null) Clicking.SetActive(value); } }
    private GameObject Clicking;
    private GameObject ClickingSign;
    private TextMeshPro ClickingText;
    
    private float timeElapsed = 0;
    //private InputInterface.ControllerType Controller;
    private SteamVR_Input_Sources Controller;


    // Use this for initialization
    public void Init () {
        if (tag.Equals("leftArm"))
        {
            //Controller = InputInterface.ControllerType.LHAND;
            Controller = SteamVR_Input_Sources.LeftHand;
            Movement = transform.Find("Movement").gameObject;
            MovementSign = Movement.transform.Find("Sign").gameObject;
            MovementText = MovementSign.transform.Find("Text").GetComponent<TextMeshPro>();
            Movement.SetActive(false);
        }
        if (tag.Equals("rightArm"))
        {
            //Controller = InputInterface.ControllerType.RHAND;
            Controller = SteamVR_Input_Sources.RightHand;
            Clicking = transform.Find("Clicking").gameObject;
            ClickingSign = Clicking.transform.Find("Sign").gameObject;
            ClickingText = ClickingSign.transform.Find("Text").GetComponent<TextMeshPro>();
            Clicking.SetActive(false);
        }

        Trigger = transform.Find("Trigger").gameObject;
        Grab = transform.Find("Grab").gameObject;

        Grabbing = transform.Find("Grabbing").gameObject;
        GrabbingSign = Grabbing.transform.Find("Sign").gameObject;
        GrabbingText = GrabbingSign.transform.Find("Text").GetComponent<TextMeshPro>();
        Grabbing.SetActive(false);

        Pointing = transform.Find("Pointing").gameObject;
        PointingSign = Pointing.transform.Find("Sign").gameObject;
        PointingText = PointingSign.transform.Find("Text").GetComponent<TextMeshPro>();
        Pointing.SetActive(false);

    }

    private float _timer; private int _last;
    bool _grabPressed, _triggerPressed, _lastPointingState;
    bool _movPadUsed, _lastMovementState, _lastGrabbingState;
    bool _clickPressed, _lastClickState;
	// Update is called once per frame
	void Update ()
    {
        _timer = (_timer + Time.deltaTime) % 2f;

        //transform.localPosition = InputInterface.getControllerPosition(Controller);
        //transform.localRotation = InputInterface.getControllerRotation(Controller);
        transform.localPosition = SteamVR_Actions.default_Pose.GetLocalPosition(Controller);
        transform.localRotation = SteamVR_Actions.default_Pose.GetLocalRotation(Controller);
        _grabPressed = SteamVR_Actions.default_grab.GetState(Controller);
        _triggerPressed = SteamVR_Actions.default_trigger.GetState(Controller);

        if (Pointing != null && Pointing.activeInHierarchy)
        {
            PointingSign.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform.position);
            if (SteamVR_Actions.default_grab.GetStateDown(Controller))
            {
                Grab.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.green);
                Grab.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            } else if (!_grabPressed)
            {
                //if (InputInterface.getButtonUp(Controller, InputInterface.ButtonType.GRAB))
                if (SteamVR_Actions.default_grab.GetStateUp(Controller))
                    Grab.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
                if ((int)_timer != _last)
                    if (_last == 0)
                    {
                        Grab.GetComponent<Renderer>().material.SetColor("_EmissionColor", StolperwegeHelper.GUCOLOR.GOETHEBLAU);
                        Grab.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                    }                        
                    else
                        Grab.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
            }
            if (_triggerPressed)
            {
                Trigger.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.red);
                Trigger.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            } else
                Trigger.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");

            if (_lastPointingState != (_grabPressed && !_triggerPressed))
                if (_grabPressed && !_triggerPressed)
                    PointingText.color = Color.green;
                else
                    PointingText.color = Color.white;

            _lastPointingState = _grabPressed && !_triggerPressed;
        }

        if (Grabbing != null && Grabbing.activeInHierarchy)
        {
            GrabbingSign.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform.position);
            if (SteamVR_Actions.default_grab.GetStateDown(Controller))
            {
                Grab.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.green);
                Grab.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            }
            else if (!_grabPressed)
            {
                if (SteamVR_Actions.default_grab.GetStateUp(Controller))
                    Grab.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
                if ((int)_timer != _last)
                    if (_last == 0)
                    {
                        Grab.GetComponent<Renderer>().material.SetColor("_EmissionColor", StolperwegeHelper.GUCOLOR.GOETHEBLAU);
                        Grab.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                    }
                    else
                        Grab.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
            }

            if (_triggerPressed)
            {
                Trigger.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.green);
                Trigger.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
            }
            else
            {
                Trigger.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
                if ((int)_timer != _last)
                    if (_last == 0)
                    {
                        Trigger.GetComponent<Renderer>().material.SetColor("_EmissionColor", StolperwegeHelper.GUCOLOR.GOETHEBLAU);
                        Trigger.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                    }
                    else
                        Trigger.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
            }

            if (_lastGrabbingState != (_grabPressed && _triggerPressed))
                if (_grabPressed && _triggerPressed)
                    GrabbingText.color = Color.green;
                else
                    GrabbingText.color = Color.white;

            _lastGrabbingState = _grabPressed && _triggerPressed;
        }

        if (Clicking != null && Clicking.activeInHierarchy)
        {
            ClickingSign.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform.position);
            //_clickPressed = InputInterface.getButton(Controller, InputInterface.ButtonType.STICK);
            _clickPressed = SteamVR_Actions.default_click.GetState(Controller);

            if (_lastClickState != _clickPressed)
                if (_clickPressed)
                    ClickingText.color = Color.green;
                else
                    ClickingText.color = Color.white;

            _lastClickState = _clickPressed;
        }

        if (Movement != null && Movement.activeInHierarchy)
        {
            MovementSign.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform.position);
            _movPadUsed = SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand) != Vector2.zero;
            if (_lastMovementState != _movPadUsed)
                if (_movPadUsed)
                    MovementText.color = Color.green;
                else
                    MovementText.color = Color.white;

            _lastMovementState = _movPadUsed;
        }

        _last = (int)_timer;
    }

    public void ResetHighlight()
    {
        Grab.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
        Trigger.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");
    }

}

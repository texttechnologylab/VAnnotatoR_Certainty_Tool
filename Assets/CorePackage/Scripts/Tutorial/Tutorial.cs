using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;
using Valve.VR;

public class Tutorial : MonoBehaviour
{

    private Orientation PlayerOrientationOnStart;
    private Vector3 UserStartPoint = new Vector3(-3.95f, 1, 3.95f);
    private Orientation InfoStatePoint0 = new Orientation(new Vector3(-3.95f, 1.3f, 2.95f), Quaternion.identity);
    private Orientation InfoStatePoint1 = new Orientation(new Vector3(-3.95f, 1.3f, -4.85f), Quaternion.identity);
    private Orientation InfoStatePoint2 = new Orientation(new Vector3(-3.01f, 1.3f, -4f), Quaternion.Euler(0, -90, 0));
    private Orientation InfoStatePoint3 = new Orientation(new Vector3(-2.35f, 1.3f, -3.9f), Quaternion.Euler(0, -90, 0));
    private Orientation InfoStatePoint4 = new Orientation(new Vector3(2.25f, 1.475f, -2.95f), Quaternion.Euler(0, -180, 0));
    //private Orientation InfoStatePoint5 = new Orientation(new Vector3(4.85f, 1.3f, -4.3f), Quaternion.Euler(0, -90, 0));

    private Orientation Door1Closed;
    private Orientation Door1HalfOpened;
    private Orientation Door1Opened;
    private Orientation Door2Closed;
    private Orientation Door2Opened;
    private float Door2Movement;

    private GameObject Checkpoint1;
    private GameObject Checkpoint2;

    private GameObject Door1;
    private GameObject Door2;

    private InteractiveObject Opener1;
    private InteractiveObject SafetyOpener;
    private TutorialLever Opener2;

    private GameObject Info;
    private TextMeshPro InfoText;

    private GameObject RightControllerModel;
    private ControllerHighlighter RightHighlighter;
    private GameObject LeftControllerModel;
    private ControllerHighlighter LeftHighLighter;

    private System.Random Random;
    private SlidingPuzzle SlidingPuzzle;    
    private SlotPuzzle SlotPuzzle;
    private GameObject GreenCodeEquation;
    private GameObject FirstTerm;
    private GameObject SecondTerm;
    private TextMeshPro Operator;

    private InteractiveObject CloseButton;
    private InteractiveObject RestartButton;
    private KeyboardEditText TerminalInput;
    private TextMeshPro TerminalMessage;

    private Coroutine RunningTutorial;

    private int RedCode;
    private int GreenCode;
    private int BlueCode;
    private string Solution;

    private bool _solved;
    public bool Solved
    {
        get { return _solved; }
        set
        {
            _solved = value;
            if (_solved)
                TerminalMessage.text = "Gratulation! Der Code ist richtig :)";
            else
                TerminalMessage.text = "Der Code ist leider falsch :(";
        }
    }

    private string State0 = "Willkommen zum Basis-Tutorial!\n\nZuerst lernen wir uns zu bewegen. Schau deine linke Hand an, dort siehst du den Controller und wie man sich bewegt.\n\nAufgabe: Begib dich zum Ende des Flurs und stelle dich auf das <color=#006186>Rechteck<color=#ffffff>.";
    private string State1 = "Perfekt!\nEs gibt interaktive Objekte in StolperwegeVR. Man erkennt sie daran, dass sie etwas heller werden, wenn man sie anschaut.\nLinks an der <color=#3d3d3d>Tür<color=#ffffff> befindet sich eine interaktive <color=#006186>Halbkugel<color=#ffffff>.\n\nAufgabe: Schau dir die <color=#006186>Halbkugel<color=#ffffff> an.";
    private string State2 = "Sehr gut!\nDie meisten interaktive Objekte haben eine Funktion. Die der <color=#006186>Halbkugel<color=#ffffff> ist es das Öffnen der <color=#3d3d3d>Tür<color=#ffffff>.\n\nAufgabe: Strecke deinen virtuellen Arm nach der <color=#006186>Halbkugel<color=#ffffff> aus und berühre sie mit dem Zeigefinger.";
    private string State3 = "Hoppla, die <color=#3d3d3d>Tür<color=#ffffff> klemmt.\nDie <color=#ff0000>Kapsel<color=#ffffff> am Ende des Flurs oben ist für solche Notfälle gedacht. Ein interaktives Objekt lässt sich auch per \"Click\" aus der Ferne aktivieren und kann auch ein langes \"Click\"-Funktion besitzen. \n\nAufgabe: Zeige auf die <color=#ff0000>Kapsel<color=#ffffff> (oder fokussiere sie), so dass sie aufleuchtet und halte \"Click\" gedrückt (siehe rechte Hand).";
    private string State4 = "Gut gemacht!\nÜbrigens um schneller voranzukommen kann man an manchen Orten auch teleportieren. Dazu musst du auf einen beliebigen Punkt auf dem Boden zeigen und \"Click\" drücken.\n\nAufgabe: Bewege dich zum Ende des Ganges und stelle dich auf das nächste <color=#006186>Rechteck<color=#ffffff>.";
    private string State5 = "Viele interaktive Objekte sind greifbar. Genau wie dieser <color=#ffff00>Hebel<color=#ffffff>, der die nächste <color=#3d3d3d>Tür<color=#ffffff> öffnet. \n\nAufgabe: Strecke deinen virtuellen Arm nach dem <color=#ffff00>Hebel<color=#ffffff> aus, ergreife ihn (siehe Controller) und ziehe ihn nach unten.";
    private string State6 = "Wunderbar! Nun kennst du die Basis-Steuerung in StolperwegeVR. Wenn du noch etwas üben möchtest, wartet im nächsten Raum ein Rätsel auf dich. Zum Verlassen des Tutorials wähle in dem nächsten Raum im Terminal gegenüber dem Eingang \"Tutorial beenden\"";
    public struct Orientation
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public Orientation(Vector3 pos, Quaternion rot)
        {
            Position = pos;
            Rotation = rot;
        }
    }

    private bool _active;
    public bool Active
    {
        get { return _active; }
        set
        {
            if (_active == value) return;
            _active = value;
            if (_active)
            {
                PlayerOrientationOnStart = new Orientation(StolperwegeHelper.User.transform.position, StolperwegeHelper.User.transform.rotation);
                RunningTutorial = StartCoroutine(StartTutorial());
            }
            else
            {
                LeftControllerModel.SetActive(false);
                RightControllerModel.SetActive(false);
                StartCoroutine(StolperwegeHelper.FadeOutTeleportationTo(PlayerOrientationOnStart.Position, () =>
                {
                    StolperwegeHelper.User.transform.position = PlayerOrientationOnStart.Position;
                    StolperwegeHelper.User.transform.rotation = PlayerOrientationOnStart.Rotation;
                }));
            }
        }
    }

    private void Start()
    {
        _wait = new WaitForEndOfFrame();
    }

    private bool _initialized; WaitForEndOfFrame _wait;
    private void Init()
    {
        Checkpoint1 = transform.Find("Checkpoint1").gameObject;
        Checkpoint2 = transform.Find("Checkpoint2").gameObject;

        Door1 = transform.Find("Door1").gameObject;
        Door1Closed = new Orientation(Door1.transform.localPosition, Door1.transform.localRotation);
        Door1HalfOpened = new Orientation(Door1.transform.localPosition - Vector3.up * 2.2f, Door1.transform.localRotation);
        Door1Opened = new Orientation(Door1.transform.localPosition - Vector3.up * 4, Door1.transform.localRotation);

        Door2 = transform.Find("Door2").gameObject;
        Door2Closed = new Orientation(Door2.transform.localPosition, Door2.transform.localRotation);
        Door2Opened = new Orientation(Door2.transform.localPosition - Vector3.up * 4, Door2.transform.localRotation);        

        Opener1 = Door1.transform.Find("Opener").GetComponent<InteractiveObject>();
        Opener2 = transform.Find("Opener/Lever").GetComponent<TutorialLever>();
        Opener2.Init(this);
        SafetyOpener = transform.Find("SafetyOpener").GetComponent<InteractiveObject>();
        SafetyOpener.LongClickTime = 2f;
        SafetyOpener.LoadingText = "Öffne Tür...";
        SafetyOpener.OnLongClick = () =>
        {
            SafetyOpener.GetComponent<Collider>().enabled = false;
            SafetyOpener.GetComponent<Renderer>().material.color = Color.gray;
            _door1Click = true;
        };
        
        Info = transform.Find("Info").gameObject;
        InfoText = Info.transform.Find("Text").GetComponent<TextMeshPro>();

        string ctrlName = "Tutorial/Prefabs/";
        if (RightControllerModel == null)
        {
            ctrlName += (SteamVR.instance.hmd_ModelNumber.Contains("Oculus")) ? "OculusTouchR" : "ViveCtrlRight";
            RightControllerModel = (GameObject)Instantiate(Resources.Load(ctrlName));
            RightHighlighter = RightControllerModel.GetComponent<ControllerHighlighter>();
            RightHighlighter.Init();
            RightControllerModel.transform.parent = StolperwegeHelper.User.transform;
            RightControllerModel.SetActive(false);
        }
        ctrlName = "Tutorial/Prefabs/";
        if (LeftControllerModel == null)
        {
            ctrlName += (SteamVR.instance.hmd_ModelNumber.Contains("Oculus")) ? "OculusTouchL" : "ViveCtrlLeft";
            LeftControllerModel = (GameObject)Instantiate(Resources.Load(ctrlName));
            LeftHighLighter = LeftControllerModel.GetComponent<ControllerHighlighter>();
            LeftHighLighter.Init();
            LeftControllerModel.transform.parent = StolperwegeHelper.User.transform;
            LeftControllerModel.SetActive(false);
        }

        Random = new System.Random();
        SlidingPuzzle = GetComponentInChildren<SlidingPuzzle>();

        SlotPuzzle = GetComponentInChildren<SlotPuzzle>();
        SlotPieceInstantiator spi = GetComponentInChildren<SlotPieceInstantiator>();
        spi.Initialize(this);
        SlotPuzzle.Init(transform.Find("SlotPuzzleScheme").gameObject, spi);
        GreenCodeEquation = transform.Find("GreenCodeEquation").gameObject;
        FirstTerm = GreenCodeEquation.transform.Find("FirstTerm").gameObject;
        SecondTerm = GreenCodeEquation.transform.Find("SecondTerm").gameObject;
        Operator = GreenCodeEquation.transform.Find("Operator").GetComponent<TextMeshPro>();

        TerminalInput = GetComponentInChildren<KeyboardEditText>();
        TerminalInput.OnCommit = (input, go) =>
        {
            if (input.Length == 3)
                Solved = input.Equals(Solution);
            else
                if (input.Length > 3)
                    TerminalMessage.text = "Der Code ist zu lang.";
                else
                    TerminalMessage.text = "Der Code ist zu kurz.";
            
            
        };
        TerminalInput.GetComponentInChildren<TextMeshPro>().color = Color.black;
        TerminalMessage = transform.Find("Terminal/TerminalMessage").GetComponent<TextMeshPro>();

        CloseButton = transform.Find("CloseButton").GetComponent<InteractiveObject>();
        CloseButton.OnClick = () => 
        {
            if (RunningTutorial != null)
                StopCoroutine(RunningTutorial);
            Active = false;
        };

        RestartButton = transform.Find("RestartButton").GetComponent<InteractiveObject>();
        RestartButton.OnClick = () => {
            if (RunningTutorial != null)
                StopCoroutine(RunningTutorial);

            RunningTutorial = StartCoroutine(StartTutorial());
        };

        _initialized = true;
    }

    private float _div = 10, _lerp;
    bool _changeInfoPos, _infoPosReached, _sphereFocused, _checkPoint2Reached;
    bool _door1HalfClicked, _door1HalfOpen, _door1Click, _door1Open, _door2Open;
    Orientation infoOri = new Orientation(), door1Ori = new Orientation();
    Vector3 _lastPlayerPos;

    public IEnumerator StartTutorial()
    {
        Reset();

        _changeInfoPos = false;
        _infoPosReached = false;
        _sphereFocused = false;
        _door1HalfClicked = false;
        _door1HalfOpen = false;
        _door1Click = false;
        _door1Open = false;
        _checkPoint2Reached = false;
        _door2Open = false;
        infoOri = new Orientation();
        door1Ori = new Orientation();
        _lerp = 0;

        // STATE 0
        while (true)
        {

            if (StolperwegeHelper.GetDistance2DToPlayer(Checkpoint1) < 0.75f)
            {
                Checkpoint1.GetComponent<Renderer>().material.color = Color.green;
                break;
            }
            if (!_changeInfoPos && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.LeftHand) != Vector2.zero)
                _changeInfoPos = true;
            if (_changeInfoPos && !_infoPosReached)
            {
                _lerp += Time.deltaTime;
                infoOri.Position = Vector3.Lerp(Info.transform.localPosition, InfoStatePoint1.Position, _lerp / _div);
                infoOri.Rotation = Quaternion.Lerp(Info.transform.localRotation, InfoStatePoint1.Rotation, _lerp / _div);
                SetObjectOrientation(Info, infoOri);
                _infoPosReached = Info.transform.localPosition == InfoStatePoint1.Position;
            }
            yield return _wait;
        }

        _changeInfoPos = false;
        _infoPosReached = false;
        _lerp = 0;
        LeftControllerModel.SetActive(false);
        LeftHighLighter.MovementInfo = false;
        InfoText.text = State1;
        Opener1.gameObject.SetActive(true);
        Opener1.OnClick = null;
        Opener1.OnLongClick = null;
        Opener1.OnFocus += (Vector3 hit) => { _sphereFocused = true; };        

        // STATE 1
        while (!_infoPosReached)
        {
            if (_sphereFocused && ! _infoPosReached)
            {
                if (InfoText.text != State2) InfoText.text = State2;
                _lerp += Time.deltaTime;
                infoOri.Position = Vector3.Lerp(Info.transform.localPosition, InfoStatePoint2.Position, _lerp / _div);
                infoOri.Rotation = Quaternion.Lerp(Info.transform.localRotation, InfoStatePoint2.Rotation, _lerp / _div);
                SetObjectOrientation(Info, infoOri);
                _infoPosReached = Info.transform.localPosition == InfoStatePoint2.Position;
            }
            yield return _wait;
        }            

        _infoPosReached = false;
        _lerp = 0;
        LeftControllerModel.SetActive(true);
        RightControllerModel.SetActive(true);
        LeftHighLighter.PointingInfo = true;
        RightHighlighter.PointingInfo = true;
        Opener1.OnClick = () => { _door1HalfClicked = true; };

        // STATE 2
        while (!_infoPosReached)
        {
            if (_door1HalfClicked)
            {
                _lerp += Time.deltaTime;
                if (!_door1HalfOpen)
                {
                    door1Ori.Position = Vector3.Lerp(Door1.transform.localPosition, Door1HalfOpened.Position, _lerp / _div);
                    door1Ori.Rotation = Quaternion.Lerp(Door1.transform.localRotation, Door1HalfOpened.Rotation, _lerp / _div);
                    SetObjectOrientation(Door1, door1Ori);
                    _door1HalfOpen = Door1.transform.localPosition == Door1HalfOpened.Position;
                }                
                
                if (Door1.transform.localPosition.y < -0.5f)
                {
                    infoOri.Position = Vector3.Lerp(Info.transform.localPosition, InfoStatePoint3.Position, _lerp / _div);
                    infoOri.Rotation = Quaternion.Lerp(Info.transform.localRotation, InfoStatePoint3.Rotation, _lerp / _div);
                    SetObjectOrientation(Info, infoOri);
                    _infoPosReached = Info.transform.localPosition == InfoStatePoint3.Position;
                    
                }                
            }
            yield return _wait;
        }

        _lerp = 0;        
        RightHighlighter.ClickingInfo = true;
        InfoText.text = State3;

        // STATE 3
        while (!_door1Open)
        {
            if (_door1Click && !_door1Open)
            {
                _lerp += Time.deltaTime;
                door1Ori.Position = Vector3.Lerp(Door1.transform.localPosition, Door1Opened.Position, _lerp / _div);
                door1Ori.Rotation = Quaternion.Lerp(Door1.transform.localRotation, Door1Opened.Rotation, _lerp / _div);
                SetObjectOrientation(Door1, door1Ori);
                _door1Open = Door1.transform.localPosition == Door1Opened.Position;
            }
            yield return _wait;
        }

        _infoPosReached = false;
        _lerp = 0;
        InfoText.text = State4;
        _lastPlayerPos = StolperwegeHelper.User.transform.position;
        
        // STATE 4
        while (!_checkPoint2Reached || !_infoPosReached)
        {
            if (!_checkPoint2Reached && StolperwegeHelper.GetDistance2DToPlayer(Checkpoint2) < 0.75f)
            {
                Checkpoint2.GetComponent<Renderer>().material.color = Color.green;
                _checkPoint2Reached = true;                
            }   
            
            if (!_changeInfoPos && _lastPlayerPos != StolperwegeHelper.User.transform.position)
                _changeInfoPos = true;
            if (_changeInfoPos && !_infoPosReached)
            {
                _lerp += Time.deltaTime;
                infoOri.Position = Vector3.Lerp(Info.transform.localPosition, InfoStatePoint4.Position, _lerp / _div);
                infoOri.Rotation = Quaternion.Lerp(Info.transform.localRotation, InfoStatePoint4.Rotation, _lerp / _div);
                SetObjectOrientation(Info, infoOri);
                _infoPosReached = Info.transform.localPosition == InfoStatePoint4.Position;
            }
            yield return _wait;
        }

        _infoPosReached = false;
        _lerp = 0;
        LeftHighLighter.PointingInfo = false;
        RightHighlighter.PointingInfo = false;
        RightHighlighter.ClickingInfo = false;
        LeftHighLighter.GrabbingInfo = true;
        RightHighlighter.GrabbingInfo = true;
        LeftHighLighter.ResetHighlight();
        RightHighlighter.ResetHighlight();
        InfoText.text = State5;

        while (!_door2Open)
            yield return _wait;

        InfoText.text = State6;
        LeftHighLighter.GrabbingInfo = false;
        RightHighlighter.GrabbingInfo = false;
        LeftHighLighter.ResetHighlight();
        RightHighlighter.ResetHighlight();

        while (true)
        {
            if (Solved) break;
            yield return _wait;
        }

        TerminalInput.GetComponent<Collider>().enabled = false;        
        RestartButton.gameObject.SetActive(true);
        yield break;

    }


    private void Reset()
    {
        if (!_initialized) Init();
        
        StartCoroutine(StolperwegeHelper.FadeOutTeleportationTo(transform.TransformPoint(UserStartPoint), () => 
        {
            InfoText.text = State0;
            SetObjectOrientation(Info, InfoStatePoint0);

            Checkpoint1.GetComponent<Renderer>().material.color = StolperwegeHelper.GUCOLOR.GOETHEBLAU;
            Checkpoint2.GetComponent<Renderer>().material.color = StolperwegeHelper.GUCOLOR.GOETHEBLAU;
            SetObjectOrientation(Door1, Door1Closed);
            SetObjectOrientation(Door2, Door2Closed);
            Opener2.ResetLever();
            SafetyOpener.GetComponent<Collider>().enabled = true;
            SafetyOpener.GetComponent<Renderer>().material.color = Color.red;
            RestartButton.gameObject.SetActive(false);

            Opener1.gameObject.SetActive(false);
            LeftControllerModel.SetActive(true);
            LeftHighLighter.MovementInfo = true;

            SlidingPuzzle.Reset();
            SlotPuzzle.Reset();
            BlueCode = Random.Next(10);
            RedCode = Random.Next(10);

            if (BlueCode + RedCode > 9)
            {
                Operator.text = "-";
                GreenCode = (BlueCode >= RedCode) ? BlueCode - RedCode : RedCode -BlueCode;
                FirstTerm.GetComponent<Renderer>().material.color = (BlueCode >= RedCode) ? Color.blue : Color.red;
                SecondTerm.GetComponent<Renderer>().material.color = (BlueCode >= RedCode) ? Color.red : Color.blue;

            } else
            {
                Operator.text = "+";
                FirstTerm.GetComponent<Renderer>().material.color = Color.blue;
                SecondTerm.GetComponent<Renderer>().material.color = Color.red;
                GreenCode = RedCode + BlueCode;
            }
            
            Solution = RedCode + "" + GreenCode + "" + BlueCode;

            SlidingPuzzle.RewardChar = "" + BlueCode;
            SlotPuzzle.RewardChar = "" + RedCode;

            TerminalInput.Text = "";
            TerminalMessage.text = "";
        }));        
    }
    
    private void SetObjectOrientation(GameObject go, Orientation ori)
    {
        go.transform.localPosition = ori.Position;
        go.transform.localRotation = ori.Rotation;
    }

    public void SetDoor2Position(float heightPercent)
    {
        Door2.transform.localPosition = Door2Opened.Position + Vector3.up * 4 * heightPercent;
        _door2Open = Door2.transform.localPosition == Door2Opened.Position;
    }
    
}

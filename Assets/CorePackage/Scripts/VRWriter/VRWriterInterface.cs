using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VRWriterInterface : MonoBehaviour {

    public static KeyboardLayouts.Layout DEFAULT_LAYOUT = KeyboardLayouts.Layout.German;

    public string INFO_TEXT
    {
        get
        {
            string res = "In order to use any key, point on it first with the active hand, " + 
                         "then press the trigger-button on that hand, to activate it.";
            return res;
        }
    }


    public VRWriter VRWriter { get; private set; }
    public VRWriterKeyboard Keyboard { get; private set; }
    public VRWriterEditor Editor { get; private set; }
    public InteractiveButton OptionButton { get; private set; }
    public InteractiveButton RecordButton { get; private set; }
    public InteractiveButton EmptyFieldButton { get; private set; }
    public InteractiveButton InformationButton { get; private set; }
    public InteractiveButton DoneButton { get; private set; }
    public InteractiveButton CloseButton { get; private set; }
    public InteractiveCheckbox LinebreakController { get; private set; }
    public KeyboardEditText Inputfield { get { return VRWriter.Inputfield; } }
    public GameObject Options { get; private set; }
    public GameObject Informations { get; private set; }

    public event Action<string> DoneClicked;


    private KeyboardLayouts.Layout _activeKeyboardLayoutType;
    public KeyboardLayouts.Layout ActiveKeyboardLayoutType
    {
        get { return _activeKeyboardLayoutType; }
        set
        {
            if (!KeyboardLayouts.Layouts.ContainsKey(value)) return;
            _activeKeyboardLayoutType = value;
            Keyboard.ActiveKeyboardLayoutType = _activeKeyboardLayoutType;
        }
    }

    private bool _optionsOn = false;
    private bool _optAnimOn = false;
    private float _optLerp;
    private Vector3 _optVisiblePos;
    private Vector3 _optHiddenPos;
    private Quaternion _optTargetRot;
    public bool OptionsOn
    {
        get
        {
            return _optionsOn;
        }
        set
        {
            if (_optionsOn == value) return;
            _optionsOn = value;
            _optAnimOn = true;
            _optLerp = 0;
        }
    }

    private bool _infoOn = false;
    private bool _infoAnimOn = false;
    private float _infoLerp;
    private Vector3 _infoVisiblePos;
    private Vector3 _infoHiddenPos;
    private Quaternion _infoTargetRot;
    public bool InformationOn
    {
        get
        {
            return _infoOn;
        }
        set
        {
            if (_infoOn == value) return;
            _infoOn = value;
            _infoAnimOn = true;
            _infoLerp = 0;
        }
    }

    public bool TextRecordingOn { get; private set; }

    public void Init()
    {
        VRWriter = transform.parent.GetComponent<VRWriter>();
        Keyboard = transform.Find("Keyboard").GetComponent<VRWriterKeyboard>();
        Editor = transform.Find("Inputfield").GetComponent<VRWriterEditor>();
        Options = transform.Find("Options").gameObject;
        Informations = transform.Find("Informations").gameObject;
        OptionButton = transform.Find("Controller/OptionsButton").GetComponent<InteractiveButton>();
        RecordButton = transform.Find("Controller/RecordButton").GetComponent<InteractiveButton>();
        EmptyFieldButton = transform.Find("Controller/EmptyFieldButton").GetComponent<InteractiveButton>();
        InformationButton = transform.Find("Controller/InformationButton").GetComponent<InteractiveButton>();
        DoneButton = transform.Find("Controller/DoneButton").GetComponent<InteractiveButton>();
        CloseButton = transform.Find("Controller/CloseButton").GetComponent<InteractiveButton>();

        LinebreakController = transform.Find("Options/LinebreakController").GetComponent<InteractiveCheckbox>();

        _infoVisiblePos = Informations.transform.localPosition;
        _infoHiddenPos = transform.InverseTransformPoint(InformationButton.transform.position);
        _infoTargetRot = Informations.transform.localRotation;
        _optVisiblePos = Options.transform.localPosition;
        _optHiddenPos = transform.InverseTransformPoint(OptionButton.transform.position);
        _optTargetRot = Options.transform.localRotation;

        Informations.transform.Find("Infotext").GetComponent<TextMeshPro>().text = INFO_TEXT;
        Informations.transform.localPosition = _infoHiddenPos;
        Informations.transform.localScale = Vector3.one * 0.01f;
        Informations.transform.localRotation = Quaternion.identity;

        Options.transform.localPosition = _optHiddenPos;
        Options.transform.localScale = Vector3.one * 0.01f;
        Options.transform.localRotation = Quaternion.identity;

        OptionButton.OnClick = () => 
        {
            OptionsOn = !OptionsOn;
            OptionButton.ButtonOn = OptionsOn;
        };


        InformationButton.OnClick = () =>
        {
            InformationOn = !InformationOn;
            InformationButton.ButtonOn = InformationOn;
        };

        EmptyFieldButton.OnClick = () =>
        {            
            Editor.EmptyInputfield();
            EmptyFieldButton.Active = false;
            DoneButton.Active = false;
        };
        EmptyFieldButton.Active = false;

        RecordButton.OnClick = () =>
        {
            TextRecordingOn = !TextRecordingOn;
            RecordButton.ButtonOn = !RecordButton.ButtonOn;
            if (TextRecordingOn) StolperwegeHelper.WordRecognizer.StartDictation();
            else StolperwegeHelper.WordRecognizer.StartListening();
        };
        TextRecordingOn = false;

        DoneButton.OnClick = () =>
        {
            if (VRWriter.Inputfield != null)
            {
                if (VRWriter.Inputfield.IsNumberField && VRWriter.Inputfield.EnableFloatingPoint)
                {
                    if (Editor.InputText.StartsWith("."))
                    {
                        if (VRWriter.Inputfield.ChangeTextOnCommit) VRWriter.Inputfield.Text = "0" + Editor.InputText;
                        VRWriter.Inputfield.Text = "0" + Editor.InputText;
                    }
                    else if (Editor.InputText.StartsWith("-."))
                    {
                        if (VRWriter.Inputfield.ChangeTextOnCommit) VRWriter.Inputfield.Text = Editor.InputText[0] + "0" + Editor.InputText.Substring(1);
                        VRWriter.Inputfield.Text = Editor.InputText[0] + "0" + Editor.InputText.Substring(1);
                    }
                    else
                    {
                        if (VRWriter.Inputfield.ChangeTextOnCommit) VRWriter.Inputfield.Text = Editor.InputText;
                        VRWriter.Inputfield.Text = Editor.InputText;
                    }
                }
                else
                {
                    if (VRWriter.Inputfield.ChangeTextOnCommit) VRWriter.Inputfield.Text = Editor.InputText;
                    VRWriter.Inputfield.Text = Editor.InputText;
                }
                VRWriter.Inputfield.Commit();
                VRWriter.Active = false;
            }
            else if(DoneClicked != null)
            {
                DoneClicked(Editor.InputText);
                VRWriter.Active = false;
            }
            else
                Editor.CreateTextObject();
            Editor.EmptyInputfield();
            EmptyFieldButton.Active = false;
            DoneButton.Active = false;
        };
        DoneButton.Active = false;

        CloseButton.OnClick = () => { StolperwegeHelper.VRWriter.Active = false; };
        //FadeoutController.executeOnClick = () =>
        //{
        //    VRWriter.CabinTransparency = 1 - FadeoutController.SliderValue;
        //};
        //FadeoutController.InfoText = "Use the right " + StolperwegeHelper.CONTROLLER_AXIS_NAME + "\nto change the background transparency.";

        LinebreakController.OnClick = () =>
        {
            Editor.ShowLinebreaks = !Editor.ShowLinebreaks;
            LinebreakController.Status = Editor.ShowLinebreaks ? InteractiveCheckbox.CheckboxStatus.AllChecked : InteractiveCheckbox.CheckboxStatus.NoneChecked;
        };
    }

    public void Update()
    {
        if (_optAnimOn)
        {
            _optLerp += Time.deltaTime;
            if (OptionsOn) ShowOptions();
            else HideOptions();
        }

        if (_infoAnimOn)
        {
            _infoLerp += Time.deltaTime;
            if (InformationOn) ShowInformations();
            else HideInformations();
        }
    }

    private void ShowOptions()
    {
        Options.transform.localPosition = Vector3.Slerp(Options.transform.localPosition, _optVisiblePos, _optLerp);
        Options.transform.localRotation = Quaternion.Slerp(Options.transform.localRotation, _optTargetRot, _optLerp);
        Options.transform.localScale = Vector3.Slerp(Options.transform.localScale, Vector3.one, _optLerp);
        if (Options.transform.localPosition == _optVisiblePos)
        {
            _optAnimOn = false;
            _optLerp = 0;
        }
    }

    private void HideOptions()
    {
        Options.transform.localPosition = Vector3.Slerp(Options.transform.localPosition, _optHiddenPos, _optLerp);
        Options.transform.localRotation = Quaternion.Slerp(Options.transform.localRotation, Quaternion.identity, _optLerp);
        Options.transform.localScale = Vector3.Slerp(Options.transform.localScale, Vector3.one * 0.01f, _optLerp);
        if (Options.transform.localPosition == Vector3.zero)
        {
            _optAnimOn = false;
            _optLerp = 0;
        }
    }

    private void ShowInformations()
    {
        Informations.transform.localPosition = Vector3.Slerp(Informations.transform.localPosition, _infoVisiblePos, _infoLerp);
        Informations.transform.localRotation = Quaternion.Slerp(Informations.transform.localRotation, _infoTargetRot, _infoLerp);
        Informations.transform.localScale = Vector3.Slerp(Informations.transform.localScale, Vector3.one, _infoLerp);
        if (Informations.transform.localPosition == _infoVisiblePos)
        {
            _infoAnimOn = false;
            _infoLerp = 0;
        }
    }

    private void HideInformations()
    {
        Informations.transform.localPosition = Vector3.Slerp(Informations.transform.localPosition, _infoHiddenPos, _infoLerp);
        Informations.transform.localRotation = Quaternion.Slerp(Informations.transform.localRotation, Quaternion.identity, _infoLerp);
        Informations.transform.localScale = Vector3.Slerp(Informations.transform.localScale, Vector3.one * 0.01f, _infoLerp);
        if (Informations.transform.localPosition == Vector3.zero)
        {
            _infoAnimOn = false;
            _infoLerp = 0;
        }
    }
}

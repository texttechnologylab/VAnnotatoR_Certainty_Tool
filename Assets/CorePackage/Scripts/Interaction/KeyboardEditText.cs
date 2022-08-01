using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class KeyboardEditText : InteractiveObject {

    public TextMeshPro inputField;

    public bool Private = false;
    [HideInInspector]
    public bool IsNumberField = false;

    [HideInInspector]
    public bool EnableNegativeNumber = true;
    [HideInInspector]
    public bool EnableFloatingPoint = true;
    [HideInInspector]
    public int MaxChars = -1;

    public bool OnlyActiveAllowed = false;
    public bool ChangeTextOnCommit = true;

    public string _descrtiption;
    public string Description
    {
        get
        {
            return _descrtiption;
        }

        set
        {
            _descrtiption = value;

            if (Text.Equals(""))
                inputField.text = _descrtiption;
        }
    }

    float _oldValue, _newValue;
    public bool InputChanged
    {
        get
        {
            if (IsNumberField)
            {

                _oldValue = float.Parse(_lastValue.Replace('.', ','));
                _newValue = float.Parse(Text.Replace('.', ','));
                return _oldValue != _newValue;
            }
            return !_lastValue.Equals(Text);
        }
    }

    public CommitCallback OnCommit { get; set; }

    public delegate void CommitCallback(string text, GameObject go);

    public override void Awake()
    {
        base.Awake();
        inputField = GetComponentInChildren<TextMeshPro>();
        if (inputField == null)
            Debug.LogError("Could not find TextMeshPro!");
    }

    public override void Start()
    {

        base.Start();

        Transform plane = transform.Find("Plane");
        //if (plane != null)
        //    plane.GetComponent<MeshRenderer>().material.color = StolperwegeHelper.GUCOLOR.LICHTBLAU;

        //blockOutline = true;

        Description = Description;

        if (OnClick == null ) OnClick = ActivateWriter;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        //if (StolperwegeHelper.VRWriter != null && other.name.Contains("Finger"))
        //{

        //    InputInterface.ControllerType type = (other.tag.Contains("left")) ? InputInterface.ControllerType.LHAND : InputInterface.ControllerType.RHAND;
        //    InputInterface.setVibration(type, 0.5f, 10);
        //    ActivateWriter();

        //}

        if (other.GetComponent<SimpleText>() != null)
            Text = other.GetComponent<SimpleText>().Text;
            
    }

    private string _lastValue;
    public void ActivateWriter()
    {
        _lastValue = Text;
        StolperwegeHelper.VRWriter.Inputfield = this;
    }

    private string _text = null;
    public virtual string Text
    {
        get
        {
            if (_text == null) _text = inputField.text;
            return _text;
        }

        set
        {
            //double i;
            //if (NumberField)
            //    if (!double.TryParse(value, out i))
            //        return;

            if (IsNumberField)
            {
                if (EnableFloatingPoint) _text = value.Replace(",", ".");
                else _text = value.Replace(",", "");
                _text = _text.Replace("+", "");
            } else
                _text = value;

            if (!Private)
            {
                inputField.text = _text;
            }
            else
                inputField.text = System.Text.RegularExpressions.Regex.Replace(_text, ".", "*", System.Text.RegularExpressions.RegexOptions.Singleline);
        }
    }

    public virtual void Commit()
    {
        OnCommit?.Invoke(Text,gameObject);
    }

    public virtual void Deselect()
    {

    }

}



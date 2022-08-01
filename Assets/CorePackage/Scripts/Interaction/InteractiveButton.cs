using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class InteractiveButton : InteractiveObject
{
    
    [NonSerialized]
    public Color ActiveButtonOffColor = new Color32(255, 255, 255, 255);
    [NonSerialized]
    public Color ActiveButtonOnColor = StolperwegeHelper.GUCOLOR.LICHTBLAU;
    [NonSerialized]
    public Color InactiveButtonOffColor = new Color32(10, 10, 10, 255);
    [NonSerialized]
    public Color InactiveButtonOnColor = StolperwegeHelper.GUCOLOR.GOETHEBLAU;

    private TextMeshPro _description;
    public TextMeshPro Description
    {
        get
        {
            if (_description == null && transform.Find("Description") != null)
                _description = transform.Find("Description").GetComponent<TextMeshPro>();
            return _description;
        }
    }
    
    private TextMeshPro _buttonText;
    public TextMeshPro ButtonText
    {
        get
        {
            if (_buttonText == null)
            {
                Transform tag = transform.Find("Tag");
                _buttonText = (tag != null) ? tag.GetComponent<TextMeshPro>() : null;
            }
            return _buttonText;
        }
            
    }

    private object _buttonValue;
    public object ButtonValue
    {
        get { return _buttonValue; }
        set
        {
            _buttonValue = value;
            if (Description != null)
                Description.text = _buttonValue.ToString();
        }
    }

    protected bool _buttonOn = false;
    public virtual bool ButtonOn
    {
        get
        {
            return _buttonOn;
        }
        set
        {
            _buttonOn = value;
            Color color = DetermineStatusColor();
            ButtonText.color = color;
            ButtonText.ForceMeshUpdate();
        }
    }

    protected bool _active = true;
    public virtual bool Active
    {
        get
        {
            return _active;
        }
        set
        {
            if (value == _active) return;

            _active = value;

            GetComponent<Collider>().enabled = _active;
            if (ButtonText == null) return;
            Color color = DetermineStatusColor();
            ButtonText.color = color;
            ButtonText.ForceMeshUpdate();
        }
    }

    public override void Start()
    {
        //PartsToHighlight = new List<Renderer>();
        if (transform.Find("Outliner") != null)
        {
            SearchForParts = false;
            PartsToHighlight.Add(transform.Find("Outliner").GetComponent<MeshRenderer>());
        }

        else
        {
            SearchForParts = true;
        }
        HighlightPower = 0.8f;
        base.Start();           
        
    }

    //protected Vector4 highlight;
    protected override void SetHighlight()
    {
        if (PartsToHighlight == null) return;
        base.SetHighlight();
        //if (ButtonText == null) return;
        //ButtonText.color = (!Active) ? InactiveButtonOffColor : ActiveButtonOffColor;
    }

    //public virtual void ChangeColor()
    //{
    //    if (!_baseInit) Start();
    //    SetHighlight();
    //}

    public virtual void ChangeText(string text)
    {
        ButtonText.text = text;
        ButtonText.ForceMeshUpdate();
    }

    public virtual void ChangeTextColor(Color color)
    {
        if (ButtonText == null) return;
        ButtonText.color = color;
    }

    public void ResetButtonStatus()
    {
        ButtonOn = false;
        Active = true;
    }

    private Color DetermineStatusColor()
    {
        Color color;
        if (Active)
            color = (_buttonOn) ? ActiveButtonOnColor : ActiveButtonOffColor;
        else
            color = (_buttonOn) ? InactiveButtonOnColor : InactiveButtonOffColor;
        return color;
    }


    //protected override void OnTriggerEnter(Collider other)
    //{
    //    base.OnTriggerEnter(other);
    //    if (other.tag.Contains("Finger"))
    //    {
    //        executeOnClick?.Invoke();
    //        if (asyncExecuteOnClick != null) StartCoroutine(asyncExecuteOnClick());
    //    }
    //}
}

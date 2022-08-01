using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveCheckbox : InteractiveButton 
{
    [NonSerialized]
    public new Color ActiveButtonOnColor = new Color32(255, 255, 255, 255);
    [NonSerialized]
    public new Color InactiveButtonOnColor = new Color32(10, 10, 10, 255);

    public enum CheckboxStatus { AllChecked, NoneChecked, PartsChecked };
    public string AllChecked = "\xf00c";
    public string PartsChecked = "\xf0c8";
    public string NoneChecked = "";
    public override bool ButtonOn
    {
        get
        {
            return base.ButtonOn;
        }

        set
        {
            _buttonOn = value;
            if (_buttonOn) Status = CheckboxStatus.AllChecked;
            else Status = CheckboxStatus.NoneChecked;
        }
    }

    private CheckboxStatus _status;
    public CheckboxStatus Status
    {
        get { return _status; }
        set
        {
            _status = value;
            if (_status == CheckboxStatus.AllChecked) ChangeText(AllChecked);
            if (_status == CheckboxStatus.NoneChecked) ChangeText(NoneChecked);
            if (_status == CheckboxStatus.PartsChecked) ChangeText(PartsChecked);
        }
    }

}

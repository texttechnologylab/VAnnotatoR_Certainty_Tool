using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class VRWriterInterfaceKey : InteractiveButton {

    private VRWriterKeyboard Keyboard;
    public Key Key { get; private set; }
    
    public void Init(Key key)
    {
        Key = key;
        Keyboard = transform.parent.GetComponent<VRWriterKeyboard>();
        SetKeyMode();
        OnClick = Write;
    }
    
    public void SetKeyMode()
    {
        ButtonText.text = "";
        switch (Keyboard.Mode)
        {
            case VRWriterKeyboard.KeyboardMode.Shift:
                if (Key.ShiftValue != '\0')
                    ButtonText.text = "" + Key.ShiftIcon;
                break;
            case VRWriterKeyboard.KeyboardMode.AltGr:
                if (Key.AltGrValue != '\0')
                    ButtonText.text = "" + Key.AltGrIcon;
                break;
            default:
                if (Key.Value != '\0')
                    ButtonText.text = "" + Key.Icon;
                break;
        }
        GetComponent<Collider>().enabled = !ButtonText.text.Equals("");
    }

    public void Write()
    {
        if (Keyboard.Interface.Inputfield != null &&
            Keyboard.Interface.Inputfield.MaxChars == Keyboard.Interface.Editor.InputText.Length) return;
        if (Keyboard.Mode == VRWriterKeyboard.KeyboardMode.Normal)
            Keyboard.Interface.Editor.Write("" + Key.Value, transform.position, transform.rotation);
        else if (Keyboard.Mode == VRWriterKeyboard.KeyboardMode.Shift)
            Keyboard.Interface.Editor.Write("" + Key.ShiftValue, transform.position, transform.rotation);
        else
            Keyboard.Interface.Editor.Write("" + Key.AltGrValue, transform.position, transform.rotation);
    }

    public void WriteWithMode(VRWriterKeyboard.KeyboardMode mode)
    {
        string res = "";
        switch (mode)
        {
            case VRWriterKeyboard.KeyboardMode.Shift:
                if (Key.ShiftValue != '\0')
                    res += Key.ShiftValue;
                break;
            case VRWriterKeyboard.KeyboardMode.AltGr:
                if (Key.AltGrValue != '\0')
                    res += Key.AltGrValue;
                break;
            default:
                if (Key.Value != '\0')
                    res += Key.Value;
                break;
        }
        Keyboard.Interface.Editor.Write(res, transform.position, transform.rotation);
    }

}

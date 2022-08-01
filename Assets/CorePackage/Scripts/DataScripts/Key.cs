using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key {

    public char Value { get; private set; }
    public string Icon { get; private set; }
    public char ShiftValue { get; private set; }
    public string ShiftIcon { get; private set; }
    public char AltGrValue { get; private set; }
    public string AltGrIcon { get; private set; }
    public Vector2 Position { get; private set; }

    public Key(char value, char shiftValue, char altGrValue, float x, float y, string icon=null, string sIcon= null, string aIcon= null)
    {
        Value = value;
        ShiftValue = shiftValue;
        AltGrValue = altGrValue;
        Icon = (icon == null) ? "" + Value : icon;
        ShiftIcon = (sIcon == null) ? "" + ShiftValue : sIcon;
        AltGrIcon = (aIcon == null) ? "" + AltGrValue : aIcon;
        Position = new Vector2(x, y);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRWriterCursor : InteractiveObject
{

    public VRWriterInputfieldPosition Position { get; private set; }
    public VRWriterEditor InputField { get; private set; }

    public int Column { get { return Position.Column; } }
    public int Row { get { return Position.Row; } }

    public override void Awake()
    {
        base.Awake();
    }

    public void Init(VRWriterInputfieldPosition pos)
    {
        Position = pos;
        InputField = Position.InputField;        
    }

    public void ChangePosition(int[] pos)
    {
        Position.ChangePosition(pos);
        transform.localPosition = Position.ToInputfieldPosition();
    }

    public void ChangePosition(VRWriterInputfieldPosition pos)
    {
        Position.ChangePosition(pos);
        transform.localPosition = Position.ToInputfieldPosition();
    }
}

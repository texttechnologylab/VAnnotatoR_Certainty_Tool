using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VRWriterLetter : InteractiveButton {

    public static string LINEBREAK = "\xf3be";

    public VRWriterInputfieldPosition Position { get; private set; }
    public VRWriterEditor Inputfield { get; private set; }
    
    private Vector3 _targetPos;
    private Vector3 _targetRot;
    private bool _animOn;
    private float _animTime;
    public Vector3 TargetPosition
    {
        get { return _targetPos; }
        set
        {
            if (value == _targetPos) return;
            _targetPos = value;
            _targetRot = new Vector3(90, 180, 0);
            _animOn = true;
            _animTime = 0;
        }
    }

    public string Character { get; private set; }
    public int Column { get { return Position.Column; } }
    public int Row { get { return Position.Row; } }
    public bool IsLastLetter { get { return Inputfield.Letters[Inputfield.Letters.Count - 1].Equals(this); } }
    public int[] PreviousPosition { get { return Position.GetPreviousPositon(); } }
    public int[] NextPosition { get { return Position.GetNextPosition(); } }
    public bool IsLinebreak { get { return Character.Equals("\n"); } }
    public bool IsWhiteSpace { get { return Character.Equals(" "); } }
    public int ArrayIndex { get { return Position.CalculateArrayIndex(); } }
    
    public override void Awake()
    {
        base.Awake();
        OnClick = ActivateCursor;
    }
    
    public void Update()
    {
    	
    	if (_animOn)
        {
            _animTime += Time.deltaTime / Inputfield.LETTER_ANIMATION_SPEED;
            Move();
        }
    }
    
    private void Move() 
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, _targetPos, _animTime);
        transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, _targetRot, _animTime);
        if (transform.localPosition == _targetPos)
        {
            _animOn = false;
            _animTime = 0;
        }
    }

    private GameObject _plane;
    private GameObject _outliner;

    public void Init(string input, int column, int row, VRWriterEditor field, bool hideChar)
    {
        base.Start();
        Character = input;
        ButtonText.text = (IsLinebreak) ? LINEBREAK : (hideChar) ? "*" : input;
        name = (IsLinebreak) ? "Linebreak" : (IsWhiteSpace) ? "Whitespace" : (hideChar) ? "*" : input;
        Inputfield = field;
        VRWriterInputfieldPosition.Type type = (IsLinebreak) ? VRWriterInputfieldPosition.Type.Linebreak : VRWriterInputfieldPosition.Type.Letter;
        Position = new VRWriterInputfieldPosition(column, row, Inputfield, type);
        TargetPosition = Position.ToInputfieldPosition();
        _plane = transform.Find("Plane").gameObject;
        _outliner = transform.Find("Outliner").gameObject;
        _plane.SetActive(!IsLinebreak || Inputfield.ShowLinebreaks);
        _outliner.SetActive(_plane.activeInHierarchy);
        ButtonText.enabled = !IsLinebreak || Inputfield.ShowLinebreaks;
    }

    public void ChangePosition(int[] pos)
    {
        Position.ChangePosition(pos);
        //transform.localPosition = Position.ToInputfieldPosition();
        TargetPosition = Position.ToInputfieldPosition();
    }

    public void ActivateCursor()
    {
        Inputfield.Cursor.gameObject.SetActive(false);
        Inputfield.LetterCursor.SetActive(true);
        Inputfield.LetterCursor.transform.SetParent(transform);
        Inputfield.LetterCursor.transform.localPosition = new Vector3(-0.02f, 0, 0.028f);
        Inputfield.LetterCursor.transform.localEulerAngles = Vector3.right * 180;
        Inputfield.CursorLocation.ChangePosition(Position);
        Inputfield.Interface.Keyboard.ToEnd.Active = true;
    }

    public override bool Equals(object other)
    {
        if (other == null || !(other is VRWriterLetter)) return false;
        VRWriterLetter letter = (VRWriterLetter)other;
        return letter.Character.Equals(Character) && letter.Position.Equals(Position);
    }

    public override int GetHashCode()
    {
        var hashCode = 582049701;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Character);
        hashCode = hashCode * -1521134295 + IsLinebreak.GetHashCode();
        return hashCode;
    }

    public override string ToString()
    {
        return "Letter: " + ButtonText.text + " in Row: " + Row + " and Column: " + Column;
    }
}
    
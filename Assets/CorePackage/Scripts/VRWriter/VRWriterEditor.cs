using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class VRWriterEditor : MonoBehaviour {

    public static float INPUTFIELD_WIDTH = 1.4f;
    public static float INPUTFIELD_HEIGHT = 0.8f;
    public static float LETTER_SIZE = 0.05f;
    public static float CURSOR_SIZE = 0.01f;
    public static int ROW_LENGTH = Mathf.CeilToInt(INPUTFIELD_WIDTH / LETTER_SIZE);
    public static float ROW_PADDING = 0.025f;
    public float LETTER_ANIMATION_SPEED = 1f;
    
    public VRWriterInterface Interface { get; private set; }

    public VRWriterCursor Cursor;
    public GameObject LetterCursor;
    public VRWriterInputfieldPosition CursorLocation;

    public string InputText
    {
        get
        {
            string res = "";
            for (int i = 0; i < Letters.Count; i++)
                res += Letters[i].Character;
            return res;
        }
    }
    public List<VRWriterLetter> Letters;
    public VRWriterLetter LastLetter
    {
        get
        {
            if (Letters.Count == 0) return null;
            return Letters[Letters.Count - 1];
        }
    }

    public List<int> RowLengths;
    public int FirstVisibleRow = 0;
    
    private int _rowCount = 0;
    public int RowCount
    {
    	get
    	{
    		if (_rowCount == 0) 
    		{
    			float actual = 0;
    			while (actual < INPUTFIELD_HEIGHT)
    			{
    				actual += LETTER_SIZE + ROW_PADDING;
    				_rowCount += 1;
    			}
    		}
    		return _rowCount;
    	}
    }
    public bool ShowLinebreaks;

    public bool HasPrivateText { get { return Interface.Inputfield != null && Interface.Inputfield.Private; } }

    public void Awake()
    {
        Letters = new List<VRWriterLetter>();
        RowLengths = new List<int> { 0 };
        Interface = transform.parent.GetComponent<VRWriterInterface>();
        CursorLocation = new VRWriterInputfieldPosition(0, 0, this, VRWriterInputfieldPosition.Type.Cursor);
        Cursor.gameObject.SetActive(true);
        Cursor.Init(CursorLocation);
        LetterCursor.SetActive(false);
        Cursor.transform.localPosition = Cursor.Position.ToInputfieldPosition();
        ShowLinebreaks = false;
    }

    GameObject letterObject;
    VRWriterLetter w2tl;
    int[] nextPos;
    public void Write(string input, Vector3 keyPosition, Quaternion keyRotation)
    {
        Interface.DoneButton.Active = true;
        Interface.EmptyFieldButton.Active = true;
        letterObject = (GameObject)Instantiate(Resources.Load("Prefabs/VRWriter/LetterPlaceholder"));
        letterObject.transform.SetParent(transform);
        letterObject.transform.position = keyPosition;
        letterObject.transform.rotation = keyRotation;
        w2tl = letterObject.GetComponent<VRWriterLetter>();
        w2tl.Init(input, CursorLocation.Column, CursorLocation.Row, this, HasPrivateText);
        Insert(w2tl);
        nextPos = w2tl.NextPosition;
        if (w2tl.IsLastLetter)
        {
            LetterCursor.SetActive(false);
            Cursor.gameObject.SetActive(true);
            Cursor.ChangePosition(nextPos);
        }
        else
        {
            LetterCursor.SetActive(true);
            Cursor.gameObject.SetActive(false);
            LetterCursor.transform.parent = Letters[w2tl.ArrayIndex + 1].transform;
            LetterCursor.transform.localPosition = new Vector3(-0.02f, 0, 0.028f);
            LetterCursor.transform.localEulerAngles = Vector3.zero;
            CursorLocation.ChangePosition(nextPos);
        }
    }

    VRWriterLetter toDestroy;
    public void Delete(bool forward=false)
    {
        int cArrayPos = CursorLocation.CalculateArrayIndex();
        if (forward)
        {
            if (cArrayPos == Letters.Count) return;
            toDestroy = Letters[cArrayPos];
            Delete(toDestroy);
            if (cArrayPos >= Letters.Count - 1 || cArrayPos == toDestroy.ArrayIndex)
            {
                LetterCursor.transform.parent = transform;
                LetterCursor.SetActive(false);                
                Cursor.gameObject.SetActive(true);
                Cursor.ChangePosition(CursorLocation);
            } else
            {
                LetterCursor.SetActive(true);
                Cursor.gameObject.SetActive(false);
                LetterCursor.transform.parent = Letters[cArrayPos].transform;
                LetterCursor.transform.localPosition = new Vector3(-0.02f, 0, 0.028f);
                LetterCursor.transform.localEulerAngles = Vector3.zero;
            }
            
        } else
        {
            if (cArrayPos == 0) return;
            toDestroy = Letters[cArrayPos - 1];
            Delete(toDestroy);
            if (cArrayPos > Letters.Count)
            {
                LetterCursor.SetActive(false);
                Cursor.gameObject.SetActive(true);
                Cursor.ChangePosition(CursorLocation.GetPreviousPositon());
            } else
            {
                LetterCursor.SetActive(true);
                Cursor.gameObject.SetActive(false);
                CursorLocation.ChangePosition(CursorLocation.GetPreviousPositon());
            }
        }        
        Destroy(toDestroy.gameObject);
        Interface.DoneButton.Active = Letters.Count > 0;
        Interface.EmptyFieldButton.Active = Letters.Count > 0;
    }
    
    private void Insert(VRWriterLetter w2tl)
    {
        int column = w2tl.Column; int row = w2tl.Row;
        
        if (w2tl.IsLinebreak)
        {
            if (row == RowLengths.Count) RowLengths.Add(1);
            else
            {
                int oldLength = RowLengths[row];
                RowLengths.Insert(row + 1, oldLength - column);
                RowLengths[row] = column + 1;
            }
        } else
        {
            if (row == RowLengths.Count) RowLengths.Add(1);
            else
            {
                while (RowLengths[row] == ROW_LENGTH)
                {
                    row++;
                    if (row == RowLengths.Count) RowLengths.Add(0);
                }
                RowLengths[row] += 1;
            }            
        }        
        
        Letters.Insert(w2tl.ArrayIndex, w2tl);
        nextPos = w2tl.NextPosition;

        for (int i = w2tl.ArrayIndex + 1; i < Letters.Count; i++)
        {
            Letters[i].ChangePosition(nextPos);
            nextPos = Letters[i].NextPosition;
        }
    }

    private void Delete(VRWriterLetter w2tl)
    {
        int column = w2tl.Column; int row = w2tl.Row;
        int lastRow = 0;
        bool nextLbFound = false;
        if (w2tl.IsLastLetter && !w2tl.IsLinebreak)
        {
            RowLengths[row] -= 1;
            Letters.RemoveAt(Letters.Count - 1);
            return;
        }
        else if (!w2tl.IsLinebreak)
        {
            bool nextLbOnLineStart = false;
            for (int i=w2tl.ArrayIndex + 1; i<Letters.Count; i++)
            {
                if (!nextLbFound)
                {
                    if (Letters[i].IsLinebreak)
                    {
                        nextLbFound = true;
                        if (Letters[i].Column == 0) nextLbOnLineStart = true;
                    }
                    Letters[i].ChangePosition(Letters[i].PreviousPosition);                    
                }

                else if (nextLbOnLineStart)
                    Letters[i].ChangePosition(new int[] {Letters[i].Column, Letters[i].Row - 1});
                
                if (i == Letters.Count - 1) lastRow = Letters[i].Row;
            }                       
        }
        else
        {
            bool lbOnLineStart = column == 0;
            bool lbRowChanged = false;
            int lbLastRow = 0;
            nextPos = new int[] { w2tl.Column, w2tl.Row };
            for (int i = w2tl.ArrayIndex + 1; i < Letters.Count; i++)
            {
                if (lbOnLineStart)
                    Letters[i].ChangePosition(new int[] { Letters[i].Column, Letters[i].Row - 1 });
                else
                {
                    if (!nextLbFound)
                    {
                        if (Letters[i].IsLinebreak)
                        {
                            nextLbFound = true;
                            lbLastRow = Letters[i].Row;
                        }
                        Letters[i].ChangePosition(nextPos);
                        lbRowChanged = Letters[i].IsLinebreak && lbLastRow != Letters[i].Row;
                        nextPos = Letters[i].NextPosition;
                    }
                    else if (lbRowChanged)
                        Letters[i].ChangePosition(new int[] { Letters[i].Column, Letters[i].Row - 1 });
                }
                if (i == Letters.Count - 1) lastRow = Letters[i].Row;
            }
        }
        Letters.RemoveAt(w2tl.ArrayIndex);
        if (RowLengths.Count > lastRow + 1) RowLengths.RemoveAt(RowLengths.Count - 1);
        int rowLength = 0; int r = 0;
        for (int i = 0; i < Letters.Count; i++)
        {
            if (Letters[i].Row == r) rowLength += 1;
            else
            {
                RowLengths[r] = rowLength;
                rowLength = 1;
                r += 1;
            }
        }
        RowLengths[r] = rowLength;
    }

    public int GetLengthOfRow(int row)
    {
        string s = InputText.Substring(ROW_LENGTH * row);
        if (s.Length >= ROW_LENGTH) return ROW_LENGTH;
        else return s.Length;
    }

    public void ActivateCursor()
    {
        Cursor.gameObject.SetActive(true);
        LetterCursor.SetActive(false);
    }

    public void CursorToEnd()
    {
        ActivateCursor();
        // TODO line changes
        if (Letters.Count == 0 || CursorLocation.EqualsWithArray(LastLetter.NextPosition)) return;
        Cursor.ChangePosition(LastLetter.NextPosition);
    }

    public void EmptyInputfield()
    {
        LetterCursor.transform.parent = transform;
        ActivateCursor();

        for (int i=0; i<Letters.Count; i++)
            Destroy(Letters[i].gameObject);
            

        Cursor.ChangePosition(new int[] { 0, 0 });

        Letters = new List<VRWriterLetter>();
    }

    public void CreateTextObject()
    {
        GameObject result = GameObject.CreatePrimitive(PrimitiveType.Cube);
        SimpleText text = result.AddComponent<SimpleText>();
        text.Init(InputText);
        result.transform.position = transform.position - transform.up * 0.3f;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SimpleText>() != null)
            StartCoroutine(Interface.Keyboard.AutoWriteText(other.GetComponent<SimpleText>().Text));
    }

    public void ChangeRow(int dir)
    {
    	//if (dir > 0 && FirstVisibleRow + RowCount - 1 +dir)
    }

    public void CheckNumericalButtonStatus()
    {
        if (Interface.ActiveKeyboardLayoutType != KeyboardLayouts.Layout.Numerical) return;
        Interface.Keyboard.NegativeOn = !InputText.Contains("-") && CursorLocation.Row == 0 && CursorLocation.Column == 0;
        Interface.Keyboard.FloatingPointOn = !InputText.Contains(".");
    }
}

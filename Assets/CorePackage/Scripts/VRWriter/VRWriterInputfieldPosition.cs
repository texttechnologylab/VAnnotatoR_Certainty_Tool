using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VRWriterInputfieldPosition : IComparable<VRWriterInputfieldPosition> {

    public int Column;
    public int Row;
    public VRWriterEditor InputField;
    public enum Type { Cursor, Letter, Linebreak };
    public Type PosType;

    public VRWriterInputfieldPosition(int c, int r, VRWriterEditor i, Type type)
    {
        Column = c;
        Row = r;
        InputField = i;
        PosType = type;
    }

    public Vector3 ToInputfieldPosition()
    {
        float size = (PosType == Type.Cursor) ? VRWriterEditor.CURSOR_SIZE : VRWriterEditor.LETTER_SIZE;
        float x = VRWriterEditor.INPUTFIELD_WIDTH / 2 - Column * VRWriterEditor.LETTER_SIZE - size / 2;
        float z = VRWriterEditor.INPUTFIELD_HEIGHT / 2 - VRWriterEditor.LETTER_SIZE / 2 - 
                  Row * VRWriterEditor.ROW_PADDING - Row * VRWriterEditor.LETTER_SIZE;
        return new Vector3(x, 0, z);
    }
    
    public int[] GetPreviousPositon()
    {
        if (Column == 0 && Row == 0) return new int[] { -1, -1 };
        if (Column == 0) return new int[] { Mathf.Max(InputField.RowLengths[Row - 1] - 1, 0), Row - 1 };
        return new int[] { Column - 1, Row };
    }

    public int[] GetNextPosition()
    {
        if (Column == VRWriterEditor.ROW_LENGTH - 1 || PosType == Type.Linebreak) return new int[] { 0, Row + 1 };
        else return new int[] { Column + 1, Row };     
    }

    public int CalculateArrayIndex()
    {
        int res = 0;
        for (int r = 0; r < Row; r++)
            res += InputField.RowLengths[r];
        res += Column;
        return res;
    }

    public void ChangePosition(int[] pos)
    {
        Column = pos[0];
        Row = pos[1];
        InputField.CheckNumericalButtonStatus();
    }

    public void ChangePosition(VRWriterInputfieldPosition pos)
    {
        Column = pos.Column;
        Row = pos.Row;
        InputField.CheckNumericalButtonStatus();
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is VRWriterInputfieldPosition)) return false;
        VRWriterInputfieldPosition pos = (VRWriterInputfieldPosition)obj;
        return pos.Column == Column && pos.Row == Row;    
    }

    public bool EqualsWithArray(int[] posArray)
    {
        return posArray[0] == Column && posArray[1] == Row;
    }

    public override int GetHashCode()
    {
        var hashCode = 656739706;
        hashCode = hashCode * -1521134295 + Column.GetHashCode();
        hashCode = hashCode * -1521134295 + Row.GetHashCode();
        return hashCode;
    }

    public override string ToString()
    {
        return "Row: " + Row + " Column: " + Column;
    }

    public int CompareTo(VRWriterInputfieldPosition pos)
    {
        if (Row > pos.Row || (Row == pos.Row && Column > pos.Column)) return 1;
        if (Row == pos.Row && Column == pos.Column) return 0;
        else return -1;
    }

    // Define the is greater than operator.
    public static bool operator > (VRWriterInputfieldPosition operand1, VRWriterInputfieldPosition operand2)
    {
        return operand1.CompareTo(operand2) == 1;
    }

    // Define the is less than operator.
    public static bool operator <(VRWriterInputfieldPosition operand1, VRWriterInputfieldPosition operand2)
    {
        return operand1.CompareTo(operand2) == -1;
    }

    // Define the is greater than or equal to operator.
    public static bool operator >=(VRWriterInputfieldPosition operand1, VRWriterInputfieldPosition operand2)
    {
        return operand1.CompareTo(operand2) >= 0;
    }

    // Define the is less than or equal to operator.
    public static bool operator <=(VRWriterInputfieldPosition operand1, VRWriterInputfieldPosition operand2)
    {
        return operand1.CompareTo(operand2) <= 0;
    }
}

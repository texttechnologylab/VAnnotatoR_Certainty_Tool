using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingPuzzlePiece : InteractiveObject
{

    public SlidingPuzzle Puzzle;

    public bool OnRightPlace { get { return ActualPosition == RightPosition; } }

    private int _actualPosition;
    public int ActualPosition
    {
        get { return _actualPosition; }
        set
        {
            _actualPosition = value;
            transform.localPosition = SlidingPuzzle.POSITION_MAP[_actualPosition] + Vector3.forward * 0.02f;
        }
    }

    public int RightPosition;
    

    public void Init(SlidingPuzzle puzzle, int rightPos)
    {
        Puzzle = puzzle;
        RightPosition = rightPos;

        OnClick = () => { Puzzle.MovePiece(this); };
    }

    public void SetRightPosition()
    {
        ActualPosition = RightPosition;
    }
}

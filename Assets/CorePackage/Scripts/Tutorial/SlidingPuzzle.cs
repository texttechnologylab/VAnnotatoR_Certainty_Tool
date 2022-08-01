using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SlidingPuzzle : MonoBehaviour
{

    public static Vector3[] POSITION_MAP = new Vector3[]
    {
        new Vector3(0.2f, 0.2f, 0f), new Vector3(0f, 0.2f, 0f), new Vector3(-0.2f, 0.2f, 0f),
        new Vector3(0.2f, 0f, 0f), new Vector3(0f, 00f, 0f), new Vector3(-0.2f, 0f, 0f),
        new Vector3(0.2f, -0.2f, 0f), new Vector3(0f, -0.2f, 0f), new Vector3(-0.2f, -0.2f, 0f),
    };

    private SlidingPuzzlePiece[] Pieces;
    private InteractiveButton SlidingPuzzleSolver;
    private GameObject Reward;
    private TextMeshPro RewardText;
    private System.Random Random;
    private int FreeSlot;
    private bool Solved;
    public Color Color = Color.blue;
    public string RewardChar;
    private int _stepCounter;


    // Start is called before the first frame update
    void Start()
    {
        SlidingPuzzleSolver = transform.Find("SlidingPuzzleSolver").GetComponent<InteractiveButton>();
        SlidingPuzzleSolver.OnClick = SolvePuzzle;
        Pieces = GetComponentsInChildren<SlidingPuzzlePiece>();
        for (int i = 0; i < Pieces.Length; i++)
            Pieces[i].Init(this, i + 1);
        Reward = transform.Find("PuzzlePieces/Reward").gameObject;
        RewardText = Reward.GetComponentInChildren<TextMeshPro>();
        RewardText.color = Color;
        Random = new System.Random();
    }

    private int _piecePos;
    public void MovePiece(SlidingPuzzlePiece piece)
    {
        _piecePos = piece.ActualPosition;
        _stepCounter += 1;
        if (_stepCounter > 20) SlidingPuzzleSolver.gameObject.SetActive(true);
        if (Mathf.Abs(_piecePos - FreeSlot) == 1 || Mathf.Abs(_piecePos - FreeSlot) == 3) 
        {
            piece.ActualPosition = FreeSlot;
            FreeSlot = _piecePos;
        }
        CheckPuzzle();
    }

    private void CheckPuzzle()
    {
        Solved = true;
        for (int i = 0; i < Pieces.Length; i++)
            Solved &= Pieces[i].OnRightPlace;

        if (!Solved) return;

        for (int i = 0; i < Pieces.Length; i++)
            Pieces[i].GetComponent<Collider>().enabled = false;
        SlidingPuzzleSolver.gameObject.SetActive(false);
        RewardText.text = RewardChar;
    }

    private int[] toShuffle; private int temp;
    public void Reset()
    {
        RewardText.text = "";
        SlidingPuzzleSolver.gameObject.SetActive(false);
        _stepCounter = 0;
        toShuffle = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        for (int i=0; i<toShuffle.Length; i++)
            Swap(toShuffle, i, i + Random.Next(toShuffle.Length - i));

        FreeSlot = toShuffle[0];

        for (int i = 0; i < Pieces.Length; i++)
        {
            Pieces[i].ActualPosition = toShuffle[i + 1];
            Pieces[i].GetComponent<Collider>().enabled = true;
        }

    }

    private void Swap(int[] arr, int a, int b)
    {
        temp = arr[a];
        arr[a] = arr[b];
        arr[b] = temp;
    }

    private void SolvePuzzle()
    {
        for (int i = 0; i < Pieces.Length; i++)
            Pieces[i].SetRightPosition();
        CheckPuzzle();
    }
}

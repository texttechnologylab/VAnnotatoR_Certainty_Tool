using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SlotPuzzle : MonoBehaviour
{

    private SlotPieceInstantiator Instantiator;
    private GameObject Scheme;
    private TextMeshPro RewardText;
    private GameObject SlotPieces;
    private SlotPuzzleSlot[] SlotList;
    private Vector3[] FilledSlots;
    private System.Random Random;
    private HashSet<int> SlotsToFill;

    public bool Solved { get; private set; }
    public string RewardChar;
    

    public void Init(GameObject scheme, SlotPieceInstantiator inst)
    {
        Instantiator = inst;
        Scheme = scheme;
        RewardText = Scheme.transform.Find("RewardText").GetComponent<TextMeshPro>();
        RewardText.color = Color.red;
        SlotPieces = Scheme.transform.Find("SlotPieces").gameObject;
        SlotList = GetComponentsInChildren<SlotPuzzleSlot>();
        Random = new System.Random();
        SlotsToFill = new HashSet<int>();
    }

    public void CheckPuzzle()
    {
        Solved = true;
        for (int i = 0; i < SlotList.Length; i++)
            Solved &= SlotList[i].Solved;        
        
        if (Solved)
        {
            Instantiator.GetComponent<Collider>().enabled = false;
            Instantiator.PuzzleSolved = true;

            RewardText.text = RewardChar;

            for (int i = 0; i < SlotList.Length; i++)
                SlotList[i].GetComponent<Collider>().enabled = false;            
        }

    }

    GameObject _newSlot;
    public void Reset()
    {
        for (int i = 0; i < SlotPieces.transform.childCount; i++)
            Destroy(SlotPieces.transform.GetChild(i).gameObject);

        for (int i = 0; i < SlotList.Length; i++)
        {
            SlotList[i].GetComponent<Collider>().enabled = true;
            if (SlotList[i].SlotPuzzlePiece != null)
                Destroy(SlotList[i].SlotPuzzlePiece);

        }
        
        Instantiator.GetComponent<Collider>().enabled = true;
        Instantiator.PuzzleSolved = false;

        RewardText.text = "";

        SlotsToFill.Clear();

        for (int i=0; i<3; i++)
        {
            SlotsToFill.Add(Random.Next(i * 3, (i + 1) * 3));
            SlotsToFill.Add(Random.Next(i * 3, (i + 1) * 3));
        }
        

        for (int i = 0; i < SlotList.Length; i++)
        {
            SlotList[i].Init(this, SlotsToFill.Contains(i));
            if (SlotsToFill.Contains(i))
            {
                _newSlot = (GameObject)Instantiate(Resources.Load("Tutorial/Prefabs/SlotPiece"));
                _newSlot.transform.parent = SlotPieces.transform;
                _newSlot.transform.localRotation = Quaternion.identity;
                _newSlot.transform.localPosition = SlidingPuzzle.POSITION_MAP[i] + Vector3.forward * 0.005f;
            }
        }       

    }
}

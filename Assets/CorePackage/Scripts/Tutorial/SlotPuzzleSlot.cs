using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotPuzzleSlot : MonoBehaviour
{

    private SlotPuzzle Puzzle;
    public GameObject SlotPuzzlePiece { get; private set; }
    private bool ShouldBeFilled;

    public bool Solved { get { return (ShouldBeFilled && SlotPuzzlePiece != null) || 
                                      (!ShouldBeFilled && SlotPuzzlePiece == null); } }

    public void Init(SlotPuzzle puzzle, bool shouldBeFilled)
    {
        Puzzle = puzzle;
        ShouldBeFilled = shouldBeFilled;
    }

    private void Update()
    {
        if (SlotPuzzlePiece != null && (SlotPuzzlePiece.transform.position - transform.position).magnitude > 0.15f)
        {   
            SlotPuzzlePiece = null;
            GetComponent<Collider>().enabled = true;
        }
    }

    

    private void OnTriggerEnter(Collider other)
    {
        if (SlotPuzzlePiece == null && other.tag.Equals("SlotPuzzlePiece"))
        {
            SlotPuzzlePiece = other.gameObject;
            SlotPuzzlePiece.transform.SetParent(transform);
            SlotPuzzlePiece.transform.localPosition = Vector3.zero;
            SlotPuzzlePiece.transform.localRotation = Quaternion.identity;
            SlotPuzzlePiece.GetComponent<Collider>().enabled = false;
            Puzzle.CheckPuzzle();
        }
        if (SlotPuzzlePiece != null && other.GetComponent<DragFinger>() != null && other.GetComponent<DragFinger>().GrabedObject == null)
        {
            GetComponent<Collider>().enabled = false;
            SlotPuzzlePiece.GetComponent<Collider>().enabled = true;
            SlotPuzzlePiece.transform.position = other.transform.position;
            SlotPuzzlePiece.GetComponent<InteractiveObject>().OnGrab(other);
            Puzzle.CheckPuzzle();
        }
    }
    
}

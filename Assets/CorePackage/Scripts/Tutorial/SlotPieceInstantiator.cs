using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SlotPieceInstantiator : MonoBehaviour
{

    private string OpenedHand = "\xf256";
    private string ClosedHand = "\xf255";

    private GameObject SlotPuzzlePiece;
    private TextMeshPro GrabAnimator;

    private bool _solved = false;

    private Tutorial Tutorial;
    public bool PuzzleSolved
    {
        get { return _solved; }
        set
        {
            _solved = value;
            if (_solved) GrabAnimator.text = "";
        }
    }

    public void Initialize(Tutorial tutorial)
    {
        Tutorial = tutorial;
        Start();
    }

    // Start is called before the first frame update
    void Start()
    {
        SlotPuzzlePiece = transform.Find("SlotPuzzlePiece").gameObject;
        GrabAnimator = transform.Find("GrabAnimator").GetComponent<TextMeshPro>();
    }

    public float _time;
    public void Update()
    {
        if (Tutorial != null && Tutorial.Active && !PuzzleSolved)
        {
            _time = (_time + Time.deltaTime) % 2f;
            if ((int)_time == 0 && GrabAnimator.text != OpenedHand) GrabAnimator.text = OpenedHand;
            if ((int)_time == 1 && GrabAnimator.text != ClosedHand) GrabAnimator.text = ClosedHand;
        }
    }

    InteractiveObject newPiece;
    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<DragFinger>() != null && other.GetComponent<DragFinger>().GrabedObject == null)
        {
            newPiece = Instantiate(SlotPuzzlePiece).AddComponent<InteractiveObject>();
            newPiece.Grabable = true;
            newPiece.transform.position = other.transform.position;
            newPiece.OnGrab(other);
        }
    }
}

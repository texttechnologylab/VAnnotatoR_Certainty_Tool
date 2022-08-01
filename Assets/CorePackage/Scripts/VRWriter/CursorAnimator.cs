using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CursorAnimator : MonoBehaviour {

    public float AnimationSpeed = 0.8f;
    private TextMeshPro Cursor;

	// Use this for initialization
	void Start () {
        Cursor = GetComponent<TextMeshPro>();
	}

    private float time = 0f;
	void Update () {
        time += Time.deltaTime;
        if (time >= AnimationSpeed)
        {
            time = 0f;
            Cursor.text = (Cursor.text == "") ? "|" : "";
        }
	}
}

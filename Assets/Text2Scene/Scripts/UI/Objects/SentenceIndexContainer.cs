using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SentenceIndexContainer : InteractiveButton
{

    public TextMeshPro Label { get; private set; }
    public BoxCollider Collider { get; private set; }
    private GameObject Background;
    
    public int SentenceIndex { get; private set; }

    public float Width { get { return Background.transform.localScale.x; } }
    public float Height { get { return Background.transform.localScale.y; } }

    public void Initialize()
    {
        Label = transform.Find("Label").GetComponent<TextMeshPro>();
        Background = transform.Find("Background").gameObject;
        Collider = GetComponent<BoxCollider>();
        Collider.enabled = true;
        PartsToHighlight = new List<Renderer>() { Background.GetComponent<MeshRenderer>() };
        SearchForParts = false;
        UseHighlighting = true;
        base.Start();
    }

    public void Setup(int index, float width, float height)
    {
        SentenceIndex = index;
        Label.text = "" + (index + 1);
        Label.rectTransform.sizeDelta = new Vector2(width, height);
        Background.transform.localScale = new Vector3(width, height, 0.001f);
        Collider.size = Background.transform.localScale;
    }

}

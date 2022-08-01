using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ARDisplay : InteractiveObject
{
    private const float LoadingTime = 1f;
    private const float ButtonSize = 0.05f;
    private const float CornerSize = 0.01f;
    private const float MinStartWidth = 0.1f;
    public const float MinWidth = 0.4f;    
    private const float MinStartHeight = 0.1f;
    public const float MinHeight = 0.25f;
    private const string LeftArrow = "\xf0d9";
    private const string RightArrow = "\xf0da";



    private GameObject Corner0;
    private GameObject Corner1;
    private GameObject Corner2;
    private GameObject Corner3;

    private GameObject Edge0;
    private GameObject Edge1;
    private GameObject Edge2;
    private GameObject Edge3;

    private GameObject Display;
    private TextMeshPro DisplayText;
    private InteractiveButton Left;
    private InteractiveButton Right;

    private BoxCollider Collider;

    private GameObject Pointer;
        
    private float _width = MinStartWidth;
    public float Width
    {
        get { return _width; }
        set
        {
            if (value < MinStartWidth || value == _width) return;            
            _width = value;
        }
    }

    private float _height = MinStartHeight;    
    public float Height
    {
        get { return _height; }
        set
        {
            if (value < MinStartHeight || value == _height) return;
            _height = value;
        }
    }

    private int _loadedPage = -1;
    private int LoadedPage
    {
        get { return _loadedPage; }
        set
        {
            int newValue = Mathf.Min(1, Mathf.Max(value, 0));
            if (newValue == _loadedPage) return;
            _loadedPage = newValue;
            Left.gameObject.SetActive(_loadedPage == 1);
            Right.gameObject.SetActive(_loadedPage == 0);
            DisplayInfo();
        }
    }

    private InteractiveObject LastHit;
    private Ray Ray;
    private RaycastHit Hit;
    private InteractiveObject LastDisplayedObject;
    private float LoadingTimer; 
    private bool IsReadingObject;    

    public override void Start()
    {
        UseHighlighting = false;
        Grabable = true;
        DestroyOnObjectRemover = true;
        Removable = true;     
    }

    // Start is called before the first frame update
    public void Initialize()
    {
        name = "ARDisplay";
        Corner0 = (GameObject)Instantiate(Resources.Load("Prefabs/UI/TabletCorner"));
        Corner0.transform.SetParent(transform);        

        Corner1 = (GameObject)Instantiate(Resources.Load("Prefabs/UI/TabletCorner"));
        Corner1.transform.SetParent(transform);        

        Corner2 = (GameObject)Instantiate(Resources.Load("Prefabs/UI/TabletCorner"));
        Corner2.transform.SetParent(transform);        

        Corner3 = (GameObject)Instantiate(Resources.Load("Prefabs/UI/TabletCorner"));
        Corner3.transform.SetParent(transform);               

        Edge0 = (GameObject)Instantiate(Resources.Load("Prefabs/UI/TabletEdge"));
        Edge0.transform.SetParent(transform);        

        Edge1 = (GameObject)Instantiate(Resources.Load("Prefabs/UI/TabletEdge"));
        Edge1.transform.SetParent(transform);

        Edge2 = (GameObject)Instantiate(Resources.Load("Prefabs/UI/TabletEdge"));
        Edge2.transform.SetParent(transform);

        Edge3 = (GameObject)Instantiate(Resources.Load("Prefabs/UI/TabletEdge"));
        Edge3.transform.SetParent(transform);

        Display = (GameObject)Instantiate(Resources.Load("Prefabs/UI/TabletDisplay"));
        Display.transform.SetParent(transform);

        GameObject displayTextObject = new GameObject("DisplayText");
        displayTextObject.transform.SetParent(transform);
        displayTextObject.transform.localPosition = Vector3.zero;
        displayTextObject.transform.localRotation = Quaternion.identity;

        DisplayText = displayTextObject.AddComponent<TextMeshPro>();
        DisplayText.color = Color.black;
        DisplayText.overflowMode = TextOverflowModes.Truncate;
        DisplayText.fontSize = 0.24f;
        DisplayText.alignment = TextAlignmentOptions.Center;

        Collider = gameObject.AddComponent<BoxCollider>();
        Collider.isTrigger = true;

        Pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(Pointer.GetComponent<SphereCollider>());
        Pointer.transform.localScale = Vector3.one * 0.05f;
        Pointer.GetComponent<Renderer>().material = (Material)Instantiate(Resources.Load("Materials/UI/Pointer"));
        Pointer.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Pointer.SetActive(false);

        GameObject lButton = new GameObject("Left");
        lButton.AddComponent<BoxCollider>().size = new Vector3(ButtonSize, ButtonSize, 0.01f);
        lButton.transform.SetParent(transform);
        GameObject tag = new GameObject("Tag");
        tag.transform.SetParent(lButton.transform);
        TextMeshPro text = tag.AddComponent<TextMeshPro>();
        text.color = Color.black;
        text.fontSize = 10f;
        text.fontMaterial = (Material)Instantiate(Resources.Load("Materials/FontPresets/FontAwesomeSolid5Glow"));
        text.rectTransform.localScale = Vector3.one * ButtonSize;
        text.alignment = TextAlignmentOptions.Center;
        tag.transform.localPosition = Vector3.zero;
        tag.transform.localEulerAngles = Vector3.zero;
        GameObject rButton = Instantiate(lButton);
        rButton.name = "Right";
        rButton.transform.SetParent(transform);

        Left = lButton.AddComponent<InteractiveButton>();
        Left.SearchForParts = false;
        Left.UseHighlighting = false;
        Left.Start();
        Left.OnClick = () => { LoadedPage -= 1; };
        Left.gameObject.SetActive(false);
        
        Right = rButton.AddComponent<InteractiveButton>();
        Right.SearchForParts = false;
        Right.UseHighlighting = false;
        Right.Start();
        Right.OnClick = () => { LoadedPage += 1; };
        Right.gameObject.SetActive(false);

        UpdateSize();        
    }

    public void Resize(float width, float height)
    {
        if (Width == width && Height == height) return;
        Width = width;
        Height = height;
        UpdateSize();
    }

    private void UpdateSize()
    {
        Corner0.transform.localPosition = new Vector3(Width / 2, Height / 2, 0);
        Corner0.transform.localEulerAngles = new Vector3(0, 0, 0);

        Corner1.transform.localPosition = new Vector3(-Width / 2, Height / 2, 0);
        Corner1.transform.localEulerAngles = new Vector3(0, 0, 90);

        Corner2.transform.localPosition = new Vector3(-Width / 2, -Height / 2, 0);
        Corner2.transform.localEulerAngles = new Vector3(0, 0, 180);

        Corner3.transform.localPosition = new Vector3(Width / 2, -Height / 2, 0);
        Corner3.transform.localEulerAngles = new Vector3(0, 0, 270);

        Edge0.transform.localScale = new Vector3(Width - CornerSize, CornerSize, CornerSize);
        Edge0.transform.localPosition = new Vector3(0, Height / 2, 0);

        Edge1.transform.localScale = new Vector3(CornerSize, Height - CornerSize, CornerSize);
        Edge1.transform.localPosition = new Vector3(Width / 2, 0, 0);

        Edge2.transform.localScale = new Vector3(Width - CornerSize, CornerSize, CornerSize);
        Edge2.transform.localPosition = new Vector3(0, -Height / 2, 0);

        Edge3.transform.localScale = new Vector3(CornerSize, Height - CornerSize, CornerSize);
        Edge3.transform.localPosition = new Vector3(-Width / 2, 0, 0);

        Display.transform.localPosition = Vector3.zero;
        Display.transform.localScale = new Vector3(Width - CornerSize, Height - CornerSize, CornerSize / 4);
        DisplayText.rectTransform.sizeDelta = Display.transform.localScale;

        Collider.size = new Vector3(Width + CornerSize, Height + CornerSize, CornerSize);
    }

    public void CheckRay()
    {
        int dir = Vector3.Dot(transform.forward, StolperwegeHelper.CenterEyeAnchor.transform.forward) >= 0 ? 1 : -1;
        Vector3 forward = dir * transform.forward;
        Ray = new Ray(transform.position, forward);        
        if (Physics.Raycast(Ray, out Hit, Mathf.Infinity))
        {
            Pointer.SetActive(true);
            Pointer.transform.position = Hit.point;
            if (Hit.collider.GetComponent<InteractiveObject>() != null)
            {
                if (LastHit != null)
                {
                    if (LastDisplayedObject == null || LastHit != LastDisplayedObject || DisplayText.text == "")
                    {
                        LastDisplayedObject = null;
                        DisplayText.GetComponent<RectTransform>().localEulerAngles = (forward == transform.forward) ? Vector3.zero : Vector3.up * 180;
                        DisplayText.transform.localPosition = (forward == transform.forward) ? Vector3.zero + forward * 0.02f : 
                                                                                               Vector3.zero - forward * 0.02f;
                        DisplayText.text = "Collecting Informations...\n\n";
                        DisplayText.text += (int)(LoadingTimer / LoadingTime * 100) + " %";
                        LoadingTimer += Time.deltaTime;
                        IsReadingObject = true;
                        float xPos = (forward == transform.forward) ? -Width / 2 + ButtonSize : Width / 2 - ButtonSize;
                        Left.transform.localPosition = new Vector3(xPos, -Height / 2 + ButtonSize, -dir * 0.02f);
                        Left.ChangeText(forward == transform.forward ? LeftArrow : RightArrow);
                        Right.transform.localPosition = new Vector3(-xPos, -Height / 2 + ButtonSize, -dir * 0.02f);
                        Right.ChangeText(forward == transform.forward ? RightArrow : LeftArrow);

                        if (LoadingTimer >= LoadingTime)
                        {
                            LastDisplayedObject = LastHit;
                            LoadedPage = 0;
                            DisplayInfo();
                            IsReadingObject = false;
                        }
                    }
                    if (Hit.collider.gameObject == LastHit.gameObject)
                    {
                        if (LastHit.TriggerOnFocus) LastHit.OnFocus?.Invoke(Hit.point);
                        return;
                    }
                    ResetLastHit();
                }

                LastHit = Hit.collider.gameObject.GetComponent<InteractiveObject>();
                if (LastHit.TriggerOnFocus) LastHit.Highlight = true;
            }
            else
            {
                ResetLastHit();
                ResetDisplay();
            }

        }
        else
        {
            ResetLastHit();            
            ResetDisplay();
        }
    }

    private void DisplayInfo()
    {
        string displayedText = "";
        if (LastDisplayedObject != null)
        {
            if (LoadedPage == 0)
            {
                displayedText += "<u>Supported interactions:</u>\n\n";
                displayedText += "\xf256 Grabable: ";
                displayedText += (LastDisplayedObject.Grabable) ? "<#00ff00>\xf00c" : "<#ff0000>\xf00d";
                displayedText += "\n<#000000>\xf245 Clickable: ";
                displayedText += (LastDisplayedObject.OnClick != null) ? "<#00ff00>\xf00c" : "<#ff0000>\xf00d";
                displayedText += "\n<#000000>\xf245+\xf2f2 Long-Click: ";
                displayedText += (LastDisplayedObject.OnLongClick != null) ? "<#00ff00>\xf00c" : "<#ff0000>\xf00d";
                displayedText += "\n<#000000>\xf1f8 Removable: ";
                displayedText += (LastDisplayedObject.Removable) ? "<#00ff00>\xf00c" : "<#ff0000>\xf00d";
            }
            if (LoadedPage == 1)
            {
                displayedText += "<#000000>\n\n<u>Description:</u>\n";
                displayedText += (LastDisplayedObject.HasInfoText) ? LastDisplayedObject.InfoText : "No informations available.";
            }
        }
        DisplayText.text = displayedText;
    }

    private void ResetLastHit()
    {
        if (LastHit != null)
        {
            if (LastHit.StatusBoxTriggered)
                LastHit.ShutDownStatusBox();
            LastHit.Highlight = false;
            LastHit = null;
        }
    }

    private void ResetDisplay()
    {
        Pointer.SetActive(false);
        LoadingTimer = 0;
        if (LastDisplayedObject == null) LoadedPage = -1;
        if (IsReadingObject)
        {
            IsReadingObject = false;
            DisplayText.text = "";
        }
    }

    public override bool OnDrop(Collider other)
    {
        ResetDisplay();
        return base.OnDrop(other);        
    }
}

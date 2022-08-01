using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExpandView : InteractiveObject {

    public enum LAYOUT { FULL, LEFT, RIGHT, TOP, BOTTOM, TOPLEFT,TOPRIGHT, BOTTOMLEFT, BOTTOMRIGHT, LEFTCENTER, MIDDLE, RIGHTCENTER};

    private const float MARGIN = 0.05f;
    private const float depth = -0.52f;
    private Transform topLeft;
    public Vector3 StartScale { get; set; }
    public bool dragable = true;
    public bool clickable = true;

    private Collider currentDrag, currentGrab;

    private UnityEngine.Color normalColor;
    private UnityEngine.Color lichtblau;

    public bool DestroyWhithoutReferent = true;

    private bool _embeded = false;
    public bool Embeded
    {
        get
        {
            return _embeded;
        }

        set
        {
            _embeded = value;
        }
    }

    // Use this for initialization
    public override void Start () {
        base.Start();
    }
	
	// Update is called once per frame
	public void Update ()
    {
        

		if(dragable && currentDrag != null && GetComponentsInChildren<ExpandView>().Length <= 1)
        {
            if (currentDrag.enabled )
            {
                onDrag(currentDrag);
                GetComponent<MeshRenderer>().material.color = lichtblau;
            }
            else
            {
                currentDrag = null;
                ScaleMultiplier = new Vector2(1,1);
                GetComponent<MeshRenderer>().material.color = normalColor;
            }
                
        }

        if (currentGrab != null && transform.parent.name.Contains("Container"))
        {
            if(currentGrab.enabled && (transform.parent.parent == null || transform.parent.parent == ParentObject))
            {
                transform.parent.parent = currentGrab.transform;
            }
            else if (!currentGrab.enabled)
            {
                transform.parent.parent = ParentObject?.transform;
                currentGrab = null;
            }
        }

        if (DestroyWhithoutReferent)
        {
            if (!Embeded && transform.parent.name.Contains("Container") && StObject.transform.parent != transform.parent)
            {
                Destroy(gameObject);
            }

            if (Embeded && transform.parent.name.Contains("Container") && transform.parent.GetComponentInChildren<StolperwegeObject>() == null)
                Destroy(gameObject);
        }
    }

    public override GameObject OnGrab(Collider other)
    {
        Bounds bounds = new Bounds(GetComponent<Collider>().bounds.center, GetComponent<Collider>().bounds.size * 0.85f);

        if (bounds.Contains(other.bounds.center))
        {
            if(StObject.GetType() != typeof(StolperwegeURIObject))
                currentDrag = other;
        }      
        else
            currentGrab = other;

        return gameObject;
    }

    private void onDrag(Collider drag)
    {
        float distance = (new Vector2(transform.position.x, transform.position.z) - new Vector2(currentDrag.transform.position.x, currentDrag.transform.position.z)).magnitude;
        float scale = 1 - distance*3;

        if (scale <= 0.2f) OnDrop(drag);
        else ScaleMultiplier = new Vector2(scale,scale);
    }

    public override void OnPointerEnter(Collider other)
    {
        //blockOutline = false;
        base.OnPointerEnter(other);
    }

    public override void OnPointerExit()
    {
        base.OnPointerExit();
        //blockOutline = true;
    }

    public override bool OnPointerClick()
    {
        if (!base.OnPointerClick() || !clickable) return false;
        OnDrop(GameObject.Find("leftArm").GetComponent<Collider>());
        return true;
    }

    public override bool OnDrop(Collider drag)
    {
        StObject.gameObject.SetActive(true);
        StObject.OnGrab(drag);
        if(transform.parent.name.Contains("Container"))
        {
            Destroy(gameObject);
        }
        else if (transform.parent.GetComponent<ExpandView>() != null && StObject == transform.parent.GetComponent<ExpandView>().StObject)
            Destroy(transform.parent.gameObject);
        else
        {
            ScaleMultiplier = Vector2.one;
            currentDrag = null;
            GetComponent<MeshRenderer>().material.color = normalColor;
        }

        return true;
    }


    public void SetColor(Color c)
    {
        c.a = GetComponent<MeshRenderer>().material.color.a;
        GetComponent<MeshRenderer>().material.color = c;

    }

    public override void Awake()
    {
        base.Awake();
        topLeft = transform.Find("TopLeft");
        StartScale = transform.localScale;
        _scaleMultiplier = Vector2.one;

        normalColor = GetComponent<MeshRenderer>().material.color;
        lichtblau = new UnityEngine.Color(72f/255f, 169f/255f, 218f/255f, 194f/255f);

        //blockOutline = true;
    }

    private StolperwegeObject _stObject;

    public StolperwegeObject StObject
    {
        get
        {
            return _stObject;
        }

        set
        {
            _stObject = value;
            //_stObject.gameObject.SetActive(false);

        }
    }

    public new Vector2 ScaleMultiplier
    {
        get
        {
            return _scaleMultiplier;
        }

        set
        {
            _scaleMultiplier = value;

            Vector3 scale = new Vector3(StartScale.x, StartScale.y, StartScale.z);
            scale.x *= _scaleMultiplier.x;
            scale.z *= ScaleMultiplier.y;
            transform.localScale = scale;

            if(currentDrag == null &&  transform.parent != null && transform.parent.name.Contains("Container"))
            {
                transform.parent.GetComponentInChildren<StolperwegeObject>().transform.localPosition = Vector3.down * 0.75f *  scale.x;
            }
        }
    }

    public void drawComponent(Transform toDraw, LAYOUT layout)
    {
        Vector3 position = Vector3.zero, scale = Vector3.zero ;

        position.y = depth;
        scale.y = 1f;

        switch (layout)
        {
            case LAYOUT.FULL:
                scale.x = 1 - MARGIN;
                scale.z = scale.x;
                break;
            case LAYOUT.LEFT:
                position.x = topLeft.localPosition.x / 2;
                scale.x = 0.5f - MARGIN;
                scale.z = 1f - MARGIN;
                break;
            case LAYOUT.RIGHT:
                position.x = topLeft.localPosition.x / -2;
                scale.x = 0.5f - MARGIN;
                scale.z = 1f - MARGIN;
                break;
            case LAYOUT.TOP:
                position.z = topLeft.localPosition.z / -2;
                scale.x = 1f - MARGIN;
                scale.z = 0.5f - MARGIN;
                break;
            case LAYOUT.BOTTOM:
                position.z = topLeft.localPosition.z / 2;
                scale.x = 1f - MARGIN;
                scale.z = 0.5f - MARGIN;
                break;
            case LAYOUT.TOPLEFT:
                position.x = topLeft.localPosition.x / 2;
                position.z = topLeft.localPosition.z / -2;
                scale.x = 0.5f - MARGIN;
                scale.z = 0.5f - MARGIN;
                break;
            case LAYOUT.BOTTOMLEFT:
                position.x = topLeft.localPosition.x / 2;
                position.z = topLeft.localPosition.z / 2;
                scale.x = 0.5f - MARGIN;
                scale.z = 0.5f - MARGIN;
                break;
            case LAYOUT.TOPRIGHT:
                position.x = topLeft.localPosition.x / -2;
                position.z = topLeft.localPosition.z / -2;
                scale.x = 0.5f - MARGIN;
                scale.z = 0.5f - MARGIN;
                break;
            case LAYOUT.BOTTOMRIGHT:
                position.x = topLeft.localPosition.x / -2;
                position.z = topLeft.localPosition.z / 2;
                scale.x = 0.5f - MARGIN;
                scale.z = 0.5f - MARGIN;
                break;
            case LAYOUT.LEFTCENTER:
                position.x = topLeft.localPosition.x / 2;
                scale.x = 0.75f - MARGIN;
                scale.z = 0.75f - MARGIN;
                break;
            case LAYOUT.RIGHTCENTER:
                position.x = topLeft.localPosition.x / -2;
                scale.x = 0.75f - MARGIN;
                scale.z = 0.75f - MARGIN;
                break;
            case LAYOUT.MIDDLE:
                scale.x = 0.5f - MARGIN;
                scale.z = 0.5f - MARGIN;
                break;
            default:
                return;
        }

        toDraw.parent = transform;

        toDraw.localPosition = position;
        toDraw.localScale = scale;
        toDraw.localEulerAngles = Vector3.zero;

        if(toDraw.GetComponent<ExpandView>() != null)
        {
            toDraw.GetComponent<ExpandView>().StartScale = scale;
        }
    }

    private void OnDestroy()
    {
        if (transform.parent.name.Contains("Container") && transform.parent.GetComponentsInChildren<ExpandView>().Length <= 1)
        {
            foreach(StolperwegeObject obj in transform.parent.GetComponentsInChildren<StolperwegeObject>())
                obj.ResetObject();

            Destroy(transform.parent.gameObject);
        }
    }

    public static GameObject createContentBox()
    {
        GameObject contentBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 scale = contentBox.transform.localScale;
        contentBox.transform.localScale = scale;
        contentBox.GetComponent<MeshRenderer>().material = Resources.Load("materials/StolperwegeMaterials/ExpandViewContentMaterial") as Material;
        contentBox.GetComponent<Collider>().enabled = false;

        return contentBox;
    }

    public static GameObject createText(string text)
    {
        GameObject textGO = (GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeText"));
        textGO.GetComponent<TextMeshPro>().text = text;

        return textGO;
    }

    
}

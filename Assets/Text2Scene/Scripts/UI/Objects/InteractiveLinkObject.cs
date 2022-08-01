using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MathHelper;
using Valve.VR;

public class InteractiveLinkObject : InteractiveObject
{
    //private TextMeshPro Label;

    private string _name;
    private bool _rotate = false;


    private GameObject ArrowObject;
    private LineRenderer Arrow;
    private GameObject LabelObject;
    private TextMeshPro ArrowLabel;
    private BoxCollider LabelCollider;


    public string Name
    {
        get
        {
            if (_name == null) _name = name;
            return _name;
        }
        set
        {
            if (value == null) return;
            _name = value;
            name = _name;
            LoadingText = "Destroying " + _name;
        }
    }

    public IsoLink Link { get; private set; }

    public InteractiveShapeNetObject FigureObject
    {
        get
        {
            return Link.Figure.Object3D.GetComponent<InteractiveShapeNetObject>();
        }
    }

    public InteractiveShapeNetObject GroundObject
    {
        get
        {
            return Link.Ground.Object3D.GetComponent<InteractiveShapeNetObject>();
        }
    }

    public InteractiveShapeNetObject ReferencePtObject
    {
        get
        {
            if (Link is IsoOLink)
            {
                IsoOLink olink = (IsoOLink)Link;
                if (olink.Reference_Point != null)
                    return olink.Reference_Point.Object3D.GetComponent<InteractiveShapeNetObject>();
                else 
                    return null;
            }
            else
                return null;
        }
    }

    public override bool Highlight
    {
        get => base.Highlight;
        set
        {
            base.Highlight = value;
            //if (!spacialobjects.Contains(SpatialEntity.Object_ID)) Label.gameObject.SetActive(_highlight);
            //if (!SpatialEntity.Object_ID.Equals("objectgroup")) Label.gameObject.SetActive(_highlight);
        }
    }

    //public bool RotateOnGrab = false;
    //public bool SaveOrientationOnDrop = false;



    Vector3 _start, _end, _middle; Color _color; Vector3[] _points;
    public void Init(IsoLink link, bool rotate = false)
    {
        base.Start();
        _rotate = rotate;
        Link = link;
        Link.Object3D = gameObject;
        Grabable = false;
        InitArrow();
        PositioningArrow();
        AsyncClick = HandleClick;

    }
    private void Update()
    {
        PositioningLabel();
    }


    private void InitArrow()
    {
        ArrowObject = new GameObject("ArrowLine");
        ArrowObject.transform.parent = transform;

        Arrow = ArrowObject.AddComponent<LineRenderer>();
        Arrow.enabled = true;
        Arrow.material = (Material)(Instantiate(Resources.Load("Materials/UI/GoetheBlauUnlit")));
        Arrow.positionCount = 9;
        _points = new Vector3[9];
        Arrow.enabled = true;
        Arrow.widthMultiplier = 0.005f;
        Arrow.useWorldSpace = true;
        Arrow.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Arrow.receiveShadows = false;
        


        LabelObject = new GameObject("ArrowLabel");
        LabelObject.transform.parent = transform;
        ArrowLabel = LabelObject.AddComponent<TextMeshPro>();
        ArrowLabel.fontSize = 0.75f;
        ArrowLabel.font = Resources.Load<TMP_FontAsset>("Font/FontAwesomeSolid5");
        ArrowLabel.alignment = TextAlignmentOptions.Center;
        ArrowLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(1f, 0.2f);

        ArrowLabel.outlineWidth = 0.2f;

        LabelCollider = gameObject.AddComponent<BoxCollider>();

        LabelCollider.size = new Vector3(1f, 0.2f, 0.01f);
        //ArrowLabel.text = "Link";

        //_color = Link.ClassColor

        ActializeLabel();
    }


    public void ActializeLabel()
    {
        if (Link.GetType() == typeof(IsoQsLink))
        {
            ArrowLabel.text = "QsLink (" + Link.Rel_Type + ")";
            _color = IsoQsLink.ClassColor;
        }
        else if (Link.GetType() == typeof(IsoOLink))
        {
            ArrowLabel.text = "OLink (" + Link.Rel_Type + ")";
            _color = IsoOLink.ClassColor;
        }
        else if (Link.GetType() == typeof(IsoMoveLink))
        {
            ArrowLabel.text = "MoveLink (" + Link.Rel_Type + ")";
            _color = IsoOLink.ClassColor;
        }
        else if (Link.GetType() == typeof(IsoSrLink))
        {
            ArrowLabel.text = "SrLink (" + Link.Rel_Type + ")";
            _color = IsoSrLink.ClassColor;
        }
        else if (Link.GetType() == typeof(IsoMetaLink))
        {
            ArrowLabel.text = "MetaLink (" + Link.Rel_Type + ")";
            _color = IsoMetaLink.ClassColor;
        }
        else if (Link.GetType() == typeof(IsoMLink))
        {
            ArrowLabel.text = "MLink (" + Link.Rel_Type + ")";
            _color = IsoMLink.ClassColor;
        }
        else
        {
            ArrowLabel.text = "Link (" + Link.Rel_Type + ")";
            _color = Color.black;
        }
        Arrow.material.SetColor("_Color", _color);
        ArrowLabel.color = _color;
        ActualizeMesh();
    }

    public void ActualizeMesh()
    {
        ArrowLabel.ForceMeshUpdate();
        //TODO?
    }

    public void PositioningArrow()
    {
        //int figure_offset = Link.Figure.LinkedVia.Count;
        Vector3 figure_center = Link.Figure.Position.Vector;//FigureObject.Object_center;// + new Vector3(0, figure_offset / 50.0f, 0);
        Vector3 ground_center = Link.Ground.Position.Vector;//GroundObject.Object_center;// + new Vector3(0, ground_offset / 50.0f, 0);

        //Vector3 ground_center = GroundObject != null ? GroundObject.object_center : GroundObject.Entity.TokenObject.gameObject.transform.position; // + new Vector3(0, ground_offset / 100.0f, 0);
        PositioningArrow(figure_center, ground_center);
    }


    private void PositioningArrow(Vector3 start, Vector3 end)
    {
        _start = start;
        _end = end;
        _middle = (_start + _end) / 2;
        if(_rotate)
            _middle.x = Mathf.Max(_end.z, _start.z) + 0.2f;
        else
            _middle.y = Mathf.Max(_end.y, _start.y) + 0.2f;
        _points = BezierCurve.CalculateCurvePoints(_start, _middle, _end, _points);
        PositioningLabel();
        Arrow.SetPositions(_points);
    }

    
    private void PositioningLabel()
    {
        //TODO: Optimize with position = _middle
        if (LabelCollider != null)
        {
            LabelCollider.transform.position = _middle;
            LabelCollider.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform.position);
            LabelCollider.transform.Rotate(Vector3.up * 180, Space.Self);

            //LabelCollider.gameObject.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
            //LabelCollider.gameObject.transform.Rotate(Vector3.up * 180, Space.Self);

        }
        
        if (LabelObject != null && LabelObject.gameObject.activeInHierarchy)
        {
            LabelObject.transform.position = _middle;
            LabelObject.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform.position);
            LabelObject.transform.Rotate(Vector3.up * 180, Space.Self);

        }
    }


    public void ActivatePanel()
    {
        if (Link.Panel == null)
        {
            Link.Panel = ((GameObject)Instantiate(Resources.Load("Prefabs/LinkAnnotationPanel/LinkAnnotationPanel"))).GetComponent<LinkAnnotationPanel>();
            Link.Panel.Init(Link);
            Link.Panel.gameObject.SetActive(false);
        }
        Link.Panel.gameObject.SetActive(!Link.Panel.gameObject.activeInHierarchy);
        if (Link.Panel.gameObject.activeInHierarchy) StolperwegeHelper.PlaceInFrontOfUser(Link.Panel.transform, 0.5f);
    }

    //InteractiveObject _lastHit; float _clickTimer;
    private IEnumerator HandleClick()
    {
        Debug.Log("Click Registred");
        ActivatePanel();

        yield break;

    }



    public void Destroy()
    {
        if (Link.Panel != null)
            Link.Panel.Destroy();
        Destroy(gameObject);
    }

}

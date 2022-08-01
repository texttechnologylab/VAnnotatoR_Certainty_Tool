 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Valve.VR;
using System;
using MathHelper;
/*
 * TODO: 
- löschen von Triggern: TEST
- RelationType ...
 */
public class LinkAnnotationPanel : InteractiveObject
{

    /**************************************
    *      TAB BUTTONS
    **************************************/
    private InteractiveButton _annotationTabBtn;


    /**************************************
     *      Annotation TAB 
     **************************************/

    private GameObject _annotationTab;
    private InteractiveButton _removeButton;
    private InteractiveButton _closeButton;

    private InteractiveButton _figureToken;
    private InteractiveButton _groundToken;
    private InteractiveButton _triggerToken;
    private InteractiveButton _triggerDelete;

    private KeyboardEditText CommentInput;


    private InteractiveButton _relationType;


    /**************************************
    *      OLink Attribute 
    **************************************/
    private GameObject _oLinkAttrbute;
    private InteractiveCheckbox _projective;

    private InteractiveButton _frameType_absolute;
    private InteractiveButton _frameType_intrinsic;
    private InteractiveButton _frameType_relative;
    private InteractiveButton _frameType_undefined;
    private InteractiveButton _frameType_viewer;
    private InteractiveButton _referencePoint;
    private InteractiveButton _referencePointDelete;

    public enum PanelTab { Annotation }

    private PanelTab _activeTab;
    public PanelTab ActiveTab
    {
        get { return _activeTab; }
        set
        {
            _activeTab = value;
            _annotationTabBtn.ButtonOn = _activeTab == PanelTab.Annotation;
            //if (_activeTab == PanelTab.MultiTokens) ActualizeMultiTokenTab();
        }
    }

    public IsoLink Link { get; private set; }
    //public InteractiveLinkObject InteractiveLinkObject { get; private set; }
    //public InteractiveLinkObject InteractiveLinkObject { get; private set; }


    private TextAnnotatorInterface TextAnnotator { get { return SceneController.GetInterface<TextAnnotatorInterface>(); } }

    public enum MatchTypes {IsoSpatialEntity, IsoSignal, IsoEntity }
    public void Init(IsoLink link)
    {
        UseHighlighting = false;
        SearchForParts = false;
        base.Start();
        LookAtUserOnHold = true;
        KeepYRotation = true;
        Grabable = true;
        Removable = true;
        DestroyOnObjectRemover = true;

        Link = link;
        //InteractiveLinkObject = linkObj;
        GetComponent<Collider>().isTrigger = true;

        // INITIALIZING COMPONENTS

        // Tabs and buttons

        _annotationTab = transform.Find("AnnotationTab").gameObject;

        _annotationTabBtn = transform.Find("AnnotationButton").GetComponent<InteractiveButton>();
        _annotationTabBtn.OnClick = () => { ActiveTab = PanelTab.Annotation; };


        _removeButton = transform.Find("Remove/Button").GetComponent<InteractiveButton>();
        _removeButton.OnLongClick = () =>
        {
            _removeButton.Active = false;
            SceneController.GetInterface<TextAnnotatorInterface>().DeleteElement("" + Link.ID);
        };
        _removeButton.LoadingText = "Removing link...";

        _closeButton = transform.Find("CloseWindow/Button").GetComponent<InteractiveButton>();
        _closeButton.OnClick = () =>
        {
            Destroy();
        };


        // Orientation section

        _figureToken = transform.Find("AnnotationTab/FGTTab/Figure/Button").GetComponent<InteractiveButton>();
        _figureToken.ChangeText(Link.Figure == null ? "null" : Link.Figure.TextContent);
        _figureToken.ButtonValue = "ChangeFigure";
        _figureToken.OnClick = () =>
        {
            _figureToken.ButtonOn = true;
            StartCoroutine(ActualizeConnector(_figureToken, MatchTypes.IsoEntity));
        };

        if(Link.Figure.Object3D != null)
            _figureToken.PartsToHighlight.AddRange(Link.Figure.Object3D.GetComponent<InteractiveShapeNetObject>().PartsToHighlight);



        _groundToken = transform.Find("AnnotationTab/FGTTab/Ground/Button").GetComponent<InteractiveButton>();
        _groundToken.ChangeText(Link.Ground == null ? "null" : Link.Ground.TextContent);
        _groundToken.ButtonValue = "ChangeGround";
        _groundToken.OnClick = () =>
        {
            _groundToken.ButtonOn = true;
            StartCoroutine(ActualizeConnector(_groundToken, MatchTypes.IsoEntity));
        };
        if (Link.Ground.Object3D != null)
            _groundToken.PartsToHighlight.AddRange(Link.Ground.Object3D.GetComponent<InteractiveShapeNetObject>().PartsToHighlight);


        _triggerToken = transform.Find("AnnotationTab/FGTTab/Trigger/Button").GetComponent<InteractiveButton>();
        _triggerToken.ChangeText(Link.Trigger == null ? "null" : Link.Trigger.TextContent);
        _triggerToken.ButtonValue = "ChangeTrigger";
        _triggerToken.OnClick = () =>
        {
            _triggerToken.ButtonOn = true;
            StartCoroutine(ActualizeConnector(_triggerToken, MatchTypes.IsoEntity));
        };
        //TODO
        //_triggerToken.PartsToHighlight.AddRange(linkObj.FigureShapeNet.PartsToHighlight);

        _triggerDelete = transform.Find("AnnotationTab/FGTTab/Trigger/Remove").GetComponent<InteractiveButton>();
        _triggerDelete.ButtonValue = "DeleteTrigger";
        _triggerDelete.OnClick = () =>
        {
            _triggerDelete.ButtonOn = true;
            HandleIDCanges(null, _triggerToken, "trigger"); //_triggerToken übergeben um diesen anzupassen
        };


        CommentInput = transform.Find("AnnotationTab/FGTTab/Comment/InputField").GetComponent<KeyboardEditText>();
        CommentInput.ChangeTextOnCommit = false;
        CommentInput.IsNumberField = false;
        CommentInput.Text = Link.Comment ?? "-";
        CommentInput.OnCommit = (text, go) =>
        {
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "comment", text } };
            TextAnnotator.ChangeEventMap.Add((int)Link.ID, (updated) =>
            {
                IsoLink u = (IsoLink)updated;
                CommentInput.Text = u.Comment ?? "-";
            });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Link.ID, updateMap } }, null);
        };


        _relationType = transform.Find("AnnotationTab/AttributeTab/RelType/Button").GetComponent<InteractiveButton>();
        if (Link.Rel_Type != null) _relationType.ChangeText(Link.Rel_Type);
        _relationType.OnClick = () => {
            StartCoroutine(SetRadialMenuForSelection()); 
            //_relationType.Active = false; 
        };


        _oLinkAttrbute = transform.Find("AnnotationTab/AttributeTab/OLinkAttribute").gameObject;

        _projective = transform.Find("AnnotationTab/AttributeTab/OLinkAttribute/Projective/Checkbox").GetComponent<InteractiveCheckbox>();
        _projective.OnClick = () =>
        {
            _projective.Active = false;
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "projective", !_projective.ButtonOn } };
            IsoOLink olink = (IsoOLink)Link;
            TextAnnotator.ChangeEventMap.Add((int)olink.ID, (updated) =>
            {
                IsoOLink u = (IsoOLink)updated;
                _projective.ButtonOn = u.Projective;
                _projective.Active = true;
            });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + olink.ID, updateMap } }, null);
        };

        _referencePoint = transform.Find("AnnotationTab/AttributeTab/OLinkAttribute/ReferencePoint/Button").GetComponent<InteractiveButton>();
        _referencePoint.OnClick = () =>
        {
            _referencePoint.ButtonOn = true;
            StartCoroutine(ActualizeConnector(_referencePoint, MatchTypes.IsoEntity));
        };

        //TODO
        //
        _referencePointDelete = transform.Find("AnnotationTab/AttributeTab/OLinkAttribute/ReferencePoint/Remove").GetComponent<InteractiveButton>();
        _referencePointDelete.ButtonValue = "DeleteReference";
        _referencePointDelete.OnClick = () =>
        {
            _referencePointDelete.ButtonOn = true;
            HandleIDCanges(null, _referencePointDelete, "reference_pt");
        };


        _frameType_absolute = transform.Find("AnnotationTab/AttributeTab/OLinkAttribute/FrameType/absolute").GetComponent<InteractiveButton>();
        _frameType_absolute.OnClick = () =>
        {
            changeOLinkFrameType("absolut");
        };

        _frameType_intrinsic = transform.Find("AnnotationTab/AttributeTab/OLinkAttribute/FrameType/intrinsic").GetComponent<InteractiveButton>();
        _frameType_intrinsic.OnClick = () =>
        {
            changeOLinkFrameType("intrinsic");
        };

        _frameType_relative = transform.Find("AnnotationTab/AttributeTab/OLinkAttribute/FrameType/relative").GetComponent<InteractiveButton>();
        _frameType_relative.OnClick = () =>
        {
            changeOLinkFrameType("relative");
        };

        _frameType_undefined = transform.Find("AnnotationTab/AttributeTab/OLinkAttribute/FrameType/undefined").GetComponent<InteractiveButton>();
        _frameType_undefined.OnClick = () =>
        {
            changeOLinkFrameType("undefined");
        };

        _frameType_viewer = transform.Find("AnnotationTab/AttributeTab/OLinkAttribute/FrameType/viewer").GetComponent<InteractiveButton>();
        _frameType_viewer.OnClick = () =>
        {
            changeOLinkFrameType("undefined viewer");
        };


        if (Link is IsoOLink)
            activateOLinkAtributes();
        else
            _oLinkAttrbute.SetActive(false);

        ActiveTab = PanelTab.Annotation;

    }

    private LineRenderer Arrow;
    private GameObject Cone;
    InteractiveObject _hit; TokenObject _token; InteractiveShapeNetObject _object;
    bool _match;
    Vector3[] _points; Vector3 _start, _end, _middle; Color _color;
    private IEnumerator ActualizeConnector(InteractiveButton button, MatchTypes matchType)
    {
        if (Arrow == null)
        {
            Arrow = StolperwegeHelper.CenterEyeAnchor.gameObject.GetComponent<LineRenderer>();
            if (Arrow == null)
                Arrow = StolperwegeHelper.CenterEyeAnchor.gameObject.AddComponent<LineRenderer>();
        }
        Arrow.positionCount = 9;
        _points = new Vector3[9];
        Arrow.SetPositions(_points);
        Arrow.material = (Material)Instantiate(Resources.Load("Materials/UI/Pointer"));
        Arrow.widthMultiplier = 0.005f;
        Arrow.useWorldSpace = true;
        Arrow.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Arrow.receiveShadows = false;
        Cone = (GameObject)(Instantiate(Resources.Load("Prefabs/UI/Cone")));
        Cone.GetComponent<MeshRenderer>().material = (Material)Instantiate(Resources.Load("Materials/UI/Pointer"));
        Cone.transform.localScale *= 2;

        while (!SteamVR_Actions.default_trigger.GetStateUp(StolperwegeHelper.User.PointerHandType))
        {
            // get the hit
            _hit = StolperwegeHelper.User.PointerHand.IsPointing ? StolperwegeHelper.User.PointerHand.Hit : null;
            Cone.SetActive(_hit != null);
            Arrow.enabled = _hit != null;
            if (_hit == null)
            {
                yield return null;
                continue;
            }

            // check matches
            _match = false;

            if (_hit != null)
            {
                if (_hit is TokenObject)
                {
                    _token = (TokenObject)_hit;
                    if(matchType == MatchTypes.IsoSpatialEntity)
                    {
                        _match = button.ButtonOn && _token.QuickTreeNode != null &&
                            _token.QuickTreeNode.IsoEntity != null && _token.QuickTreeNode.IsoEntity is IsoSpatialEntity;
                    } 
                    else if(matchType == MatchTypes.IsoSignal)
                    {
                        _match = button.ButtonOn && _token.QuickTreeNode != null &&
                            _token.QuickTreeNode.IsoEntity != null && _token.QuickTreeNode.IsoEntity is IsoSignal;
                    }
                    else
                    {
                        _match = button.ButtonOn && _token.QuickTreeNode != null &&
                            _token.QuickTreeNode.IsoEntity != null && _token.QuickTreeNode.IsoEntity is IsoEntity;
                    }
                }

                if (_hit is InteractiveShapeNetObject)
                {
                    _object = (InteractiveShapeNetObject)_hit;
                    if (matchType == MatchTypes.IsoSpatialEntity)
                    {
                        _match = button.ButtonOn && _object.Entity != null && _object.Entity is IsoSpatialEntity;
                    }
                    else if (matchType == MatchTypes.IsoSignal)
                    {
                        _match = button.ButtonOn && _object.Entity != null && _object.Entity is IsoSignal;
                    }
                    else
                    {
                        _match = button.ButtonOn && _object.Entity != null && _object.Entity is IsoEntity;
                    }

                }
            }

            // actualize arrow
            _start = button.transform.position;

            _end = _match ? _hit.transform.position : StolperwegeHelper.User.PointerHand.RaySphere.transform.position;
            _middle = (_start + _end) / 2;
            _middle = transform.InverseTransformPoint(_middle);
            _middle.z = 0.5f;
            _middle = transform.TransformPoint(_middle);
            _color = _match ? Color.green : Color.red;
            Arrow.material.SetColor("_Color", _color);
            Arrow.material.SetColor("_EmissionColor", _color * 5);
            _points = BezierCurve.CalculateCurvePoints(_start, _middle, _end, _points);
            Cone.GetComponent<Renderer>().material.SetColor("_Color", _color);
            Cone.GetComponent<Renderer>().material.SetColor("_EmissionColor", _color * 5);
            Cone.transform.forward = _points[_points.Length - 1] - _points[_points.Length - 2];
            Cone.transform.position = _points[_points.Length - 1] - Cone.transform.forward * Cone.GetComponent<Renderer>().bounds.size.z / 2;
            Cone.transform.forward = _points[_points.Length - 1] - _points[_points.Length - 2];
            _points[_points.Length - 1] = Cone.transform.position;
            Arrow.SetPositions(_points);

            yield return null;
        }
        Destroy(Arrow);
        Destroy(Cone);
        if (_hit != null)
        {
            if (_match)
            {
                IsoEntity hit = null;
                if (_hit is TokenObject)
                    hit = _token.QuickTreeNode.IsoEntity;
                else if (_hit is InteractiveShapeNetObject)
                    hit = _object.Entity;

                if (button == _figureToken)
                {
                    removeFromHighlightList(Link.Figure, button);
                    HandleIDCanges(hit, button, "figure");
                }
                else if (button == _groundToken)
                {
                    removeFromHighlightList(Link.Ground, button);
                    HandleIDCanges(hit, button, "ground");
                }
                else if (button == _triggerToken)
                {
                    HandleIDCanges(hit, button, "trigger");
                }
                else if (button == _referencePoint)
                {
                    removeFromHighlightList(((IsoOLink)Link).Reference_Point, button);
                    HandleIDCanges(hit, button, "reference_pt");
                }

            }

        }

        _figureToken.ButtonOn = false;
        _groundToken.ButtonOn = false;
        _triggerToken.ButtonOn = false;
        _referencePoint.ButtonOn = false;
    }



    private void removeFromHighlightList(IsoEntity entity, InteractiveButton button)
    {
        if (entity == null)
            return;

        if (entity.Object3D != null)
        {
            InteractiveShapeNetObject iso = entity.Object3D.GetComponent<InteractiveShapeNetObject>();
            foreach (Renderer r in iso.PartsToHighlight)
            {
                button.PartsToHighlight.Remove(r);
            }
        }
    }

    private void activateOLinkAtributes()
    {
        IsoOLink olink = (IsoOLink)Link;
        _oLinkAttrbute.SetActive(true);


        _projective.ButtonOn = olink.Projective;


        if (olink.Frame_Type != null) highlightOLinkFrameType(olink.Frame_Type);
        else highlightOLinkFrameType("undefined");

        _referencePoint.ChangeText(olink.Reference_Point == null ? "null" : olink.Reference_Point.TextContent);
        _referencePoint.ButtonValue = "ChangeReferencePt";
        if(olink.Reference_Point != null && olink.Reference_Point.Object3D != null) 
            _referencePoint.PartsToHighlight.AddRange(
                olink.Reference_Point.Object3D.GetComponent<InteractiveShapeNetObject>().PartsToHighlight);

        
    }

    private void changeOLinkFrameType(string type)
    {
        Debug.Log("Change FrameType to: " + type);
        IsoOLink olink = (IsoOLink)Link;
        Dictionary<string, object> updateMap = new Dictionary<string, object>() { {"frame_type", type } };
        TextAnnotator.ChangeEventMap.Add((int)olink.ID, (updated) =>
        {
            IsoOLink u = (IsoOLink)updated;
            Debug.Log("Changed to: " + u.Frame_Type);
            highlightOLinkFrameType(u.Frame_Type);
        });
        TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + olink.ID, updateMap } }, null);
    }

    private void highlightOLinkFrameType(string highlight)
    {
        Color InactiveButtonColor = StolperwegeHelper.GUCOLOR.HELLGRAU;
        Color ActiveButtonColor = StolperwegeHelper.GUCOLOR.GOETHEBLAU;
        if (highlight.Equals("absolut"))
        {
            _frameType_absolute.ChangeTextColor(ActiveButtonColor);
            _frameType_intrinsic.ChangeTextColor(InactiveButtonColor);
            _frameType_relative.ChangeTextColor(InactiveButtonColor);
            _frameType_undefined.ChangeTextColor(InactiveButtonColor);
            _frameType_viewer.ChangeTextColor(InactiveButtonColor);
        }
        else if (highlight == "intrinsic")
        {
            _frameType_absolute.ChangeTextColor(InactiveButtonColor);
            _frameType_intrinsic.ChangeTextColor(ActiveButtonColor);
            _frameType_relative.ChangeTextColor(InactiveButtonColor);
            _frameType_undefined.ChangeTextColor(InactiveButtonColor);
            _frameType_viewer.ChangeTextColor(InactiveButtonColor);
        }
        else if (highlight == "relative")
        {
            _frameType_absolute.ChangeTextColor(InactiveButtonColor);
            _frameType_intrinsic.ChangeTextColor(InactiveButtonColor);
            _frameType_relative.ChangeTextColor(ActiveButtonColor);
            _frameType_undefined.ChangeTextColor(InactiveButtonColor);
            _frameType_viewer.ChangeTextColor(InactiveButtonColor);
        }
        else if (highlight == "undefined viewer")
        {
            _frameType_absolute.ChangeTextColor(InactiveButtonColor);
            _frameType_intrinsic.ChangeTextColor(InactiveButtonColor);
            _frameType_relative.ChangeTextColor(InactiveButtonColor);
            _frameType_undefined.ChangeTextColor(InactiveButtonColor);
            _frameType_viewer.ChangeTextColor(ActiveButtonColor);
        }
        else
        {
            _frameType_absolute.ChangeTextColor(InactiveButtonColor);
            _frameType_intrinsic.ChangeTextColor(InactiveButtonColor);
            _frameType_relative.ChangeTextColor(InactiveButtonColor);
            _frameType_undefined.ChangeTextColor(ActiveButtonColor);
            _frameType_viewer.ChangeTextColor(InactiveButtonColor);
        }
    }

    /*
    void Update()
    {

    }
    */

    public void HandleIDCanges(IsoEntity entity, InteractiveButton button, string value)
    {
        IsoLink link = Link;
        Dictionary<string, object> updateMap = new Dictionary<string, object>();
        if (entity != null)
        {
            updateMap.Add(value, (int)entity.ID);
        }
        else
            updateMap.Add(value, null);


        TextAnnotator.ChangeEventMap.Add((int)link.ID, (updated) =>
        {
            //TODO:  Nicht ideal .... Aus dem Link nehmen, sobald aufgeräumt.
            link = (IsoLink)updated;
            if(button != null)
            {
                button.ChangeText(entity == null ? "null" : entity.TextContent);
                button.Active = true;
                //button.PartsToHighlight.AddRange(entity.Object3D.GetComponent<InteractiveShapeNetObject>().PartsToHighlight);
            }
            if(link.Object3D != null)
                link.Object3D.GetComponent<InteractiveLinkObject>().PositioningArrow();
            Debug.Log("Parts to Highlight Changed: " + entity.Object3D.GetComponent<InteractiveShapeNetObject>().PartsToHighlight);
        });
        TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + link.ID, updateMap } }, null);
    }





    private IEnumerator SetRadialMenuForSelection()
    {
        List<RadialSection> _selectionList = null;

        if (Link is IsoOLink)
        {
            _selectionList = RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.OLinkType];
        }
        else if (Link is IsoQsLink)
        {
            _selectionList = RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.QsLinkType];
        }
        else
        {
            yield break;
        }

        String oldValue = Link.Rel_Type.ToLower();
        int preselectedSection = -1;
        int i = 0;
        foreach(RadialSection sec in _selectionList)
        {
            if (sec.Title.ToLower().Equals(oldValue))
            {
                preselectedSection = i;
                break;
            }
        }
        StolperwegeHelper.RadialMenu.UpdateSection(_selectionList, preselectedSection, true);
        StolperwegeHelper.RadialMenu.Show(true);

        while (!SteamVR_Actions.default_trigger.GetStateUp(StolperwegeHelper.User.PointerHandType))
        {
            if (!StolperwegeHelper.RadialMenu.Visible){
                yield break;
            }
            yield return null;
        }

        if (StolperwegeHelper.RadialMenu.GetSelectedSection() != null)
        {
            string rel_type = StolperwegeHelper.RadialMenu.GetSelectedSection().Title;
            if (!rel_type.Equals(Link.Rel_Type))
            {
                Debug.Log("Radial Selection");
                if (StolperwegeHelper.RadialMenu.GetSelectedSection().Value is RadialInputType)
                {
                    Debug.Log("RadialInputTypen");
                    if ((RadialInputType)StolperwegeHelper.RadialMenu.GetSelectedSection().Value == RadialInputType.Keyboard)
                    {
                        Debug.Log("RadialInputType.Keyboard");
                        StolperwegeHelper.VRWriter.Inputfield = null;
                        StolperwegeHelper.VRWriter.Interface.DoneClicked += SendRelTypeRequest;
                    }
                }
                else
                    SendRelTypeRequest(rel_type);
            }
        }
        StolperwegeHelper.RadialMenu.Show(false);
        _relationType.Active = true;

    }

    private void SendRelTypeRequest(String reltype)
    {
        Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "rel_type", reltype } };
        IsoLink link = Link;
        TextAnnotator.ChangeEventMap.Add((int)link.ID, (updated) =>
        {
            IsoLink u = (IsoLink)updated;
            if (u.Rel_Type != null) _relationType.ChangeText(u.Rel_Type);
            else _relationType.ChangeText("null");
            if(u.Object3D != null)
            {
                u.Object3D.GetComponent<InteractiveLinkObject>().ActializeLabel();
            }
            StolperwegeHelper.VRWriter.Interface.DoneClicked -= SendRelTypeRequest;
        });
        TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + link.ID, updateMap } }, null);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}

using LitJson;
using MathHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;

public class TokenObject : InteractiveObject
{

    public const float MagnifierActivationDistance = 0.5f;
    public const float MagnifierDelay = 0.6f;
    //public AnnotatorTool AnnoTool { get; private set; }
    public QuickAnnotatorTool QuickAnnoTool { get; private set; }
    public AnnotationWindow AnnotationWindow { get; private set; }
    public QuickTreeNode QuickTreeNode { get; private set; }
    public IsoEntity Entity { get; private set; }
    public TextMeshPro Label { get; private set; }
    public GameObject Cube { get; private set; }
    //public NamedEntityAnnotator ActiveAnnotator;

    private LineRenderer Arrow;
    private GameObject Cone;
    public float Width { get; private set; }

    private float FontSize;
    private float Height;
    private float Thickness;

    private Vector3 _tablePos;
    public Vector3 TablePosition
    {
        get { return _tablePos; }
        set
        {
            _tablePos = value;
            transform.localPosition = value;
        }
    }

    public override bool Highlight
    {
        get => base.Highlight;
        set
        {
            if (!gameObject.activeInHierarchy || value == _highlight) return;
            base.Highlight = value;
            if (QuickAnnoTool != null)
            {
                if ((StolperwegeHelper.CenterEyeAnchor.transform.position -
               QuickAnnoTool.transform.position).magnitude > DataPanel.MagnifierActivationDistance && _highlight)
                    StartCoroutine(DelayMagnifier());
                else if (Equals(QuickAnnoTool.MagnifiedToken))
                    QuickAnnoTool.MagnifierActive = false;
            }
            if (AnnotationWindow != null)
            {
                if ((StolperwegeHelper.CenterEyeAnchor.transform.position -
               AnnotationWindow.transform.position).magnitude > DataPanel.MagnifierActivationDistance && _highlight)
                    StartCoroutine(DelayMagnifier());
                else if (Equals(AnnotationWindow.MagnifiedToken))
                    AnnotationWindow.MagnifierActive = false;
            }
        }
    }

    public bool HasEntityToken { get { return Entity != null; } }
    public bool HasQuickTreeNode { get { return QuickTreeNode != null; } }

 

    public IsoEntity GetEntity() { 
        if (HasEntityToken) return Entity;
        else if(HasQuickTreeNode) return QuickTreeNode.IsoEntity;
        else return null;
    }

    public void DeleteEntityInToken()
    {
        if (HasEntityToken) Entity = null;
        if (HasQuickTreeNode) QuickTreeNode.IsoEntity = null;
    }

    public bool HasEntity { get { return GetEntity() != null; } }

    public void Init(QuickTreeNode mToken, QuickAnnotatorTool quickAnnoTool, float availableSpace, float fontSize = 0.4f, float height = 0.05f, float thickness = 0.01f)
    {
        HighlightPower = 0.8f;
        QuickTreeNode = mToken;
        QuickTreeNode.TokenObjects.Add(this);
        QuickTreeNode.Object3D = gameObject;

        if (QuickTreeNode.IsoEntity != null)
        {
            QuickTreeNode.IsoEntity.TokenObject = this;
        }

        InfoText = QuickTreeNode.ToString();
        FontSize = fontSize;
        Height = height;
        Thickness = thickness;
        QuickAnnoTool = quickAnnoTool;
        Label = transform.Find("Label").GetComponent<TextMeshPro>();
        Cube = transform.Find("Cube").gameObject;
        PartsToHighlight = new List<Renderer>() { Cube.GetComponent<MeshRenderer>() };
        SearchForParts = false;
        SetToken(availableSpace);
        AsyncClick = ActualizeConnectorV2;
    }

    public void Init(QuickTreeNode mToken, AnnotationWindow annoWindow, float fontSize = 0.4f, float height = 0.05f, float thickness = 0.01f)
    {
        HighlightPower = 0.8f;
        QuickTreeNode = mToken;
        QuickTreeNode.TokenObjects.Add(this);
        QuickTreeNode.Object3D = gameObject;
        if (QuickTreeNode.IsoEntity != null)
            QuickTreeNode.IsoEntity.TokenObject = this;        
        InfoText = QuickTreeNode.ToString();
        FontSize = fontSize;
        Height = height;
        Thickness = thickness;
        AnnotationWindow = annoWindow;
        Label = transform.Find("Label").GetComponent<TextMeshPro>();
        Cube = transform.Find("Cube").gameObject;
        PartsToHighlight = new List<Renderer>() { Cube.GetComponent<MeshRenderer>() };
        SearchForParts = false;
        SetToken(-1);
        AsyncClick = ActualizeConnectorV2;
    }

    public void Init(IsoEntity entity, AnnotationWindow annoWindow, float fontSize = 0.4f, float height = 0.05f, float thickness = 0.01f)
    {
        HighlightPower = 0.8f;
        Entity = entity;
        Entity.TokenObject = this;
        FontSize = fontSize;
        Height = height;
        Thickness = thickness;
        AnnotationWindow = annoWindow;
        Label = transform.Find("Label").GetComponent<TextMeshPro>();
        Cube = transform.Find("Cube").gameObject;
        PartsToHighlight = new List<Renderer>() { Cube.GetComponent<MeshRenderer>() };
        SearchForParts = false;
        SetToken(-1);
        AsyncClick = ActualizeConnectorV2;
    }

    private void SetToken(float availableSpace)
    {
        Label.fontSize = FontSize;
        string text; Color color = Color.white;
        if (QuickTreeNode != null)
        {
            Width = Label.GetPreferredValues(QuickTreeNode.TextContent).x;
            text = QuickTreeNode.TextContent;
            if (QuickTreeNode.IsoEntity != null)
                color = (Color)QuickTreeNode.IsoEntity.GetType().GetProperty("ClassColor").GetValue(null);

        }
        else
        {
            Width = Label.GetPreferredValues(Entity.TextContent).x;
            text = Entity.TextContent;
            color = (Color)GetEntity().GetType().GetProperty("ClassColor").GetValue(null);
        }
        if (availableSpace != -1 && Width > availableSpace)
            Width = availableSpace;

        Label.GetComponent<RectTransform>().sizeDelta = new Vector2(Width, Height);
        int visibleChars = StolperwegeHelper.GetVisibleCharacters(Label, text);
        if (visibleChars > -1 && visibleChars < text.Length)
            text = text.Substring(0, visibleChars - 3) + "...";
        Label.text = text.Trim();
        Label.color = color;

        if(HasEntity && GetEntity().Object3D != null)
            Cube.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.75f, 0.75f, 0.75f));
        else
            Cube.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.588f, 0.588f, 0.588f));


        Vector3 _scale = new Vector3(Width, Height, Thickness);
        Cube.transform.localScale = _scale;
        GetComponent<BoxCollider>().size = _scale;
    }

    private int beginFirst; private int endFirst;
    private int beginSecond; private int endSecond;
    private void MergeOrSplit(TokenObject other = null)
    {
        TextAnnotatorInterface ta = SceneController.GetInterface<TextAnnotatorInterface>();
        if (other != null)
        {
            beginFirst = other.QuickTreeNode.Begin;
            endFirst = other.QuickTreeNode.End;
            beginSecond = QuickTreeNode.Begin;
            endSecond = QuickTreeNode.End;

            int begin = Math.Min(beginFirst, beginSecond);
            int end = Math.Max(endFirst, endSecond);

            Debug.Log(begin + " - " + end);
            ta.SendQuickTreeNodeCreatingRequest(begin, end);


            //Beachtet nicht, ob beide Entitäten haben, da diese Situation nicht vorkommen dürfte!
            IsoEntity entity = other.HasEntity ? other.GetEntity() : GetEntity();


            if (entity != null)
            {
                Debug.Log("Inkl. Entity Merge");
                Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "begin", begin }, { "end", end } };
                ta.ChangeEventMap.Add((int)entity.ID, (updated) =>
                {
                    IsoEntity u = (IsoEntity)updated;
                    Debug.Log("Changed Start and End to: " + u.Begin + " - " + u.End);

                });
                ta.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + entity.ID, updateMap } }, null);
            }

        }
        else
        {
            TokenObject quickAnnoToken = QuickAnnoTool != null ? this : null;
            TokenObject annoWindowToken = AnnotationWindow != null ? this : null;
            ta.RemoveEventMap.Add((int)QuickTreeNode.ID, () =>
            {
                if (quickAnnoToken != null) quickAnnoToken.QuickAnnoTool.UpdateTokenContainer();
                if (annoWindowToken != null)
                {
                    if (Entity.Begin == 0 && Entity.End == 0)
                        annoWindowToken.AnnotationWindow.UpdateEmptyTokenContainer(true);
                    else annoWindowToken.AnnotationWindow.UpdateTokenContainer();
                }
            });
            ta.FireWorkBatchCommand(new List<string>() { "" + QuickTreeNode.ID }, null, null, null);
        }
            
    }



    private void InitArrow(bool active)
    {
        if (Arrow == null)
        {
            Arrow = StolperwegeHelper.CenterEyeAnchor.gameObject.GetComponent<LineRenderer>();
            if (Arrow == null)
                Arrow = StolperwegeHelper.CenterEyeAnchor.gameObject.AddComponent<LineRenderer>();
        }
        Arrow.positionCount = 9;
        Vector3[] _points = new Vector3[9];
        Arrow.SetPositions(_points);
        Arrow.material = (Material)Instantiate(Resources.Load("Materials/UI/Pointer"));
        Arrow.widthMultiplier = 0.005f;
        Arrow.useWorldSpace = true;
        Arrow.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Arrow.receiveShadows = false;
        Cone = (GameObject)(Instantiate(Resources.Load("Prefabs/UI/Cone")));
        Cone.GetComponent<MeshRenderer>().material = (Material)Instantiate(Resources.Load("Materials/UI/Pointer"));
        Cone.transform.localScale *= 2;
        _start = transform.position;
        Cone.SetActive(active);
        Arrow.enabled = active;
    }

    private void UpdateArrow(Vector3 start, Vector3 end, Color color)
    {
        Vector3 _middle = (start + end) / 2;
        _middle = transform.parent.InverseTransformPoint(_middle);
        _middle.z = 10 * Height;
        _middle = transform.parent.TransformPoint(_middle);

        Arrow.material.SetColor("_Color", color);
        Arrow.material.SetColor("_EmissionColor", color * 5);
        Vector3[] _points = new Vector3[9];
        _points = BezierCurve.CalculateCurvePoints(start, _middle, end, _points);
        Cone.GetComponent<Renderer>().material.SetColor("_Color", color);
        Cone.GetComponent<Renderer>().material.SetColor("_EmissionColor", color * 5);
        Cone.transform.forward = _points[_points.Length - 1] - _points[_points.Length - 2];
        Cone.transform.position = _points[_points.Length - 1] - Cone.transform.forward * Cone.GetComponent<Renderer>().bounds.size.z / 2;
        Cone.transform.forward = _points[_points.Length - 1] - _points[_points.Length - 2];
        _points[_points.Length - 1] = Cone.transform.position;
        Arrow.SetPositions(_points);
    }



    InteractiveObject _hit; float _clickTimer;
    bool _tokenMatch, _objectMatch, _searchfieldMatch,
        _elevationMatch, _scopeMatch, _pathStartMatch, _pathEndMatch, _pathMidMatch, _triggerMatch, _relatorMatch, _mannerMatch, _goalMatch,
        _objectVolumeMatch, _objectAreaMatch, _objectLineMatch, _objectPointMatch;

    Vector3 _start;
    bool matchCheck;
    private IEnumerator ActualizeConnectorV2()
    {
        if (StolperwegeHelper.BlockInteractiveObjClick)
        {
            yield break;
        }

        if (HasEntity)
        {
            _clickTimer = 0;
            while (SteamVR_Actions.default_trigger.GetState(StolperwegeHelper.User.PointerHandType) &&
                   _clickTimer < 0.5f)
            {
                _clickTimer += Time.deltaTime;
                yield return null;
            }
            if (_clickTimer < 0.5f)
            {
                IsoEntity ent = GetEntity();
                if (ent.Panel == null)
                {
                    GameObject panelObj = (GameObject)Instantiate(Resources.Load("Prefabs/AnnotationObjectPanel"));
                    panelObj.GetComponent<AnnotationObjectPanel>().Init(ent);
                    ent.Panel.gameObject.SetActive(false);
                }
                ent.Panel.Active = !ent.Panel.Active;
                if (ent.Panel.Active) StolperwegeHelper.PlaceInFrontOfUser(ent.Panel.transform, 0.5f);
                else GetComponent<Collider>().enabled = true;
                yield break;
            }

        }

        //TODO: Anable click for ANnotation Window
        _start = transform.position;
        //Arrow init
        InitArrow(true);

        //Color selected Token
        Color oldColor = Cube.GetComponent<Renderer>().material.GetColor("_Color");
        Cube.GetComponent<Renderer>().material.SetColor("_Color", StolperwegeHelper.DEFAULT_ORANGE);

        if (HasEntity) //Linking Mode
            StolperwegeHelper.RadialMenu.UpdateSection(RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoLinks]);
        else //EntityMode
            StolperwegeHelper.RadialMenu.UpdateSection(RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoEntities]);

        StolperwegeHelper.RadialMenu.Show(true);
        //StolperwegeHelper.RadialMenu.OnInterrupt = () => { CleanupLinking(oldColor); };

        while (SteamVR_Actions.default_trigger.GetState(StolperwegeHelper.User.PointerHandType))
        {
            while(SteamVR_Actions.default_grab.GetStateDown(StolperwegeHelper.User.PointerHandType))
            {
                yield return true;
            }


            ResetMatches();
            _hit = StolperwegeHelper.User.PointerHand.IsPointing ? StolperwegeHelper.User.PointerHand.Hit : null;


            if (_hit is KeyboardEditText && HasQuickTreeNode && 
                ((KeyboardEditText)_hit).transform.parent.GetComponent<DataSearchPanel>() != null)
            {
                //Suchfeldeingabe
                _searchfieldMatch = true;
                StolperwegeHelper.RadialMenu.Show(false);
            }
            else if (HasEntity && 
                _hit is InteractiveButton && ((InteractiveButton)_hit).ButtonValue != null)
            {
                //Annotationsfenster Button Hit
                InteractiveButton hitButton = (InteractiveButton)_hit;
                CheckInteractiveButtonHit(hitButton);
                StolperwegeHelper.RadialMenu.Show(false);
            }
            else
            {
                StolperwegeHelper.RadialMenu.Show(true);
                if (StolperwegeHelper.RadialMenu.GetSelectedSection() != null && StolperwegeHelper.RadialMenu.GetSelectedSection().Title.Equals("CANCEL") && StolperwegeHelper.RadialMenu.InZeroZone())
                {
                    CleanupLinking(oldColor);
                    yield break;
                }


                if (HasEntity)
                {
                    if (_hit != null)
                    {
                        if (_hit is TokenObject)
                        {
                            TokenObject hitToken = (TokenObject)_hit;
                            if (!hitToken.HasEntity) 
                                StolperwegeHelper.RadialMenu.UpdateSectionLock(new List<bool> { true, false, false, false, true, false, false, false });

                            _tokenMatch = StolperwegeHelper.RadialMenu.GetSelectedSection() != null;
                        }
                        else if (_hit is InteractiveShapeNetObject)
                        {
                            _objectMatch = StolperwegeHelper.RadialMenu.GetSelectedSection() != null;
                        }
                    }
                    else
                        //Ausgleich für oben. SOllte funktionieren, weil etwas anderes erwischt wird, man nichts trifft ...
                        StolperwegeHelper.RadialMenu.UpdateSectionLock(null);
                }
                else if(HasQuickTreeNode)
                {
                    if(StolperwegeHelper.RadialMenu.GetSelectedSection() != null &&
                        StolperwegeHelper.RadialMenu.GetSelectedSection().Title.Equals("Multitoken"))
                    {
                        _tokenMatch = (_hit != null) && (_hit is TokenObject);
                    }
                }
                else
                {
                    Debug.LogError("Should not Happen!!!");
                    CleanupLinking(oldColor);
                    yield break;
                }
            }


            matchCheck = _searchfieldMatch || _elevationMatch ||
                _scopeMatch || _pathStartMatch || _pathEndMatch || _pathMidMatch || _triggerMatch ||
                _relatorMatch || _goalMatch || _mannerMatch ||
                _objectVolumeMatch || _objectAreaMatch || _objectLineMatch || _objectPointMatch ||
                ((_tokenMatch || _objectMatch) & StolperwegeHelper.RadialMenu.GetSelectedSection() != null) ||
                (StolperwegeHelper.RadialMenu.GetSelectedSection() != null & !HasEntity);

            Vector3 _end = (matchCheck & _hit != null) ? _hit.transform.position : StolperwegeHelper.User.PointerHand.RaySphere.transform.position;
            Color _color = (matchCheck & _hit != null) ? Color.green : Color.red;

            UpdateArrow(_start, _end, _color);


            yield return null;
        }
        Debug.Log("Trigger release");

        if (!matchCheck)
        {
            CleanupLinking(oldColor);
            yield break;
        }
        


        if (_searchfieldMatch || _elevationMatch || _scopeMatch || _pathStartMatch || _pathEndMatch || _pathMidMatch ||
            _triggerMatch || _relatorMatch || _goalMatch || _mannerMatch ||
            _objectVolumeMatch || _objectAreaMatch || _objectLineMatch || _objectPointMatch)
        {
            Debug.Log("Connect to Button");
            if (_searchfieldMatch)
            {
                KeyboardEditText input = (KeyboardEditText)_hit;
                input.Text = QuickTreeNode.TextContent;
                input.Commit();
            }
            else if (_elevationMatch || _scopeMatch || _pathStartMatch || _pathEndMatch || _pathMidMatch || _triggerMatch || _relatorMatch || _goalMatch || _mannerMatch)
            {
                AnnotationObjectPanel op = _hit.GetComponentInParent<AnnotationObjectPanel>();
                if (_elevationMatch) op.HandleElevation((IsoMeasure)GetEntity());
                if (_scopeMatch) op.HandleScope(GetEntity());
                if (_pathStartMatch || _pathEndMatch || _pathMidMatch) op.HandlePath((IsoSpatialEntity)GetEntity(), _pathStartMatch, _pathEndMatch, _pathMidMatch);
                if (_triggerMatch) op.HandleTrigger((IsoMotion)GetEntity());
                if (_relatorMatch) op.HandleRelator((IsoSRelation)GetEntity());
                if (_goalMatch) op.HandleGoal((IsoSpatialEntity)GetEntity());
                if (_mannerMatch) op.HandleManner((IsoSRelation)GetEntity());
            }

            //Rotate Object to User
            if (_objectVolumeMatch || _objectAreaMatch || _objectLineMatch || _objectPointMatch)
            {
                Vector3 pos = StolperwegeHelper.User.PointerHand.transform.position;
                //pos -= StolperwegeHelper.PointerPath.transform.up * 5;
                Quaternion rot = Quaternion.identity;
                Vector3 scale = Vector3.one;
                if (_objectVolumeMatch) Handle3DObject(ObjectTab.ABSTRACT_VOLUME, pos, rot, scale, null);
                if (_objectAreaMatch) Handle3DObject(ObjectTab.ABSTRACT_AREA, pos, rot, scale, null);
                if (_objectLineMatch) Handle3DObject(ObjectTab.ABSTRACT_CYLINDER, pos, rot, scale, null);
                if (_objectPointMatch) Handle3DObject(ObjectTab.ABSTRACT_POINT, pos, rot, scale, null);
            }
        }
        else if (HasEntity)
        {
            Debug.Log("Hit Detected");
            string sdesc = StolperwegeHelper.RadialMenu.GetSelectedSection().Description;
            string stitle = StolperwegeHelper.RadialMenu.GetSelectedSection().Title;
            string sdata = (string)StolperwegeHelper.RadialMenu.GetSelectedSection().Value;

            IsoEntity _hitEntity = null;
            if (_tokenMatch)
                _hitEntity = ((TokenObject)_hit).GetEntity();
            else if (_objectMatch)
                _hitEntity = ((InteractiveShapeNetObject)_hit).Entity;

            if (_hitEntity != null)
            {
                Debug.Log("Token Match");
                if (!_hitEntity.Equals(GetEntity()))
                {

                    if (sdesc.Equals("Overwrite"))
                    {
                        if (stitle.Equals("3DObject"))
                            GetEntity().OverrideObj(_hitEntity);
                        else if (stitle.Equals("Entity"))
                            GetEntity().OverrideEntity(_hitEntity);
                    }
                    else
                    {
                        GetEntity()._createrequest_linkdatatype = sdata;
                        GetEntity()._createrequest_ground = _hitEntity;
                        GetEntity()._createrequest_frameType = sdata.Equals(AnnotationTypes.OLINK) ? sdesc : null;

                        if (stitle.Equals("Other"))
                        {
                            StolperwegeHelper.VRWriter.Inputfield = null;
                            StolperwegeHelper.VRWriter.Interface.DoneClicked += GetEntity().SendLinkRequest;
                        }
                        else
                            GetEntity().SendLinkRequest(stitle);
                    }
                }
            }

        }
        else if (HasQuickTreeNode)
        {
            Debug.Log("Change Type");
            List<string> toRemove = null;

            string stitle = StolperwegeHelper.RadialMenu.GetSelectedSection().Title;

            if (stitle.Equals("Multitoken") && _tokenMatch)
            {
                if (!_hit.Equals(this))
                {
                    Debug.Log("Merge Token");
                    MergeOrSplit((TokenObject)_hit);
                }
                else
                {
                    Debug.Log("Split Token");
                    MergeOrSplit();
                }
            }
            else
            {
                string selection = (string)StolperwegeHelper.RadialMenu.GetSelectedSection().Value;
                if(selection == null)
                {
                    CleanupLinking(oldColor);
                    yield break;
                }

                Type selectedType = AnnotationTypes.TypesystemClassTable[selection];

                int tokenStart = 0;
                int tokenEnd = 0;
                if (HasQuickTreeNode)
                {
                    tokenStart = QuickTreeNode.Begin;
                    tokenEnd = QuickTreeNode.End;
                }

                string old_obj_id = null;
                Vector3 old_pos = StolperwegeHelper.CenterEyeAnchor.transform.position + StolperwegeHelper.CenterEyeAnchor.transform.forward * 0.4f; ;
                Vector3 old_scale = Vector3.one;
                Quaternion old_rot = Quaternion.identity;
                IEnumerable<IsoObjectAttribute> old_features = null;

                if (HasEntity)
                {
                    toRemove = new List<string>();
                    toRemove.Add("" + QuickTreeNode.IsoEntity.ID);

                    old_obj_id = QuickTreeNode.IsoEntity.Object_ID;
                    old_pos = QuickTreeNode.IsoEntity.Position.Vector;
                    old_scale = QuickTreeNode.IsoEntity.Scale.Vector;
                    old_rot = QuickTreeNode.IsoEntity.Rotation.Quaternion;
                    old_features = QuickTreeNode.IsoEntity.Object_Feature;
                }

                Dictionary<string, object> features = null;
                TextAnnotatorInterface ta = SceneController.GetInterface<TextAnnotatorInterface>();


                if (selectedType.Equals(typeof(IsoSRelation)))
                {
                    String stype = StolperwegeHelper.RadialMenu.GetSelectedSection().Description;
                    String svalue = StolperwegeHelper.RadialMenu.GetSelectedSection().Title;
                    features = ta.CreateSRelationAttributeMap(ObjectTab.ABSTRACT_POINT, old_pos, Quaternion.identity, old_scale, tokenStart, tokenEnd, stype, null, svalue); //TODO Free eingaben 
                }
                else if (selectedType.Equals(typeof(IsoMeasure)))
                    features = ta.CreateMeasureAttributeMap(old_obj_id == null ? ObjectTab.ABSTRACT_POINT : old_obj_id, old_pos, old_rot, old_scale, tokenStart, tokenEnd, null, null, features: old_features);
                else if (selectedType.Equals(typeof(IsoMRelation)))
                    features = ta.CreateMRelationAttributeMap(old_obj_id == null ? ObjectTab.ABSTRACT_POINT : old_obj_id, old_pos, old_rot, old_scale, tokenStart, tokenEnd, null, features: old_features); //TODO: Freie Eingabe
                else if (selectedType.Equals(typeof(IsoNonMotionEvent)))
                    features = ta.CreateNonMotionAttributeMap(old_obj_id == null ? ObjectTab.ABSTRACT_CYLINDER : old_obj_id, old_pos, old_rot, old_scale, tokenStart, tokenEnd, null, null, features: old_features);
                else if (selectedType.Equals(typeof(IsoMotion)))
                    features = ta.CreateMotionAttributeMap(old_obj_id == null ? ObjectTab.ABSTRACT_CYLINDER : old_obj_id, old_pos, old_rot, old_scale, tokenStart, tokenEnd, null, null, features: old_features);
                else if (selectedType.Equals(typeof(IsoSpatialEntity)))
                    features = ta.CreateSpatialEntityAttributeMap(old_obj_id == null ? ObjectTab.ABSTRACT_VOLUME : old_obj_id, old_pos, old_rot, old_scale, tokenStart, tokenEnd, null, null, features: old_features);
                else if (selectedType.Equals(typeof(IsoLocationPlace)))
                    features = ta.CreateLocationPlaceAttributeMap(old_obj_id == null ? ObjectTab.ABSTRACT_AREA : old_obj_id, old_pos, old_rot, old_scale, tokenStart, tokenEnd, null, null, features: old_features);
                else if (selectedType.Equals(typeof(IsoLocationPath)))
                    features = ta.CreateLocationPathAttributeMap(old_obj_id == null ? ObjectTab.ABSTRACT_LINE : old_obj_id, old_pos, old_rot, old_scale, tokenStart, tokenEnd, null, null, features: old_features);
                else if (selectedType.Equals(typeof(IsoEventPath)))
                    features = ta.CreateEventPathAttributeMap(old_obj_id == null ? ObjectTab.ABSTRACT_LINE : old_obj_id, old_pos, old_rot, old_scale, tokenStart, tokenEnd, null, null, features: old_features);
                else if (selectedType.Equals(typeof(IsoLocation)))
                    features = ta.CreateLocationAttributeMap(old_obj_id == null ? ObjectTab.ABSTRACT_AREA : old_obj_id, old_pos, old_rot, old_scale, tokenStart, tokenEnd, null, null, features: old_features);

                if (features != null)
                {
                    if (old_obj_id == null && !((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<ObjectTab>().CheckAbstractAutoCreation)
                    {
                        features.Remove("object_id");
                        features.Remove("position");
                        features.Remove("rotation");
                        features.Remove("scale");
                        features.Remove("object_feature_array");
                    }

                    Dictionary<string, List<Dictionary<string, object>>> featureMap = new Dictionary<string, List<Dictionary<string, object>>>();
                    featureMap.Add(selection, new List<Dictionary<string, object>>() { features });
                    ta.OnElemCreated = (elem) =>
                    {
                        IsoEntity entity = (IsoEntity)elem;
                        ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<ObjectTab>().CreateObject(entity);
                        SceneBuilderSceneScript.WaitingForResponse = false;

                        if (entity.TextReference == null)
                        {
                            Debug.Log("Text reference of element with {begin: " + entity.Begin + ", end: " + entity.End + "} not found.");
                            return;
                        }
                        TokenObject quickAnnoToken = QuickAnnoTool != null ? this : null;
                        TokenObject annoWindowToken = AnnotationWindow != null ? this : null;
                        foreach (TokenObject to in QuickTreeNode.TokenObjects)
                        {
                            if (annoWindowToken == null && to.AnnotationWindow != null)
                                annoWindowToken = to;
                            if (quickAnnoToken == null && to.QuickAnnoTool != null)
                                quickAnnoToken = to;
                            if (quickAnnoToken != null && annoWindowToken != null)
                                break;
                        }
                        if (quickAnnoToken != null) quickAnnoToken.QuickAnnoTool.UpdateTokenContainer();
                        if (annoWindowToken != null)
                        {
                            if (Entity.Begin == 0 && Entity.End == 0)
                                annoWindowToken.AnnotationWindow.UpdateEmptyTokenContainer(true);
                            else annoWindowToken.AnnotationWindow.UpdateTokenContainer();
                        }
                    };
                    ta.FireWorkBatchCommand(toRemove, featureMap, null, null);
                }
            }
        }
        else
        {
            Debug.LogError("How Again???");
        }

        CleanupLinking(oldColor);
        Debug.Log("Cleanup finished");
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (!HasEntity) return;
        if (other.GetComponent<DataBrowserResource>() != null &&
            other.GetComponent<DataBrowserResource>().IsGrabbed &&
            other.GetComponent<DataBrowserResource>().Data != null &&
            SceneController.ActiveSceneScript is SceneBuilderSceneScript)
        {
            ObjectTab tab = ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<ObjectTab>();
            if (tab.TriggerLocked) return;
            VRData data = other.GetComponent<DataBrowserResource>().Data;
            if (data is ShapeNetModel) StartCoroutine(LoadShapeNetModel((string)data.ID));
        }
    }

    private IEnumerator LoadShapeNetModel(string id)
    {
        ObjectTab tab = ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<ObjectTab>();
        if (tab.TriggerLocked) yield break;
        tab.Active = true;
        tab.AddLoadedObject(id);
        if (AnnotationWindow != null)
        {
            AnnotationWindow.LockTokenColliders(true);
            AnnotationWindow.LockEmptyTokenColliders(true);
        }
        if (QuickAnnoTool != null) QuickAnnoTool.LockColliders(true);
        yield return new WaitUntil(() => { return !tab.TriggerLocked; });
        tab.QuickLinkEntity = GetEntity();
        if (AnnotationWindow != null)
        {
            AnnotationWindow.LockTokenColliders(false);
            AnnotationWindow.LockEmptyTokenColliders(false);
        }
        if (QuickAnnoTool != null) QuickAnnoTool.LockColliders(false);
    }

    public void Handle3DObject(string objectID, Vector3 pos, Quaternion rot, Vector3 scale, List<Dictionary<string, object>> featureMap = null)
    {

        TextAnnotatorInterface ta = SceneController.GetInterface<TextAnnotatorInterface>();
        Dictionary<string, object> updateMap = ta.CreateSpatialEntityAttributeMap(objectID, pos, rot, scale, featureMap: featureMap);

        ta.ChangeEventMap.Add((int)GetEntity().ID, (updated) =>
        {
            Debug.Log("3d Object Handler: " + objectID);
            Entity = (IsoEntity)updated;
            if (Entity.Object3D != null)
                Destroy(Entity.Object3D);
            ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<ObjectTab>().CreateObject(Entity);

        });
        Debug.Log("Fire Request: " + GetEntity().ID);
        ta.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + GetEntity().ID, updateMap } }, null);
    }

    private void ResetMatches()
    {
        //Connect with Object
        _tokenMatch = false;

        //Connect with Object -> No Multitoken
        _objectMatch = false;

        //CopyTextToSearchField
        _searchfieldMatch = false;

        //InteractiveButtonMatches
        _elevationMatch = false;
        _scopeMatch = false;
        _pathStartMatch = false;
        _pathEndMatch = false;
        _pathMidMatch = false;
        _triggerMatch = false;
        _relatorMatch = false;
        _mannerMatch = false;
        _goalMatch = false;

        //AddAbstractObjectMatch
        _objectVolumeMatch = false;
        _objectAreaMatch = false;
        _objectLineMatch = false;
        _objectPointMatch = false;
    }

    private void CheckInteractiveButtonHit(InteractiveButton button)
    {
        if (GetEntity() is IsoMeasure && button.ButtonValue.Equals("AddElevation"))
            _elevationMatch = true;
        else if (GetEntity() is IsoEntity && button.ButtonValue.Equals("AddScope"))
            _scopeMatch = true;
        else if (GetEntity() is IsoSpatialEntity && button.ButtonValue.Equals("AddBeginEntity"))
            _pathStartMatch = true;
        else if (GetEntity() is IsoSpatialEntity && button.ButtonValue.Equals("AddEndEntity"))
            _pathEndMatch = true;
        else if (GetEntity() is IsoSpatialEntity && button.ButtonValue.Equals("AddPathMidEntity"))
            _pathMidMatch = true;
        else if (GetEntity() is IsoMotion && button.ButtonValue.Equals("AddTrigger"))
            _triggerMatch = true;
        else if (GetEntity() is IsoSRelation && button.ButtonValue.Equals("AddRelator"))
            _relatorMatch = true;
        else if (GetEntity() is IsoSpatialEntity && button.ButtonValue.Equals("AddGoal"))
            _goalMatch = true;
        else if (GetEntity() is IsoSRelation && button.ButtonValue.Equals("AddManner"))
            _mannerMatch = true;


        else if (GetEntity() != null)
        {
            if (GetEntity().Object3D == null)
            {
                if (button.ButtonValue.Equals("AddVolumeObject"))
                {
                    _objectVolumeMatch = true;
                }
                else if (button.ButtonValue.Equals("AddAreaObject"))
                {
                    _objectAreaMatch = true;
                }
                else if (button.ButtonValue.Equals("AddLineObject"))
                {
                    _objectLineMatch = true;
                }
                else if (button.ButtonValue.Equals("AddPointObject"))
                {
                    _objectPointMatch = true;
                }
            }
        }
    }

    private void CleanupLinking(Color oldColor)
    {
        Destroy(Arrow);
        Destroy(Cone);
        StolperwegeHelper.RadialMenu.Show(false);
        Cube.GetComponent<Renderer>().material.SetColor("_Color", oldColor);
        StolperwegeHelper.User.ActionBlocked = false;
    }

    private IEnumerator DelayMagnifier()
    {
        yield return new WaitForSeconds(MagnifierDelay);
        if (!Highlight) yield break;
        Vector3 magnifierTargetPos = StolperwegeHelper.CenterEyeAnchor.transform.position +
                                     StolperwegeHelper.CenterEyeAnchor.transform.forward * MagnifierActivationDistance +
                                     StolperwegeHelper.CenterEyeAnchor.transform.right * -0.2f;
        if (QuickAnnoTool != null)
        {
            QuickAnnoTool.MagnifierTargetPosition = magnifierTargetPos;
            QuickAnnoTool.SetupMagnifier(this);
        }
        if (AnnotationWindow != null)
        {
            AnnotationWindow.MagnifierTargetPosition = magnifierTargetPos;
            AnnotationWindow.SetupMagnifier(this);
        }
    }

    protected override void SetHighlight()
    {
        base.SetHighlight();

        if (QuickTreeNode != null)
        {
            foreach (TokenObject t in QuickTreeNode.TokenObjects)
                if ((Highlight && !t.Highlight) || (!Highlight && t.Highlight))
                    t.Highlight = Highlight;
        }
        else return;

        IsoEntity entity = GetEntity();
        if (entity == null)
        {
            AnnotationDocument doc = QuickTreeNode.DetermineDocument();
            List<IsoEntity> entities = new List<IsoEntity>(doc.GetElementsOfTypeFromTo<IsoEntity>(QuickTreeNode.Begin, QuickTreeNode.End, true));
            if (entities.Count > 0) entity = entities[0];
        }

        if (entity != null && entity.InteractiveShapeNetObject != null)
        {
            if ((Highlight && !entity.InteractiveShapeNetObject.Highlight) ||
                (!Highlight && entity.InteractiveShapeNetObject.Highlight))
                entity.InteractiveShapeNetObject.Highlight = Highlight;

            List<IsoEntity> coreflist = entity.GetAllLinkedCoref();
            if (coreflist.Count > 0)
                foreach (IsoEntity e in coreflist)
                {
                    QuickTreeNode q = Entity.DetermineDocument().GetElementOfTypeFromTo<QuickTreeNode>(e.Begin, e.End);
                    if (q != null)
                        foreach (TokenObject t in q.TokenObjects) t.Highlight = Highlight;
                }
        }
    }

    public void OnDestroy()
    {
        if (HasQuickTreeNode)
            QuickTreeNode.TokenObjects.Remove(this);
        if (HasEntityToken)
            Entity.TokenObject = null;
    }

}
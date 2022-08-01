using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MathHelper;
using Valve.VR;
using System.Security.AccessControl;
using Text2Scene.NeuralNetwork;
using System;

public class InteractiveShapeNetObject : InteractiveObject
{
    private TextMeshPro Label;
    public AvatarObjectPanel AvatarPanel { get; private set; }

    private UMAManagerInterface UMAManager
    {
        get { return SceneController.GetInterface<UMAManagerInterface>(); }
    }

    public IsoEntity Entity { get; private set; }
    public ShapeNetModel ShapeNetModel
    {
        get
        {
            if (ObjectTab.ABSTRACT_TYPES.Contains(Entity.Object_ID)) return null;
            //if (SpatialEntity.Object_ID.Equals("objectgroup")) return null;
            return SceneController.GetInterface<ShapeNetInterface>().ShapeNetModels[Entity.Object_ID];
        }
    }

    public Vector3 Object_center
    {
        get
        {
            return gameObject.transform.position;
        }
    }

    //public override bool Highlight
    //{
    //    get => base.Highlight;
    //    set
    //    {
    //        base.Highlight = value;
    //        //if(!spacialobjects.Contains(SpatialEntity.Object_ID)) Label.gameObject.SetActive(_highlight);
    //        //if (!SpatialEntity.Object_ID.Equals("objectgroup")) Label.gameObject.SetActive(_highlight);
    //    }
    //}
    public bool SaveOrientationOnDrop = true;
    private LineRenderer Arrow;
    private GameObject Cone;


    private GameObject ColliderFrame;
    //public bool IsObjectGroup { get { return SpatialEntity.Object_ID.Equals("objectgroup");  } }
    //public bool IsSpecialObject { get { return spacialobjects.Contains(SpatialEntity.Object_ID); } }

    public void Init(IsoEntity spatialEntity, bool grabable, bool createLabel)
    {
        base.Start();
        EmissionColor = new Color(0, 0.627451f, 5.992157f) * 3;
        Entity = spatialEntity;
        Entity.Object3D = gameObject;
        AsyncClick = ActualizeConnectorV2;
        Grabable = grabable;

        GameObject labelObject;
        if (createLabel)
        {
            labelObject = new GameObject("Label");
            labelObject.transform.parent = transform;
            labelObject.transform.localPosition = Vector3.up * (0.5f);
            Label = labelObject.AddComponent<TextMeshPro>();
            Label.fontSize = 1;
            Label.font = Resources.Load<TMP_FontAsset>("Font/FontAwesomeSolid5");
            Label.alignment = TextAlignmentOptions.Center;
            Label.faceColor = Color.white;
            Label.GetComponent<RectTransform>().sizeDelta = new Vector2(1f, 0.2f);

        }
        else
        {
            labelObject = transform.Find("Label").gameObject;
            Label = labelObject.GetComponent<TextMeshPro>();
        }

        OnHold += () =>
        {
            Vector3 center2HandDir = GrabHand.transform.position - transform.position;
            //transform.rotation = Entity.Rotation.Quaternion;
            Vector3 oldHandPos = transform.position + center2HandDir;
            transform.position += GrabHand.transform.position - oldHandPos;
        };
        Label.outlineColor = Color.black;
        Label.outlineWidth = 0.2f;
        ActualizeLabel();

        ColliderFrame = transform.Find("FrameCube").gameObject;
        ColliderFrame.SetActive(false);

        OnDistanceGrab = ControlObjectDistanceGrabPosition;

        if (spatialEntity is IsoEventPath)
            RepositionCorespondingEventPath();

		NN_Helper.ObjectsInScene.Add(this);
    }

  

    public void ActivateColliderFrame(bool active)
    {
        ColliderFrame.SetActive(active);
    }

    public void ActualizeLabel()
    {
        Label.text = Entity.TextContent;
        Label.ForceMeshUpdate();
        if (Entity.Panel != null && Entity.Panel.ActiveTab == AnnotationObjectPanel.PanelTab.MultiTokens)
            Entity.Panel.ActualizeMultiTokenTab();
    }


    public void ChangeColliderType(bool useMeshCollider)
    {
        if (useMeshCollider)
        {
            if (GetComponent<BoxCollider>() != null) Destroy(GetComponent<BoxCollider>());
            MeshCollider mc = gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = transform.GetChild(0).GetComponentInChildren<MeshFilter>().mesh;
        }
        else
        {
            if (GetComponent<MeshCollider>() != null) Destroy(GetComponent<MeshCollider>());
            BoxCollider bc = gameObject.AddComponent<BoxCollider>();
            bc.size = ShapeNetModel.AlignedDimensions / 100;
            bc.center = Vector3.up * bc.size.y / 2;
        }
    }

    protected override void SetHighlight()
    {
        base.SetHighlight();

        if (Label != null && Label.gameObject.activeInHierarchy)
        {
            Label.gameObject.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
            Label.gameObject.transform.Rotate(Vector3.up * 180, Space.Self);
        }
        if (Entity == null) return;
        if (Entity.TokenObject != null) Entity.TokenObject.Highlight = Highlight;
        QuickTreeNode qtn = null;
        if(Entity.DetermineDocument() != null)
            qtn = Entity.DetermineDocument().GetElementOfTypeFromTo<QuickTreeNode>(Entity.Begin, Entity.End);
        if (qtn != null)
            foreach (TokenObject t in qtn.TokenObjects) t.Highlight = Highlight;

        List<IsoEntity> coreflist = Entity.GetAllLinkedCoref();
        if (coreflist.Count > 0)
            foreach (IsoEntity e in coreflist)
            {
                QuickTreeNode q = Entity.DetermineDocument().GetElementOfTypeFromTo<QuickTreeNode>(e.Begin, e.End);
                if (q != null)
                    foreach (TokenObject t in q.TokenObjects) t.Highlight = Highlight;
            }
    }

    InteractiveObject _hit; float _clickTimer;
    bool _tokenMatch, _objectMatch,
        _elevationMatch, _scopeMatch, _pathStartMatch, _pathEndMatch, _pathMidMatch, _triggerMatch, _relatorMatch, _mannerMatch, _goalMatch;

    bool matchCheck;
    private IEnumerator ActualizeConnectorV2()
    {
        if (StolperwegeHelper.BlockInteractiveObjClick)
        {
            yield break;
        }

        _clickTimer = 0;
        while (SteamVR_Actions.default_trigger.GetState(StolperwegeHelper.User.PointerHandType) &&
               _clickTimer < 0.5f)
        {
            _clickTimer += Time.deltaTime;
            yield return null;
        }
        if (_clickTimer < 0.5f)
        {
            if (transform.GetComponentInChildren<UMAController>() != null)
            {
                UMAManager.SetAvatarObjectPanel(this);
            }
            else
            {
                if (Entity.Panel == null)
                {
                    GameObject panelObj = (GameObject)Instantiate(Resources.Load("Prefabs/AnnotationObjectPanel"));
                    panelObj.GetComponent<AnnotationObjectPanel>().Init(Entity);
                    Entity.Panel.gameObject.SetActive(false);
                }
                Entity.Panel.Active = !Entity.Panel.Active;
                if (Entity.Panel.Active) StolperwegeHelper.PlaceInFrontOfUser(Entity.Panel.transform, 0.5f);
                else GetComponent<Collider>().enabled = true;
                yield break;
            }
        }

        InitArrow(true);
        StolperwegeHelper.RadialMenu.UpdateSection(RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoLinks]);
        StolperwegeHelper.RadialMenu.Show(true);

        while (SteamVR_Actions.default_trigger.GetState(StolperwegeHelper.User.PointerHandType))
        {
            while (SteamVR_Actions.default_grab.GetStateDown(StolperwegeHelper.User.PointerHandType))
            {
                yield return true;
            }


            ResetMatches();
            _hit = StolperwegeHelper.User.PointerHand.IsPointing ? StolperwegeHelper.User.PointerHand.Hit : null;

            if (_hit is InteractiveButton && ((InteractiveButton)_hit).ButtonValue != null)
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
                    CleanupLinking();
                    yield break;
                }


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


            matchCheck = _elevationMatch ||
                _scopeMatch || _pathStartMatch || _pathEndMatch || _pathMidMatch || _triggerMatch ||
                _relatorMatch || _goalMatch || _mannerMatch ||
                ((_tokenMatch || _objectMatch) & StolperwegeHelper.RadialMenu.GetSelectedSection() != null);

            Vector3 _end = matchCheck ? _hit.transform.position : StolperwegeHelper.User.PointerHand.RaySphere.transform.position;
            Color _color = matchCheck ? Color.green : Color.red;

            UpdateArrow(transform.position, _end, _color);


            yield return null;
        }
        Debug.Log("Trigger release");
        if (!matchCheck)
        {
            CleanupLinking();
            yield break;
        }

        if (_elevationMatch || _scopeMatch || _pathStartMatch || _pathEndMatch || _pathMidMatch ||
        _triggerMatch || _relatorMatch || _goalMatch || _mannerMatch)
        {
            Debug.Log("Connect to Button");
            if (_elevationMatch || _scopeMatch || _pathStartMatch || _pathEndMatch || _pathMidMatch || _triggerMatch || _relatorMatch || _goalMatch || _mannerMatch)
            {
                AnnotationObjectPanel op = _hit.GetComponentInParent<AnnotationObjectPanel>();
                if (_elevationMatch) op.HandleElevation((IsoMeasure)Entity);
                if (_scopeMatch) op.HandleScope(Entity);
                if (_pathStartMatch || _pathEndMatch || _pathMidMatch) op.HandlePath((IsoSpatialEntity)Entity, _pathStartMatch, _pathEndMatch, _pathMidMatch);
                if (_triggerMatch) op.HandleTrigger((IsoMotion)Entity);
                if (_relatorMatch) op.HandleRelator((IsoSRelation)Entity);
                if (_goalMatch) op.HandleGoal((IsoSpatialEntity)Entity);
                if (_mannerMatch) op.HandleManner((IsoSRelation)Entity);
            }

        }
        else
        {
            RadialSection selectedSection = StolperwegeHelper.RadialMenu.GetSelectedSection();
            string stitle = StolperwegeHelper.RadialMenu.GetSelectedSection().Title;
            string sdesc = StolperwegeHelper.RadialMenu.GetSelectedSection().Description;
            string sdata = (string)StolperwegeHelper.RadialMenu.GetSelectedSection().Value;

            IsoEntity _hitEntity = null;
            if (_tokenMatch)
                _hitEntity = ((TokenObject)_hit).GetEntity();
            else if (_objectMatch)
                _hitEntity = ((InteractiveShapeNetObject)_hit).Entity;

            if(_hitEntity != null)
            {
                Debug.Log("Token Match");
                if (!_hitEntity.Equals(Entity))
                {

                    if (sdesc.Equals("Overwrite"))
                    {
                        if (stitle.Equals("3DObject"))
                            Entity.OverrideObj(_hitEntity);
                        else if (stitle.Equals("Entity"))
                            Entity.OverrideEntity(_hitEntity);
                    }
                    else
                    {
                        Entity._createrequest_linkdatatype = sdata;
                        Entity._createrequest_ground = _hitEntity;
                        Entity._createrequest_frameType = sdata.Equals(AnnotationTypes.OLINK) ? sdesc : null;

                        if (stitle.Equals("Other"))
                        {
                            StolperwegeHelper.VRWriter.Inputfield = null;
                            StolperwegeHelper.VRWriter.Interface.DoneClicked += Entity.SendLinkRequest;
                        }
                        else
                            Entity.SendLinkRequest(stitle);
                    }
                }
            }
        }
        CleanupLinking();
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
        Cone.SetActive(active);
        Arrow.enabled = active;
    }

    private void CleanupLinking()
    {
        Destroy(Arrow);
        Destroy(Cone);
        StolperwegeHelper.RadialMenu.Show(false);
        StolperwegeHelper.User.ActionBlocked = false;
    }


    private void UpdateArrow(Vector3 start, Vector3 end, Color color)
    {
        Vector3 _middle = (start + end) / 2;
        _middle = transform.parent.InverseTransformPoint(_middle);
        _middle.y = Mathf.Max(end.y, start.y) + 0.2f;
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

    private void CheckInteractiveButtonHit(InteractiveButton button)
    {
        if (Entity is IsoMeasure && button.ButtonValue.Equals("AddElevation"))
            _elevationMatch = true;
        else if (Entity is IsoEntity && button.ButtonValue.Equals("AddScope"))
            _scopeMatch = true;
        else if (Entity is IsoSpatialEntity && button.ButtonValue.Equals("AddBeginEntity"))
            _pathStartMatch = true;
        else if (Entity is IsoSpatialEntity && button.ButtonValue.Equals("AddEndEntity"))
            _pathEndMatch = true;
        else if (Entity is IsoSpatialEntity && button.ButtonValue.Equals("AddPathMidEntity"))
            _pathMidMatch = true;
        else if (Entity is IsoMotion && button.ButtonValue.Equals("AddTrigger"))
            _triggerMatch = true;
        else if (Entity is IsoSRelation && button.ButtonValue.Equals("AddRelator"))
            _relatorMatch = true;
        else if (Entity is IsoSpatialEntity && button.ButtonValue.Equals("AddGoal"))
            _goalMatch = true;
        else if (Entity is IsoSRelation && button.ButtonValue.Equals("AddManner"))
            _mannerMatch = true;
    }

    private void ResetMatches()
    {
        //Connect with Object
        _tokenMatch = false;

        //Connect with Object -> No Multitoken
        _objectMatch = false;

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
    }


    Dictionary<string, object> _updates;
    public override bool OnDrop()
    {
        base.OnDrop();
        transform.SetParent(((SceneBuilderSceneScript)SceneController.ActiveSceneScript).ObjectContainer.transform, true);

        SendObjectUpdateRequest(transform.position, transform.localScale, transform.rotation);

        RepositionAllCorespondingLinks();

        foreach (IsoEntity ent in Entity.LinkedDirect.Keys)
            if (ent.GetType() == typeof(IsoEventPath) && ent.InteractiveShapeNetObject != null)
                ent.InteractiveShapeNetObject.RepositionCorespondingEventPath();

        if (Entity.GetType() == typeof(IsoSRelation))
        {
            BoxCollider collider = gameObject.GetComponent<BoxCollider>();
            Collider[] hitColliders = Physics.OverlapBox(transform.position, collider.bounds.size, transform.rotation);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                InteractiveLinkObject link = hitColliders[i].gameObject.GetComponent<InteractiveLinkObject>();
                if(link != null && link.Link.Trigger != Entity) 
                { 
                    Dictionary<string, Dictionary<string, object>> updates = new Dictionary<string, Dictionary<string, object>>();

                    Dictionary<string, object> _attributes = new Dictionary<string, object>();
                    _attributes.Add("trigger", Entity.ID);
                    updates.Add("" + link.Link.ID, _attributes);

                    SceneController.GetInterface<TextAnnotatorInterface>().FireWorkBatchCommand(null, null, updates, null);
                    break;
                }
            }
        }
        return true;
    }

    public void RepositionAllCorespondingLinks()
    {
        foreach (KeyValuePair<IsoLink, IsoLink.Connected> entry in Entity.LinkedVia)
        {
            if (entry.Key.Object3D != null)
                entry.Key.Object3D.GetComponent<InteractiveLinkObject>().PositioningArrow();
            else
                entry.Key.CreateInteractiveLinkObject();
        }
    }

    public void RepositionCorespondingEventPath()
    {
        if (Entity.GetType() != typeof(IsoEventPath))
            return;

        IsoEventPath path = (IsoEventPath)Entity;
        IsoEntity start = path.StartID;
        List<IsoEntity> mids = path.MidIDs;
        IsoEntity end = path.EndID;

        List<Vector3> locationList = new List<Vector3>();

        Transform eventLineGameObject = transform.Find("EventPathLine");
        LineRenderer eventPathLineRenderer;
        if (eventLineGameObject == null)
        {
            GameObject obj = new GameObject("EventPathLine");
            obj.transform.parent = transform;
            eventPathLineRenderer = obj.AddComponent<LineRenderer>();
        }else
            eventPathLineRenderer = eventLineGameObject.GetComponent<LineRenderer>();


        eventPathLineRenderer.startColor = StolperwegeHelper.GUCOLOR.ORANGE;
        eventPathLineRenderer.endColor = StolperwegeHelper.GUCOLOR.ORANGE;
        eventPathLineRenderer.startWidth = 0.05f;
        eventPathLineRenderer.endWidth = 0.025f;
        eventPathLineRenderer.useWorldSpace = true;

        if (start != null && start.InteractiveShapeNetObject != null)
            locationList.Add(start.InteractiveShapeNetObject.Object_center);
        if (mids != null && mids.Count > 0)
            foreach (IsoEntity ent in mids)
                if (ent != null && ent.InteractiveShapeNetObject != null)
                    locationList.Add(ent.InteractiveShapeNetObject.Object_center);
        if (end != null && end.InteractiveShapeNetObject != null)
            locationList.Add(end.InteractiveShapeNetObject.Object_center);

        eventPathLineRenderer.positionCount = locationList.Count;
        for (int i = 0; i < locationList.Count; i++)
            eventPathLineRenderer.SetPosition(i, locationList[i]);
    }

    public void SendObjectUpdateRequest(Vector3 pos, Vector3 scale, Quaternion rot)
    {

        Dictionary<string, Dictionary<string, object>> command = new Dictionary<string, Dictionary<string, object>>();

        Dictionary<string, object> pos_updates;
        if (pos != null)
        {
            pos_updates = new Dictionary<string, object>();
            pos_updates.Add("x", (double)pos.x);
            pos_updates.Add("y", (double)pos.y);
            pos_updates.Add("z", (double)pos.z);
            command.Add("" + Entity.Position.ID, pos_updates);
        }

        Dictionary<string, object> scale_updates;
        if (scale != null)
        {
            scale_updates = new Dictionary<string, object>();
            scale_updates.Add("x", (double)scale.x);
            scale_updates.Add("y", (double)scale.y);
            scale_updates.Add("z", (double)scale.z);
            command.Add("" + Entity.Scale.ID, scale_updates);
        }

        Dictionary<string, object> rot_updates;
        if (rot != null)
        {
            rot_updates = new Dictionary<string, object>();
            rot_updates.Add("x", (double)rot.x);
            rot_updates.Add("y", (double)rot.y);
            rot_updates.Add("z", (double)rot.z);
            rot_updates.Add("w", (double)rot.w);
            command.Add("" + Entity.Rotation.ID, rot_updates);
        }

        if (command.Count > 0)
        {
            SceneController.GetInterface<TextAnnotatorInterface>()
                .FireWorkBatchCommand(null, null, command, null);

        }

    }

    public void SetObjColor(Color color)
    {
        Debug.Log(color);
        gameObject.GetComponentInChildren<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
        gameObject.GetComponentInChildren<Renderer>().material.SetColor("_Color", color);
    }

    public void Destroy()
    {
        List<IsoLink> linkIDs = new List<IsoLink>(Entity.LinkedVia.Keys);
        foreach (IsoLink link in linkIDs)
            if (link.Object3D != null)
                link.Object3D.GetComponent<InteractiveLinkObject>().Destroy();

        Destroy(gameObject);

    }

    private enum PlaceMode { Distance, Scale, Rotation }
    private readonly float x_buffer = 0.5f;  //When to react on rotation change
    private IEnumerator ControlObjectDistanceGrabPosition()
    {
        PlaceMode currentMode = PlaceMode.Distance;
        StolperwegeHelper.User.TeleportBlocked = true;
        bool blockrotationchange = false;

        AutoSpatialLinkHelper olinkhelper = gameObject.AddComponent<AutoSpatialLinkHelper>();
        ActivateColliderFrame(true);

        LineRenderer rotationLine = gameObject.GetComponent<LineRenderer>();
        if (rotationLine == null)
            rotationLine = gameObject.AddComponent<LineRenderer>();

        rotationLine.enabled = false;

        float distanceUpdate = (StolperwegeHelper.User.PointerHand.transform.position - gameObject.transform.position).magnitude;
        //gameObject.transform.position = StolperwegeHelper.PointerSphere.transform.position;
        gameObject.transform.position = StolperwegeHelper.PointerPath.transform.up * distanceUpdate;

        Vector3 newPosition;
        int rot_axis = 0;  //Rotation Modus

        while (SteamVR_Actions.default_grab.GetState(StolperwegeHelper.User.PointerHandType))
        {
            if (SteamVR_Actions.default_click.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                if (currentMode == PlaceMode.Distance)
                {
                    //Scaling
                    StolperwegeHelper.User.RotationBlocked = true;
                    currentMode = PlaceMode.Scale;
                    rotationLine.enabled = false;
                    PointFinger.SetPointerEmissionColor(StolperwegeHelper.GUCOLOR.SONNENGELB);
                    PointFinger.SetPointerColor(StolperwegeHelper.GUCOLOR.SENFGELB);
                }
                else if (currentMode == PlaceMode.Scale)
                {
                    //Rotating
                    StolperwegeHelper.User.RotationBlocked = true;
                    rot_axis = 0;
                    currentMode = PlaceMode.Rotation;
                    LineRendererHelper.LineToCircle(gameObject, rot_axis);
                    rotationLine.enabled = true;
                    PointFinger.SetPointerEmissionColor(StolperwegeHelper.GUCOLOR.ORANGE);
                    PointFinger.SetPointerColor(StolperwegeHelper.GUCOLOR.EMOROT);
                }
                else if (currentMode == PlaceMode.Rotation)
                {
                    //Moving
                    StolperwegeHelper.User.RotationBlocked = false;
                    currentMode = PlaceMode.Distance;
                    rotationLine.enabled = false;
                    PointFinger.SetPointerEmissionColor(StolperwegeHelper.GUCOLOR.GOETHEBLAU);
                    PointFinger.SetPointerColor(Color.black);
                }
            }
            newPosition = StolperwegeHelper.User.PointerHand.transform.position;
            float touchInput = SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).y;

            if (currentMode == PlaceMode.Distance)
            {
                if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x < x_buffer && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x > -x_buffer)
                {
                    //Update Distance to User
                    distanceUpdate += touchInput / 6f;
                }
            }
            else if (currentMode == PlaceMode.Scale)
            {
                if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x < x_buffer && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x > -x_buffer)
                {
                    //Update Scale between 0.05f and 10f
                    Vector3 newScale = gameObject.transform.localScale + new Vector3(touchInput, touchInput, touchInput) / 10;
                    gameObject.transform.localScale = Vector3.Min(new Vector3(10f, 10f, 10f), Vector3.Max(newScale, new Vector3(0.05f, 0.05f, 0.05f)));
                }
            }
            else if (currentMode == PlaceMode.Rotation)
            {
                //Only move once, when moveing trigger on x-Achsis
                if (!blockrotationchange)
                {

                    //Change Rotaton Axis
                    if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x > 0.5)
                    {
                        rot_axis = (rot_axis + 1) % 3;
                        blockrotationchange = true;
                        LineRendererHelper.LineToCircle(gameObject, rot_axis);
                    }
                    else if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x < -0.5)
                    {
                        rot_axis = Mathf.Abs((rot_axis - 1) % 3);
                        blockrotationchange = true;
                        LineRendererHelper.LineToCircle(gameObject, rot_axis);
                    }
                }
                else
                {
                    //Unblock Rotationchange
                    if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x > -0.1 & SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x < 0.1)
                    {
                        blockrotationchange = false;
                    }
                }


                if (rot_axis == 0)
                {
                    //Rotate around y-Achsis
                    if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x < x_buffer && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x > -x_buffer)
                    {
                        gameObject.transform.Rotate(Vector3.up * touchInput);
                    }
                }
                if (rot_axis == 1)
                {
                    if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x < x_buffer && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x > -x_buffer)
                    {
                        gameObject.transform.Rotate(Vector3.right * touchInput);
                    }
                }
                if (rot_axis == 2)
                {
                    if (SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x < x_buffer && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).x > -x_buffer)
                    {
                        gameObject.transform.Rotate(Vector3.forward * touchInput);
                    }
                }

            }
            newPosition -= StolperwegeHelper.PointerPath.transform.up * Mathf.Min(20, Mathf.Max(distanceUpdate, 0.1f));

            gameObject.transform.position = newPosition;
            yield return null;
        }

        StolperwegeHelper.User.TeleportBlocked = false;
        PointFinger.SetPointerEmissionColor(StolperwegeHelper.GUCOLOR.GOETHEBLAU);
        PointFinger.SetPointerColor(Color.black);
        rotationLine.enabled = false;
        ActivateColliderFrame(false);
        OnDrop();

        olinkhelper.ActivateAllConnectedColliderFrames(false);
        HashSet<InteractiveShapeNetObject> all_qs_links = olinkhelper.GetAllQSConnectedObjects();
        Dictionary<InteractiveShapeNetObject, string> all_o_links = olinkhelper.GetAllOConnectedObjects();
        Destroy(olinkhelper);

        ObjectTab objTab = ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder.GetTab<ObjectTab>();

        if (!objTab.CheckGrabLinkAutoCreation)
        {
            yield break;
        }

        if (objTab.CheckQSLinkAutoCreation && Entity.GetType()!= typeof(IsoSRelation))
        {
            foreach (InteractiveShapeNetObject iso in all_qs_links)
            {
                bool skipIso = false;
                foreach (IsoLink link in Entity.LinkedVia.Keys)
                {
                    if (link.Figure == iso.Entity || link.Ground == iso.Entity)
                    {
                        skipIso = true;
                        break;
                    }
                }
                    
                if (skipIso) continue;
                foreach (IsoEntity ent in iso.Entity.AllRelatedQSLinkFigures())
                {
                    Debug.Log(ent.InteractiveShapeNetObject.name);
                    if (ent.InteractiveShapeNetObject != null && all_qs_links.Contains(ent.InteractiveShapeNetObject))
                    {
                        skipIso = true;
                        break;
                    }
                }
                    
                if (skipIso) continue;

                while (SceneBuilderSceneScript.WaitingForResponse)
                    yield return null;

                string rcc8type = Rcc8Test.Rcc8(gameObject, iso.gameObject);
                if (rcc8type == "TPPc")
                    iso.Entity.SendLinkRequest(AnnotationTypes.QSLINK, Entity, "TTP", comment: "auto generated");
                else if (rcc8type == "NTTPc")
                    iso.Entity.SendLinkRequest(AnnotationTypes.QSLINK, Entity, "IN", comment: "auto generated");
                else
                    Entity.SendLinkRequest(AnnotationTypes.QSLINK, iso.Entity, rcc8type, comment: "auto generated");
            }
        }

        if (objTab.CheckOLinkAutoCreation && Entity.GetType() != typeof(IsoSRelation))
        {
            foreach (InteractiveShapeNetObject iso in all_o_links.Keys)
            {
                bool skip = false;
                string relType = all_o_links[iso];
                foreach (IsoLink link in Entity.LinkedVia.Keys)
                    if ((link.Figure == iso.Entity || link.Ground == iso.Entity) && link.Rel_Type.Equals(relType))
                    { 
                        skip = true;
                        break;
                    }
                if (skip)
                    continue;

                while (SceneBuilderSceneScript.WaitingForResponse)
                    yield return null;
                
                Entity.SendOLinkRequest(iso.Entity, objTab.frameMode, objTab.referencePt, relType, comment: "auto generated");
            }
        }

    }
}
using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class StolperwegeObject : InteractiveObject 
{

    protected bool Clone = false;
    protected bool OnlyBaseReset;
    protected GameObject OriginalObject = null;
    public bool ShowTitle = true;
    public bool Linkable = true;
    protected StolperwegeInterface StolperwegeInterface;

    public Vector3 StartPos { get; set; }

    private InverseRotation _label;
    public InverseRotation Label {

        get
        {
            if (_label == null) HoverTitle = _hoverTitle;

            return _label;
        }

        set
        {
            _label = value;
        }
    }



    /// <summary>
    /// Anzeigen eines Labels über des Elementes
    /// </summary>
    private bool _hoverTitle = true;
    public bool HoverTitle
    {
        get
        {
            return _hoverTitle;
        }

        set
        {
            _hoverTitle = value;
            if(_label == null)
            {
                GameObject labelGo;
                labelGo = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeLabel")));
                labelGo.GetComponent<TextMeshPro>().text = Referent.Value;
                Label = labelGo.GetComponent<InverseRotation>();
                Label.hover = gameObject;
            }

            Label.gameObject.SetActive(_hoverTitle);
        }
    }

    public override void Start()
    {
        base.Start();
        StolperwegeInterface = SceneController.GetInterface<StolperwegeInterface>();
        IsCollectable = false;
        gameObject.tag = "StolperwegeObject";
        Removable = true;
        Grabable = true;
        ManipulatedByGravity = false;
        UpdateElement();

        OnLongClick = () =>
        {
            if (GetComponentInParent<StolperwegeWordObject>() != null)
            {
                GetComponentInParent<StolperwegeWordObject>().RemoveDR(this);

                return;
            }

            if (!allRelatedShown)
                ShowAllRelatedObjects();
            else
                HideRelatedObjects();
        };

        OnClick = () =>
        {
            if (GetComponentInParent<StolperwegeWordObject>() == null)
                OnGrab(StolperwegeHelper.User.PointerHand.GetComponentInParent<DragFinger>().GetComponent<Collider>());
            else
                Referent.Draw().GetComponent<StolperwegeObject>().OnGrab(StolperwegeHelper.User.PointerHand.GetComponentInParent<DragFinger>().GetComponent<Collider>());
        };
        OnHold += OnDrag;
    }

    //public virtual void Awake()
    //{
    //    normalScale = transform.localScale;
    //    backpack = GameObject.Find("Backpack");

    //}

    /// <summary>
    /// Zurücksetzen auf den Stand, welches das Element nach dem Start des Programmes besaß
    /// </summary>
    public override void ResetObject()
    {

        base.ResetObject();
        transform.eulerAngles = Vector3.zero;

        // listobjects inherits from this class, but must have only the reset method of interactive object
        if (OnlyBaseReset) return;

        gameObject.SetActive((Referent == null || Referent.GetUnityPosition() != null));

        HighlightWords(false, 1);
    }

    private CircleMenu menu;
    private float RemoverTiggeredTime = float.PositiveInfinity;
    private bool RemoverTriggered = false;

    protected override void OnTriggerEnter(Collider other)
    {

        if (Removable && other.name.Equals("StolperwegeObjectRemover") && Grabbing.Count == 1)
        {
            RemoverTiggeredTime = Time.time;
            RemoverTriggered = true;
            return;
        }
            

        base.OnTriggerEnter(other);

        //Interaktionen mit dem Finger
        if (other.GetComponent<PointFingerColliderScript>() != null)
        {
            //Erstellung von Relationen
            if (Linkable)
            {
                StolperwegeRelationAnchor anchor = other.GetComponentInChildren<StolperwegeRelationAnchor>();
                if (anchor != null && anchor.Relation)
                {
                    foreach (StolperwegeRelationAnchor a in GetComponentsInChildren<StolperwegeRelationAnchor>())
                    {
                        if (a.Relation == anchor.Relation) return;
                    }

                    if(anchor.Relation.StartAnchor.parent.GetComponent<StolperwegeObject>() != null)
                    {
                        anchor.Relation.StartAnchor.parent.GetComponent<StolperwegeObject>().Referent.AddStolperwegeRelation(Referent,true);
                        //StolperwegeRelation.AddRelation(Referent, anchor.Relation.StartAnchor.parent.GetComponent<StolperwegeObject>().Referent, StolperwegeRelation.RelationType.EQUIVALENT);
                        Destroy(anchor.gameObject);
                    }

                    ConnectionPoint cp = anchor.Relation.StartAnchor.GetComponentInParent<ConnectionPoint>();
                    if(cp != null)
                    {
                        if(Referent.GetType() == cp.Type.to || Referent.GetType().IsSubclassOf(cp.Type.to))
                        {
                            other.GetComponentInChildren<StolperwegeRelationAnchor>().Relation.type = cp.Type;
                            other.GetComponentInChildren<StolperwegeRelationAnchor>().transform.SetParent(transform, false);
                            cp.Value = Referent.ID;
                        }
                    }

                    return;
                }

                GameObject path = (GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeRelation"));

                Transform start = path.transform.Find("StartAnchor").transform, end = path.transform.Find("EndAnchor").transform;

                start.parent = transform;
                start.localPosition = Vector3.zero;
                end.parent = other.transform;
                end.localPosition = Vector3.zero;



                path.GetComponent<StolperwegeRelation>().DoOnClick = () =>
                {

                    if (menu != null) return;
                    menu = (((GameObject)Instantiate(Resources.Load("StolperwegeElements/CircleMenu")))).GetComponent<CircleMenu>();

                    menu.transform.parent = path.transform;

                    //Hashtable types = new Hashtable();
                    //foreach (string type in StolperwegeInferface.typeSystemTable.Keys)
                    //    if (type.StartsWith("org.hucompute"))
                    //        types.Add(type.Replace("org.hucompute.publichistory.datastore.typesystem.", ""), StolperwegeInferface.typeSystemTable[type]);

                    Hashtable types = new Hashtable();

                    HashSet<string> outTypes = Referent.getOutgoingTypes();

                    foreach(string str in outTypes)
                    {
                        types.Add(str, StolperwegeInterface.TypeClassTable[str]);
                    }

                    menu.Init((string key, object o) => {
                        GameObject dummy = StolperwegeInterface.CreateElementDummy("org.hucompute.publichistory.datastore.typesystem." + key);
                        dummy.transform.position = menu.transform.position;
                        path.GetComponent<StolperwegeRelation>().EndAnchor.parent = dummy.transform;
                        path.GetComponent<StolperwegeRelation>().EndAnchor.localPosition = Vector3.zero;
                        Destroy(menu.gameObject);
                    }, types);

                    menu.transform.position = path.GetComponent<StolperwegeRelation>().EndAnchor.position;
                    menu.transform.forward = -StolperwegeHelper.CenterEyeAnchor.transform.forward;
                    menu.transform.localScale = Vector3.one * 0.2f;
                };
            }
        }



    }

    protected virtual void OnTriggerExit(Collider other)
    {

        if (Removable && other.name.Equals("StolperwegeObjectRemover") && RemoverTriggered)
        {
            RemoverTriggered = false;
        }
    }

    protected StolperwegeElement _referent;

    /// <summary>
    /// Das zu dieser Repräsentation gehöhrige StolperwegeElement
    /// </summary>
    public virtual StolperwegeElement Referent
    {

        get
        {
            return _referent;
        }

        set
        {
            if (_referent != null || value == null) return;



            _referent = value;

            if(GetComponent<MeshRenderer>()!= null)
                GetComponent<MeshRenderer>().material.color = Referent.Color;

            gameObject.name = _referent.Value;
            UnityPosition stPos = (_referent is DiscourseReferent)?((DiscourseReferent)_referent).GetUnityPosition():null;
            
            _referent.StolperwegeObject = this;


            if (stPos != null)
                transform.position = ((DiscourseReferent)_referent).GetUnityPosition().Position;
            else
                Debug.Log("War auf inaktive gesetzt");
                //gameObject.SetActive(false);

            RelationsVisible = true;

            StartPos = transform.position;

            HoverTitle = ShowTitle;
        }
    }

    /// <summary>
    /// Legt das Element in den Zwischenspeicher ab
    /// </summary>
    public override void OnPack()
    {
        base.OnPack();

        if (ScaleMultiplier.x < 0.3f)
        {
            RelationsVisible = false;
            Label.Scale = 0.5f;
        }

        HighlightWords(false, 1);

    }

    public override GameObject OnGrab(Collider other)
    {
        if (!Grabable) return gameObject;

        GameObject result = gameObject;
        //if (!clone && false)
        //{
        //    GameObject copyObj = Instantiate(gameObject);
        //    copyObj.GetComponent<Collider>().enabled = false;
        //    copyObj.GetComponent<Collider>().enabled = true;
        //    copyObj.GetComponent<StolperwegeObject>().clone = true;
        //    copyObj.GetComponent<StolperwegeObject>().originalObject = gameObject;
        //    copyObj.GetComponent<StolperwegeObject>().Transparent = false;
        //    copyObj.GetComponent<StolperwegeObject>().onGrab(other);
        //    grabable = false;
        //    Transparent = true;

        //    copyObj.GetComponent<StolperwegeObject>().Referent = Referent;
        //    copyObj.SetActive(true);

            

        //    result = copyObj;
        //}
        //else
        //{
        if (transform.parent != null && (transform.parent.GetComponent<SearchRequest>() != null || transform.parent.GetComponent<InventoryController>() != null))
        {
            RelationsVisible = true;
            Label.Scale = 1;
            transform.localEulerAngles = new Vector3(90, 180, 0);
        }
                

        base.OnGrab(other);
        if (OnlyBaseReset) return null;
        gameObject.SetActive(true);
        //}

        HighlightWords(true, 1);

        if (result.transform.localPosition.magnitude > 0.15 && !GetComponent<Collider>().bounds.Intersects(other.bounds))
        {
            int a = (other.name.Contains("left")) ? -1 : 1;
            result.transform.localPosition = Vector3.zero + other.transform.forward * 0.1f * a;
        }


        return result;

    }

    public override bool OnDrop(Collider other)
    {
        //if (!clone || !base.onDrop(other)) return false;
        base.OnDrop(other);
        if (OnlyBaseReset) return true;
        HighlightWords(false, 1);
        //GetComponent<StolperwegeObject>().originalObject.GetComponent<StolperwegeObject>().Transparent = false;
        //GetComponent<StolperwegeObject>().originalObject.GetComponent<StolperwegeObject>().grabable = true;
        //Destroy(gameObject);

        
        return true;
    }

    public virtual void DeleteElement()
    {
        StartCoroutine(StolperwegeInterface.DeleteElement((string)Referent.ID));
        gameObject.SetActive(false);
    }

    public override bool OnDrop()
    {
        base.OnDrop();

        if (Removable && RemoverTriggered)
        {
            if (Time.time - RemoverTiggeredTime > 1 && Referent != null)
            {
                DeleteElement();
            }
            else
            {
                OnRemove?.Invoke();
                if (!DestroyOnObjectRemover) ResetObject();
                else Destroy(gameObject);
            }

            RemoverTriggered = false;
            RemoverTiggeredTime = float.PositiveInfinity;
        }

        if (OnlyBaseReset) return true;
        HighlightWords(false, 1);

        return true;
    }



    public override void OnPointerEnter(Collider other)
    {
        base.OnPointerEnter(other);

        HightlightRelations(true);

        HighlightWords(true, 1);
    }

    public override void OnPointerExit()
    {
        base.OnPointerExit();

        HightlightRelations(false);

        HighlightWords(false, 1);
    }

    private void HightlightRelations(bool active)
    {
        foreach (StolperwegeRelationAnchor anchor in GetComponentsInChildren<StolperwegeRelationAnchor>())
            anchor.Relation.Highlight = active;
    }

    public bool dragging = false;
    protected float lastdeltax;

    //Auseinanderziehen des Objektes mit beiden Händen
    public virtual void OnDrag()
    {
        if (Grabbing.Count != 2) return;
        GameObject hand1 = Grabbing.LeftArm.gameObject;
        GameObject hand2 = Grabbing.RightArm.gameObject;
        if (!dragging)
        {
            lastdeltax = (hand1.transform.position - hand2.transform.position).magnitude;
            dragging = true;
        }
        else
        {
            float deltadrag = (hand1.transform.position - hand2.transform.position).magnitude / lastdeltax;
            if(deltadrag > 1) ScaleMultiplier = new Vector3(deltadrag, deltadrag, deltadrag);

            if (transform.localScale.x >= (_normalScale.x * 2)) OnExpand();
            
                
        }

    }

    //Alleinstehende ExpandView
    public virtual ExpandView OnExpand()
    {

        GameObject container = new GameObject("ExpandViewContainer");
        dragging = false;
        Grabbing.Clear();

        ExpandView expandView = ((GameObject) Instantiate(Resources.Load("StolperwegeElements/StolperewegeExpandView"))).GetComponent<ExpandView>();
        expandView.StObject = this;
        expandView.SetParentObject(ParentObject);
        

        container.transform.position = expandView.transform.position;
        expandView.transform.parent = container.transform;
        container.transform.parent = ParentObject?.transform;

        container.transform.position = transform.position;
        container.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
        container.transform.eulerAngles = new Vector3(0, container.transform.eulerAngles.y+180, 0);
        Vector3 diff = container.transform.position - StolperwegeHelper.CenterEyeAnchor.transform.position;
        diff.y = 0;
        diff.Normalize();
        container.transform.position += 0.5f * diff;

        transform.parent = container.transform;
        //transform.parent = expandView.transform;
        transform.localEulerAngles = Vector3.up * 180;
        transform.localPosition = Vector3.up * -0.75f;
        //transform.localScale = normalScale + (normalScale.x * Vector3.forward * 99);
        gameObject.SetActive(true);
        resetScale();
        HighlightWords(false, 1);

        if (Referent != null)
        {
            //ShowAllRelatedObjects();

            Color expandViewColor = Referent.Color;
            expandViewColor.a = 178f / 255f;
            expandView.GetComponent<MeshRenderer>().material.color = expandViewColor;
        }
        return expandView;
    }

    Hashtable shownObjects;
    bool allRelatedShown = false;

    public void ShowAllRelatedObjects()
    {
        shownObjects = new Hashtable();

        HashSet<StolperwegeObject> calculatePositions = new HashSet<StolperwegeObject>();
        foreach (StolperwegeElement element in Referent.RelatedElements)
        {
            bool drawn = false;
            if (element.StolperwegeObject == null)
            {
                drawn = true;
                element.Draw();

                if(element.StolperwegeObject == null) continue;
            }

            
            UnityPosition stPos = (element is DiscourseReferent) ? ((DiscourseReferent)element).GetUnityPosition() : null;

            
            if (!element.StolperwegeObject.gameObject.activeInHierarchy || drawn)
            {
                element.StolperwegeObject.gameObject.SetActive(true);
                if (stPos == null)
                {
                    calculatePositions.Add(element.StolperwegeObject);
                }
                else
                {
                    element.StolperwegeObject.transform.position = stPos.Position;
                    shownObjects.Add(element, stPos.Position);
                }
                
            }

            
        }

        float deltaRot = 180f / calculatePositions.Count;
        int i = 0;
        foreach (StolperwegeObject obj in calculatePositions)
        {
            //obj.transform.position = transform.position + Vector3.forward * 0.5f;

            //obj.transform.RotateAround(transform.position, transform.position + Vector3.up, deltaRot*i++);

            Quaternion q = Quaternion.AngleAxis(deltaRot * i++, Vector3.up);

            obj.transform.position = transform.position + q * Vector3.forward;
            //transform.rotation = q;

            shownObjects.Add(obj, transform.position + q * Vector3.forward);
        }

        allRelatedShown = true;
    }

    public void HideRelatedObjects()
    {

        if (shownObjects == null) return;

        foreach(StolperwegeObject obj in shownObjects.Keys)
        {
            if (obj.transform.position.Equals(shownObjects[obj]))
                obj.gameObject.SetActive(false);
        }

        allRelatedShown = false;
    }

    //Eingebettete ExpandView 
    public virtual ExpandView OnEnbed() {
        ExpandView expandView = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperewegeExpandView"))).GetComponent<ExpandView>();
        expandView.Embeded = true;
        expandView.StObject = this;
        return expandView;
    }


    private bool _relationsVisible;

    public bool RelationsVisible
    {
        get
        {
            return _relationsVisible;
        }

        set
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                StolperwegeRelationAnchor anchor;
                if ((anchor = transform.GetChild(i).GetComponent<StolperwegeRelationAnchor>()) != null)
                    anchor.gameObject.SetActive(value);
            }

            if (value == _relationsVisible || Clone) return;

            

            _relationsVisible = value;
            // unused varible below, commented out because of cleanup
            //StolperwegeRelationAnchor[] anchors = GetComponentsInChildren<StolperwegeRelationAnchor>();
            

            if (_relationsVisible && transform.Find("StartAnchor") == null)
            {
                foreach(StolperwegeInterface.RelationType type in Referent.Relations.Keys)
                {
                    HashSet<StolperwegeElement> relations = (HashSet < StolperwegeElement > )Referent.Relations[type];

                    foreach(StolperwegeElement element in relations)
                    {
                        StolperwegeRelation temp = StolperwegeRelation.AddRelation(Referent, element, type);
                        if (temp == null) continue;
                        GameObject path = temp.gameObject;

                        if (!gameObject.activeInHierarchy && path != null) path.GetComponent<StolperwegeRelation>().Visibility = false;
                    }
                }
                //foreach (DiscourseReferent dr in Referent.equivalents)
                //{
                //    if (dr.Object3D == null) continue;
                //    GameObject path = StolperwegeRelation.AddRelation(Referent, dr).gameObject;

                //    if (!gameObject.activeInHierarchy && path != null) path.GetComponent<StolperwegeRelation>().Visibility = false;
                //}
            }
        }
    }

    public void HighlightWords(bool highlight, int depth)
    {
        StolperwegeWordObject[] words = ConnectedWords;

        if (words[1] != null)
        {
            foreach (StolperwegeWordObject word in ((StolperwegeTextObject)words[0].Referent.StolperwegeObject).GetWords((int)words[0].Range.x, (int)words[1].Range.y))
            {
                if (word != null)
                    word.Highlighted = highlight;
            }
        }
        else if (words[0] != null)
            words[0].Highlighted = highlight;

        if (depth > 0)
        {
            foreach (StolperwegeRelationAnchor anchor in GetComponentsInChildren<StolperwegeRelationAnchor>())
            {
                if (anchor.OtherEnd.GetComponentInParent<StolperwegeObject>() != null) anchor.OtherEnd.GetComponentInParent<StolperwegeObject>().HighlightWords(highlight, depth - 1);
                else if (anchor.OtherEnd.GetComponentInParent<ConnectionPoint>() != null)
                {
                    anchor.OtherEnd.GetComponentInParent<ConnectionPoint>().GetComponentInParent<NewElement>().HighlightWords(highlight, depth - 1);
                }
            }
            foreach (ConnectionPoint cp in GetComponentsInChildren<ConnectionPoint>())
            {
                foreach (StolperwegeRelationAnchor anchor in cp.GetComponentsInChildren<StolperwegeRelationAnchor>())
                {
                    if (anchor.OtherEnd.GetComponentInParent<NewElement>() != null) anchor.OtherEnd.GetComponentInParent<NewElement>().HighlightWords(highlight, depth - 1);
                    else if (anchor.OtherEnd.GetComponentInParent<ConnectionPoint>() != null)
                    {
                        anchor.OtherEnd.GetComponentInParent<ConnectionPoint>().GetComponentInParent<NewElement>().HighlightWords(highlight, depth - 1);
                    }
                }
            }


        }

    }

    public StolperwegeWordObject[] ConnectedWords
    {
        get
        {
            StolperwegeWordObject[] words = new StolperwegeWordObject[2];
            int i = 0;
            foreach (StolperwegeRelationAnchor a in GetComponentsInChildren<StolperwegeRelationAnchor>())
            {
                if (a.OtherEnd == null) continue;
                StolperwegeWordObject word = a.OtherEnd.GetComponentInParent<StolperwegeWordObject>();
                if (word != null && i < 2)
                {
                    words[i++] = word;
                }
            }

            return words;
        }
    }

    public void CalculatePos(Vector3 offset)
    {
        if (Referent is DiscourseReferent && Referent.GetUnityPosition() != null) return;

        Vector3 pos = Vector3.zero;
        int i = 0;
        foreach(StolperwegeElement element in Referent.RelatedElements)
        {
            if(element.StolperwegeObject != null && element.StolperwegeObject.gameObject.activeInHierarchy)
            {
                i++;
                pos += element.StolperwegeObject.transform.position;
            }
        }

        if (i == 0) return;

        pos /= i;

        transform.position = pos+offset;


    }

    /*public override bool Equals(object obj)
    {
        if (this is NewElement) return base.Equals(obj);
        if (obj == null || Referent == null || obj.GetType() != GetType()) return false;
        StolperwegeObject other = (StolperwegeObject)obj;
        return Referent.Equals(other.Referent);
    }*/

    /*public override int GetHashCode()
    {
        if (Referent == null) return base.GetHashCode();
        return Referent.GetHashCode();
    }*/

    public override string ToString()
    {
        if (Referent == null) return "";
        return Referent.ToString();
    }

    public virtual void UpdateElement()
    {

    }

    public GameObject GetIcon(Texture t)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.GetComponent<Renderer>().material.mainTexture = t;
        StolperwegeHelper.ChangeBlendMode(plane.GetComponent<Renderer>().material, "Fade");

        Destroy(plane.GetComponent<Collider>());


        plane.transform.parent = transform;
        plane.transform.localPosition = Vector3.zero;
        plane.transform.localScale = Vector3.one * 0.08f;

        StolperwegeHelper.ChangeBlendMode(GetComponent<Renderer>().material, "Fade");
        GetComponent<Renderer>().material.color = new Color(Referent.Color.r, Referent.Color.g, Referent.Color.b, 0.4f);

        return plane;
    }
}

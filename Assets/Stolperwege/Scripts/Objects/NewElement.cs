using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewElement : StolperwegeObject {

    public delegate void ExcecuteOnUpload(StolperwegeElement e);
    public ExcecuteOnUpload excecuteOnUpload;

    public override void Start()
    {
        base.Start();

        Transparent = true;
    }

    public new string Type { get; set; }

    public override StolperwegeElement Referent
    {
        get
        {
            return base.Referent;
        }

        set
        {
            _referent = value;
        }
    }

    private bool[,,] CPAnchors;
    private int p = 1;

    private Vector3 getAnchor()
    {

        bool free = false;
        int x = 0, y = 0, z = 0;

        while (!free)
        {
            x = Random.Range(-p, p);
            y = Random.Range(-p, p);
            z = Random.Range(-p, p);

            if(!CPAnchors[x+p, y+p, z+p] && x+y+z != 0)
            {
                CPAnchors[x+p, y+p, z+p] = true;
                free = true;
            }
        }

        Vector3 vec = new Vector3(x , y , z );
        vec.Normalize();

        return vec;
    }

    public void Init(string type, HashSet<StolperwegeInterface.RelationType> relations)
    {
        if (type.Equals("org.hucompute.publichistory.datastore.typesystem.ArgumentRole"))
        {
            ShowRolesMenu();
            Destroy(gameObject);
            return;
        }

        CPAnchors = new bool[10,10,10];
        Debug.Log("Init");
        Type = type;
        GameObject sphere = transform.Find("Sphere").gameObject;
        // unused, commented out because of project-cleanup
        //HashSet<StolperwegeInferface.RelationType> propertys = new HashSet<StolperwegeInferface.RelationType>();
        IEnumerator enumerator = relations.GetEnumerator();

        for (int c = 0; c < 3; c++)
            for (int x = -1; x < 2; x += 1)
                for (int y = -1; y < 2; y += 1)
                    for (int z = -1; z < 2; z += 1)
                        if (c == getZeroCount(x, y, z) && enumerator.MoveNext())
                        {
                            GameObject property = (GameObject)Instantiate(sphere);
                            property.transform.parent = transform;
                            property.transform.localPosition = new Vector3(0.6f* x, 0.6f * y, 0.6f * z);
                            property.transform.localScale = Vector3.one * 0.15f;
                            property.GetComponent<ConnectionPoint>().Type = (StolperwegeInterface.RelationType)enumerator.Current;
                        }


        GameObject center = Instantiate(sphere);
        center.transform.parent = transform;
        center.transform.localPosition = Vector3.zero;
        center.transform.localScale = Vector3.one * 0.15f;
        center.GetComponent<ConnectionPoint>().setLabel(Type.Replace("org.hucompute.publichistory.datastore.typesystem.", ""));

        Destroy(sphere);
                       
    }

    private int getZeroCount(int x, int y, int z)
    {
        int c = 0;

        if (x == 0) c++;
        if (y == 0) c++;
        if (z == 0) c++;

        return c;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PointFingerColliderScript>() != null)
        {
            StolperwegeRelationAnchor anchor = other.GetComponentInChildren<StolperwegeRelationAnchor>();
            if(anchor != null)
            {
                ConnectionPoint cp = anchor.OtherEnd.GetComponentInParent<ConnectionPoint>();

                if (cp != null && cp.transform.parent != transform && 
                    ((System.Type)Referent.StolperwegeInterface.TypeClassTable[Type] == cp.Type.to || 
                    ((System.Type)Referent.StolperwegeInterface.TypeClassTable[Type]).IsSubclassOf(cp.Type.to)))
                {
                    anchor.transform.parent = transform;
                    anchor.transform.localPosition = Vector3.zero;
                    cp.Value = gameObject;
                    return;
                }

                StolperwegeWordObject word = anchor.OtherEnd.GetComponentInParent<StolperwegeWordObject>();
                if(word != null && word != ConnectedWords[0] &&  
                    ((System.Type)Referent.StolperwegeInterface.TypeClassTable[Type] == (System.Type)Referent.StolperwegeInterface.TypeClassTable["org.hucompute.publichistory.datastore.typesystem.DiscourseReferent"] 
                    || ((System.Type)Referent.StolperwegeInterface.TypeClassTable[Type]).IsSubclassOf((System.Type)Referent.StolperwegeInterface.TypeClassTable["org.hucompute.publichistory.datastore.typesystem.DiscourseReferent"])))
                {
                    NewElement newDummy = StolperwegeInterface.CreateElementDummy("org.hucompute.publichistory.datastore.typesystem.DiscourseReferent").GetComponent<NewElement>();

                    Vector3 pos = anchor.transform.position - anchor.OtherEnd.transform.position;
                    pos.Normalize();

                    newDummy.transform.position = anchor.OtherEnd.transform.position + 0.4f * pos;

                    newDummy.SetProperty("value", word.Text);

                    anchor.transform.parent = newDummy.transform;
                    anchor.transform.localPosition = Vector3.zero;

                    newDummy.SetProperty("equivalent", this);

                    StolperwegeRelation relation = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeRelation"))).GetComponent<StolperwegeRelation>();

                    relation.StartAnchor.SetParent(newDummy.getPropertyCP("equivalent").transform, false);
                    relation.EndAnchor.SetParent(transform, false);
                    relation.StartAnchor.localPosition = Vector3.zero;
                    relation.EndAnchor.localPosition = Vector3.zero;
                }
            }

            return;
        }

        base.OnTriggerEnter(other);
    }

    

    public bool Ready
    {
        get
        {
            foreach (ConnectionPoint cp in GetComponentsInChildren<ConnectionPoint>())
                if (!cp.Ready) return false;

            return true;
        }
    }

    private bool MandatorysReady
    {
        get
        {
            foreach (ConnectionPoint cp in GetComponentsInChildren<ConnectionPoint>())
                if(cp.Type.mandatory && cp.Type.type == StolperwegeInterface.RelType.RELATION)
                    foreach(StolperwegeObject obj in cp.RelatedObjects)
                        if (obj.GetType() == typeof(NewElement))
                            if (((NewElement)obj).Referent == null) return false;

            return true;
        }
    }

    public override void OnDrag()
    {
        if(Ready)
            base.OnDrag();
    }

    bool uploading = false;

    public void UploadThis()
    {
        uploading = true;
        Grabable = false;
        Grabbing.Clear();
        OnDrop();
        resetScale();
        StartCoroutine(Upload(this));

        HighlightWords(false, 1);
    }


    public override ExpandView OnExpand()
    {
        if(!uploading && MandatorysReady)
            UploadThis();
        else
        {
            Grabbing.Clear();
            OnDrop();
            resetScale();
        }


        return null;

        // code below unreachable, commented out because of project-cleanup
        /*
        HashSet<NewElement> elements = new HashSet<NewElement>();

        Queue<NewElement> elementQueue = new Queue<NewElement>();
        elementQueue.Enqueue(this);

        while (elementQueue.Count > 0)
        {
            NewElement e = elementQueue.Dequeue();

            elements.Add(e);

            foreach(ConnectionPoint cp in GetComponentsInChildren<ConnectionPoint>())
            {
                if(!cp.Property)
                {
                    foreach(StolperwegeRelationAnchor anchor in cp.GetComponentsInChildren<StolperwegeRelationAnchor>())
                    {
                        if (anchor.Relation.EndAnchor.transform.parent != transform && anchor.Relation.EndAnchor.GetComponentInParent<NewElement>() != null)
                            if (!elements.Contains(anchor.Relation.EndAnchor.GetComponentInParent<NewElement>()))
                                elementQueue.Enqueue(anchor.Relation.EndAnchor.GetComponentInParent<NewElement>());

                        else if (anchor.Relation.StartAnchor.transform.parent != transform && anchor.Relation.StartAnchor.GetComponentInParent<NewElement>() != null)
                            if (!elements.Contains(anchor.Relation.StartAnchor.GetComponentInParent<NewElement>()))
                                elementQueue.Enqueue(anchor.Relation.StartAnchor.GetComponentInParent<NewElement>());
                    }
                        
                }
            }
        }

        Helper.stolperwegeInterface.StartCoroutine(StolperwegeInferface.CreateElements(elements, (StolperwegeElement e) => { e.Object3D.onGrab(grabbing.Get); }));

        Destroy(gameObject);

        return null;*/
    }

    public bool Invisible
    {
        set
        {
            GetComponent<MeshRenderer>().enabled = !value;
            GetComponent<BoxCollider>().enabled = !value;
        }
    }

    //bool marked = false;

    public static IEnumerator Upload(NewElement newE)
    {
        HashSet<NewElement> elements = new HashSet<NewElement>();

        Queue<NewElement> elQueue = new Queue<NewElement>();

        elQueue.Enqueue(newE);
        elements.Add(newE);
        newE.Grabbing.Clear();

        newE.OnDrop();

        newE.ChangeColor(UnityEngine.Color.blue);




        while (elQueue.Count > 0)
        {
            NewElement current = elQueue.Dequeue();
            Debug.Log(current.GetHashCode());


            if(!current.Ready)
            {
                newE.resetScale();
                newE.ChangeColor(UnityEngine.Color.red);
                newE.Grabable = true;
                yield break;
            }

            foreach (ConnectionPoint cp in current.GetComponentsInChildren<ConnectionPoint>())
                foreach(StolperwegeObject obj in cp.RelatedObjects)
                    if (obj.GetType() == typeof(NewElement) && !elQueue.Contains((NewElement)obj) && !elements.Contains((NewElement)obj))
                    {
                        elQueue.Enqueue((NewElement)obj);
                        elements.Add((NewElement)obj);
                    }
                        

            foreach(StolperwegeRelationAnchor anchor in current.GetComponentsInChildren<StolperwegeRelationAnchor>())
            {
                StolperwegeObject obj = anchor.OtherEnd.GetComponentInParent<StolperwegeObject>();

                if(obj != null && obj.GetType() == typeof(NewElement) && !elQueue.Contains((NewElement)obj) && !elements.Contains((NewElement)obj) )
                {
                    elQueue.Enqueue((NewElement)obj);
                    elements.Add((NewElement)obj);
                }
                    
            }
        }

        Debug.Log(elements.Count);

        HashSet<NewElement> uploaded = new HashSet<NewElement>();
        bool blocked = false;

        while (elements.Count > 0 && !blocked)
        {
            blocked = true;
            foreach(NewElement element in elements)
            {
                if (element.Referent != null) {
                    elements.Remove(element);
                    uploaded.Add(element);
                    break;
                }

                if (element.MandatorysReady)
                {
                    element.Grabable = false;
                    element.ChangeColor(Color.blue);
                    yield return element.StolperwegeInterface.CreateElement(element, (StolperwegeElement e) => { e.StolperwegeObject.transform.position = element.transform.position; element.Referent = e;
                        element.excecuteOnUpload?.Invoke(e);
                    });
                    
                    elements.Remove(element);
                    uploaded.Add(element);
                    blocked = false;
                    break;
                }
            }
        }


        foreach (NewElement element in uploaded)
        {
            yield return Link(element);
        }


        /*foreach (NewElement element in uploaded)
        {
            foreach (Renderer r in element.GetComponentsInChildren<Renderer>())
                r.enabled = false;
            foreach (Collider r in element.GetComponentsInChildren<Collider>())
                r.enabled = false;
        }*/

        foreach (NewElement element in uploaded)
            Destroy(element.gameObject);
            

    }

    public static IEnumerator Link(NewElement element)
    {
        StolperwegeElement stElement = element.Referent;

        if (stElement == null) yield break;

        foreach (ConnectionPoint cp in element.GetComponentsInChildren<ConnectionPoint>())
            foreach (StolperwegeObject obj in cp.RelatedObjects)
            {
                if (stElement.Relations.Contains(cp.Type) && ((HashSet<StolperwegeElement>)stElement.Relations[cp.Type]).Contains(obj.Referent)) continue;

                yield return stElement.AddStolperwegeRelationAsynch(obj.Referent, cp.Type);
            }

        foreach(StolperwegeRelationAnchor anchor in element.GetComponentsInChildren<StolperwegeRelationAnchor>())
        {
            if (anchor.GetComponentInParent<ConnectionPoint>() != null || anchor.OtherEnd.GetComponentInParent<ConnectionPoint>() != null) continue;

            StolperwegeObject obj = anchor.OtherEnd.GetComponentInParent<StolperwegeObject>();

            if (obj != null && !(obj is NewElement))
            {
                yield return stElement.AddStolperwegeRelationAsynch(obj.Referent);
            }
        }
    }

    public void ChangeColor(UnityEngine.Color c)
    {
        UnityEngine.Color color = GetComponent<MeshRenderer>().material.color;

        c.a = color.a;

        GetComponent<MeshRenderer>().material.color = c;
    }

    public void SetProperty(string key, object value)
    {
        foreach (ConnectionPoint cp in GetComponentsInChildren<ConnectionPoint>())
            if (cp.Type.title != null &&  cp.Type.title.Equals(key)) cp.Value = value;
    }

    public object GetProperty(string key)
    {
        foreach (ConnectionPoint cp in GetComponentsInChildren<ConnectionPoint>())
            if (cp.Type.title != null && cp.Type.title.Equals(key)) return cp.Value;

        return null;
    }

    public ConnectionPoint getPropertyCP(string key)
    {
        foreach (ConnectionPoint cp in GetComponentsInChildren<ConnectionPoint>())
            if (cp.Type.title != null && cp.Type.title.Equals(key)) return cp;

        return null;
    }

    private void ShowRolesMenu()
    {
        CircleMenu menu = (((GameObject)Instantiate(Resources.Load("StolperwegeElements/CircleMenu")))).GetComponent<CircleMenu>();

        

        Hashtable types = new Hashtable();
        foreach (ArgumentRole role in ArgumentRole.Roles)
        {
           types.Add(role.Value, role);
        }


        menu.Init((string key, object o) =>
        {
            ArgumentRole role = (ArgumentRole)o;

            role.StolperwegeObject.transform.position = menu.transform.position;
            role.StolperwegeObject.gameObject.SetActive(true);
            Destroy(menu.gameObject);

        }, types);

        StolperwegeHelper.PlaceInFrontOfUser(menu.transform, 0.5f);
        menu.transform.localScale = Vector3.one * 0.2f;

    }
}

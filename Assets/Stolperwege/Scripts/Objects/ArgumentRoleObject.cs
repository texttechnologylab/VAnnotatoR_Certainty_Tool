using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Beschreibt größtenteils die alte Funktionsweise der ArgumentRoles (Boxendarstellung), die neue Funktionsweise (unter Text-Token) wurde in StolperwegeTextObject festgelegt
/// </summary>
public class ArgumentRoleObject : InteractiveObject {

    private InverseRotation Label = null;
    private DiscourseReferentObject linked = null;

    private bool _mendatory = false;
    public bool Mendatory
    {
        get
        {
            return _mendatory;
        }

        set
        {
            _mendatory = value;

            Ready = !_mendatory || linked != null;
        }
    }

    public DiscourseReferent Referent { get; set; }

    private bool _ready = false;
    public bool Ready
    {
        get
        {
            return _ready;
        }

        set
        {
            _ready = value;

            Color c = Color.blue;
            if (_ready) c = Color.green;
            else if (_mendatory) c = Color.red;

            c.a = 0.4f;

            GetComponent<MeshRenderer>().material.color = c;

            gameObject.layer = (_ready)? 2 : 0;
        }
    }

    public Predicate Predicate
    {
        get;set;
    }

    private ArgumentRole _role = null;
    public ArgumentRole Role
    {
        get
        {
            return _role;
        }

        set
        {
            _role = value;
            SetName(_role.Value);
        }
    }

    public override void Start()
    {
        base.Start();
        OnLongClick = () => { RemoveRole(); };
    }

    public void RemoveRole()
    {
        if (linked != null) linked.transform.parent = null;

        Role.RmvStolperwegeRelation(Predicate, (Mendatory) ? PredicateObject.mendatoryRole : PredicateObject.optionalRole);

        if(Term != null)
        {
            StolperwegeInterface.RelationType conTermRel = SceneController.GetInterface<StolperwegeInterface>().GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.TermConnector", "terms");

            foreach(StolperwegeElement conTerm in Term.GetRelatedElementsByType(conTermRel))
            {
                conTerm.RmvStolperwegeRelation(Term, conTermRel);
            }
        }

        Destroy(gameObject);
    }

    DiscourseReferentObject lastEnteredGrabbed = null;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        DiscourseReferentObject dr = other.GetComponent<DiscourseReferentObject>();
        if(dr != null && dr.IsGrabbed)
        {
            lastEnteredGrabbed = dr;
        }
    }

    protected override void OnTriggerStay(Collider other)
    {
        base.OnTriggerStay(other);

        DiscourseReferentObject dr = other.GetComponent<DiscourseReferentObject>();
        if(dr != null && linked == null && dr != linked && !dr.IsGrabbed && lastEnteredGrabbed == dr)
        {
            SetLinkedDiscoureReferent(dr);

            dr.transform.parent = transform;
            dr.transform.localPosition = Vector3.zero;
            dr.transform.localRotation = Quaternion.identity;

            lastEnteredGrabbed = null;
        }

        if(dr != null && dr==linked && !dr.IsGrabbed && dr.transform.parent != transform)
        {
            dr.transform.parent = transform;
            dr.transform.localPosition = Vector3.zero;
            dr.transform.localRotation = Quaternion.identity;
        }
    }

    protected void OnTriggerExit(Collider other)
    {

        DiscourseReferentObject dr = other.GetComponent<DiscourseReferentObject>();
        if (dr != null )
        {
            if(dr == linked && dr.IsGrabbed)
            {
                SetLinkedDiscoureReferent(null);
            }
            if (dr == lastEnteredGrabbed)
                lastEnteredGrabbed = null;
        }

    }

    private void SetName(string name)
    {
        if(Label == null)
        {
            Label = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeLabel"))).GetComponent<InverseRotation>();
            Label.hover = gameObject;
            Label.Scale = -0.35f;
        }

        Label.Text = name;
    }

    public void SetTerm(StolperwegeTerm term)
    {
        this.Term = term;

        StolperwegeInterface.RelationType termArgumentRel = SceneController.GetInterface<StolperwegeInterface>().GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Term", "argument");

        foreach (StolperwegeElement stEl in term.GetRelatedElementsByType(termArgumentRel))
        {
            //if (stEl.Object3D == null) stEl.draw();

            /*

            stEl.Object3D.transform.parent = transform;
            stEl.Object3D.transform.localPosition = Vector3.zero;
            stEl.Object3D.transform.localRotation = Quaternion.identity;
            

            StolperwegeRelation relation = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeRelation"))).GetComponent<StolperwegeRelation>();

            relation.StartAnchor.SetParent(stEl.Object3D.transform, false);
            relation.EndAnchor.SetParent(transform, false);
            relation.StartAnchor.localPosition = Vector3.zero;
            relation.EndAnchor.localPosition = Vector3.zero;
            */
            linked = (DiscourseReferentObject) stEl.StolperwegeObject;
            Referent = (DiscourseReferent)stEl;
            Ready = true;

            return;
        }
    }

    public StolperwegeTerm Term = null;

    public void SetLinkedDiscoureReferent(DiscourseReferentObject dr)
    {
        DiscourseReferentObject oldDR = linked;
        linked = dr;
        Ready = linked != null;

        if (oldDR == linked) return;

        if (Ready)
        {
            if(Term == null)
                StartCoroutine(SetTerm(linked.Referent));
            else
            {
                Term.AddStolperwegeRelation(linked.Referent, SceneController.GetInterface<StolperwegeInterface>().GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Term", "argument"), true);
            }
        }
        else
        {
            if (Term != null && oldDR != null)
                Term.RmvStolperwegeRelation(oldDR.Referent, SceneController.GetInterface<StolperwegeInterface>().GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Term", "argument"));
        }
        
    }

    public IEnumerator SetTerm(DiscourseReferent dr)
    {

        Hashtable keys = new Hashtable
        {
            {"predicate",Role.ID },
            {"arguments",dr.ID }
        };

        yield return SceneController.GetInterface<StolperwegeInterface>().CreateElement("term", keys, (StolperwegeElement e) => { Term = (StolperwegeTerm)e; });

        if (Term == null) yield break;

        StolperwegeInterface.RelationType termConPropRel = SceneController.GetInterface<StolperwegeInterface>().GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.Proposition", "termconnectors");
        StolperwegeInterface.RelationType termConRel = SceneController.GetInterface<StolperwegeInterface>().GetRelationTypeFromName("org.hucompute.publichistory.datastore.typesystem.TermConnector", "terms");

        HashSet<StolperwegeElement> termConns = (HashSet<StolperwegeElement>)Predicate.proposition.Relations[termConPropRel];

        if (termConns.Count == 0) yield break;

        foreach(StolperwegeElement termConn in termConns)
        {
            termConn.AddStolperwegeRelation(Term, termConRel, true);
            break;
        }

        Referent = dr;
    }
}

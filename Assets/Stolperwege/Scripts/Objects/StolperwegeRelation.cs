using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class StolperwegeRelation : MonoBehaviour {

    /// <summary>
    /// Besteht aus einem Zylinder, welcher zwischen zwei Ankern gezeichnet wird.
    /// Wenn auf den Zylinder gezeigt wird, wird der Relationstyp angezeigt und kann ggf. verändert werden
    /// </summary>

    private Transform  path;

    public Transform StartAnchor { get; private set;}
    public Transform EndAnchor { get; private set; }

    private string id; //Besteht aus den URIs der beiden Elemente und des Types

    private bool _visibility;

    public StolperwegeInterface.RelationType type;

    public delegate void OnClick();
    
    public OnClick DoOnClick { get; set; } //Wird ausgelöst, wenn die Aktionstaste während des Erstellens einer Relation betätigt wird

    public bool DrawOnGround = false; //Benötigt Navmesh
    public bool PathConnected;
    public bool SettingUp { get; private set; }

    public bool Visibility {
        get
        {
            return _visibility;
        }

        set
        {
            _visibility = value;
            for(int i=0; i<transform.childCount; i++)
            {
                if (transform.GetChild(i).name.Contains("Cylinder"))
                    transform.GetChild(i).gameObject.SetActive(_visibility);
            }
        }
    }

	void Awake () {

        if (StartAnchor == null)
        {
            StartAnchor = transform.Find("StartAnchor");
            StartAnchor.GetComponent<StolperwegeRelationAnchor>().Relation = this;
        }


        if (EndAnchor == null)
        {
            EndAnchor = transform.Find("EndAnchor");
            EndAnchor.GetComponent<StolperwegeRelationAnchor>().Relation = this;
        }

        if (path == null)
            path = transform.Find("Cylinder");

        Visibility = false;

        
    }

    public bool Highlight
    {
        set
        {
            foreach (StolperwegeRelationPath p in GetComponentsInChildren<StolperwegeRelationPath>())
                p.Highlight = value;

            StolperwegeObject start = StartAnchor.GetComponentInParent<StolperwegeObject>();
            if (start != null) start.Highlight = value;
            StolperwegeObject end = EndAnchor.GetComponentInParent<StolperwegeObject>();
            if (end != null) end.Highlight = value;
        }
    }

    Vector3 lastPosStart, lastPosEnd;

    void Update () {
        //if(EndAnchor.GetComponentInParent<PointFinger>()!= null)
        //{
        //    path.position = EndAnchor.position + (StartAnchor.position - EndAnchor.position) * 0.5f;
        //    path.up = (StartAnchor.position - EndAnchor.position);
        //    path.localScale = new Vector3(0.01f, (StartAnchor.position - EndAnchor.position).magnitude / 2, 0.01f);

            //    if (DoOnClick != null && EndAnchor.GetComponentInParent<PointFinger>().IsClicking)
            //        DoOnClick();
            //}
            //else if(EndAnchor.GetComponentInParent<StolperwegeObject>() != null &&( lastPosStart != StartAnchor.position || lastPosEnd != EndAnchor.position))
        if (EndAnchor.GetComponentInParent<StolperwegeObject>() != null && (lastPosStart != StartAnchor.position || lastPosEnd != EndAnchor.position))
        {
            lastPosStart = StartAnchor.position;
            lastPosEnd = EndAnchor.position;


            //Navmesh Logik für AR ggf. entfernen und nur Fall corners.Length == 2 nutzen
            //Berechnet Pfad zwischen den Ankern. Falls laut Navmesh Objekte zwischen diesen befinden, verläuft er auf dem Boden
            NavMeshPath navPath = new NavMeshPath();
            NavMeshHit navHitStart;
            NavMesh.SamplePosition(StartAnchor.position, out navHitStart, 3f, NavMesh.AllAreas);

            NavMeshHit navHitEnd;
            NavMesh.SamplePosition(EndAnchor.position, out navHitEnd, 3f, NavMesh.AllAreas);

            if (navHitEnd.hit && navHitStart.hit)
                if (!NavMesh.CalculatePath(navHitStart.position, navHitEnd.position, NavMesh.AllAreas, navPath)) return;

            Vector3 lastCorner = StartAnchor.position;
            int i;

            Vector3[] corners = navPath.corners;

            if (corners.Length == 1)
                corners[0] = StartAnchor.position;

            float starty = (StartAnchor.GetComponentInParent<ConnectionPoint>() != null || StartAnchor.GetComponentInParent<StolperwegeWordObject>() != null || true) ? 0.05f : 0.2f;
            float endy = (EndAnchor.GetComponentInParent<ConnectionPoint>() != null || EndAnchor.GetComponentInParent<StolperwegeWordObject>() != null || true) ? 0.05f : 0.2f;

            if (corners.Length == 2 && !DrawOnGround)
            {
                corners[0] = (corners[0].y < StartAnchor.position.y- starty) ? StartAnchor.position + Vector3.up * -starty : corners[0];
                corners[1] = (corners[1].y < EndAnchor.position.y - endy) ? EndAnchor.position + Vector3.up * -endy : corners[1];
            }

            //Für jeden Teilabschnitt des Pfades wird ein Zylinder erstellt
            for (i = 0; i <= corners.Length; i++)
            {  
                Vector3 corner = (i < corners.Length) ? corners[i] : EndAnchor.position;

                Transform tempPath = (i < transform.childCount) ? transform.GetChild(i) : Instantiate(path);
                tempPath.parent = transform;
                tempPath.position = corner + (lastCorner - corner) * 0.5f;
                tempPath.up = (lastCorner - corner);
                tempPath.localScale = new Vector3(0.03f, (lastCorner - corner).magnitude / 2, 0.03f);
                tempPath.GetComponent<CapsuleCollider>().height = 1.8f;

                if ((lastCorner - corner).magnitude < 0.1f)
                    tempPath.GetComponent<CapsuleCollider>().enabled = false;
                else
                    tempPath.GetComponent<CapsuleCollider>().enabled = true;

                lastCorner = corner;
            }

            while (i < transform.childCount)
            {
                Destroy(transform.GetChild(i).gameObject);
                i++;
            }

            Visibility = Visibility;
        }
        

        if ((!StartAnchor.gameObject.activeInHierarchy || !EndAnchor.gameObject.activeInHierarchy) && Visibility)
            Visibility = false;
        else if ((StartAnchor.gameObject.activeInHierarchy && EndAnchor.gameObject.activeInHierarchy) && !Visibility)
            Visibility = true;
	}

    public IEnumerator HandlePathSetup()
    {
        PathConnected = false;
        SettingUp = true;
        PointFinger finger = EndAnchor.GetComponentInParent<PointFinger>();
        while (!PathConnected)
        {
            path.position = EndAnchor.position + (StartAnchor.position - EndAnchor.position) * 0.5f;
            path.up = (StartAnchor.position - EndAnchor.position);
            path.localScale = new Vector3(0.01f, (StartAnchor.position - EndAnchor.position).magnitude / 2, 0.01f);

            if (DoOnClick != null && finger.IsClicking)
                DoOnClick();

            if (!finger.IsPointing)
            {
                Destroy(this);
                yield break;
            }
            yield return null;
        }
        SettingUp = false;
    }

    public void OnDestroy()
    {
        Destroy(path.gameObject);
        if(StartAnchor != null) Destroy(StartAnchor.gameObject);
        if(EndAnchor != null) Destroy(EndAnchor.gameObject);

        Destroy(gameObject);
    }

    //HashSet<GameObject> attachedObjects;

    //public void attachObject(GameObject go)
    //{
    //    attachedObjects.Add(go);
    //}

    private static Hashtable relations = new Hashtable(); //Beinhaltet alle erstellten Relationen

    /// <summary>
    /// Erstellt eine Relation eines bestimmten Types zwischen zwei Elementen, falls diese noch nicht vorhanden ist
    /// </summary>
    /// <param name="ref1"></param>
    /// <param name="ref2"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static StolperwegeRelation AddRelation(StolperwegeElement ref1, StolperwegeElement ref2, StolperwegeInterface.RelationType type)
    {
        if (ref1.StolperwegeObject == null || ref2.StolperwegeObject == null || ref1 == ref2) return null;

        //Prüft, ob Relation schon vorhanden ist
        string rel1 = "";
        rel1 = (string)ref1.ID + (string)ref2.ID + type.title;
        string rel2 = (string)ref2.ID + (string)ref1.ID + type.title;
        if (relations.Contains(rel1)) return (StolperwegeRelation)relations[rel1];
        if (relations.Contains(rel2)) return (StolperwegeRelation)relations[rel2];

        StolperwegeRelation relation = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeRelation"))).GetComponent<StolperwegeRelation>();

        relation.StartAnchor.SetParent(ref1.StolperwegeObject.transform,false);
        relation.EndAnchor.SetParent(ref2.StolperwegeObject.transform,false);
        relation.StartAnchor.localPosition = Vector3.zero;
        relation.EndAnchor.localPosition = Vector3.zero;
        relation.type = type;
        relation.id = rel1; 

        relations.Add(rel1, relation);

        return relation;
    }

    /// <summary>
    /// Erstellt eine noch nicht vorhandene Relation zwischen zwei Elementen
    /// </summary>
    /// <param name="ref1"></param>
    /// <param name="ref2"></param>
    /// <returns></returns>
    public static StolperwegeRelation AddRelation(StolperwegeElement ref1, StolperwegeElement ref2)
    {
        if (ref1.StolperwegeObject == null || ref2.StolperwegeObject == null || ref1 == ref2) return null;

        HashSet<StolperwegeInterface.RelationType> types = ref1.getRelationsTypesTo(ref2);
        HashSet<StolperwegeInterface.RelationType> types2 = ref2.getRelationsTypesTo(ref1);
        if(types2 != null) types.UnionWith(types2);

        if (types.Count == 0) return null;

        int i = types.Count;
        string rel1 = "";

        foreach (StolperwegeInterface.RelationType type in types)
        {
            rel1 = (string)ref1.ID + (string)ref2.ID + type.title;
            string rel2 = (string)ref2.ID + (string)ref1.ID + type.title;
            i--;
            if (!(relations.Contains(rel1) || relations.Contains(rel2)))
                return AddRelation(ref1, ref2, type);

            if (relations.Contains(rel1) && i <= 1) return (StolperwegeRelation)relations[rel1];
            if (relations.Contains(rel2) && i <= 1) return (StolperwegeRelation)relations[rel2];
        }

        return null;
    }

    public static void ChangeType(StolperwegeRelation relation, StolperwegeInterface.RelationType newType)
    {
        StolperwegeElement ref1 = relation.StartAnchor.parent.GetComponent<StolperwegeObject>().Referent;
        StolperwegeElement ref2 = relation.EndAnchor.parent.GetComponent<StolperwegeObject>().Referent;

        string rel1 = (string)ref1.ID + (string)ref2.ID + relation.type.title;
        string rel2 = (string)ref2.ID + (string)ref1.ID + relation.type.title;

        if (relations.Contains(rel1))
            relations.Remove(rel1);
        if (relations.Contains(rel2))
            relations.Remove(rel2);

        ref1.RmvStolperwegeRelation(ref2, relation.type);
        ref1.AddStolperwegeRelation(ref2, newType,true);

        Destroy(relation);

        //relation.type = newType;

        //string newRel = ref1.id + ref2.id + relation.type.title;

        //relations.Add(newRel, relation);
    }

    public static void RmvRelation(StolperwegeElement ref1, StolperwegeElement ref2, StolperwegeInterface.RelationType type)
    {
        string rel1 = (string)ref1.ID + (string)ref2.ID + type.title;
        string rel2 = (string)ref2.ID + (string)ref1.ID + type.title;
        StolperwegeRelation relation = (StolperwegeRelation)((relations.Contains(rel1)) ? relations[rel1] : (relations.Contains(rel2)) ? relations[rel2] : null);

        if (relation == null) return;

        relations.Remove(relation.id);
        Destroy(relation.gameObject);
    }

    public static void RmvRelation(StolperwegeRelation rel)
    {
        if (rel.id == null || !relations.Contains(rel.id))
        {
            Destroy(rel.gameObject);
            return;
        }

        relations.Remove(rel.id);

        StolperwegeElement start = rel.StartAnchor.GetComponentInParent<StolperwegeObject>().Referent;
        StolperwegeElement end = rel.EndAnchor.GetComponentInParent<StolperwegeObject>().Referent;

        start.RmvStolperwegeRelation(end, rel.type);
        end.RmvStolperwegeRelation(start, rel.type);

        SceneController.GetInterface<StolperwegeInterface>().StartCoroutine(SceneController.GetInterface<StolperwegeInterface>().RemoveRelation(start, end, rel.type));

        Destroy(rel.gameObject);
    }


}

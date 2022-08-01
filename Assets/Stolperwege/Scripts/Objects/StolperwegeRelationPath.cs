using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StolperwegeRelationPath : InteractiveObject {

    StolperwegeRelation Relation;

    GameObject label;

    public override void Start()
    {
        base.Start();

        //blockOutline = true;

        if (Relation == null && transform.parent != null)
            Relation = transform.parent.GetComponent<StolperwegeRelation>();
    }


    public override void OnPointerEnter(Collider other)
    {
        base.OnPointerEnter(other);

        if (Relation == null && transform.parent != null)
            Relation = transform.parent.GetComponent<StolperwegeRelation>();

        Relation.Highlight = true;

        if (label != null) return;

        //Erstellt ein Label, welches den Relationstyp darstellt
        label = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/StolperwegeRelationLabel")));

        label.transform.Find("Label").GetComponent<TextMeshPro>().text = Relation.type.title;
        GameObject go = new GameObject("sdasd");
        go.transform.parent = transform;
        go.transform.position = other.transform.position;
        label.GetComponent<InverseRotation>().hover = go;

        //Bei Klick mit dem Pointer, wird ein Auswahlmenü zum ändern des Relationstypen erstellt
        label.transform.Find("Label").GetComponent<InverseRotation>().OnClick = () =>
        {
            GameObject l = label.transform.Find("Label").gameObject;
            GameObject menu = ((GameObject)Instantiate(Resources.Load("StolperwegeElements/CircleMenu")));
            
            HashSet<StolperwegeInterface.RelationType> typesTo = Relation.StartAnchor.parent.GetComponent<StolperwegeObject>().Referent.getRelationsTypesTo(Relation.EndAnchor.parent.GetComponent<StolperwegeObject>().Referent);
            HashSet<StolperwegeInterface.RelationType> typesFrom = Relation.EndAnchor.parent.GetComponent<StolperwegeObject>().Referent.getRelationsTypesTo(Relation.StartAnchor.parent.GetComponent<StolperwegeObject>().Referent);

            CircleMenu.onSelection executor = (string key, object o) =>
            {

                if(key.Equals("Neuer Typ"))
                {
                    StolperwegeHelper.VRWriter.Interface.DoneClicked += (string query) => {
                        StartCoroutine(SceneController.GetInterface<StolperwegeInterface>().CreateRelationType(query, Relation.StartAnchor.parent.GetComponent<StolperwegeObject>().Referent.Type, Relation.EndAnchor.parent.GetComponent<StolperwegeObject>().Referent.Type, (StolperwegeInterface.RelationType rType) =>
                        {
                            l.GetComponent<TextMeshPro>().text = rType.title;
                            StolperwegeRelation.ChangeType(Relation, rType);
                        }));
                    };
                    StolperwegeHelper.VRWriter.Active = true;
                    return;
                }

                StolperwegeInterface.RelationType type = (StolperwegeInterface.RelationType)o;
                l.GetComponent<TextMeshPro>().text = type.title;
                StolperwegeRelation.ChangeType(Relation, type);
                //Relation.type = new StolperwegeInferface.RelationType();

                l.SetActive(true);

                Destroy(menu);
            };

            Hashtable relations = new Hashtable();

            foreach (StolperwegeInterface.RelationType t in typesTo)
                relations.Add(t.title, t);

            foreach (StolperwegeInterface.RelationType t in typesFrom)
                if(!relations.Contains(t.title))
                    relations.Add(t.title, t);

            relations.Add("Neuer Typ", null);

            if(relations.Count> 1)
            {
                menu.GetComponent<CircleMenu>().Init(executor, relations);

                menu.transform.position = l.transform.position + Vector3.up * 0.5f;
                menu.transform.LookAt(Player);
                l.SetActive(false);

                menu.transform.localScale *= 0.2f;
            }
            
        };
        label.transform.Find("GoToStart").GetComponent<InverseRotation>().OnClick = () =>
        {
            StolperwegeHelper.User.Teleport(Relation.StartAnchor.position, 1);
        };
        label.transform.Find("GoToEnd").GetComponent<InverseRotation>().OnClick = () =>
        {
            StolperwegeHelper.User.Teleport(Relation.EndAnchor.position, 1);
        };

        print(label.name);

        toggle = true;

    }

    bool toggle = true;

    public override bool OnPointerClick()
    {
        OnPointerEnter(StolperwegeHelper.PointerSphere.GetComponent<Collider>());
        base.OnPointerClick();

        if(label != null)
            if (toggle)
            {
                for (int i = 0; i < label.transform.childCount; i++)
                    label.transform.GetChild(i).gameObject.SetActive(toggle);
            }
            else
            {
                Destroy(label.GetComponent<InverseRotation>().hover.gameObject);
                Destroy(label);
                label = null;
            }
            

        toggle = !toggle;

        return true;
    }

    public override void OnPointerExit()
    {
        base.OnPointerExit();

        Relation.Highlight = false;

        if (label != null && label.GetComponent<InverseRotation>().hover != null && toggle)
        {
            Destroy(label.GetComponent<InverseRotation>().hover.gameObject);
            Destroy(label);
            label = null;
        }
            
    }

    private DragFinger currentHand;

    //Beim verdrehen der Relation, wird sie entfernt
    public bool onTwist(DragFinger hand, float state)
    {
        if (currentHand != null && currentHand != hand) return false;
        currentHand = hand;
        UnityEngine.Color c = GetComponent<MeshRenderer>().material.color;
        c.r = 1;
        c.g = 1-state;
        c.b = 1-state;

        GetComponent<MeshRenderer>().material.color = c;

        if (state >= 1) StolperwegeRelation.RmvRelation(transform.parent.GetComponent<StolperwegeRelation>());

        return true;


    }

    public void onTwistRelease()
    {
        UnityEngine.Color c = GetComponent<MeshRenderer>().material.color;
        c.r = 1;
        c.g = 1;
        c.b = 1;

        GetComponent<MeshRenderer>().material.color = c;

        currentHand = null;
    }

}

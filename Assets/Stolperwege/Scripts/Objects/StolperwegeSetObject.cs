using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StolperwegeSetObject : StolperwegeObject {

    private bool _extended = false;

    private Transform Cube;

    private Hashtable positions = new Hashtable();

    bool Extended
    {
        get
        {
            return _extended;
        }

        set
        {
            if (_extended == value) return;


            _extended = value;
            if (_extended)
            {
                

                foreach (StolperwegeElement e in Referent.GetRelatedElementsByType(StolperwegeInterface.ContainsRelation))
                    AddObjectToBox(e.StolperwegeObject);

                UpdatePositions();
            }
            else
            {
                foreach (StolperwegeElement e in Referent.GetRelatedElementsByType(StolperwegeInterface.ContainsRelation))
                    e.StolperwegeObject.gameObject.SetActive(false);
            }

            Locked = true;
            HoverTitle = !_extended;
        }
    }

    private bool _locked = false;

    bool Locked
    {
        get
        {
            return _locked;
        }

        set
        {
            _locked = value;
            Color c;
            if (_locked)
            {
                c = StolperwegeHelper.GUCOLOR.EMOROT;
            }
            else
            {
                c = StolperwegeHelper.GUCOLOR.GOETHEBLAU;
            }

            foreach (StolperwegeElement e in Referent.GetRelatedElementsByType(StolperwegeInterface.ContainsRelation))
                e.StolperwegeObject.Grabable = !_locked;

            c.a = 0;
            Cube.GetComponent<MeshRenderer>().material.SetColor("_TintColor", c);
        }
    }

    public override void Start()
    {
        base.Start();

        Cube = transform.Find("StolperwegeSetCube");
        _normalScale = Cube.localScale;
        Removable = false;
        Locked = true;

        Label.Scale = 0;

        Label.Text = ""+Referent.GetRelatedElementsByType(StolperwegeInterface.ContainsRelation).Count;

        OnLongClick += () =>
        {
            if (!Extended) return;
            foreach (StolperwegeElement e in Referent.GetRelatedElementsByType(StolperwegeInterface.ContainsRelation))
                e.StolperwegeObject.OnExpand();
        };
    }

    public override bool OnPointerClick()
    {
        if(Extended) Locked = !Locked;

        return true;
    }


    public override void OnDrag()
    {
        GameObject hand1 = Grabbing.LeftArm.gameObject;
        GameObject hand2 = Grabbing.RightArm.gameObject;
        if (!dragging)
        {
            lastdeltax = (hand1.transform.position - hand2.transform.position).magnitude;
            _normalScale = Cube.localScale;
            dragging = true;
        }
        else
        {
            float deltadrag = (hand1.transform.position - hand2.transform.position).magnitude / lastdeltax;
             ScaleMultiplier = new Vector3(deltadrag, deltadrag, deltadrag);    

            Extended = (Cube.localScale.x >= (0.35f));

            if(Extended) UpdatePositions();
        }

    }

    protected override void OnTriggerEnter(Collider other)
    {

        if (other.GetComponent<PointFingerColliderScript>() != null)
        {
            Bounds bounds = new Bounds(GetComponent<BoxCollider>().bounds.center, GetComponent<BoxCollider>().bounds.size * 0.9f);

            if (bounds.Contains(other.transform.position)) return;
        }

        base.OnTriggerEnter(other);
        StolperwegeObject stObj;
        if ((stObj = other.GetComponent<StolperwegeObject>()) != null && stObj.IsGrabbed && !Locked && Extended)
        {
            AddElement(stObj);
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        StolperwegeObject stObj;
        if ((stObj = other.GetComponent<StolperwegeObject>()) != null && stObj.IsGrabbed && !Locked && Extended)
        {
            RmvElement(stObj);
        }
    }

    private void AddElement(StolperwegeObject obj)
    {
        if(Referent.GetRelatedElementsByType(StolperwegeInterface.ContainsRelation).Contains(obj.Referent)) return;

        Referent.AddStolperwegeRelation(obj.Referent, StolperwegeInterface.ContainsRelation, true);

        obj.OnDrop();

        AddObjectToBox(obj);

        positions.Add(obj, new Vector3(Random.Range(-0.045f, 0.045f), Random.Range(-0.045f, 0.045f), Random.Range(-0.045f, 0.045f)));
        UpdatePositions();

        Label.Text = "" + Referent.GetRelatedElementsByType(StolperwegeInterface.ContainsRelation).Count;

        foreach (StolperwegeRelationAnchor anchor in GetComponentsInChildren<StolperwegeRelationAnchor>())
            if (anchor.Relation.type.id.Equals(StolperwegeInterface.ContainsRelation.id)) anchor.gameObject.SetActive(false);

    }

    private void RmvElement(StolperwegeObject obj)
    {
        if (!Referent.GetRelatedElementsByType(StolperwegeInterface.ContainsRelation).Contains(obj.Referent)) return;

        Referent.RmvStolperwegeRelation(obj.Referent, StolperwegeInterface.ContainsRelation);

        positions.Remove(obj);

        Label.Text = "" + Referent.GetRelatedElementsByType(StolperwegeInterface.ContainsRelation).Count;
    }

    private void AddObjectToBox(StolperwegeObject go)
    {
        go.gameObject.SetActive(true);
        go.transform.parent = transform;
        go.Grabable = !Locked;
    }

    private void UpdatePositions()
    {
        Vector3 scale = Cube.localScale / 0.135f;

        foreach (StolperwegeElement e in Referent.GetRelatedElementsByType(StolperwegeInterface.ContainsRelation))
        {
            Vector3 pos = (Vector3)positions[e.StolperwegeObject];
            pos.x *= scale.x;
            pos.y *= scale.y;
            pos.z *= scale.z;
            e.StolperwegeObject.transform.localPosition = pos;
        }
            
    }

    public override Vector3 ScaleMultiplier
    {
        get
        {
            return _scaleMultiplier;
        }

        set
        {
            _scaleMultiplier = value;

            Vector3 scale = _normalScale;
            scale.x *= _scaleMultiplier.x;
            scale.z *= _scaleMultiplier.z;
            scale.y *= _scaleMultiplier.y;
            Cube.localScale = scale;

            GetComponent<BoxCollider>().size = scale;
        }
    }
}

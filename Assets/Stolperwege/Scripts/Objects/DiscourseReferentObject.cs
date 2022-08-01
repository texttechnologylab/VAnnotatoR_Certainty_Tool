using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DiscourseReferentObject : StolperwegeObject {


    public override GameObject OnGrab(Collider other)
    {
        if(Hyperedge)
            _normalScale = transform.localScale;

        //if (dummy)
        //{
        //    DiscourseReferent referent = new DiscourseReferent("", "", null);
        //    referent.draw();

        //    Destroy(gameObject);
        //    return referent.Object3D.onGrab(other);
        //}

        return base.OnGrab(other);
    }

    private bool _hyperedge = false;
    public bool Hyperedge {
        get
        {
            return _hyperedge;
        }

        set
        {
            if (_hyperedge == value) return;

            _hyperedge = value;

            Transparent = value;

            transform.Find("Cube").gameObject.SetActive(value);

            return;
            // unreachable code below, commented out because of cleanup
            /*
            if (_hyperedge)
            {
                int i = -Referent.equivalents.Count / 2;
                foreach (DiscourseReferent referent in Referent.equivalents)
                {
                    if (referent == null || referent.Object3D == null) continue;

                    //addReferent(referent);
                }
            }

            else
            {
                foreach (DiscourseReferent referent in Referent.equivalents)
                {
                    if (referent == null || referent.Object3D == null) continue;

                    referent.Object3D.resetScale();
                    referent.Object3D.gameObject.SetActive(false);
                    referent.Object3D.transform.parent = null;
                    referent.Object3D.transform.position = referent.Object3D.StartPos;
                    referent.Object3D.gameObject.SetActive(true);
                }
            }*/
        }
    }

    private void addReferent(DiscourseReferent referent)
    {
        referent.StolperwegeObject.transform.parent = transform;
        referent.StolperwegeObject.transform.localScale = 0.75f * transform.localScale;
        referent.StolperwegeObject.transform.localPosition = 0.5f * Random.insideUnitSphere;
    }

    public new DiscourseReferent Referent
    {
        get
        {
            return (DiscourseReferent)base.Referent;
        }

        set
        {
            base.Referent = value;

            Hyperedge = Referent.IsHyperedge();
                
        }
    }
}

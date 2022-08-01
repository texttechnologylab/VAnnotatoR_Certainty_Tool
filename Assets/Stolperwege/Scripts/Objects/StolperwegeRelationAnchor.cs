using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StolperwegeRelationAnchor : MonoBehaviour {

    public StolperwegeRelation Relation
    {
        get; set;
    }

    public StolperwegeRelationAnchor OtherEnd
    {
        get
        {
            Transform t = (Relation.StartAnchor == transform) ? Relation.EndAnchor : Relation.StartAnchor;            
            return t != null ? t.GetComponent<StolperwegeRelationAnchor>() : null;
        }
    }

    private void OnDestroy()
    {
        if(Relation != null) Destroy(Relation.gameObject);
    }
}

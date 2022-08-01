using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropositionObject : StolperwegeObject
{

    public override void Start()
    {
        OnClick = () =>
        {
            StolperwegeWordObject wordObject = GetComponentInParent<StolperwegeWordObject>();
            if (wordObject != null)
            {
                GetComponent<MeshRenderer>().material.color = Color.white;

                if (wordObject.SubText.MainTextObject.ActivatedPredicate != null)
                    wordObject.SubText.MainTextObject.ActivatedPredicate.GetComponentInChildren<PropositionObject>().GetComponent<MeshRenderer>().material.color =
                    wordObject.SubText.MainTextObject.ActivatedPredicate.GetComponent<MeshRenderer>().material.color;

                wordObject.SubText.MainTextObject.ActivatedPredicate = GetComponent<MeshRenderer>().material.color == Color.white ? GetComponentInParent<PredicateObject>() : null;
            }

        };
    }
}

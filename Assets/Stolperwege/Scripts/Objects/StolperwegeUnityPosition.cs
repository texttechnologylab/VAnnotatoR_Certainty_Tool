using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StolperwegeUnityPosition : StolperwegeObject {

    public override bool OnDrop(Collider other)
    {
        base.OnDrop(other);

        if (Grabbing.Count <= 0)
        {
            StartCoroutine(((UnityPosition)Referent).Update());
        }

        return true;
    }

    public override bool OnDrop()
    {
        base.OnDrop();

        if (Grabbing.Count <= 0)
        {
            StartCoroutine(((UnityPosition)Referent).Update());
        }

        return true;
    }

}

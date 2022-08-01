using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityBuildingSubMesh : InteractiveObject {

    UnityBuilding Referent;

    public override void Awake()
    {
        Grabable = false;
        Removable = false;
        UseHighlighting = false;

        OnLongClick += () =>
        {
            GetComponentInParent<StolperwegeUnityBuilding>().OnLongClick() ;
        };

        OnClick += () =>
        {
            GetComponentInParent<StolperwegeUnityBuilding>().OnClick() ;
        };
    }
}

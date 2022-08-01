using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebcamObject : StolperwegeObject {

    public override ExpandView OnExpand()
    {
        ExpandView expandView = base.OnExpand();

        expandView.SetColor(StolperwegeHelper.GUCOLOR.HELLGRAU);

        GameObject videoBox = ExpandView.createContentBox();
        videoBox.GetComponent<MeshRenderer>().material.color = Color.white;

        expandView.drawComponent(videoBox.transform, ExpandView.LAYOUT.FULL);

        //videoBox.AddComponent<webCamTest>();

        expandView.ScaleMultiplier = new Vector2(0.5f, 0.5f);
        expandView.StartScale = expandView.transform.localScale;


        expandView.dragable = false;

        return expandView;
    }
}

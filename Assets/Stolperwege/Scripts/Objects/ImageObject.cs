using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ImageObject : StolperwegeObject {

    public override StolperwegeElement Referent
    {

        get
        {
            return base.Referent;
        }

        set
        {
            base.Referent = value;

            StartCoroutine(((StolperwegeImage)Referent).LoadImgToRenderer(GetComponent<MeshRenderer>()));

            if(ShowTitle)
                Label.Text = Referent.Description;
        }
    }

    public override ExpandView OnExpand()
    {
        ExpandView expandView = base.OnExpand();

        if (expandView == null) return null;

        addImageView(expandView);

        transform.localPosition -= Vector3.up * 0.1f;

        expandView.ScaleMultiplier = new Vector2(2, 2);        
        
        return expandView;
    }

    public override ExpandView OnEnbed()
    {
        ExpandView expandView = base.OnEnbed();

        if (expandView == null) return null;

        addImageView(expandView);

        return expandView;
    }

    private void addImageView(ExpandView expandView)
    {
        GameObject imageBox = ExpandView.createContentBox();

        if (Referent.Value.Contains("dummysubimage")) expandView.SetColor(StolperwegeHelper.GUCOLOR.GRUEN);
        else expandView.SetColor(StolperwegeHelper.GUCOLOR.HELLESGRUEN);

        StolperwegeImage stImage = (StolperwegeImage)Referent;

        StartCoroutine(stImage.LoadImgToRenderer(imageBox.GetComponent<MeshRenderer>()));

        expandView.drawComponent(imageBox.transform, ExpandView.LAYOUT.FULL);

        imageBox.AddComponent<StolperwegeImageExtended>();
        imageBox.GetComponent<Collider>().enabled = true;
        imageBox.GetComponent<Collider>().isTrigger = true;

        expandView.ScaleMultiplier = new Vector2(0.5f, 0.5f);
        expandView.StartScale = expandView.transform.localScale;

        expandView.dragable = false;
    }
}

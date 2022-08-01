using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleWebBrowser;

public class StolperwegeURIObject : StolperwegeObject {

    public override StolperwegeElement Referent
    {

        get
        {
            return base.Referent;
        }

        set
        {
            base.Referent = value;
            StolperwegeImage image = new StolperwegeImage("", "https://www.google.com/s2/favicons?domain=" + getBaseURI(), null);
            StartCoroutine(image.LoadImgToRenderer(GetComponent<MeshRenderer>()));
        }
    }

    public override ExpandView OnExpand()
    {
        GameObject webbrowser = (GameObject)Instantiate(Resources.Load("StolperwegeElements/Webbrowser"));


        webbrowser.GetComponent<WebBrowser>().InitialURL = Referent.Value.Replace("%26", "&");

        webbrowser.SetActive(true);

        ExpandView result = base.OnExpand();
        result.drawComponent(webbrowser.transform, ExpandView.LAYOUT.FULL);

        result.SetColor(StolperwegeHelper.GUCOLOR.SENFGELB);

        webbrowser.transform.localEulerAngles = Vector3.forward * 180;
        webbrowser.transform.localScale = Vector3.one * 0.095f;

        ShowAllRelatedObjects();

        return result;
    }

    private string getBaseURI()
    {
        int i = 0;
        string baseURI = "";

        foreach (char c in Referent.Value)
        {
            if (c.Equals('/')) i++;
            if (i >= 3) break;

            baseURI += c;
        }

        return baseURI;
    }

}

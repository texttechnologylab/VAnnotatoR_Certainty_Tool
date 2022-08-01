using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.StereoRendering;

public class StolperwegeCameraObject : StolperwegeObject {

    private Transform CameraObject;
    private StereoRenderer Portal;

    private UnityPosition UPos;
    private StolperwegeElement AnchorElement;

    StolperwegeCameraObject ConnectedCam;

    private static Material StandardShader;

    public override StolperwegeElement Referent
    {
        get
        {
            return base.Referent;
        }

        set
        {
            base.Referent = value;
        }
    }

    public override void Awake()
    {
       // CameraObject = transform.Find("stolperwegeCamera");
        Portal = GetComponent<StereoRenderer>();
        if (StandardShader == null)
        {
            StandardShader = new Material(Shader.Find("Diffuse"));
            StandardShader.name = "StandardShader";
        }
    }

    public override void OnDrag()
    {
        GameObject hand1 = Grabbing.LeftArm.gameObject;
        GameObject hand2 = Grabbing.RightArm.gameObject;
        if (!dragging)
        {
            lastdeltax = (hand1.transform.position - hand2.transform.position).magnitude;
            dragging = true;
        }
        else
        {
            float deltadrag = (hand1.transform.position - hand2.transform.position).magnitude / lastdeltax;
            if (deltadrag > 1) ScaleMultiplier = new Vector3(deltadrag, deltadrag, deltadrag);

            //Stream = (transform.localScale.x >= (NormalScale.x * 1.1f));

            Vector3 dir = hand1.transform.position - hand2.transform.position;
            dir.y = 0;

            //transform.right = dir;
        }
    }

    //private bool lastState = true;
    public void Update()
    {

        if(ConnectedCam != null)
            Portal.anchorPos = ConnectedCam.transform.position;

        if(!IsGrabbed && UPos != null && AnchorElement != null && AnchorElement.StolperwegeObject != null)
        {
            transform.position = UPos.Position + AnchorElement.StolperwegeObject.transform.position;
        }
    }

    private bool _stream;
    private bool Stream
    {
        set
        {
            if (value == _stream) return;

            _stream = value;
        }
    }

    private Material CamShader;

    private void SetConnectedCamera()
    {

        ConnectedCam = null;

        foreach(StolperwegeElement se in Referent.GetRelatedElementsByType(StolperwegeInterface.GetRelationTypeFromName(Referent.Type,"target")))
        {
            if (se is StolperwegeCamera)
                ConnectedCam = (StolperwegeCameraObject) se.StolperwegeObject;
        }

        if(ConnectedCam == null)
        {
            foreach (StolperwegeElement se in Referent.GetRelatedElementsByType(StolperwegeInterface.GetRelationTypeFromName(Referent.Type, "equivalent")))
            {
                if (se != Referent && se is StolperwegeCamera && se.StolperwegeObject != null &&  ((StolperwegeCameraObject)se.StolperwegeObject).ConnectedCam != null)
                    ConnectedCam = ((StolperwegeCameraObject)se.StolperwegeObject).ConnectedCam;
            }
        }

        Material[] mats = GetComponent<Renderer>().materials;

        if (ConnectedCam == null)
        {
            for (int i = 0; i < mats.Length; i++)
                if (mats[i].name.Contains("Stereo"))
                {
                    CamShader = mats[i];
                    mats[i] = StandardShader;
                }
                    
            GetComponent<Renderer>().materials = mats;

            return;
        }

        for (int i = 0; i < mats.Length; i++)
            if (mats[i].name.Contains("StandardShader"))
                mats[i] = CamShader;

        GetComponent<Renderer>().materials = mats;
    }



    private void SetAnchorPosition()
    {
        UPos = null;
        AnchorElement = null;

        foreach (StolperwegeElement se in Referent.GetRelatedElementsByType(StolperwegeInterface.GetRelationTypeFromName(Referent.Type, "equivalent")))
        {
            if(se is DiscourseReferent && ((DiscourseReferent)se).IsHyperedge())
            {
                foreach(StolperwegeElement stEl in se.GetRelatedElementsByType(StolperwegeInterface.GetRelationTypeFromName(Referent.Type, "equivalent")))
                {
                    if (stEl is UnityPosition)
                        UPos = (UnityPosition)stEl;
                    else if (!(stEl is StolperwegeCamera))
                        AnchorElement = stEl;
                }
            }
        }
    }

    public override void UpdateElement()
    {
        base.UpdateElement();

        SetConnectedCamera();
        SetAnchorPosition();
    
    }

    public override bool OnDrop()
    {
        if (UPos != null && AnchorElement != null)
        {
            UPos.StolperwegeObject.transform.position = transform.position - AnchorElement.StolperwegeObject.transform.position;

            StartCoroutine(UPos.Update());
        }

        return base.OnDrop();
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;

public class DataContainer : InteractiveObject
{

    public const float MagnifierDelay = 0.6f;

    private GameObject _outliner;
    public GameObject Outliner
    {
        get
        {
            if (_outliner == null)
                _outliner = transform.Find("Outliner").gameObject;
            return _outliner;
        }
    }

    public TextMeshPro Name { get; private set; }
    public TextMeshPro DataType { get; private set; }
    public TextMeshPro DataTextIcon { get; private set; }
    public MeshRenderer Thumbnail { get; private set; }
    public string DataInfoText;
    public string DataPath;
    public DataBrowser Browser { get { return transform.parent.GetComponent<DataPanel>().Browser; } }

    public override bool Highlight
    {
        get => base.Highlight;
        set
        {
            if (value == _highlight) return;
            _highlight = value;
            if ((StolperwegeHelper.CenterEyeAnchor.transform.position - 
                Browser.DataPanel.transform.position).magnitude >  DataPanel.MagnifierActivationDistance && _highlight) 
                StartCoroutine(DelayMagnifier());
            else if (Equals(Browser.DataPanel.MagnifiedContainer))
                Browser.DataPanel.MagnifierActive = false;
            Outliner.SetActive(_highlight);
            SetHighlight();
        }
    }

    private InteractiveObject ResourceObject;
    private VRData _data;
    public VRData Resource
    {
        get { return _data; }
        set
        {
            if (_data != null)
            {
                if (_data.Equals(value)) return;
                _data.SetupDataContainer(null);
            }
            _data = value;
            _data.SetupDataContainer(this);
        }
    }

    public override void Awake()
    {
        base.Awake();
        SearchForParts = false;
        PartsToHighlight = new List<Renderer>() { Outliner.GetComponent<Renderer>() };
        Name = transform.Find("Name").GetComponent<TextMeshPro>();
        DataType = transform.Find("Type").GetComponent<TextMeshPro>();
        DataTextIcon = transform.Find("TextIcon").GetComponent<TextMeshPro>();
        Thumbnail = transform.Find("Thumbnail").GetComponent<MeshRenderer>();
        Thumbnail.material = StolperwegeHelper.ThumbnailMaterial;
        Outliner.SetActive(_highlight);
        LongClickTimer = 1;
        OnLongClick = GetResource;
        LoadingText = "Creating virtual resource...";
    }


    //private StolperwegeElement elem;
    private void SetInfo()
    {
        //if (Resource is StolperwegeElement)
        //{
        //    elem = (StolperwegeElement)Resource;
        //    Name.text = elem.value;
        //    DataType.text = "Type: " + elem.PrettyType;
        //    DataTextIcon.gameObject.SetActive(true);
        //    DataTextIcon.text = elem.ClassIcon;
        //    Thumbnail.enabled = false;
        //}
    }

    private void GetResource()
    {
        if (ResourceObject != null && ResourceObject.transform.parent != null &&
            ResourceObject.transform.parent.Equals(transform)) return;
        Resource.Setup3DObject();
        ResourceObject = Resource.Object3D.GetComponent<InteractiveObject>();

        ResourceObject.transform.parent = transform;
        ResourceObject.transform.rotation = Quaternion.identity;
        if (StolperwegeHelper.GetDistance2DToPlayer(Browser.gameObject) < 0.5f)
            ResourceObject.transform.position = transform.position + transform.forward * 0.3f;
        else
            ResourceObject.transform.position = StolperwegeHelper.CenterEyeAnchor.transform.position + StolperwegeHelper.CenterEyeAnchor.transform.forward * 0.4f;
    }

    private IEnumerator DelayMagnifier()
    {
        yield return new WaitForSeconds(MagnifierDelay);
        if (!Highlight) yield break;
        Browser.DataPanel.SetupMagnifier(this);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DataPanel : MonoBehaviour
{

    public TextMeshPro Title { get; private set; }
    private TextMeshPro SiteIndicator;
    public List<VRData> Datas { get; private set; }
    public DataContainer[] DataContainers { get; private set; }
    private int MaxSites;
    private InteractiveButton NextSite;
    private InteractiveButton PreviousSite;
    public InteractiveButton ParentDir { get; private set; }
    public InteractiveButton Root { get; private set; }
    private Vector3 _standardPos;
    private Quaternion _standardRot;
    private Transform _parent;
    public DataBrowser Browser { get; private set; }

    // Magnifier
    private GameObject Magnifier;
    private GameObject MagnifierOutliner;
    private MeshRenderer Thumbnail;
    private TextMeshPro TextIcon;
    private TextMeshPro Description1;
    private TextMeshPro Description2;
    private LineRenderer LineRenderer;
    public DataContainer MagnifiedContainer;
    private bool _magnifierActive = true;
    private Vector3 TargetPosition;
    private bool _animOn;
    private float _animLerp;
    public const float MagnifierActivationDistance = 0.5f;
    public bool MagnifierActive
    {
        get { return _magnifierActive; }
        set
        {
            if (value == _magnifierActive) return;
            _magnifierActive = value;
            Magnifier.SetActive(_magnifierActive);
            LineRenderer.enabled = false;
            if (_magnifierActive)
            {
                
                TargetPosition = StolperwegeHelper.CenterEyeAnchor.transform.position +
                         StolperwegeHelper.CenterEyeAnchor.transform.forward * MagnifierActivationDistance +
                         StolperwegeHelper.CenterEyeAnchor.transform.right * -0.2f;
                _animLerp = 0;
                _animOn = true;
            } else
            {
                Magnifier.transform.localPosition = Vector3.forward * -0.1f;
                Magnifier.transform.localRotation = Quaternion.identity;
                Magnifier.transform.localScale = Vector3.one * 0.01f;
                MagnifiedContainer = null;
            }
        }
    }

    private int _containerPointer;
    public int ContainerPointer
    {
        get { return _containerPointer; }
        set
        {
            SetComponentStatus(Datas != null);
            if (Datas == null) return;
            _containerPointer = Mathf.Max(0, Mathf.Min(value, (Datas.Count / DataContainers.Length) * DataContainers.Length));
            ActualizeDataContainers();
            ActualizeSiteVariables();
        }
    }

    private bool _baseInit;
    private void BaseInit()
    {
        Browser = transform.parent.GetComponent<DataBrowser>();
        _parent = transform.parent;
        _standardPos = transform.localPosition;
        _standardRot = transform.localRotation;        
        Title = transform.Find("Title").GetComponent<TextMeshPro>();
        SiteIndicator = transform.Find("SiteIndicator").GetComponent<TextMeshPro>();
        DataContainers = GetComponentsInChildren<DataContainer>();
        PreviousSite = transform.Find("PreviousSite").GetComponent<InteractiveButton>();
        PreviousSite.OnClick = () => { ContainerPointer -= DataContainers.Length; };
        NextSite = transform.Find("NextSite").GetComponent<InteractiveButton>();
        NextSite.OnClick = () => { ContainerPointer += DataContainers.Length; };
        ParentDir = transform.Find("ParentDir").GetComponent<InteractiveButton>();
        Root = transform.Find("Root").GetComponent<InteractiveButton>();

        // Magnifier
        Magnifier = (GameObject)Instantiate(Resources.Load("Prefabs/DataBrowser/DataMagnifier"));
        Magnifier.transform.SetParent(transform);
        MagnifierOutliner = Magnifier.transform.Find("Outliner").gameObject;
        MagnifierOutliner.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION_ON");
        MagnifierOutliner.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0, 0.627451f, 5.992157f) * 3);
        Thumbnail = Magnifier.transform.Find("Thumbnail").GetComponent<MeshRenderer>();
        TextIcon = Magnifier.transform.Find("TextIcon").GetComponent<TextMeshPro>();
        Description1 = Magnifier.transform.Find("Description1").GetComponent<TextMeshPro>();
        Description2 = Magnifier.transform.Find("Description2").GetComponent<TextMeshPro>();        
        LineRenderer = gameObject.AddComponent<LineRenderer>();
        LineRenderer.positionCount = 4;
        Material lineMat = (Material)Instantiate(Resources.Load("Materials/UI/MenuButtonMaterial"));
        lineMat.EnableKeyword("_EMISSION_ON");
        lineMat.SetColor("_EmissionColor", new Color(0, 0.627451f, 5.992157f) * 3);
        LineRenderer.material = lineMat;
        LineRenderer.widthMultiplier = 0.001f;
        LineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        LineRenderer.useWorldSpace = false;
        LineRenderer.receiveShadows = false;
        LineRenderer.enabled = false;
        MagnifierActive = false;
        _baseInit = true;
    }

    //public void Init(bool parentButtonActive, string title = null, IEnumerable<object> datas = null)
    public void Init(string title = null, IEnumerable<VRData> datas = null)
    {
        if (!_baseInit) BaseInit();
        Datas = (datas == null) ? null : new List<VRData>(datas);       
        if (title == null)
            Title.text = "Nothing to show, please choose a data space.";
        else
            Title.text = title;

        if (Datas != null)
            MaxSites = Mathf.CeilToInt(Datas.Count / (float)DataContainers.Length);

        ContainerPointer = 0;
    }

    public void SetComponentStatus(bool status)
    {
        SiteIndicator.gameObject.SetActive(status);
        PreviousSite.gameObject.SetActive(status);
        NextSite.gameObject.SetActive(status);
        ParentDir.gameObject.SetActive(status);
        Root.gameObject.SetActive(status);
        foreach (DataContainer dc in DataContainers)
            dc.gameObject.SetActive(status);
    }

    private void ActualizeSiteVariables()
    {
        SiteIndicator.text = "Site " + ((ContainerPointer / DataContainers.Length) + 1) + " of " + Mathf.Max(1, MaxSites);
        PreviousSite.Active = ContainerPointer > 0;
        NextSite.Active = (ContainerPointer + DataContainers.Length) < Datas.Count;
    }

    public void Reset()
    {
        SetColliderStatus(true);
        transform.parent = _parent;
        transform.localPosition = _standardPos;
        transform.localRotation = _standardRot;
    }

    private void Update()
    {
        if (_animOn) ShowMagnifier();
    }

    private void ShowMagnifier()
    {
        if (MagnifiedContainer == null) return;
        Magnifier.transform.position = Vector3.Lerp(Magnifier.transform.position, TargetPosition, _animLerp);
        Magnifier.transform.localScale = Vector3.Lerp(Magnifier.transform.localScale, Vector3.one, _animLerp);
        Magnifier.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
        LineRenderer.enabled = (Magnifier.transform.position - MagnifiedContainer.transform.position).magnitude > 0.05f;
        if (LineRenderer.enabled)
        {
            Vector3 elemScale = MagnifierOutliner.transform.localScale * 0.98f;
            LineRenderer.SetPosition(0, Magnifier.transform.localPosition + Magnifier.transform.localRotation * new Vector3(-elemScale.x / 2, elemScale.y / 2, 0));
            LineRenderer.SetPosition(3, Magnifier.transform.localPosition + Magnifier.transform.localRotation * new Vector3(-elemScale.x / 2, -elemScale.y / 2, 0));
            elemScale = MagnifiedContainer.Outliner.transform.localScale;
            LineRenderer.SetPosition(1, MagnifiedContainer.transform.localPosition + new Vector3(0, elemScale.y / 2, 0));
            LineRenderer.SetPosition(2, MagnifiedContainer.transform.localPosition + new Vector3(0, -elemScale.y / 2, 0));
        }        
        if (Magnifier.transform.position == TargetPosition) _animOn = false;
        else _animLerp += Time.deltaTime;
    }

    public void SetColliderStatus(bool status)
    {
        for (int i = 0; i < GetComponentsInChildren<BoxCollider>().Length; i++)
            GetComponentsInChildren<BoxCollider>()[i].enabled = status;
    }

    private void ActualizeDataContainers()
    {
        for (int i = 0; i < DataContainers.Length; i++)
        {
            DataContainers[i].gameObject.SetActive((_containerPointer + i) < Datas.Count);
            if (DataContainers[i].gameObject.activeInHierarchy)
                DataContainers[i].Resource = Datas[i + _containerPointer];

        }
    }

    public void SetupMagnifier(DataContainer container)
    {
        MagnifiedContainer = container;
        Description1.text = container.Name.text;
        Description2.text = container.DataType.text;
        TextIcon.gameObject.SetActive(container.DataTextIcon.gameObject.activeInHierarchy);
        if (TextIcon.gameObject.activeInHierarchy)
            TextIcon.text = container.DataTextIcon.text;
        Thumbnail.enabled = container.Thumbnail.enabled;
        if (Thumbnail.enabled)
            Thumbnail.material.mainTexture = container.Thumbnail.material.mainTexture;

        MagnifierActive = true;
    }

}

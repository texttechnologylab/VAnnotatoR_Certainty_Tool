using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DataSpaceControl : InteractiveObject
{

    //public enum DataSpaceType { None, Local_Data_Store, Stolperwege_Server, Resource_Manager, ShapeNet_Models, ShapeNet_Textures };
    public delegate void OnButton();

    private TextMeshPro SiteIndicator;
    //private List<DataSpaceType> DataSpaceList = new List<DataSpaceType>()
    //    { DataSpaceType.Local_Data_Store, DataSpaceType.Stolperwege_Server,
    //      DataSpaceType.Resource_Manager, DataSpaceType.ShapeNet_Models,
    //      DataSpaceType.ShapeNet_Textures};
    //private Dictionary<DataSpaceType, OnButton> DataSpaceCommandMap;

    private List<Interface> Interfaces;
    private List<InteractiveButton> InterfaceButtons;
    private int MaxSites;
    private InteractiveButton NextSite;
    private InteractiveButton PreviousSite;
    private DataBrowser Browser;

    private Transform _parent;
    private Vector3 _visiblePos;
    private Quaternion _visibleRot;
    private Quaternion _hiddenRot;
    private Vector3 _targetPos;
    private Quaternion _targetRot;
    private Vector3 _targetScale;

    private int _spacePointer;
    public int SpacePointer
    {
        get { return _spacePointer; }
        set
        {
            _spacePointer = Mathf.Max(0, Mathf.Min(value, (Interfaces.Count / InterfaceButtons.Count) * InterfaceButtons.Count));
            ActualizeButtons();
            ActualizeSiteVariables();
        }
    }

    private bool _baseInit;
    private void BaseInit()
    {
        SearchForParts = false;
        Start();
        _parent = transform.parent;
        Browser = _parent.GetComponent<DataBrowser>();
        SiteIndicator = transform.Find("SiteIndicator").GetComponent<TextMeshPro>();
        InteractiveButton[] buttons = GetComponentsInChildren<InteractiveButton>();
        InterfaceButtons = new List<InteractiveButton>();
        foreach (InteractiveButton button in buttons)
        {
            if (button.name.Contains("Space"))
            {
                InterfaceButtons.Add(button);
                button.OnClick = () =>
                {
                    if (button.ButtonOn) return;
                    foreach (InteractiveButton b in InterfaceButtons)
                        b.ButtonOn = false;
                    button.ButtonOn = true;
                    Browser.SelectedInterface = (Interface)button.ButtonValue;
                };
            }
                
        }

        PreviousSite = transform.Find("PreviousSite").GetComponent<InteractiveButton>();
        PreviousSite.OnClick = () => { SpacePointer -= InterfaceButtons.Count; };
        NextSite = transform.Find("NextSite").GetComponent<InteractiveButton>();
        NextSite.OnClick = () => { SpacePointer += InterfaceButtons.Count; };

        _visiblePos = transform.localPosition;
        _visibleRot = transform.localRotation;
        _hiddenRot = Quaternion.AngleAxis(180, Vector3.right);

        transform.localPosition = Vector3.forward * -0.01f;
        transform.localRotation = _hiddenRot;
        transform.localScale = Vector3.one * 0.01f;

        OnVerticalScroll = (dir) =>
        {
            if (dir == 1 && (SpacePointer + InterfaceButtons.Count) < Interfaces.Count)
                SpacePointer += InterfaceButtons.Count;
            if (dir == -1 && SpacePointer > 0)
                SpacePointer -= InterfaceButtons.Count;
        };

        Interfaces = new List<Interface>();
        foreach (Interface iFace in SceneController.Interfaces)
            if (iFace.OnSetupBrowser != null) Interfaces.Add(iFace);


        _baseInit = true;
    }

    public void Init()
    {
        if (!_baseInit) BaseInit();
        MaxSites = Mathf.CeilToInt(Interfaces.Count / (float)InterfaceButtons.Count);
        SpacePointer = 0;        
    }

    private void ActualizeSiteVariables()
    {
        SiteIndicator.text = "Site " + Mathf.Max(SpacePointer / InterfaceButtons.Count, 1) + " of " + MaxSites;
        PreviousSite.Active = SpacePointer > 0;
        //NextSite.Active = (SpacePointer + DataSpaces.Count) < DataSpaceList.Count;
        NextSite.Active = (SpacePointer + InterfaceButtons.Count) < Interfaces.Count;
    }

    public void Reset()
    {
        SetColliderStatus(true);
        transform.parent = _parent;
        transform.localPosition = _visiblePos;
        transform.localRotation = _visibleRot;
    }

    //public override bool onDrop()
    //{
    //    bool res = base.onDrop();
    //    transform.parent = _parent;
    //    return res;
    //}

    public void SetColliderStatus(bool status)
    {
        if (status)
        {
            ActualizeButtons();
            ActualizeSiteVariables();
        } else
        {
            for (int i = 0; i < GetComponentsInChildren<BoxCollider>().Length; i++)
                GetComponentsInChildren<BoxCollider>()[i].enabled = status;
        }
            
    }

    private void ActualizeButtons()
    {
        for (int i = 0; i < InterfaceButtons.Count; i++)
        {
            InterfaceButtons[i].gameObject.SetActive((SpacePointer + i) < Interfaces.Count);
            if (InterfaceButtons[i].gameObject.activeInHierarchy)
            {
                InterfaceButtons[i].ButtonValue = Interfaces[SpacePointer + i];
                InterfaceButtons[i].ChangeText(Interfaces[SpacePointer + i].Name);
                InterfaceButtons[i].GetComponent<BoxCollider>().enabled = true;
                InterfaceButtons[i].ButtonOn = _baseInit && Interfaces[SpacePointer + i] == Browser.SelectedInterface;
            }
        }
    }

    private float _lerp;
    public IEnumerator Activate(bool status)
    {
        _targetPos = (status) ? _visiblePos : Vector3.forward * -0.01f;
        _targetScale = (status) ? Vector3.one : Vector3.one * 0.01f;
        _targetRot = (status) ? _visibleRot : _hiddenRot;
        _lerp = 0;
        while (transform.localPosition != _targetPos)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _targetPos, _lerp);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, _targetRot, _lerp);
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, _lerp);
            _lerp += Time.deltaTime / 10;
            yield return null;
        }
    }
}

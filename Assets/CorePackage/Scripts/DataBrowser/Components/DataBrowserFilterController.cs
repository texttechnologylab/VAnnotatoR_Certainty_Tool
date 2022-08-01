using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DataBrowserFilterController : InteractiveObject
{
    private TextMeshPro Title;
    private TextMeshPro SiteIndicator;

    public Dictionary<string, InteractiveCheckbox.CheckboxStatus> Types { get; private set; }
    public bool ShowingSubTypes;
    public List<string> TypeList { get; private set; }
    public InteractiveCheckbox[] Checkboxes { get; private set; }
    public InteractiveButton[] Openers { get; private set; }
    private int MaxSites;
    private InteractiveButton NextSite;
    private InteractiveButton PreviousSite;
    public InteractiveButton Back { get; private set; }
    private InteractiveButton SelectAll;
    private InteractiveButton DeselectAll;
    private DataBrowser Browser;

    private Transform _parent;
    private Vector3 _visiblePos;
    private Quaternion _visibleRot;
    private Quaternion _hiddenRot;
    private Vector3 _targetPos;
    private Quaternion _targetRot;
    private Vector3 _targetScale;

    private int _typePointer;
    public int TypePointer
    {
        get { return _typePointer; }
        set
        {
            _typePointer = Mathf.Max(0, Mathf.Min(value, (Types.Count / Checkboxes.Length) * Checkboxes.Length));
            FilterUpdater?.Invoke();
            ActualizeSiteVariables();
        }
    }

    public delegate void FilterUpdateEvent();
    public FilterUpdateEvent FilterUpdater;

    public delegate void CheckboxUpdateEvent(string category, InteractiveCheckbox.CheckboxStatus status);
    public CheckboxUpdateEvent CheckboxUpdater;

    private bool _baseInit;
    public void BaseInit()
    {
        SearchForParts = false;
        Start();
        _parent = transform.parent;
        Browser = _parent.GetComponent<DataBrowser>();
        Title = transform.Find("Title").GetComponent<TextMeshPro>();
        SiteIndicator = transform.Find("SiteIndicator").GetComponent<TextMeshPro>();
        Checkboxes = GetComponentsInChildren<InteractiveCheckbox>();
        Openers = new InteractiveButton[Checkboxes.Length];
        PreviousSite = transform.Find("PreviousSite").GetComponent<InteractiveButton>();
        PreviousSite.OnClick = () => { TypePointer -= Checkboxes.Length; };
        NextSite = transform.Find("NextSite").GetComponent<InteractiveButton>();
        NextSite.OnClick = () => { TypePointer += Checkboxes.Length; };
        Back = transform.Find("Back").GetComponent<InteractiveButton>();
        OnVerticalScroll = (dir) =>
        {
            if (dir == 1 && (TypePointer + Checkboxes.Length) < Types.Count)
                TypePointer += Checkboxes.Length;
            if (dir == -1 && TypePointer > 0)
                TypePointer -= Checkboxes.Length;
        };
        

        int i = 0;
        foreach (InteractiveCheckbox cb in Checkboxes)
        {
            cb.OnClick = () =>
            {
                if (cb.Status == InteractiveCheckbox.CheckboxStatus.NoneChecked)
                    cb.Status = InteractiveCheckbox.CheckboxStatus.AllChecked;
                else
                    cb.Status = InteractiveCheckbox.CheckboxStatus.NoneChecked;
                CheckboxUpdater((string)cb.ButtonValue, cb.Status);
                Browser.BrowserUpdater?.Invoke();
            };
            Openers[i++] = cb.transform.Find("SubcategoryOpener").GetComponent<InteractiveButton>();
        }

        SelectAll = transform.Find("SelectAll").GetComponent<InteractiveButton>();
        SelectAll.OnClick = () => { SetCategoryStates(InteractiveCheckbox.CheckboxStatus.AllChecked); };
        DeselectAll = transform.Find("DeselectAll").GetComponent<InteractiveButton>();
        DeselectAll.OnClick = () => { SetCategoryStates(InteractiveCheckbox.CheckboxStatus.NoneChecked); };

        _visiblePos = transform.localPosition;
        _visibleRot = transform.localRotation;
        _hiddenRot = Quaternion.AngleAxis(180, Vector3.up);

        transform.localPosition = Vector3.forward * -0.01f;
        transform.localRotation = _hiddenRot;
        transform.localScale = Vector3.one * 0.01f;

        _baseInit = true;
    }

    public void Init(string title, Dictionary<string, InteractiveCheckbox.CheckboxStatus> types)
    {
        if (!_baseInit) BaseInit();
        Types = types;
        
        TypeList = new List<string>(Types.Keys);
        TypeList.Sort();

        Back.gameObject.SetActive(ShowingSubTypes);
        MaxSites = Mathf.CeilToInt(types.Count / (float)Checkboxes.Length);
        Title.text = title;
        TypePointer = 0;
    }

    private void ActualizeSiteVariables()
    {
        SiteIndicator.text = "Site " + ((TypePointer / Checkboxes.Length) + 1) + " of " + Mathf.Max(1, MaxSites);
        PreviousSite.Active = TypePointer > 0;
        NextSite.Active = (TypePointer + Checkboxes.Length) < Types.Count;
    }

    private void SetCategoryStates(InteractiveCheckbox.CheckboxStatus status)
    {
        foreach (string type in TypeList)
            CheckboxUpdater(type, status);
        foreach (InteractiveCheckbox cb in Checkboxes)
            cb.Status = status;
        ActualizeCheckBoxes();
        Browser.BrowserUpdater?.Invoke();
    }

    public void Reset()
    {
        SetColliderStatus(true);
        transform.parent = _parent;
        transform.localPosition = _visiblePos;
        transform.localRotation = _visibleRot;
    }

    public void SetColliderStatus(bool status)
    {
        for (int i = 0; i < GetComponentsInChildren<BoxCollider>().Length; i++)
            GetComponentsInChildren<BoxCollider>()[i].enabled = status;
    }

    private void ActualizeCheckBoxes()
    {
        for (int i=0; i<Checkboxes.Length; i++)
            Checkboxes[i].gameObject.SetActive((_typePointer + i) < TypeList.Count);

    }

    private float _lerp; public bool IsActive { get; private set; }
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
        IsActive = status;
    }

    /// <summary>
    /// This method collects all selected collective names from the passed FilterController instance
    /// </summary>
    /// <param name="filter">The DataBrowserFilterController instance</param>
    /// <returns>The set of all selected data types</returns>
    public HashSet<string> GetSelectedItems()
    {
        HashSet<string> res = new HashSet<string>();
        foreach (string type in Types.Keys)
            if (Types[type] == InteractiveCheckbox.CheckboxStatus.AllChecked)
                res.Add(type);
        return res;
    }

    
}

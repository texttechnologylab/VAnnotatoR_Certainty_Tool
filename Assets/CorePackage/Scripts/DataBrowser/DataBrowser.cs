using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StolperwegeHelper;
using Valve.VR;

public class DataBrowser : InstantiableSmartwatchObject
{

    public new static string PrefabPath = "Prefabs/DataBrowser/DataBrowser";
    public new static string Icon = "\xf07c";

    public DataBrowserFilterController FilterPanel { get; private set; }
    public DataSpaceControl SpaceControl { get; private set; }
    public DataPanel DataPanel { get; private set; }
    public DataSearchPanel SearchPanel { get; private set; }
    //public DataVizualizer DataVizualizer { get; private set; }

    /// <summary>
    /// This dictionary stores the last state of each different type of interface, that was opened with this instance of the DataBrowser.
    /// </summary>
    public Dictionary<string, object> LastBrowserStateMap { get; private set; }

    /// <summary>
    /// The dictionary stores the last states of the filters for each different type of interface, that was opened with this instance of the DataBrowser.
    /// </summary>
    public Dictionary<string, Dictionary<string, InteractiveCheckbox.CheckboxStatus>> DataSpaceFilterMap { get; private set; }

    private Interface _selectedInterface;
    /// <summary>
    /// The property that references the actual opened interface.
    /// If setting this property, the BrowserSetupEvent-method of the setted interface will be called.
    /// </summary>
    public Interface SelectedInterface
    {
        get { return _selectedInterface; }
        set
        {
            _selectedInterface = value;
            SearchPanel.Inputfield.Text = "";
            StartCoroutine(_selectedInterface.OnSetupBrowser(this));
        }
    }

    
    public delegate void BrowserUpdateMethod();
    /// <summary>
    /// A delegate function that can be setted, to define how the browser should be updated, if changes was made.
    /// </summary>
    public BrowserUpdateMethod BrowserUpdater;

    /// <summary>
    /// The Init method initializes the DataBrowser and all of its components.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        SearchForParts = false;
        DataSpaceFilterMap = new Dictionary<string, Dictionary<string, InteractiveCheckbox.CheckboxStatus>>();
        LastBrowserStateMap = new Dictionary<string, object>();
        FilterPanel = GetComponentInChildren<DataBrowserFilterController>();
        FilterPanel.BaseInit();
        SpaceControl = GetComponentInChildren<DataSpaceControl>();
        SpaceControl.Init();
        DataPanel = GetComponentInChildren<DataPanel>();
        DataPanel.Init();
        SearchPanel = GetComponentInChildren<DataSearchPanel>();
        SearchPanel.Init();
        //DataVizualizer = GetComponentInChildren<DataVizualizer>();
        //DataVizualizer.Init();
        PartsToHighlight = new List<Renderer>()
        {
            FilterPanel.transform.Find("Outliner").GetComponent<MeshRenderer>(),
            SpaceControl.transform.Find("Outliner").GetComponent<MeshRenderer>(),
            DataPanel.transform.Find("Outliner").GetComponent<MeshRenderer>(),
            SearchPanel.transform.Find("Outliner").GetComponent<MeshRenderer>(),
            //DataVizualizer.transform.Find("Outliner").GetComponent<MeshRenderer>()
        };
        
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].tag = "UI";

        Start();
        Grabable = true;
        LookAtUserOnHold = true;
        KeepYRotation = true;
        UseHighlighting = false;
        Removable = true;
        OnRemove = Destroy;
        OnVerticalScroll = (dir) =>
        {
            if (DataPanel.Datas == null) return;
            DataPanel.ContainerPointer += dir * DataPanel.DataContainers.Length;
        };

        SetComponentStatus(false);
    }

    /// <summary>
    /// Stores the state of the browser with the selected interface. If the selected interface has no entry, it will be added to the state map.
    /// </summary>
    /// <param name="space">The name of the interface</param>
    /// <param name="lastState">The actual state of the brwoser</param>
    public void SetActualState(string space, object lastState)
    {
        if (!LastBrowserStateMap.ContainsKey(space))
            LastBrowserStateMap.Add(space, lastState);
        else LastBrowserStateMap[space] = lastState;
    }

    /// <summary>
    /// Defines what should happen, if the browser was destroyed.
    /// </summary>
    public void Destroy()
    {
        Destroy(FilterPanel.gameObject);
        Destroy(SpaceControl.gameObject);
        Destroy(DataPanel.gameObject);
        Destroy(SearchPanel.gameObject);
        Destroy(gameObject);
    }

    private void SetComponentStatus(bool status)
    {
        FilterPanel.SetColliderStatus(status);
        SpaceControl.SetColliderStatus(status);
        DataPanel.SetColliderStatus(status);
        SearchPanel.SetColliderStatus(status);
    }

    protected override IEnumerator Expand()
    {
        yield return StartCoroutine(base.Expand());
        yield return StartCoroutine(SpaceControl.Activate(true));
        SetComponentStatus(true);
    }

}

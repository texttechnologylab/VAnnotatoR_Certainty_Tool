using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Valve.VR;
using LitJson;
using MathHelper;


public class AnnotationObjectPanel : AnnotationWindow
{
    private List<string> _allTabNames = new List<string>
    {
        "SpatialEntityAttributes", "SRelationAttributes", "MotionAttributes", "EventAttributes", "MeasureAttributes",
        "LocationAttributes", "PathAttributes", "EventPathAttributes", "PlaceAttributes"
    };
    /**************************************
    *      TAB BUTTONS
    **************************************/
    #region
    private InteractiveButton GeneralBtn;
    private InteractiveButton MultiTokenBtn;
    private InteractiveButton LinksBtn;
    private InteractiveButton SwitchButton;
    #endregion

    /**************************************
     *      GENERAL TAB 
     **************************************/
    #region
    private GameObject GeneralTab;
    private InteractiveButton RemoveObjButton;
    private InteractiveButton RemoveEntityButton;
    private InteractiveCheckbox MoreButton;
    private InteractiveButton ObjectTypeButton;

    private Transform ActiveAttributes;
    private Transform MoreAttributes;

    private InteractiveButton button1;
    private InteractiveButton button2;
    private InteractiveButton button3;
    private InteractiveButton button4;
    private InteractiveButton button5;
    private InteractiveButton button6;
    private InteractiveButton button7;
    private InteractiveButton button8;
    private InteractiveButton button9;
    private InteractiveButton button10;
    private InteractiveButton button11;
    private InteractiveButton button12;
    private InteractiveButton button13;
    private InteractiveButton button14;

    private KeyboardEditText textfield1;
    private KeyboardEditText textfield2;
    private KeyboardEditText textfield3;
    private KeyboardEditText textfield4;
    private KeyboardEditText textfield5;
    private KeyboardEditText textfield6;

    private InteractiveCheckbox checkbox1;
    private InteractiveCheckbox checkbox2;

    private TextMeshPro display1;
    private TextMeshPro display2;
    private TextMeshPro display3;
    private TextMeshPro display4;

    private InteractiveButton ElevationHandler;
    private InteractiveButton RemoveElevation;


    private Transform ScopeWindow;
    private InteractiveButton PreviousScope;
    private TextMeshPro ScopeDisplay;
    private InteractiveButton NextScope;
    private InteractiveButton AddScope;
    private InteractiveButton RemoveScope;
    private TextMeshPro ScopeIndexDisplay;


    private int _scopeIndex;
    public int ScopeIndex
    {
        get { return _scopeIndex; }
        set
        {
            IsoSpatialEntity entity = Entity is IsoSpatialEntity entity1 ? entity1 : null;
            IsoEvent eentity = Entity is IsoEvent ? (IsoEvent)Entity : null;

            if (entity == null || eentity == null)
                return;

            if ((entity.Scopes == null && eentity.Scopes == null) ||
                (entity.Scopes.Count == 0 && eentity.Scopes.Count == 0))
            {
                ScopeDisplay.text = "-";
                PreviousScope.Active = false;
                NextScope.Active = false;
                RemoveScope.Active = false;
                ScopeIndexDisplay.text = "";
                return;
            }
            int c = entity == null ? entity.Scopes.Count : eentity.Scopes.Count;
            _scopeIndex = Mathf.Max(0, Mathf.Min(value, c - 1));

            string text = entity == null ? entity.Scopes[_scopeIndex].TextContent : eentity.Scopes[_scopeIndex].TextContent;
            ScopeDisplay.text = text;
            PreviousScope.Active = _scopeIndex > 0;
            NextScope.Active = _scopeIndex < c - 1;
            RemoveScope.Active = true;
            ScopeIndexDisplay.text = "" + (_scopeIndex + 1) + " of " + c;
        }
    }


    private int _pathMidIndex;
    public int PathMidIndex
    {
        get { return _pathMidIndex; }
        set
        {
            IsoLocationPath p = Entity is IsoLocationPath ? (IsoLocationPath)Entity : null;
            IsoEventPath ep = Entity is IsoEventPath ? (IsoEventPath)Entity : null;
            if ((p == null || p.MidIDs == null || p.MidIDs.Count == 0) &&
                (ep == null || ep.MidIDs == null || ep.MidIDs.Count == 0))
            {
                display1.text = "-";
                button6.Active = false;
                button7.Active = false;
                button9.Active = false;
                display2.text = "";
                return;
            }
            List<IsoEntity> midIDs = p == null ? ep.MidIDs : p.MidIDs;
            _pathMidIndex = Mathf.Max(0, Mathf.Min(value, midIDs.Count - 1));
            display1.text = midIDs[_pathMidIndex].TextContent;
            button6.Active = _pathMidIndex > 0;
            button7.Active = _pathMidIndex < midIDs.Count - 1;
            button8.Active = true;
            display2.text = "" + (_pathMidIndex + 1) + " of " + midIDs.Count;
        }
    }


    private int _relatorIndex;
    public int RelatorIndex
    {
        get { return _relatorIndex; }
        set
        {
            IsoEventPath ep = Entity is IsoEventPath ? (IsoEventPath)Entity : null;
            if (ep == null || ep.Spatial_Relator == null || ep.Spatial_Relator.Count == 0)
            {
                display3.text = "-";
                button12.Active = false;
                button13.Active = false;
                button14.Active = false;
                display4.text = "";
                return;
            }
            _relatorIndex = Mathf.Max(0, Mathf.Min(value, ep.Spatial_Relator.Count - 1));
            display3.text = ep.Spatial_Relator[_relatorIndex].TextContent;
            button12.Active = _relatorIndex > 0;
            button13.Active = _relatorIndex < ep.Spatial_Relator.Count - 1;
            button14.Active = true;
            display4.text = "" + (_relatorIndex + 1) + " of " + ep.Spatial_Relator.Count;
        }
    }

    #endregion

    /**************************************
     *      MULTI-TOKENS TAB 
     **************************************/
    #region
    private GameObject MultiTokenTab;
    private TextMeshPro MultiTokenLabel;
    private TextMeshPro MultiTokenDisplay;
    private TextMeshPro CoreferenceLabel;
    private TextMeshProScroller CoreferenceScroller;
    private TextMeshPro CoreferenceDisplay;

    private KeyboardEditText CommentInput;
    #endregion


    /**************************************
    *      Links   TAB 
    **************************************/
    #region
    private GameObject LinksTab;
    private InteractiveButton nextLinkPage;
    private InteractiveButton prevLinkPage;
    private TextMeshPro linkPageDisplay;
    private int _currentLinkPage;
    #endregion

    /**************************************
     *  Properties
     **************************************/
    #region
    public enum PanelTab { MultiTokens, General, Links }
    private PanelTab _activeTab;
    public PanelTab ActiveTab
    {
        get { return _activeTab; }
        set
        {
            _activeTab = value;

            //PhysicsBtn.ButtonOn = _activeTab == PanelTab.Collider;

            MultiTokenBtn.ButtonOn = _activeTab == PanelTab.MultiTokens;
            GeneralBtn.ButtonOn = _activeTab == PanelTab.General;
            LinksBtn.ButtonOn = _activeTab == PanelTab.Links;

            MultiTokenTab.SetActive(_activeTab == PanelTab.MultiTokens);
            GeneralTab.SetActive(_activeTab == PanelTab.General);
            LinksTab.SetActive(_activeTab == PanelTab.Links);
            if (_activeTab == PanelTab.MultiTokens) ActualizeMultiTokenTab();
        }
    }

    public IsoEntity Entity { get; private set; }
    public InteractiveShapeNetObject InteractiveShapeNetObject { get { return Entity.InteractiveShapeNetObject; } }


    private int _buttonIndex;
    private float _switchBtnPosition = -0.165f;
    public int ButtonIndex
    {
        get { return _buttonIndex; }
        set
        {
            _buttonIndex = value;
            SwitchButton.transform.localPosition = (_buttonIndex == 0) ?
                new Vector3(_switchBtnPosition, SwitchButton.transform.localPosition.y, SwitchButton.transform.localPosition.z) :
                new Vector3(_switchBtnPosition * -1, SwitchButton.transform.localPosition.y, SwitchButton.transform.localPosition.z);
            MultiTokenBtn.gameObject.SetActive(_buttonIndex == 0);
            GeneralBtn.gameObject.SetActive(_buttonIndex == 0);
            LinksBtn.gameObject.SetActive(_buttonIndex == 0);
        }
    }

    private TextAnnotatorInterface TextAnnotator { get { return SceneController.GetInterface<TextAnnotatorInterface>(); } }

    public override bool Active
    {
        get { return _active; }
        set
        {
            _active = value;
            gameObject.SetActive(_active);
            if (!_active && ActiveTab == PanelTab.General && ObjectTypeButton.ButtonOn)
                StolperwegeHelper.RadialMenu.Show(false);
        }
    }

    #endregion

    public void Init(IsoEntity iEntity)
    {
        base.Start();


        Entity = iEntity;
        Entity.Panel = this;
        OnRemove = () =>
        {
            //if (GhostObject != null) Destroy(GhostObject);
            if (InteractiveShapeNetObject != null)
                InteractiveShapeNetObject.GetComponent<Collider>().enabled = true;
        };

        GetComponent<Collider>().isTrigger = true;



        // INITIALIZING COMPONENTS

        /************************************************************************
        | Tabs and buttons                                                      
        ************************************************************************/
        #region
        MultiTokenTab = transform.Find("MultiTokenTab").gameObject;
        GeneralTab = transform.Find("GeneralTab").gameObject;
        LinksTab = transform.Find("LinksTab").gameObject;



        SwitchButton = transform.Find("SwitchButton").GetComponent<InteractiveButton>();
        SwitchButton.OnClick = () => { ButtonIndex = (ButtonIndex - 1) * -1; };
        SwitchButton.gameObject.SetActive(false);

        MultiTokenBtn = transform.Find("MultiTokenButton").GetComponent<InteractiveButton>();
        MultiTokenBtn.OnClick = () => { ActiveTab = PanelTab.MultiTokens; };

        GeneralBtn = transform.Find("GeneralButton").GetComponent<InteractiveButton>();
        GeneralBtn.OnClick = () => { ActiveTab = PanelTab.General; };

        LinksBtn = transform.Find("LinksButton").GetComponent<InteractiveButton>();
        LinksBtn.OnClick = () =>
        {
            ActiveTab = PanelTab.Links;
            _currentLinkPage = 0;
            ChangeLinkPage();
        };
        #endregion

        ActiveTab = PanelTab.General;
        DeactivateAllGeneralTabs();

        /************************************************************************
        | MultitokenSection section
        ************************************************************************/
        #region
        MultiTokenLabel = MultiTokenTab.transform.Find("MultiTokenLabel").GetComponent<TextMeshPro>();
        MultiTokenLabel.text = "Connected Multitoken:";
        MultiTokenDisplay = MultiTokenTab.transform.Find("MultiTokenDisplay/TextMeshPro").GetComponent<TextMeshPro>();
        CoreferenceLabel = MultiTokenTab.transform.Find("CoreferenceLabel").GetComponent<TextMeshPro>();
        CoreferenceScroller = MultiTokenTab.transform.Find("CoreferenceDisplay").GetComponent<TextMeshProScroller>();
        CoreferenceDisplay = CoreferenceScroller.transform.Find("Scroller").GetComponent<TextMeshPro>();
        CoreferenceScroller.Init(CoreferenceDisplay);

        CommentInput = MultiTokenTab.transform.Find("CommentInputfield").GetComponent<KeyboardEditText>();
        CommentInput.ChangeTextOnCommit = false;
        CommentInput.IsNumberField = false;
        CommentInput.Text = Entity.Comment ?? "-";
        CommentInput.OnCommit = (text, go) =>
        {
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "comment", text } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
            {
                IsoSpatialEntity u = (IsoSpatialEntity)updated;
                CommentInput.Text = u.Comment ?? "-";
            });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };


        ActualizeMultiTokenTab();
        #endregion


        /************************************************************************
        | LinkSection section
        ************************************************************************/
        #region
        nextLinkPage = LinksTab.transform.Find("Pages/NextPage").GetComponent<InteractiveButton>();
        nextLinkPage.OnClick = () => { 
            _currentLinkPage++;
            ChangeLinkPage();
        };
        prevLinkPage = LinksTab.transform.Find("Pages/PreviousPage").GetComponent<InteractiveButton>();
        prevLinkPage.OnClick = () => { 
            _currentLinkPage--;
            ChangeLinkPage();
        };


        linkPageDisplay = LinksTab.transform.Find("Pages/SiteIndicator").GetComponent<TextMeshPro>();
        if (Entity.LinkedVia != null && Entity.LinkedVia.Count > 0)
        {
            nextLinkPage.Active = true;
            prevLinkPage.Active = true;
        }
        else
        {
            nextLinkPage.Active = false;
            prevLinkPage.Active = false;
            linkPageDisplay.text = " - ";
        }
        #endregion

        /************************************************************************
        | General section
        ************************************************************************/
        //#region
        ObjectTypeButton = GeneralTab.transform.Find("ObjectType/Button").GetComponent<InteractiveButton>();
        ObjectTypeButton.OnClick = () =>
        {
            List<RadialSection> data = new List<RadialSection>(RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoEntities]);
            data[0] = null; //Löschen der Multitoken Sektion
            //StartCoroutine(SetRadialMenuForSelection(ObjectTypeButton, data, null)); //TODO!!!!
        };

        Color color;
        string name;
        if (Entity is IsoLocationPath)
        {
            color = IsoLocationPath.ClassColor;
            name = IsoLocationPath.PrettyName;
            InitIsoSpatialEntity();
            InitIsoPath();
            InitScope();
        }
        else if (Entity is IsoLocationPlace)
        {
            color = IsoLocationPlace.ClassColor;
            name = IsoLocationPlace.PrettyName;
            InitIsoSpatialEntity();
            InitLocationPlace();
            InitScope();
        }
        else if (Entity is IsoEventPath)
        {
            color = IsoEventPath.ClassColor;
            name = IsoEventPath.Description;
            InitIsoSpatialEntity();
            InitIsoEventPath();
            InitScope();
        }
        else if (Entity is IsoLocation)
        {
            color = IsoLocation.ClassColor;
            name = IsoLocation.Description;
            InitIsoSpatialEntity();
            InitIsoLocation();
            InitScope();
        }
        else if (Entity is IsoSpatialEntity)
        {
            color = IsoSpatialEntity.ClassColor;
            name = IsoSpatialEntity.Description;
            InitIsoSpatialEntity();
            InitScope();
        }
        else if (Entity is IsoSRelation)
        {
            color = IsoSRelation.ClassColor;
            name = IsoSRelation.PrettyName;
            InitSRelation();
        }
        else if (Entity is IsoMeasure)
        {
            color = IsoMeasure.ClassColor;
            name = IsoMeasure.PrettyName;
            InitMeasure();
        }
        else if (Entity is IsoMRelation)
        {
            color = IsoMRelation.ClassColor;
            name = IsoMRelation.PrettyName;
            InitMRelation();
        }
        else if (Entity is IsoMotion)
        {
            color = IsoMotion.ClassColor;
            name = IsoMotion.PrettyName;
            InitMotion();
            InitScope();
        }
        else if (Entity is IsoNonMotionEvent)
        {
            color = IsoNonMotionEvent.ClassColor;
            name = IsoNonMotionEvent.PrettyName;
            InitEvent();
            InitScope();
        }
        else
        {
            color = Color.white;
            name = "IsoEntity";
        }

        if (ActiveAttributes != null)
            ActiveAttributes.gameObject.SetActive(true);
        if (MoreAttributes != null)
            MoreAttributes.gameObject.SetActive(false);

        string buttonText = "\xf362 <color=" + StolperwegeHelper.ColorToHex(color) + ">" + name + "<color=#FFFFFF>";
        ObjectTypeButton.ChangeText(buttonText);

        RemoveObjButton = GeneralTab.transform.Find("Remove/ButtonObj").GetComponent<InteractiveButton>();
        RemoveObjButton.OnLongClick = () =>
        {
            RemoveObjButton.Active = false;
            Entity.DeleteObjRequest();
        };
        RemoveObjButton.LoadingText = "Removing object ...";

        RemoveEntityButton = GeneralTab.transform.Find("Remove/ButtonAll").GetComponent<InteractiveButton>();
        RemoveEntityButton.OnLongClick = () =>
        {
            RemoveEntityButton.Active = false;
            List<IsoLink> linklist = new List<IsoLink>(Entity.LinkedVia.Keys);
            List<string> delList = new List<string>();
            delList.Add("" + Entity.ID);
            foreach (IsoLink link in linklist)
                delList.Add(""+link.ID);


            SceneController.GetInterface<TextAnnotatorInterface>().DeleteElements(delList);
        };
        RemoveEntityButton.LoadingText = "Removing entity ...";

        MoreButton = GeneralTab.transform.Find("More").GetComponent<InteractiveCheckbox>();
        MoreButton.NoneChecked = "More...";
        MoreButton.AllChecked = "Less...";
        MoreButton.gameObject.SetActive(MoreAttributes != null);
        MoreButton.OnClick = () =>
        {
            MoreButton.ButtonOn = !MoreButton.ButtonOn;
            MoreAttributes.gameObject.SetActive(MoreButton.ButtonOn);
            ActiveAttributes.gameObject.SetActive(!MoreButton.ButtonOn);
            if (ScopeWindow != null)
                ScopeWindow.gameObject.SetActive(!MoreButton.ButtonOn);
        };

        ActiveTab = PanelTab.General;
        ButtonIndex = 0;
        //////////////////////////////////////////////////


        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].tag = "UI";
    }

    readonly int MAXLINKSONPAGE = 6;
    private void ChangeLinkPage()
    {
        int maxPageIdx = ((Entity.LinkedVia.Count-1) / MAXLINKSONPAGE);
        if (_currentLinkPage < 0)
            _currentLinkPage = maxPageIdx;
        if (_currentLinkPage > maxPageIdx)
            _currentLinkPage = 0;

        int startid = _currentLinkPage * MAXLINKSONPAGE;
        List<IsoLink> linklist = new List<IsoLink>(Entity.LinkedVia.Keys);
        Transform linksContainer = LinksTab.transform.Find("Links");


        for (int i = 0; i < linksContainer.childCount; i++)
        {
            Transform linkContainer = linksContainer.GetChild(i);
            if (startid + i < Entity.LinkedVia.Count)
            {
                IsoLink link = linklist[startid + i];

                InteractiveButton linkButton = linkContainer.Find("Link/Button").GetComponent<InteractiveButton>();
                if (link.GetType() == typeof(IsoQsLink))
                    linkButton.ChangeText("QS");
                else if (link.GetType() == typeof(IsoOLink))
                    linkButton.ChangeText("O");
                else if (link.GetType() == typeof(IsoMetaLink))
                    linkButton.ChangeText("Meta");
                else if (link.GetType() == typeof(IsoSrLink))
                    linkButton.ChangeText("Sr");
                else
                    linkButton.ChangeText("Link");

                linkButton.OnClick = () =>
                {
                    if (link.Panel == null)
                    {
                        link.Panel = ((GameObject)Instantiate(Resources.Load("Prefabs/LinkAnnotationPanel/LinkAnnotationPanel"))).GetComponent<LinkAnnotationPanel>();
                        link.Panel.Init(link);
                        link.Panel.gameObject.SetActive(false);
                    }
                    link.Panel.gameObject.SetActive(!link.Panel.gameObject.activeInHierarchy);
                    if (link.Panel.gameObject.activeInHierarchy) StolperwegeHelper.PlaceInFrontOfUser(link.Panel.transform, 0.5f);
                };
                InteractiveButton figureButton = linkContainer.Find("Figure/Button").GetComponent<InteractiveButton>();
                figureButton.ChangeText(link.Figure != null ? link.Figure.TextContent : " - ");
                figureButton.Active = false;

                InteractiveButton triggerButton = linkContainer.Find("Trigger/Button").GetComponent<InteractiveButton>();
                triggerButton.ChangeText(link.Trigger != null ? link.Trigger.TextContent : " - ");
                triggerButton.Active = false;

                InteractiveButton groundButton = linkContainer.Find("Ground/Button").GetComponent<InteractiveButton>();
                groundButton.ChangeText(link.Ground != null ? link.Ground.TextContent : " - ");
                groundButton.Active = false;

                InteractiveButton typeButton = linkContainer.Find("RelType/Button").GetComponent<InteractiveButton>();
                typeButton.ChangeText(link.Rel_Type ?? " - ");
                typeButton.Active = false;

                InteractiveButton linkRemoveButton = linkContainer.Find("Remove/Button").GetComponent<InteractiveButton>();
                linkRemoveButton.OnLongClick = () =>
                {
                    linkRemoveButton.Active = false;
                    SceneController.GetInterface<TextAnnotatorInterface>().DeleteElement("" + link.ID);
                };
                linkRemoveButton.LoadingText = "Removing link...";

                linkContainer.gameObject.SetActive(true);
            }
            else
                linkContainer.gameObject.SetActive(false);
        }
        if(Entity.LinkedVia == null || Entity.LinkedVia.Count == 0)
            linkPageDisplay.text = " - ";
        else
            linkPageDisplay.text = "page " + (_currentLinkPage + 1) + " / " + (maxPageIdx + 1);
    }


    private void InitIsoSpatialEntity()
    {
        IsoSpatialEntity entity = (IsoSpatialEntity)Entity;
        ActiveAttributes = GeneralTab.transform.Find("SpatialEntityAttributes");
        ActiveAttributes.gameObject.SetActive(true);

        button1 = ActiveAttributes.transform.Find("EntityType/Button").GetComponent<InteractiveButton>();
        button1.OnClick = () =>
        {
            StartCoroutine(SetRadialMenuForSelection(button1, RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoSpatialEntityTypes], "spatial_entitiy_type"));
        };
        button1.ChangeText(entity.Spatial_Entity_Type);

        button2 = ActiveAttributes.transform.Find("Dimension/Button").GetComponent<InteractiveButton>();
        button2.OnClick = () =>
        {
            StartCoroutine(SetRadialMenuForSelection(button2, RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoSpatialDimension], "dimensionality"));
        };
        button2.ChangeText(entity.Dimensionality.ToString());

        button3 = ActiveAttributes.transform.Find("Form/Button").GetComponent<InteractiveButton>();
        button3.OnClick = () =>
        {
            StartCoroutine(SetRadialMenuForSelection(button3, RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoSpatialForm], "form"));
        }; ;
        button3.ChangeText(entity.Form.ToString());

        checkbox1 = ActiveAttributes.transform.Find("Dcl/Checkbox").GetComponent<InteractiveCheckbox>();
        checkbox1.OnClick = () =>
        {
            checkbox1.Active = false;
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "dcl", !checkbox1.ButtonOn } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            IsoSpatialEntity u = (IsoSpatialEntity)updated;
            checkbox1.ButtonOn = u.Dcl;
            checkbox1.Active = true;
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };
        checkbox1.ButtonOn = entity.Dcl;

        checkbox2 = ActiveAttributes.transform.Find("Countable/Checkbox").GetComponent<InteractiveCheckbox>();
        checkbox2.OnClick = () =>
        {
            checkbox2.Active = false;
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "countable", !checkbox2.ButtonOn } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            IsoSpatialEntity u = (IsoSpatialEntity)updated;
            checkbox2.ButtonOn = u.Countable;
            checkbox2.Active = true;
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };
        checkbox2.ButtonOn = entity.Countable;

        textfield1 = ActiveAttributes.transform.Find("Latitude/Inputfield").GetComponent<KeyboardEditText>();
        textfield1.ChangeTextOnCommit = false;
        textfield1.IsNumberField = true;
        textfield1.EnableNegativeNumber = true;
        textfield1.Text = entity.Lat ?? "-";
        textfield1.OnCommit = (text, go) =>
        {
            float lat = Mathf.Max(-90, Mathf.Min(90, float.Parse(text)));
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "lat", "" + lat } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            IsoSpatialEntity u = (IsoSpatialEntity)updated;
            textfield1.Text = u.Lat ?? "-";
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };

        textfield2 = ActiveAttributes.transform.Find("Longitude/Inputfield").GetComponent<KeyboardEditText>();
        textfield2.ChangeTextOnCommit = false;
        textfield2.IsNumberField = true;
        textfield2.EnableNegativeNumber = true;
        textfield2.Text = entity.Lon ?? "-";
        textfield2.OnCommit = (text, go) =>
        {
            float lon = Mathf.Max(-180, Mathf.Min(180, float.Parse(text)));
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "long", "" + lon } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            IsoSpatialEntity u = (IsoSpatialEntity)updated;
            textfield2.Text = u.Lon ?? "-";
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };

        ElevationHandler = ActiveAttributes.transform.Find("Elevation/CreateButton").GetComponent<InteractiveButton>();
        ElevationHandler.ChangeText(entity.Elevation == null ? "-" : entity.Elevation.TextContent);
        ElevationHandler.ButtonValue = "AddElevation";
        ElevationHandler.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(ElevationHandler));
        };

        RemoveElevation = ActiveAttributes.transform.Find("Elevation/RemoveButton").GetComponent<InteractiveButton>();
        RemoveElevation.Active = entity.Elevation != null;
        RemoveElevation.LoadingText = "Deleting elevation annotation of this object...";
        RemoveElevation.OnLongClick = () =>
        {
            HandleElevation(null);
        };
    }

    private void InitIsoEventPath()
    {
        IsoEventPath entity = (IsoEventPath)Entity;
        MoreAttributes = GeneralTab.transform.Find("EventPathAttributes");
        MoreAttributes.gameObject.SetActive(true);

        button1.OnClick = () =>
        {
            StartCoroutine(SetRadialMenuForSelection(button1, RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoPathTypes], "spatial_entitiy_type"));
        };

        button4 = MoreAttributes.Find("PathBegin/Button").GetComponent<InteractiveButton>();
        button4.ChangeText(entity.StartID == null ? "-" : entity.StartID.TextContent);
        button4.ButtonValue = "AddBeginEntity";
        button4.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(button4));
        };


        button5 = MoreAttributes.Find("PathEnd/Button").GetComponent<InteractiveButton>();
        button5.ChangeText(entity.EndID == null ? "-" : entity.EndID.TextContent);
        button5.ButtonValue = "AddEndEntity";
        button5.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(button5));
        };


        button6 = MoreAttributes.Find("PathMid/PreviousPathEntity").GetComponent<InteractiveButton>();
        button6.OnClick = () => { PathMidIndex -= 1; };
        button7 = MoreAttributes.Find("PathMid/NextPathEntity").GetComponent<InteractiveButton>();
        button7.OnClick = () => { PathMidIndex += 1; };

        display1 = MoreAttributes.Find("PathMid/PathEntityDisplay/Display").GetComponent<TextMeshPro>();
        display2 = MoreAttributes.Find("PathMid/PathEntityIndexDisplay").GetComponent<TextMeshPro>();

        button8 = MoreAttributes.Find("PathMid/AddPathEntity").GetComponent<InteractiveButton>();
        button8.ButtonValue = "AddPathMidEntity";
        button8.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(button8));
        };

        button9 = MoreAttributes.Find("PathMid/RemovePathEntity").GetComponent<InteractiveButton>();
        button9.Active = (entity != null && entity.MidIDs != null && entity.MidIDs.Count > 0);
        button9.LoadingText = "Deleting path middle entity...";
        button9.LongClickTime = 1f;
        button9.OnLongClick = () =>
        {
            if (entity == null || PathMidIndex >= entity.MidIDs.Count)
                return;
            JsonData actualizedMids = new JsonData();
            List<IsoEntity> midIDs = entity.MidIDs;
            for (int i = 0; i < midIDs.Count; i++)
                if (i != PathMidIndex) actualizedMids.Add((int)midIDs[i].ID);
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "midID_array", !actualizedMids.IsArray ? null : actualizedMids } };

            TextAnnotator.ChangeEventMap.Add((int)entity.ID, (updated) =>
            {
                PathMidIndex = (((IsoLocationPath)updated).MidIDs == null || ((IsoLocationPath)updated).MidIDs.Count == 0) ? 0 :
                    (PathMidIndex + 1 == ((IsoLocationPath)updated).MidIDs.Count) ? PathMidIndex - 1 : PathMidIndex + 1;
            });

            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };



        button10 = MoreAttributes.Find("Trigger/AddButton").GetComponent<InteractiveButton>();
        button10.ChangeText(entity.Trigger == null ? "-" : entity.Trigger.TextContent);
        button10.ButtonValue = "AddTrigger";
        button10.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(button10));
        };

        button11 = MoreAttributes.Find("Trigger/RemoveButton").GetComponent<InteractiveButton>();
        button11.Active = entity.Trigger != null;
        button11.LoadingText = "Deleting trigger annotation of this object...";
        button11.LongClickTime = 1f;
        button11.OnLongClick = () =>
        {
            HandleTrigger(null);
        };

        button12 = MoreAttributes.Find("Relator/PreviousRelator").GetComponent<InteractiveButton>();
        button12.OnClick = () => { RelatorIndex -= 1; };
        button13 = MoreAttributes.Find("Relator/NextRelator").GetComponent<InteractiveButton>();
        button13.OnClick = () => { RelatorIndex += 1; };


        display3 = MoreAttributes.Find("Relator/RelatorDisplay/Display").GetComponent<TextMeshPro>();
        display4 = MoreAttributes.Find("Relator/RelatorIndexDisplay").GetComponent<TextMeshPro>();

        button13 = MoreAttributes.Find("Relator/AddRelator").GetComponent<InteractiveButton>();
        button13.ButtonValue = "AddRelator";
        button13.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(button13));
        };

        button14 = MoreAttributes.Find("Relator/RemoveRelator").GetComponent<InteractiveButton>();


        button14.Active = entity != null && entity.Spatial_Relator != null && entity.Spatial_Relator.Count > 0;
        button14.LoadingText = "Deleting spatial relator...";
        button14.LongClickTime = 1f;
        button14.OnLongClick = () =>
        {
            if (RelatorIndex >= entity.Spatial_Relator.Count) return;
            JsonData actualizedRelators = new JsonData();
            for (int i = 0; i < entity.Spatial_Relator.Count; i++)
                if (i != RelatorIndex) actualizedRelators.Add((int)entity.Spatial_Relator[i].ID);
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "spatial_relator_array", !actualizedRelators.IsArray ? null : actualizedRelators } };
            TextAnnotator.ChangeEventMap.Add((int)entity.ID, (updated) =>
            {
                RelatorIndex = (((IsoEventPath)updated).Spatial_Relator == null || ((IsoEventPath)updated).Spatial_Relator.Count == 0) ? 0 :
                               (RelatorIndex + 1 == ((IsoEventPath)updated).Spatial_Relator.Count) ? RelatorIndex - 1 : RelatorIndex + 1;
            });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };

        RelatorIndex = 0;


        PathMidIndex = 0;

    }

    private void InitIsoPath()
    {
        IsoLocationPath entity = (IsoLocationPath)Entity;
        MoreAttributes = GeneralTab.transform.Find("PathAttributes");
        MoreAttributes.gameObject.SetActive(true);

        button1.OnClick = () =>
        {
            StartCoroutine(SetRadialMenuForSelection(button1, RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoPathTypes], "spatial_entitiy_type"));
        };

        button4 = MoreAttributes.Find("PathBegin/Button").GetComponent<InteractiveButton>();
        button4.ChangeText(entity.BeginID == null ? "-" : entity.BeginID.TextContent);
        button4.ButtonValue = "AddBeginEntity";
        button4.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(button4));
        };


        button5 = MoreAttributes.Find("PathEnd/Button").GetComponent<InteractiveButton>();
        button5.ChangeText(entity.EndID == null ? "-" : entity.EndID.TextContent);
        button5.ButtonValue = "AddEndEntity";
        button5.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(button5));
        };


        button6 = MoreAttributes.Find("PathMid/PreviousPathEntity").GetComponent<InteractiveButton>();
        button6.OnClick = () => { PathMidIndex -= 1; };
        button7 = MoreAttributes.Find("PathMid/NextPathEntity").GetComponent<InteractiveButton>();
        button7.OnClick = () => { PathMidIndex += 1; };

        display1 = MoreAttributes.Find("PathMid/PathEntityDisplay/Display").GetComponent<TextMeshPro>();
        display2 = MoreAttributes.Find("PathMid/PathEntityIndexDisplay").GetComponent<TextMeshPro>();

        button8 = MoreAttributes.Find("PathMid/AddPathEntity").GetComponent<InteractiveButton>();
        button8.ButtonValue = "AddPathMidEntity";
        button8.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(button8));
        };

        button9 = MoreAttributes.Find("PathMid/RemovePathEntity").GetComponent<InteractiveButton>();
        button9.Active = (entity != null && entity.MidIDs != null && entity.MidIDs.Count > 0);
        button9.LoadingText = "Deleting path middle entity...";
        button9.LongClickTime = 1f;
        button9.OnLongClick = () =>
        {
            if (entity == null || PathMidIndex >= entity.MidIDs.Count)
                return;
            JsonData actualizedMids = new JsonData();
            List<IsoEntity> midIDs = entity.MidIDs;
            for (int i = 0; i < midIDs.Count; i++)
                if (i != PathMidIndex) actualizedMids.Add((int)midIDs[i].ID);
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "midID_array", !actualizedMids.IsArray ? null : actualizedMids } };

            TextAnnotator.ChangeEventMap.Add((int)entity.ID, (updated) =>
            {
                PathMidIndex = (((IsoLocationPath)updated).MidIDs == null || ((IsoLocationPath)updated).MidIDs.Count == 0) ? 0 :
                                (PathMidIndex + 1 == ((IsoLocationPath)updated).MidIDs.Count) ? PathMidIndex - 1 : PathMidIndex + 1;
            });

            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };


        PathMidIndex = 0;

    }

    private void InitIsoLocation()
    {
        IsoLocation entity = (IsoLocation)Entity;
        MoreAttributes = GeneralTab.transform.Find("PlaceAttributes");
        MoreAttributes.gameObject.SetActive(true);

        textfield3 = MoreAttributes.Find("Gazref/Inputfield").GetComponent<KeyboardEditText>();
        textfield3.ChangeTextOnCommit = false;
        textfield3.IsNumberField = false;
        textfield3.Text = entity.Gazref == null ? "" : entity.Gazref;
        textfield3.OnCommit = (text, go) =>
        {
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "gazref", text } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            IsoLocation u = (IsoLocation)updated;
            textfield3.Text = u.Gazref == null ? "" : u.Gazref;
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };
    }

    private void InitLocationPlace()
    {
        IsoLocationPlace entity = (IsoLocationPlace)Entity;
        MoreAttributes = GeneralTab.transform.Find("PlaceAttributes");
        MoreAttributes.gameObject.SetActive(true);

        button1.OnClick = () =>
        {
            StartCoroutine(SetRadialMenuForSelection(button1, RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoPlaceTypes], "spatial_entitiy_type"));
        };


        textfield3 = MoreAttributes.Find("Gazref/Inputfield").GetComponent<KeyboardEditText>();
        textfield3.ChangeTextOnCommit = false;
        textfield3.IsNumberField = false;
        textfield3.Text = entity.Gazref == null ? "" : entity.Gazref;
        textfield3.OnCommit = (text, go) =>
        {
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "gazref", text } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            IsoLocationPlace u = (IsoLocationPlace)updated;
            textfield3.Text = u.Gazref == null ? "" : u.Gazref;
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };


        textfield4 = MoreAttributes.Find("Country/Inputfield").GetComponent<KeyboardEditText>();
        textfield4.ChangeTextOnCommit = false;
        textfield4.IsNumberField = false;
        textfield4.Text = entity.Country == null ? "" : entity.Country;
        textfield4.OnCommit = (text, go) =>
        {
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "country", text } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            IsoLocationPlace u = (IsoLocationPlace)updated;
            textfield4.Text = u.Country == null ? "" : u.Country;
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };


        button4 = MoreAttributes.transform.Find("Ctv/Button").GetComponent<InteractiveButton>();
        button4.OnClick = () =>
        {
            StartCoroutine(SetRadialMenuForSelection(button4, RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoPlaceCTV], "ctv"));
        };
        button4.ChangeText(entity.Ctv.ToString());

        button5 = MoreAttributes.transform.Find("Continent/Button").GetComponent<InteractiveButton>();
        button5.OnClick = () =>
        {
            StartCoroutine(SetRadialMenuForSelection(button5, RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoPlaceContinent], "continent"));
        };
        button5.ChangeText(entity.Continent.ToString());


        textfield5 = MoreAttributes.Find("State/Inputfield").GetComponent<KeyboardEditText>();
        textfield5.ChangeTextOnCommit = false;
        textfield5.IsNumberField = false;
        textfield5.Text = entity.State == null ? "" : entity.State;
        textfield5.OnCommit = (text, go) =>
        {
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "state", text } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            IsoLocationPlace u = (IsoLocationPlace)updated;
            textfield5.Text = u.State == null ? "" : u.State;
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };


        textfield6 = MoreAttributes.Find("County/Inputfield").GetComponent<KeyboardEditText>();
        textfield6.ChangeTextOnCommit = false;
        textfield6.IsNumberField = false;
        textfield6.Text = entity.County == null ? "" : entity.County;
        textfield6.OnCommit = (text, go) =>
        {
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "county", text } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            IsoLocationPlace u = (IsoLocationPlace)updated;
            textfield6.Text = u.County == null ? "" : u.County;
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };
    }

    private void InitSRelation()
    {
        IsoSRelation entity = (IsoSRelation)Entity;
        ActiveAttributes = GeneralTab.transform.Find("SRelationAttributes");
        ActiveAttributes.gameObject.SetActive(true);

        button1 = ActiveAttributes.transform.Find("RelationType/Button").GetComponent<InteractiveButton>();
        button1.OnClick = () =>
        {
            StartCoroutine(SetRadialMenuForSelection(button1, RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoSRelationTypes], "relation_type"));
        };
        button1.ChangeText(entity.Type.ToString());


        textfield1 = ActiveAttributes.transform.Find("Value/Inputfield").GetComponent<KeyboardEditText>();
        textfield1.ChangeTextOnCommit = false;
        textfield1.IsNumberField = false;
        textfield1.Text = entity.Value ?? "-";
        textfield1.ChangeTextOnCommit = false;
        textfield1.OnCommit = (text, go) =>
        {
            string value = text;
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "value", value } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            IsoSRelation u = (IsoSRelation)updated;
            textfield1.Text = u.Value ?? "-";
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };

        textfield2 = ActiveAttributes.transform.Find("Cluster/Inputfield").GetComponent<KeyboardEditText>();
        textfield2.ChangeTextOnCommit = false;
        textfield2.IsNumberField = false;
        textfield2.Text = entity.Cluster ?? "-";
        textfield2.OnCommit = (text, go) =>
        {
            string value = text;
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "cluster", value } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            IsoSRelation u = (IsoSRelation)updated;
            textfield2.Text = u.Cluster ?? "-";
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };
    }

    private void InitMeasure()
    {
        IsoMeasure entity = (IsoMeasure)Entity;
        ActiveAttributes = GeneralTab.transform.Find("MeasureAttributes");
        ActiveAttributes.gameObject.SetActive(true);

        textfield1 = ActiveAttributes.transform.Find("Value/Inputfield").GetComponent<KeyboardEditText>();
        textfield1.ChangeTextOnCommit = false;
        textfield1.IsNumberField = false;
        textfield1.Text = entity.Value ?? "-";
        textfield1.ChangeTextOnCommit = false;
        textfield1.OnCommit = (text, go) =>
        {
            string value = text;
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "value", value } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            IsoMeasure u = (IsoMeasure)updated;
            textfield1.Text = u.Value ?? "-";
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };

        textfield2 = ActiveAttributes.transform.Find("Unit/Inputfield").GetComponent<KeyboardEditText>();
        textfield2.ChangeTextOnCommit = false;
        textfield2.IsNumberField = false;
        textfield2.Text = entity.Unit ?? "-";
        textfield2.OnCommit = (text, go) =>
        {
            string value = text;
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "unit", value } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            IsoMeasure u = (IsoMeasure)updated;
            textfield2.Text = u.Unit ?? "-";
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };
    }


    private void InitMRelation()
    {
        IsoMRelation entity = (IsoMRelation)Entity;
        ActiveAttributes = GeneralTab.transform.Find("MeasureAttributes");
        ActiveAttributes.gameObject.SetActive(true);

        textfield1 = ActiveAttributes.transform.Find("Value/Inputfield").GetComponent<KeyboardEditText>();
        textfield1.ChangeTextOnCommit = false;
        textfield1.IsNumberField = false;
        textfield1.Text = entity.Value ?? "-";
        textfield1.ChangeTextOnCommit = false;
        textfield1.OnCommit = (text, go) =>
        {
            string value = text;
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "value", value } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            IsoMRelation u = (IsoMRelation)updated;
            textfield1.Text = u.Value ?? "-";
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };

        textfield2 = ActiveAttributes.transform.Find("Unit/Inputfield").GetComponent<KeyboardEditText>();
        textfield2.gameObject.SetActive(false);
    }

    private void InitMotion()
    {
        IsoMotion entity = (IsoMotion)Entity;
        ActiveAttributes = GeneralTab.transform.Find("MotionAttributes");
        ActiveAttributes.gameObject.SetActive(true);

        button1 = ActiveAttributes.transform.Find("MotionType/Button").GetComponent<InteractiveButton>();
        button1.OnClick = () =>
        {
            StartCoroutine(SetRadialMenuForSelection(button1, RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoMotionType], "motion_type"));
        };
        button1.ChangeText(entity.Motion_Type != null ? entity.Motion_Type : "-");

        button2 = ActiveAttributes.transform.Find("MotionClass/Button").GetComponent<InteractiveButton>();
        button2.OnClick = () =>
        {
            StartCoroutine(SetRadialMenuForSelection(button2, RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoMotionClass], "motion_class"));
        };
        button2.ChangeText(entity.Motion_Class != null ? entity.Motion_Class : "-");

        button3 = ActiveAttributes.transform.Find("MotionSense/Button").GetComponent<InteractiveButton>();
        button3.OnClick = () =>
        {
            StartCoroutine(SetRadialMenuForSelection(button3, RadialLinkMenuData.RadialMenuMap[RadialLinkMenuData.MenuType.IsoMotionSense], "motion_sense"));
        };
        button3.ChangeText(entity.Motion_Sense != null ? entity.Motion_Sense : "-");


        button4 = ActiveAttributes.transform.Find("Manner/Button").GetComponent<InteractiveButton>();
        button4.ButtonValue = "AddManner";
        button4.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(button4));
        };
        button4.ChangeText(entity.Manner == null ? "-" : entity.Manner.TextContent);

        button5 = ActiveAttributes.transform.Find("Goal/Button").GetComponent<InteractiveButton>();
        button5.ButtonValue = "AddGoal";
        button5.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(button5));
        };
        button5.ChangeText(entity.MotionGoal == null ? "-" : entity.MotionGoal.TextContent);


        ElevationHandler = ActiveAttributes.transform.Find("Elevation/CreateButton").GetComponent<InteractiveButton>();
        ElevationHandler.ChangeText(entity.Elevation == null ? "-" : entity.Elevation.TextContent);
        ElevationHandler.ButtonValue = "AddElevation";
        ElevationHandler.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(ElevationHandler));
        };

        RemoveElevation = ActiveAttributes.transform.Find("Elevation/RemoveButton").GetComponent<InteractiveButton>();
        RemoveElevation.Active = entity.Elevation != null;
        RemoveElevation.LoadingText = "Deleting elevation annotation of this object...";
        RemoveElevation.OnLongClick = () =>
        {
            HandleElevation(null);
        };
    }

    public void InitEvent()
    {
        IsoNonMotionEvent entity = (IsoNonMotionEvent)Entity;
        ActiveAttributes = GeneralTab.transform.Find("EventAttributes");
        ActiveAttributes.gameObject.SetActive(true);


        ElevationHandler = ActiveAttributes.transform.Find("Elevation/CreateButton").GetComponent<InteractiveButton>();
        ElevationHandler.ChangeText(entity.Elevation == null ? "-" : entity.Elevation.TextContent);
        ElevationHandler.ButtonValue = "AddElevation";
        ElevationHandler.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(ElevationHandler));
        };


        RemoveElevation = ActiveAttributes.transform.Find("Elevation/RemoveButton").GetComponent<InteractiveButton>();
        RemoveElevation.Active = entity.Elevation != null;
        RemoveElevation.LoadingText = "Deleting elevation annotation of this object...";
        RemoveElevation.OnLongClick = () =>
        {
            HandleElevation(null);
        };
    }


    private void InitScope()
    {
        ScopeWindow = transform.Find("GeneralTab/Scopes");
        PreviousScope = ScopeWindow.Find("PreviousScope").GetComponent<InteractiveButton>();
        PreviousScope.OnClick = () => { ScopeIndex -= 1; };

        ScopeDisplay = ScopeWindow.Find("ScopeDisplay/Display").GetComponent<TextMeshPro>();
        ScopeIndexDisplay = ScopeWindow.Find("ScopeIndexDisplay").GetComponent<TextMeshPro>();

        NextScope = ScopeWindow.Find("NextScope").GetComponent<InteractiveButton>();
        NextScope.OnClick = () => { ScopeIndex += 1; };

        AddScope = ScopeWindow.Find("AddScope").GetComponent<InteractiveButton>();
        AddScope.ButtonValue = "AddScope";
        AddScope.OnClick = () =>
        {
            StartCoroutine(ActualizeConnector(AddScope)); //TODO
};

        List<IsoEntity> entityscopes = GetScopes();

        RemoveScope = ScopeWindow.Find("RemoveScope").GetComponent<InteractiveButton>();
        RemoveScope.Active = entityscopes != null &&
                         entityscopes.Count > 0;
        RemoveScope.LoadingText = "Deleting scope...";
        RemoveScope.LongClickTime = 1f;
        RemoveScope.OnLongClick = () =>
        {
            JsonData actualizedScopes = new JsonData();
            List<IsoEntity> entityscope = GetScopes();
            for (int i = 0; i < entityscope.Count; i++)
                if (i != ScopeIndex) actualizedScopes.Add((int)entityscope[i].ID);
            Dictionary<string, object> updateMap = new Dictionary<string, object>() { { "scopes_array", !actualizedScopes.IsArray ? null : actualizedScopes } };
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
    {
            ScopeIndex = (((IsoSpatialEntity)updated).Scopes == null || ((IsoSpatialEntity)updated).Scopes.Count == 0) ? 0 :
                         (ScopeIndex + 1 == ((IsoSpatialEntity)updated).Scopes.Count) ? ScopeIndex - 1 : ScopeIndex + 1;
        });
            TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
        };

        ScopeIndex = 0;
        ScopeWindow.gameObject.SetActive(true);
    }


    private List<IsoEntity> GetScopes()
    {
        List<IsoEntity> entityscopes = null;
        if (Entity is IsoSpatialEntity)
            entityscopes = ((IsoSpatialEntity)Entity).Scopes;
        else if (Entity is IsoEvent)
            entityscopes = ((IsoEvent)Entity).Scopes;
        return entityscopes;
    }


    public void HandleElevation(IsoMeasure measure)
    {
        Dictionary<string, object> updateMap = new Dictionary<string, object>();
        if (measure == null)
            updateMap.Add("elevation", null);
        else
            updateMap.Add("elevation", (int)measure.ID);
        TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
        {
            IsoMeasure m = null;
            if (updated is IsoSpatialEntity)
                m = ((IsoSpatialEntity)updated).Elevation;
            else if (updated is IsoEvent)
                m = ((IsoEvent)updated).Elevation;
            ElevationHandler.ChangeText(m == null ? "-" : m.TextContent);
            RemoveElevation.Active = m != null;
        });
        TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
    }

    public void HandleRelator(IsoSRelation spatialSignal)
    {
        IsoEventPath eventPath = (IsoEventPath)Entity;
        if (eventPath.Spatial_Relator.Contains(spatialSignal)) return;
        Dictionary<string, object> updateMap = new Dictionary<string, object>();
        JsonData relatorArray = new JsonData();
        if (eventPath.Spatial_Relator != null)
        {
            foreach (IsoSRelation e in eventPath.Spatial_Relator)
                relatorArray.Add((int)e.ID);
        }
        relatorArray.Add((int)spatialSignal.ID);
        updateMap.Add("spatial_relator_array", relatorArray);
        TextAnnotator.ChangeEventMap.Add((int)eventPath.ID, (updated) =>
        {
            eventPath = (IsoEventPath)updated;
            RelatorIndex = eventPath.Spatial_Relator.Count - 1;
        });
        TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + eventPath.ID, updateMap } }, null);
    }

    public void HandleTrigger(IsoMotion motion)
    {
        IsoEventPath eventPath = (IsoEventPath)Entity;
        Dictionary<string, object> updateMap = (motion == null) ?
        null : new Dictionary<string, object>() { { "trigger", motion.ID } };
        TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
        {
            eventPath = (IsoEventPath)updated;
            button10.ChangeText(eventPath.Trigger == null ? "-" : eventPath.Trigger.TextContent);
            button11.Active = eventPath.Trigger != null;
        });
        TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + eventPath.ID, updateMap } }, null);
    }

    public void HandleGoal(IsoSpatialEntity entity)
    {
        IsoMotion motion = (IsoMotion)Entity;
        Dictionary<string, object> updateMap = (entity == null) ?
        null : new Dictionary<string, object>() { { "motion_goal", entity.ID } };
        TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
        {
            motion = (IsoMotion)updated;
            button5.ChangeText(motion.MotionGoal == null ? "-" : motion.MotionGoal.TextContent);
            button5.Active = motion.MotionGoal != null;
        });
        TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + motion.ID, updateMap } }, null);
    }

    public void HandleManner(IsoSRelation relation)
    {
        IsoMotion motion = (IsoMotion)Entity;
        Dictionary<string, object> updateMap = (relation == null) ?
        null : new Dictionary<string, object>() { { "manner", relation.ID } };
        TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
        {
            motion = (IsoMotion)updated;
            button10.ChangeText(motion.Manner == null ? "-" : motion.Manner.TextContent);
            button11.Active = motion.Manner != null;
        });
        TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + motion.ID, updateMap } }, null);
    }

    public void HandlePath(IsoSpatialEntity entity, bool pathBeginMatch, bool pathEndMatch, bool pathMidMatch)
    {
        IsoEventPath eventPath = Entity is IsoEventPath ? (IsoEventPath)Entity : null;
        IsoLocationPath path = Entity is IsoLocationPath ? (IsoLocationPath)Entity : null;
        Dictionary<string, object> updateMap = new Dictionary<string, object>();
        if (pathBeginMatch)
        {
            if (eventPath != null) updateMap.Add("startID", (int)entity.ID);
            else updateMap.Add("beginID", (int)entity.ID);
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
            {
                eventPath = eventPath != null ? (IsoEventPath)updated : null;
                path = path != null ? (IsoLocationPath)updated : null;
                button4.ChangeText(eventPath != null ? (eventPath.StartID == null ? "-" : eventPath.StartID.TextContent) :
                                                               (path.BeginID == null ? "-" : path.BeginID.TextContent));
            });
        }
        else if (pathEndMatch)
        {
            updateMap.Add("endID", (int)entity.ID);
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
            {
                eventPath = eventPath != null ? (IsoEventPath)updated : null;
                path = path != null ? (IsoLocationPath)updated : null;
                button5.ChangeText(eventPath != null ? (eventPath.StartID == null ? "-" : eventPath.StartID.TextContent) :
                                                               (path.BeginID == null ? "-" : path.BeginID.TextContent));
            });
        }
        else if (pathMidMatch)
        {
            JsonData pathArray = new JsonData();
            if (eventPath != null && eventPath.MidIDs != null)
            {
                foreach (IsoEntity e in eventPath.MidIDs)
                    pathArray.Add((int)e.ID);
            }
            if (path != null && path.MidIDs != null)
            {
                foreach (IsoEntity e in path.MidIDs)
                    pathArray.Add((int)e.ID);
            }
            pathArray.Add((int)entity.ID);
            updateMap.Add("midID_array", pathArray);
            TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
            {
                eventPath = eventPath != null ? (IsoEventPath)updated : null;
                path = path != null ? (IsoLocationPath)updated : null;
                PathMidIndex = eventPath != null ? eventPath.MidIDs.Count - 1 : path.MidIDs.Count - 1;
            });
        }
        TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);
    }



    public void HandleScope(IsoEntity scope)
    {
        if (!(Entity is IsoSpatialEntity)) return;
        IsoSpatialEntity entity = (IsoSpatialEntity)Entity;
        if (entity.Scopes != null &&
        entity.Scopes.Contains(scope)) return;
        Dictionary<string, object> updateMap = new Dictionary<string, object>();
        JsonData scopeArray = new JsonData();
        if (entity.Scopes != null)
        {
            foreach (IsoEntity e in entity.Scopes)
                scopeArray.Add((int)e.ID);
        }
        scopeArray.Add((int)scope.ID);
        updateMap.Add("scopes_array", scopeArray);
        TextAnnotator.ChangeEventMap.Add((int)entity.ID, (updated) =>
        {
            IsoSpatialEntity newentity = (IsoSpatialEntity)updated;
            ScopeIndex = newentity.Scopes.Count - 1;
        });
        TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);

    }


    // Update is called once per frame
    public override void Update()
    {
        if (StolperwegeHelper.VRWriter.Active) return;

        if (ActiveTab == PanelTab.MultiTokens && SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).y != 0 &&
        CoreferenceScroller != null && CoreferenceScroller.MaxSites > 0)
            CoreferenceScroller.ScrollText(SteamVR_Actions.default_touchpad.GetAxis(SteamVR_Input_Sources.RightHand).y > 0 ? 1 : -1);

    }


    public void ActualizeMultiTokenTab()
    {
        MultiTokenDisplay.text = (Entity.TextReference == null) ? "none" : Entity.TextReference.TextContent;
        CoreferenceLabel.text = "Coreferences:";
    }

    public void Destroy()
    {
        //if (GhostObject != null) Destroy(GhostObject);
        Destroy(gameObject);
    }

    private IEnumerator SetRadialMenuForSelection(InteractiveButton button, List<RadialSection> radialData, string updated_attribute)
    {

        StolperwegeHelper.RadialMenu.UpdateSection(radialData);
        StolperwegeHelper.RadialMenu.Show(true);

        while (SteamVR_Actions.default_trigger.GetState(StolperwegeHelper.User.PointerHandType))
        {
            while (SteamVR_Actions.default_grab.GetStateDown(StolperwegeHelper.User.PointerHandType))
                yield return null;

            if (StolperwegeHelper.RadialMenu.GetSelectedSection() != null && StolperwegeHelper.RadialMenu.GetSelectedSection().Title.Equals("CANCEL") && StolperwegeHelper.RadialMenu.InZeroZone())
                yield break;

            yield return null;
        }

        string selected = StolperwegeHelper.RadialMenu.GetSelectedSection() != null ? StolperwegeHelper.RadialMenu.GetSelectedSection().Title : null;
        string datatype = StolperwegeHelper.RadialMenu.GetSelectedSection() != null ? (string)StolperwegeHelper.RadialMenu.GetSelectedSection().Value : null;

        StolperwegeHelper.RadialMenu.Show(false);

        if (selected != null)
        {
            if (button == ObjectTypeButton)
            {

                if (!AnnotationTypes.TypesystemClassTable[datatype].Equals(Entity.GetType()))
                {
                    JsonData conversionData = new JsonData();
                    conversionData["addr"] = "" + Entity.ID;
                    conversionData["targetType"] = datatype;
                    TextAnnotator.OnElemCreated = (element) =>
                    {
                        SceneBuilder builder = ((SceneBuilderSceneScript)SceneController.ActiveSceneScript).SceneBuilder;
                        if (builder == null) return;
                        InteractiveShapeNetObject script = builder.GetTab<ObjectTab>().CreateObject((IsoEntity)element);
                        StartCoroutine(script.AsyncClick());
                    };
                    TextAnnotator.FireJSONCommand(TextAnnotatorInterface.CommandType.convert, TextAnnotator.ActualDocument.CasId, conversionData);

                }
            }
            else
            {
                Dictionary<string, object> updateMap = new Dictionary<string, object>();
                updateMap.Add(updated_attribute, selected);
                TextAnnotator.ChangeEventMap.Add((int)Entity.ID, (updated) =>
                {
                    IsoEntity entity = (IsoEntity)updated;
                    button.ChangeText(selected); //Nicht ideal, aber naja .... TODO!!
    });
                TextAnnotator.FireWorkBatchCommand(null, null, new Dictionary<string, Dictionary<string, object>>() { { "" + Entity.ID, updateMap } }, null);

            }
        }

        yield break;
    }


    private LineRenderer Arrow;
    private GameObject Cone;
    private Vector3[] _points; Vector3 _start, _end, _middle; Color _color;
    InteractiveObject _hit;
    bool _elevationMatch, _scopeMatch, _pathStartMatch, _pathEndMatch, _pathMidMatch, _triggerMatch, _relatorMatch, _mannerMatch, _goalMatch;

    private IEnumerator ActualizeConnector(InteractiveButton button)
    {
        if (Arrow == null)
        {
            Arrow = StolperwegeHelper.CenterEyeAnchor.gameObject.GetComponent<LineRenderer>();
            if (Arrow == null)
                Arrow = StolperwegeHelper.CenterEyeAnchor.gameObject.AddComponent<LineRenderer>();
        }
        Arrow.positionCount = 9;
        _points = new Vector3[9];
        Arrow.SetPositions(_points);
        Arrow.material = (Material)Instantiate(Resources.Load("Materials/UI/Pointer"));
        Arrow.widthMultiplier = 0.005f;
        Arrow.useWorldSpace = true;
        Arrow.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Arrow.receiveShadows = false;
        Cone = (GameObject)(Instantiate(Resources.Load("Prefabs/UI/Cone")));
        Cone.GetComponent<MeshRenderer>().material = (Material)Instantiate(Resources.Load("Materials/UI/Pointer"));
        Cone.transform.localScale *= 2;

        while (!SteamVR_Actions.default_trigger.GetStateUp(StolperwegeHelper.User.PointerHandType))
        {
            // get the hit
            _hit = StolperwegeHelper.User.PointerHand.IsPointing ? StolperwegeHelper.User.PointerHand.Hit : null;
            Cone.SetActive(_hit != null);
            Arrow.enabled = _hit != null;
            if (_hit == null)
            {
                yield return null;
                continue;
            }

            // check matches
            _elevationMatch = false;
            _scopeMatch = false;
            _pathStartMatch = false;
            _pathEndMatch = false;
            _pathMidMatch = false;
            _triggerMatch = false;
            _relatorMatch = false;
            if (_hit != null)
            {
                if (_hit is TokenObject)
                {
                    TokenObject _token = (TokenObject)_hit;
                    if (_token.HasEntity)
                    {
                        _elevationMatch = button.Equals("AddElevation") && _token.GetEntity() is IsoMeasure;
                        _scopeMatch = button.Equals("AddScope") && _token.GetEntity() is IsoMeasure;
                        _pathStartMatch = button.Equals("AddScope") && _token.GetEntity() is IsoSpatialEntity;
                        _pathEndMatch = button.Equals("AddEndEntity") && _token.GetEntity() is IsoSpatialEntity;
                        _pathMidMatch = button.Equals("AddPathMidEntity") && _token.GetEntity() is IsoSpatialEntity;
                        _triggerMatch = button.Equals("AddTrigger") && _token.GetEntity() is IsoMotion;
                        _relatorMatch = button.Equals("AddRelator") && _token.GetEntity() is IsoSRelation;
                        _goalMatch = button.Equals("AddGoal") && _token.GetEntity() is IsoSpatialEntity;
                        _mannerMatch = button.Equals("AddManner") && _token.GetEntity() is IsoSRelation;
                    }
                }
                if (_hit is InteractiveShapeNetObject)
                {
                    InteractiveShapeNetObject _object = (InteractiveShapeNetObject)_hit;
                    if (_object.Entity != null)
                    {

                        _elevationMatch = button.Equals("AddElevation") && _object.Entity is IsoMeasure;
                        _scopeMatch = button.Equals("AddScope") && _object.Entity is IsoMeasure;
                        _pathStartMatch = button.Equals("AddScope") && _object.Entity is IsoSpatialEntity;
                        _pathEndMatch = button.Equals("AddEndEntity") && _object.Entity is IsoSpatialEntity;
                        _pathMidMatch = button.Equals("AddPathMidEntity") && _object.Entity is IsoSpatialEntity;
                        _triggerMatch = button.Equals("AddTrigger") && _object.Entity is IsoMotion;
                        _relatorMatch = button.Equals("AddRelator") && _object.Entity is IsoSRelation;
                        _goalMatch = button.Equals("AddGoal") && _object.Entity is IsoSpatialEntity;
                        _mannerMatch = button.Equals("AddManner") && _object.Entity is IsoSRelation;
                    }
                }
            }

            // actualize arrow
            _start = button.transform.position;


            _end = (_elevationMatch || _scopeMatch || _pathStartMatch || _pathMidMatch ||
                    _pathEndMatch || _triggerMatch || _relatorMatch) ?
                    _hit.transform.position : StolperwegeHelper.User.PointerHand.RaySphere.transform.position;
            _middle = (_start + _end) / 2;
            _middle = transform.InverseTransformPoint(_middle);
            _middle.z = 0.5f;
            _middle = transform.TransformPoint(_middle);
            _color = (_elevationMatch || _scopeMatch || _pathStartMatch || _pathMidMatch ||
                    _pathEndMatch || _triggerMatch || _relatorMatch) ? Color.green : Color.red;
            Arrow.material.SetColor("_Color", _color);
            Arrow.material.SetColor("_EmissionColor", _color * 5);
            _points = BezierCurve.CalculateCurvePoints(_start, _middle, _end, _points);
            Cone.GetComponent<Renderer>().material.SetColor("_Color", _color);
            Cone.GetComponent<Renderer>().material.SetColor("_EmissionColor", _color * 5);
            Cone.transform.forward = _points[_points.Length - 1] - _points[_points.Length - 2];
            Cone.transform.position = _points[_points.Length - 1] - Cone.transform.forward * Cone.GetComponent<Renderer>().bounds.size.z / 2;
            Cone.transform.forward = _points[_points.Length - 1] - _points[_points.Length - 2];
            _points[_points.Length - 1] = Cone.transform.position;
            Arrow.SetPositions(_points);

            yield return null;
        }

        Destroy(Arrow);
        Destroy(Cone);


        if (_hit != null)
        {
            IsoEntity hitEntity = null;
            if (_hit is TokenObject)
                hitEntity = ((TokenObject)_hit).GetEntity();
            else if (_hit is InteractiveShapeNetObject)
                hitEntity = ((InteractiveShapeNetObject)_hit).Entity;

            if (hitEntity != null)
            {
                if (_elevationMatch)
                    HandleElevation((IsoMeasure)hitEntity);

                if (_scopeMatch)
                    HandleScope(hitEntity);

                if (_pathStartMatch || _pathEndMatch || _pathMidMatch)
                    HandlePath((IsoSpatialEntity)hitEntity, _pathStartMatch, _pathEndMatch, _pathMidMatch);

                if (_triggerMatch)
                    HandleTrigger((IsoMotion)hitEntity);

                if (_relatorMatch)
                    HandleRelator((IsoSRelation)hitEntity);

            }
        }
    }


    private void DeactivateAllGeneralTabs()
    {
        foreach (string tab in _allTabNames)
        {
            GeneralTab.transform.Find(tab).gameObject.SetActive(false);
        }

    }

}

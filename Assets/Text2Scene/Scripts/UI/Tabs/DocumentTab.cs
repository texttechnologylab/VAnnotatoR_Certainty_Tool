using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Text2Scene;
using Text2Scene.NeuralNetwork;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[PrefabInterface(PrefabPath + "DocumentTab")]
public class DocumentTab : BuilderTab
{

    private string ReminderMessage = "The document has unsaved changes.\nDo you want to save them before closing?";
    //public const int EXAMPLE_REPOSITORY = 27014; //Henlein Homeverzeichnis
    //public const int EXAMPLE_REPOSITORY = 27055; //Spaceeval Dev

    //public const int EXAMPLE_REPOSITORY = 27070; //SpaceEval ANC OLD
    //public const int EXAMPLE_REPOSITORY = 27071; //SpaceEval CP OLD
    //public const int EXAMPLE_REPOSITORY = 27072; //SpaceEval RFC OLD
    //public const int EXAMPLE_REPOSITORY = 27193; //SpaceEval TrainV2

    public const int EXAMPLE_REPOSITORY = 27574; //ISA17
    //public const int EXAMPLE_REPOSITORY = "root";

    private GameObject DocumentField;
    private GameObject Text2SceneField;
    private TextMeshPro DocumentSlot;
    private TextMeshPro DocumentLoading;
    private TextMeshPro DocNameLabel;
    private TextMeshPro ExampleLabel;
    private InteractiveButton LoginButton;
    private InteractiveButton PreviousExamples;
    private InteractiveButton NextExamples;
    private InteractiveButton ExampleButton0;
    private InteractiveButton ExampleButton1;
    private InteractiveButton ExampleButton2;
    private InteractiveButton ExampleButton3;
    private InteractiveButton Text2SceneButton;
    private InteractiveButton CreateDocumentButton;
    private InteractiveButton ReleaseDocumentBtn;
    public InteractiveButton SaveDocumentBtn { get; private set; }    

    private GameObject Views;
    private List<InteractiveButton> ViewButtons;
    private InteractiveButton CancelViewSelection;
    private InteractiveButton PreviousViews;
    private InteractiveButton NextViews;
    private TextMeshPro ViewSiteIndex;
    public TextObjectBuilderWindow toWindow { get; set; }

    private GameObject ResourceBrowser;
    private TextMeshPro _resourceTitle;
    private InteractiveButton _root;
    private List<InteractiveButton> _resourceButtons;
    private InteractiveButton _previousResources;
    private InteractiveButton _nextResources;
    private TextMeshPro _siteIndicator;
    private VRResourceData _actualFolder;

    private int _resourceSitePointer;
    public int ResourceSitePointer
    {
        get { return _resourceSitePointer; }
        set
        {
            List<VRResourceData> resourceList = new List<VRResourceData>(_actualFolder.NonEmptyFolders);
            foreach (string type in _actualFolder.FileFormatMap.Keys)
                Debug.Log(type);
            if (_actualFolder.FileFormatMap.ContainsKey("application/bson"))
                resourceList.AddRange(_actualFolder.FileFormatMap["application/bson"]);
            _resourceSitePointer = value;
            int start = _resourceSitePointer * _resourceButtons.Count;
            for (int i=0; i<_resourceButtons.Count; i++)
            {
                _resourceButtons[i].gameObject.SetActive(i + start < resourceList.Count);
                if (_resourceButtons[i].gameObject.activeInHierarchy)
                {
                    VRResourceData resource = resourceList[start + i];
                    _resourceButtons[i].ButtonValue = resource;
                    _resourceButtons[i].ChangeText((resource.Type == VRResourceData.DataType.File ? "\xf15c" : "\xf07b") + " " + resource.Name);
                }
            }
            _previousResources.gameObject.SetActive(start > 0);
            _nextResources.gameObject.SetActive(start + _resourceButtons.Count < resourceList.Count);
            _siteIndicator.text = resourceList.Count > _resourceButtons.Count ?
                "Site " + (_resourceSitePointer + 1) + " of " + Mathf.CeilToInt((float)resourceList.Count / _resourceButtons.Count) : "";
        }
    }


    //public List<VRResourceData> Examples = new List<VRResourceData>();
    public ArrayList Examples = new ArrayList();
    private int _examplePointer;
    public int ExamplePointer
    {
        get { return _examplePointer; }
        set
        {
            _examplePointer = value;
            ActualizeExampleButtons();
        }
    }

    private VRResourceData _lastOpened;
    private VRResourceData _docData;
    public VRResourceData DocumentData
    {
        get { return _docData; }
        set
        {
            if (value == null)
            {
                _docData = value;
                SetDemoButtonStatus(true);
                ReleaseDocumentBtn.Active = false;
                DocNameLabel.text = "";
                DocumentSlot.text = "\xf065";
                Builder.ResetBuilder();
                ResourceBrowser.SetActive(true);
                SetBrowserStatus(true);
                Views.SetActive(false);
                ActualizeExampleButtons();
            }
            else
            {
                if (Builder.EffectActive) Builder.Effect.Stop();
                _docData = value;
                _lastOpened = value;
                _interruptViewSelection = false;
                StartCoroutine(LoadDocument());
            }
            SaveDocumentBtn.Active = false;
        }
    }

    private bool _interruptViewSelection;

    private TextAnnotatorInterface TextAnnotator
    {
        get { return SceneController.GetInterface<TextAnnotatorInterface>(); }
    }

    private UMAManagerInterface UMAManager
    {
        get { return SceneController.GetInterface<UMAManagerInterface>(); }
    }

    public TextAnnotatorDataContainer DocumentContainer
    {
        get
        {
            if (DocumentData == null) return null;
            int id = int.Parse((string) DocumentData.ID);
            if (!TextAnnotator.Document_Map.ContainsKey(id)) return null;
            return TextAnnotator.Document_Map[id];
        }
    }
    public AnnotationDocument Document
    {
        get
        {
            if (DocumentContainer == null) return null;
            return DocumentContainer.Document;
        }
    }

    //private TextObject _textObject = null;
    //public TextObject TextObject
    //{
    //    get
    //    {
    //        return _textObject;
    //    }
    //}

    private int _viewIndex;
    public int ViewIndex
    {
        get { return _viewIndex; }
        set
        {
            _viewIndex = value;
            if (DocumentContainer == null) return;
            int viewCount = DocumentContainer.Views.Count;
            for (int i = 0; i < ViewButtons.Count; i++)
            {
                ViewButtons[i].gameObject.SetActive(i + _viewIndex < viewCount);
                if (ViewButtons[i].gameObject.activeInHierarchy)
                {
                    ViewButtons[i].ChangeText(DocumentContainer.ViewNameMap[DocumentContainer.Views[i + _viewIndex]]);
                    ViewButtons[i].ButtonValue = DocumentContainer.Views[i + _viewIndex];
                }
            }
            NextViews.gameObject.SetActive(viewCount > ViewButtons.Count);
            PreviousViews.gameObject.SetActive(viewCount > ViewButtons.Count);
            ViewSiteIndex.gameObject.SetActive(viewCount > ViewButtons.Count);
            if (NextViews.gameObject.activeInHierarchy)
            {
                NextViews.Active = _viewIndex + ViewButtons.Count < viewCount;
                PreviousViews.Active = _viewIndex > 0;
                int maxSites = Mathf.CeilToInt((float) viewCount / ViewButtons.Count);
                ViewSiteIndex.text = ((_viewIndex / ViewButtons.Count) + 1) + " of " + maxSites;
            }
        }
    }

    private bool TriggerLocked;
    public bool ExamplesLoaded;

    public override void Initialize(SceneBuilder builder)
    {

        base.Initialize(builder);
        ObjectPlacer.Builder = builder;

        Name = "Documents";
        ShowOnToolbar = true;
        UsableWithoutDocument = true;

        Text2SceneField = transform.Find("Text2Scene Configurator").gameObject;
        Text2SceneButton = Text2SceneField.transform.Find("Text2SceneButton").GetComponent<InteractiveButton>();
        Text2SceneButton.ButtonValue = true;
        Text2SceneButton.OnClick = () =>
        {
            Text2SceneButton.ButtonValue = !(bool)Text2SceneButton.ButtonValue;
            if((bool)Text2SceneButton.ButtonValue)
            {
                Text2SceneButton.ButtonText.text = "\U0000f00c";
            }
            else
            {
                Text2SceneButton.ButtonText.text = "\U0000f068";
            }
        };
        //NN_Helper.TextInputInteractionInScene.Text2SceneButton = Text2SceneButton;

        DocumentField = transform.Find("DocumentField").gameObject;
        DocumentSlot = DocumentField.transform.Find("DocumentSlot").GetComponent<TextMeshPro>();
        DocumentLoading = DocumentField.transform.Find("DocumentLoading").GetComponent<TextMeshPro>();
        DocumentLoading.gameObject.SetActive(false);
        DocNameLabel = DocumentField.transform.Find("DocNameLabel").GetComponent<TextMeshPro>();
        DocNameLabel.text = "";
        ReleaseDocumentBtn = DocumentField.transform.Find("ReleaseDocumentButton").GetComponent<InteractiveButton>();
        ReleaseDocumentBtn.OnClick = () =>
        {
            if (Document.HasChanges)
            {
                ReleaseDocumentBtn.Active = false;
                SaveDocumentBtn.Active = false;
                Builder.DisableTabButtons();
                Builder.InitReminder(ReminderMessage,
                    () =>
                    {
                        SaveDocument();
                        Builder.Reminder.SetActive(false);
                        ReleaseDocument();
                    },
                    () =>
                    {
                        Builder.Reminder.SetActive(false);
                        ReleaseDocument();
                    },
                    () =>
                    {
                        Builder.Reminder.SetActive(false);
                        foreach (BuilderTab tab in Builder.BuilderTabs)
                            if (!tab.Equals(this)) tab.ActualizeControlButtonStatus();
                        ReleaseDocumentBtn.Active = true;
                        SaveDocumentBtn.Active = true;
                    });
            }
            else ReleaseDocument();
        };
        SaveDocumentBtn = DocumentField.transform.Find("SaveDocumentButton").GetComponent<InteractiveButton>();
        SaveDocumentBtn.OnClick = () =>
        {
            SaveDocument();
            SaveDocumentBtn.Active = false;
        };

        CreateDocumentButton = DocumentField.transform.Find("CreateDocumentButton").GetComponent<InteractiveButton>();
        CreateDocumentButton.ButtonValue = false;
        CreateDocumentButton.OnClick = () =>
        {
            if (toWindow == null)
            {
                toWindow = Instantiate(Resources.Load("TextObjectBuilder") as GameObject).GetComponent<TextObjectBuilderWindow>();
                toWindow.DocumentTab = this;
            }

            toWindow.transform.position = transform.position + 0.1f * transform.forward;
            toWindow.transform.rotation = transform.rotation;
        };

        ExampleLabel = DocumentField.transform.Find("ExampleLabel").GetComponent<TextMeshPro>();

        LoginButton = DocumentField.transform.Find("LoginButton").GetComponent<InteractiveButton>();
        LoginButton.OnClick = () => { StolperwegeHelper.User.CallLogin(SceneController.GetInterface<ResourceManagerInterface>()); };

        #region EXAMPLES
        PreviousExamples = DocumentField.transform.Find("PreviousExamples").GetComponent<InteractiveButton>();
        PreviousExamples.OnClick = () => { ExamplePointer -= 4; };
        PreviousExamples.Active = false;
        NextExamples = DocumentField.transform.Find("NextExamples").GetComponent<InteractiveButton>();
        NextExamples.OnClick = () => { ExamplePointer += 4; };
        NextExamples.Active = false;
        
        ExampleButton0 = DocumentField.transform.Find("ExampleButton0").GetComponent<InteractiveButton>();
        ExampleButton0.OnClick = () =>
        {
            DocumentData = (VRResourceData) ExampleButton0.ButtonValue;
            NextExamples.Active = false;
            PreviousExamples.Active = false;
            SetDemoButtonStatus(false);
        };
        ExampleButton0.gameObject.SetActive(false);

        ExampleButton1 = DocumentField.transform.Find("ExampleButton1").GetComponent<InteractiveButton>();
        ExampleButton1.OnClick = () =>
        {
            DocumentData = (VRResourceData) ExampleButton1.ButtonValue;
            NextExamples.Active = false;
            PreviousExamples.Active = false;
            SetDemoButtonStatus(false);
        };
        ExampleButton1.gameObject.SetActive(false);

        ExampleButton2 = DocumentField.transform.Find("ExampleButton2").GetComponent<InteractiveButton>();
        ExampleButton2.OnClick = () =>
        {
            DocumentData = (VRResourceData) ExampleButton2.ButtonValue;
            NextExamples.Active = false;
            PreviousExamples.Active = false;
            SetDemoButtonStatus(false);
        };
        ExampleButton2.gameObject.SetActive(false);

        ExampleButton3 = DocumentField.transform.Find("ExampleButton3").GetComponent<InteractiveButton>();
        ExampleButton3.OnClick = () =>
        {
            DocumentData = (VRResourceData) ExampleButton3.ButtonValue;
            NextExamples.Active = false;
            PreviousExamples.Active = false;
            SetDemoButtonStatus(false);
        };
        ExampleButton3.gameObject.SetActive(false);
        #endregion

        #region RESOURCE BROWSER
        ResourceBrowser = transform.Find("ResourceBrowser").gameObject;
        InteractiveButton[] buttons = ResourceBrowser.GetComponentsInChildren<InteractiveButton>();
        _resourceButtons = new List<InteractiveButton>();
        _resourceTitle = ResourceBrowser.transform.Find("Title").GetComponent<TextMeshPro>();
        _siteIndicator = ResourceBrowser.transform.Find("SiteIndicator").GetComponent<TextMeshPro>();
        foreach (InteractiveButton button in buttons)
        {
            if (button.name.Contains("Root"))
            {
                _root = button;
                _root.OnClick = () => 
                {
                    if (_actualFolder == null || _actualFolder.Parent == null) return;
                    LoadFolder(_actualFolder.Parent); 
                };
            }
            else if (button.name.Contains("Resource"))
            {
                _resourceButtons.Add(button);
                button.OnClick = () =>
                {
                    VRResourceData data = (VRResourceData)button.ButtonValue;
                    if (data.Type == VRResourceData.DataType.Folder) LoadFolder(data);
                    else {
                        DocumentData = data;
                        SetBrowserStatus(false);
                    }
                };
            } else
            {
                if (button.name.Contains("Previous"))
                {
                    _previousResources = button;
                    _previousResources.OnClick = () => { ResourceSitePointer -= 1; };
                }
                if (button.name.Contains("Next"))
                {
                    _nextResources = button;
                    _nextResources.OnClick = () => { ResourceSitePointer += 1; };
                }
            }
        }
        ResourceBrowser.SetActive(false);
        #endregion

        Views = transform.Find("Views").gameObject;
        ViewButtons = new List<InteractiveButton>();
        buttons = Views.GetComponentsInChildren<InteractiveButton>();
        foreach (InteractiveButton button in buttons)
        {
            if (button.name.Contains("ViewButton"))
            {
                ViewButtons.Add(button);
                button.OnClick = () =>
                {
                    if (DocumentContainer == null) return;
                    DocumentContainer.View = (string) button.ButtonValue;
                };
            }
            else
            {
                if (button.name.Equals("Previous"))
                {
                    PreviousViews = button;
                    PreviousViews.OnClick = () => { ViewIndex -= ViewButtons.Count; };
                }
                if (button.name.Equals("Next"))
                {
                    NextViews = button;
                    NextViews.OnClick = () => { ViewIndex += ViewButtons.Count; };
                }
                if (button.name.Equals("Cancel"))
                {
                    CancelViewSelection = button;
                    CancelViewSelection.OnClick = () => { _interruptViewSelection = true; };
                }
            }
        }

        Views.SetActive(false);
        ViewSiteIndex = Views.transform.Find("SiteIndex").GetComponent<TextMeshPro>();

        CreateDocumentButton.Active = true;
    }

    private void Update()
    {
        if (SceneController.GetInterface<ResourceManagerInterface>().SessionID != null &&
            LoginButton.gameObject.activeInHierarchy && !ExamplesLoaded)
        {            
            LoadFolder(new VRResourceData("root", ResourceManagerInterface.ROOT, null, "root", DateTime.MaxValue, DateTime.MaxValue, VRData.SourceType.Remote));
            //LoadExamples();
        }
            
        if (DocumentData == null) CheckHands();
        
        //Text2SceneField.SetActive(!Views.activeInHierarchy);
    }

    private void ActualizeExampleButtons()
    {
        PreviousExamples.Active = ExamplePointer > 0;
        NextExamples.Active = ExamplePointer + 4 < Examples.Count;
        ExampleButton0.gameObject.SetActive(ExamplePointer + 1 <= Examples.Count);
        if (ExampleButton0.gameObject.activeInHierarchy)
        {
            if (Examples[ExamplePointer] is VRResourceData)
            {
                ExampleButton0.ChangeText(((VRResourceData)Examples[ExamplePointer]).Name);
            }
            //if (Examples[ExamplePointer] is TextObject)
            //{
            //    ExampleButton0.ChangeText(((TextObject)Examples[ExamplePointer]).title);
            //    //Debug.Log("TextObject: " + ((TextObject)Examples[ExamplePointer]).title);
            //}
            //if (Examples[ExamplePointer] is VRResourceData) ExampleButton0.ChangeText(((VRResourceData)Examples[ExamplePointer]).Name);
            //if (Examples[ExamplePointer] is TextObject) ExampleButton0.ChangeText(((TextObject)Examples[ExamplePointer]).title);
            ExampleButton0.ButtonValue = Examples[ExamplePointer];
        }
        ExampleButton1.gameObject.SetActive(ExamplePointer + 2 <= Examples.Count);
        if (ExampleButton1.gameObject.activeInHierarchy)
        {
            if (Examples[ExamplePointer + 1] is VRResourceData)
            {
                ExampleButton1.ChangeText(((VRResourceData)Examples[ExamplePointer + 1]).Name);
            }
            //if (Examples[ExamplePointer + 1] is TextObject)
            //{
            //    ExampleButton1.ChangeText(((TextObject)Examples[ExamplePointer + 1]).title);
            //    //Debug.Log("TextObject: " + ((TextObject)Examples[ExamplePointer + 1]).title);
            //}
            //if (Examples[ExamplePointer + 1] is VRResourceData) ExampleButton0.ChangeText(((VRResourceData)Examples[ExamplePointer + 1]).Name);
            //if (Examples[ExamplePointer + 1] is TextObject) ExampleButton0.ChangeText(((TextObject)Examples[ExamplePointer + 1]).title);
            //ExampleButton1.ChangeText(Examples[ExamplePointer + 1].Name);
            ExampleButton1.ButtonValue = Examples[ExamplePointer + 1];
        }
        ExampleButton2.gameObject.SetActive(ExamplePointer + 3 <= Examples.Count);
        if (ExampleButton2.gameObject.activeInHierarchy)
        {
            if (Examples[ExamplePointer + 2] is VRResourceData)
            {
                ExampleButton2.ChangeText(((VRResourceData)Examples[ExamplePointer + 2]).Name);
            }
            //if (Examples[ExamplePointer + 2] is TextObject)
            //{
            //    ExampleButton2.ChangeText(((TextObject)Examples[ExamplePointer + 2]).title);
            //    //Debug.Log("TextObject: " + ((TextObject)Examples[ExamplePointer + 2]).title);
            //}
            //if (Examples[ExamplePointer + 2] is VRResourceData) ExampleButton0.ChangeText(((VRResourceData)Examples[ExamplePointer + 2]).Name);
            //if (Examples[ExamplePointer + 2] is TextObject) ExampleButton0.ChangeText(((TextObject)Examples[ExamplePointer + 2]).title);
            //ExampleButton2.ChangeText(Examples[ExamplePointer + 2].Name);
            ExampleButton2.ButtonValue = Examples[ExamplePointer + 2];
        }
        ExampleButton3.gameObject.SetActive(ExamplePointer + 4 <= Examples.Count);
        if (ExampleButton3.gameObject.activeInHierarchy)
        {
            if (Examples[ExamplePointer + 3] is VRResourceData)
            {
                ExampleButton3.ChangeText(((VRResourceData)Examples[ExamplePointer + 3]).Name);
            }
            //if (Examples[ExamplePointer + 3] is TextObject)
            //{
            //    ExampleButton3.ChangeText(((TextObject)Examples[ExamplePointer + 3]).title);
            //    //Debug.Log("TextObject: " + ((TextObject)Examples[ExamplePointer + 3]).title);
            //}
            //if (Examples[ExamplePointer + 3] is VRResourceData) ExampleButton0.ChangeText(((VRResourceData)Examples[ExamplePointer + 3]).Name);
            //if (Examples[ExamplePointer + 3] is TextObject) ExampleButton0.ChangeText(((TextObject)Examples[ExamplePointer + 3]).title);
            //ExampleButton3.ChangeText(Examples[ExamplePointer + 3].Name);
            ExampleButton3.ButtonValue = Examples[ExamplePointer + 3];
        }
    }

    private void SetDemoButtonStatus(bool status)
    {
        ExampleButton0.Active = status;
        ExampleButton1.Active = status;
        ExampleButton2.Active = status;
        ExampleButton3.Active = status;
        CreateDocumentButton.Active = status;
    }

    public void SaveDocument()
    {
        TextAnnotator.FireJSONCommand(TextAnnotatorInterface.CommandType.save_cas, TextAnnotator.ActualDocument.CasId);
        Document.HasChanges = false;
    }

    private void ReleaseDocument()
    {
        if (NN_Helper.TextInputInteractionInScene != null) NN_Helper.TextInputInteractionInScene.Exit();
        TextAnnotator.FireJSONCommand(TextAnnotatorInterface.CommandType.close_cas, TextAnnotator.ActualDocument.CasId);
        UMAManager.CloseActivePanel();
        Builder.GetTab<QuickAnnotatorTool>().AnnotationWindow.Active = false;
        Builder.GetTab<QuickAnnotatorTool>().AnnotationWindowOpener.ButtonOn = false;        
        SetDemoButtonStatus(false);
        ReleaseDocumentBtn.Active = false;
        TextAnnotator.ActualDocument = null;
        DocumentData = null;
        StolperwegeHelper.VRWriter.Active = false;
        foreach (LinkAnnotationPanel panel in FindObjectsOfType<LinkAnnotationPanel>())
        {
            panel.Destroy();
        }
    }

    //public void ReleaseTextObject()
    //{
    //    SetDemoButtonStatus(true);
    //    NextExamples.Active = true;
    //    PreviousExamples.Active = true;
    //    ReleaseDocumentBtn.Active = false;
    //    _textObject = null;
    //}

    protected override void UpdateTab()
    {
        base.UpdateTab();
        LoginButton.gameObject.SetActive(!TextAnnotator.Authorized && DocumentData == null);
        ReleaseDocumentBtn.Active = Document != null;
        SaveDocumentBtn.Active = Document != null && Document.HasChanges;
    }

    /// <summary>
    /// Loads the choosen document from the TextAnnotator. The user has the opportunity to choose a certain view.
    /// <para>After the document is unpacked, all 3D-annotations will be also loaded.</para>
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadDocument()
    {
        // setup the ui for showing the authorization-progress
        DocumentLoading.gameObject.SetActive(true);
        StartCoroutine(Builder.LoadingAnimation(DocumentLoading));
        DocNameLabel.text = "Waiting for authorization.";
        // wait until the user has logged in on the TextAnnotator
        if (!TextAnnotator.Authorized)
            yield return StartCoroutine(TextAnnotator.StartAuthorization());

        //if (DocumentData.Source != VRData.SourceType.InScene)
        //{
            // get the views for the actual document and wait until they are loaded
        DocNameLabel.text = "Loading Views of " + DocumentData.Name;
        TextAnnotator.FireJSONCommand(TextAnnotatorInterface.CommandType.open_cas, "" + DocumentData.ID);
        while (TextAnnotator.ActualDocument == null || (TextAnnotator.ActualDocument.CasId.Equals(DocumentData.ID) && !TextAnnotator.ActualDocument.ViewsLoaded))
            yield return null;
        //}
        //else
        //{

        //    while (!StolperwegeHelper.xmiReady) yield return null;
        //    StolperwegeHelper.xmiReady = false;
        //    TextAnnotator.FireJSONCommand(TextAnnotatorInterface.CommandType.create_db_cas, "" + StolperwegeHelper.xmi, fileName: DocumentData.Name + ".xmi");
        //    //NN_Helper.TextInputInteractionInScene.InitButton(DocumentData.TextContent, DocumentData.Name, false);
        //}

        // setup the ui and show the views
        Builder.InterruptLoadingAnimation = true;
        DocumentLoading.gameObject.SetActive(false);
        DocumentSlot.text = "\xf4fd";
        DocumentSlot.transform.localEulerAngles = Vector3.zero;
        Views.SetActive(true);
        ResourceBrowser.SetActive(false);
        DocNameLabel.text = "Waiting for user input.";
        ViewIndex = 0;

        // wait until the user has choosen a view
        while (!_interruptViewSelection && DocumentContainer.View == null)
            yield return null;
        if (_interruptViewSelection)
        {
            ReleaseDocument();
            yield break;
        }

        // to the last step of the loading and open the selected view
        Views.SetActive(false);
        DocumentSlot.text = "\xf065";
        DocumentSlot.transform.localEulerAngles = Vector3.zero;
        DocumentLoading.gameObject.SetActive(true);
        StartCoroutine(Builder.LoadingAnimation(DocumentLoading));
        TextAnnotator.ActualDocument.OpenTools(DocumentContainer.View);
        DocNameLabel.text = "Loading " + DocumentData.Name;

        // wait until the document is unpacked
        while (!TextAnnotator.ActualDocument.DocumentCreated)
            yield return null;

        AnnotationDocument doc = TextAnnotator.ActualDocument.Document;
        ObjectTab objectTab = Builder.GetTab<ObjectTab>();
        bool tabActive = objectTab.gameObject.activeInHierarchy;
        if (!tabActive) objectTab.gameObject.SetActive(true);
        UMAManagerInterface UMAManagerInterface = SceneController.GetInterface<UMAManagerInterface>();
        if (!UMAManagerInterface.IsInitialized) UMAManagerInterface.Initialize();
        // load the 3D-representation of all spatial-entities
        List<IsoEntity> entities = new List<IsoEntity>(doc.GetElementsOfTypeInRange<IsoEntity>(doc.Begin, doc.End, true));
        foreach (IsoEntity entity in entities)
        {
            // check the type of the object, if it is a ShapeNet object, make sure it is loaded
            if (entity.Object_ID != null && !entity.Object_ID.Equals("null"))
            {
                // Make sure the entity is not part of an UMA
                if (!UMAISOEntity.AVATAR_TYPE_NAMES.Contains(entity.Object_ID))
                {
                    if (!ObjectTab.ABSTRACT_TYPES.Contains(entity.Object_ID) &&
                        !Builder.SceneBuilderControl.LoadedModels.ContainsKey(entity.Object_ID))
                    {
                        objectTab.LoadModel(entity.Object_ID, false);
                        while (objectTab.ModelIsLoading)
                            yield return null;
                    }

                    // create the object
                    objectTab.CreateObject(entity);
                }
            }

        }

        // get all exisiting links and set them up between entities
        List<IsoLink> links = new List<IsoLink>(doc.GetElementsOfTypeInRange<IsoLink>(doc.Begin, doc.End, true));
        UMAManagerInterface.AttributeEntityLinks = new List<IsoMetaLink>();
        foreach (IsoLink link in links)
        {
            // get the ground object of the link
            IsoEntity figure = link.Figure;
            IsoEntity ground = link.Ground;
            if (figure == null || ground == null)
            {
                Debug.LogWarning("The figure is not existing anymore.");
                continue;
            }
            if (link is IsoMetaLink && figure is IsoSpatialEntity iFigure && UMAISOEntity.AVATAR_TYPE_NAMES.Contains(iFigure.Object_ID))
            {
                UMAManagerInterface.AttributeEntityLinks.Add((IsoMetaLink) link);
                continue;
            }

            link.CreateInteractiveLinkObject();
        }
        objectTab.gameObject.SetActive(tabActive);
        UMAManagerInterface.BuildAvatars();

        /*
        if (NN_Helper.TextInputInteractionInScene != null)
        {
            if (NN_Helper.isoSpatialEntities.Count == 0) NN_Helper.TextInputInteractionInScene.InitButton(doc.TextContent, doc.Name, false);
            else NN_Helper.TextInputInteractionInScene.InitWithDocument(Document);
        }*/

        // after everything is loaded, unlock all blockings and actualize the ui
        TriggerLocked = false;
        DocNameLabel.text = DocumentData.Name + " loaded.";
        Builder.InterruptLoadingAnimation = true;
        DocumentLoading.gameObject.SetActive(false);
        ReleaseDocumentBtn.Active = true;
        foreach (BuilderTab tab in Builder.BuilderTabs)
            if (!tab.Equals(this)) tab.ActualizeControlButtonStatus();
    }

    public void LoadExamples()
    {
        LoginButton.gameObject.SetActive(false);
        VRResourceData exampleFolder = new VRResourceData("Example Folder", "", null, EXAMPLE_REPOSITORY, DateTime.Now, DateTime.Now, VRData.SourceType.Remote);
        Builder.InterruptLoadingAnimation = false;
        StartCoroutine(Builder.LoadingAnimation(ExampleLabel));
        StartCoroutine(SceneController.GetInterface<ResourceManagerInterface>().GetRepositoryInformations(exampleFolder, () =>
        {
            Debug.Log("Folder Example: " + exampleFolder.FileFormatMap["application/bson"]);
            foreach (VRResourceData doc in exampleFolder.FileFormatMap["application/bson"])
                Examples.Add(doc);
            //Examples.AddRange(TextObject.textObjects);
            ExamplePointer = 0;
            Builder.InterruptLoadingAnimation = true;
            ExampleLabel.text = "Examples:";
        }));
        ExamplesLoaded = true;
    }

    public void LoadFolder(VRResourceData folder)
    {
        ResourceBrowser.SetActive(true);
        LoginButton.gameObject.SetActive(false);
        Builder.InterruptLoadingAnimation = false;
        foreach (InteractiveButton btn in _resourceButtons)
            btn.gameObject.SetActive(false);
        _root.gameObject.SetActive(false);
        _previousResources.gameObject.SetActive(false);
        _nextResources.gameObject.SetActive(false);
        _siteIndicator.text = "";
        StartCoroutine(Builder.LoadingAnimation(_resourceTitle));
        StartCoroutine(SceneController.GetInterface<ResourceManagerInterface>().GetRepositoryTAInformations(folder, () =>
        {
            _actualFolder = folder;
            _resourceTitle.text = folder.Name;
            _root.gameObject.SetActive(!folder.Name.Equals("root"));
            ResourceSitePointer = 0;            
            Builder.InterruptLoadingAnimation = true;
            SetBrowserStatus(true);
        }));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!TriggerLocked && other.GetComponent<DataBrowserResource>() != null &&
            DocumentData == null && other.GetComponent<DataBrowserResource>().Data != null)
        {
            TriggerLocked = true;
            VRData data = other.GetComponent<DataBrowserResource>().Data;
            if (data is VRResourceData && ((VRResourceData) data).IsUIMADocument)
                DocumentData = (VRResourceData) data;
        }
    }

    //GameObject testSphere;
    protected override void CheckHands()
    {
        base.CheckHands();

        if ((ObjectInLeftHand != null && (ObjectInLeftHand.Data is VRResourceData) &&
                ((VRResourceData) ObjectInLeftHand.Data).IsUIMADocument))
        {
            localPos = Builder.transform.InverseTransformPoint(ObjectInLeftHand.transform.position);
            Builder.Effect.SetVector3("Start", localPos);
            Builder.Effect.SetVector3("Target", DocumentSlot.transform.localPosition);
            Builder.Effect.SetFloat("StartRadius", ObjectInLeftHand.GetComponent<BoxCollider>().bounds.size.x);
            Builder.Effect.SetFloat("TargetRadius", DocumentSlot.rectTransform.rect.width);
            if (!Builder.EffectActive) Builder.SetEffectStatus(true);
        }
        else if ((ObjectInRightHand != null && (ObjectInRightHand.Data is VRResourceData) &&
                ((VRResourceData) ObjectInRightHand.Data).IsUIMADocument))
        {
            //testSphere.SetActive(true);
            //testSphere.transform.localPosition = DocumentSlot.transform.localPosition;
            localPos = Builder.transform.InverseTransformPoint(ObjectInRightHand.transform.position);
            Builder.Effect.SetVector3("Start", localPos);
            Builder.Effect.SetVector3("Target", DocumentSlot.transform.localPosition);
            Builder.Effect.SetFloat("StartRadius", ObjectInRightHand.GetComponent<BoxCollider>().bounds.size.x);
            Builder.Effect.SetFloat("TargetRadius", DocumentSlot.rectTransform.rect.width);
            if (!Builder.EffectActive) Builder.SetEffectStatus(true);
        }
        else if (Builder.EffectActive)
        {
            Builder.SetEffectStatus(false);
        }

    }

    public override void ResetTab()
    {

    }

    private void SetBrowserStatus(bool status)
    {
        foreach (InteractiveButton btn in _resourceButtons)
            btn.Active = status;
        _root.Active = status;
        _previousResources.Active = status;
        _nextResources.Active = status;
    }

}
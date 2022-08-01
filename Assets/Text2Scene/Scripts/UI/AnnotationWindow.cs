using MathHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using Text2Scene.NeuralNetwork;
using TMPro;
using UnityEngine;
using Valve.VR;

public class AnnotationWindow : AnimatedWindow
{

    public const int MaxSentencePerSite = 20;
    public float MaxSentenceLength { get; private set; }
    private float MaxIndexWidth;
    public AnnotationDocument Document { get; private set; }
    private List<Sentence> Sentences;
    private SentenceIndexContainer _indexDummy;
    private List<SentenceIndexContainer> SentenceIndices;
    public List<TokenObject> TokenObjects { get; private set; }
    public List<TokenObject> EmptyTokenObjects { get; private set; }

    private float TokenHeight = 0.05f;
    private float TokenPadding = 0.02f;
    private float Padding = 0.01f;
    private float MinX;
    private float MaxX = 5f;
    private float MinY = 0.4f;
    private float MaxY = 5f;
    private float SizeX;
    private float SizeY;

    private BoxCollider _collider;
    private GameObject _outliner;
    private GameObject _background;
    public InteractiveButton _closeButton { get; private set; }

    // Annotation controlfield variables
    private GameObject _controlField;
    private InteractiveButton _undo;
    private InteractiveButton _redo;
    //private InteractiveButton _addEmptyQTN;
    private InteractiveButton ObjectPlacing;
    private Vector2 _controlFieldSize
    {
        get
        {
            return new Vector2(_undo.GetComponent<BoxCollider>().size.x + Padding + _redo.GetComponent<BoxCollider>().size.x, 
                               _undo.GetComponent<BoxCollider>().size.y);
        }
    }

    // Annotation surface
    private GameObject _annotationSurface;
    private GameObject _annotationBackground;

    // Empty token container
    private GameObject _emptyEntityControl;
    private float _emptyEntityControlHeight
    {
        get
        {
            return _label.rectTransform.sizeDelta.y + Padding + _entity.GetComponent<BoxCollider>().size.y + 
                   Padding + _emptyEntityBackground.transform.localScale.y;
        }
    }

    private int EmptyTokenSitePointer;
    private List<int> VisibleEmptyTokens;
    private GameObject _emptyEntityBackground;
    private TextMeshPro _label;
    private InteractiveCheckbox _entity;
    private InteractiveCheckbox _path;
    private InteractiveCheckbox _place;
    private InteractiveCheckbox _eventPath;
    private InteractiveButton _prevEntities;
    private InteractiveButton _nextEntities;

    // Site control
    private GameObject _siteSettingsToolbar;
    private Vector2 _siteSettingsToolbarSize
    {
        get
        {
            return new Vector2(Padding + _prevSite.GetComponent<BoxCollider>().size.x +
                               Padding + _sentenceDisplay.rectTransform.rect.width +
                               Padding + _nextSite.GetComponent<BoxCollider>().size.x +
                               2 * Padding + _lessSentences.GetComponent<BoxCollider>().size.x +
                               Padding + _display.rectTransform.rect.width +
                               Padding + _moreSentences.GetComponent<BoxCollider>().size.x + Padding,
                               Mathf.Max(_display.rectTransform.rect.height, _sentenceDisplay.rectTransform.rect.height));
        }
    }


    // Site switcher variables
    private GameObject _siteSwitcher;
    private TextMeshPro _sentenceDisplay;
    private InteractiveButton _prevSite;
    private InteractiveButton _nextSite;
    

    public int Sites { get { return Mathf.CeilToInt((float)Sentences.Count / SentencePerSite); } }
    
    private int _sentencePointer;
    public int SentencePointer
    {
        get { return _sentencePointer; }
        set
        {
            _sentencePointer = value;
            _prevSite.Active = _sentencePointer > 0;
            _nextSite.Active = (_sentencePointer + SentencePerSite) < Sentences.Count;
            _sentenceDisplay.text = "Site " + ((_sentencePointer / SentencePerSite) + 1) + " of " + Sites;
            for (int i = 0; i < Sentences.Count; i++)
                Sentences[i].AnnotationWindow = i >= _sentencePointer && i < _sentencePointer + SentencePerSite ? this : null;
        }
    }

    // SentencePerSite variables
    private GameObject _sentencesPerSite;
    private TextMeshPro _textSizeTester;
    private TextMeshPro _display;
    private InteractiveButton _lessSentences;
    private InteractiveButton _moreSentences;

    private int _sentencePerSite;
    public int SentencePerSite
    {
        get { return _sentencePerSite; }
        set
        {
            _sentencePerSite = value;
            _lessSentences.Active = value > 1;
            _moreSentences.Active = value < Mathf.Min(MaxSentencePerSite, Sentences.Count);
            _display.text = "" + _sentencePerSite + " sent. / site";
            if (SentenceIndices.Count < _sentencePerSite)
            {
                int diff = _sentencePerSite - SentenceIndices.Count;
                for (int i = 0; i < diff; i++)
                {
                    SentenceIndexContainer sic = Instantiate(_indexDummy.gameObject).GetComponent<SentenceIndexContainer>();
                    sic.Initialize();                    
                    sic.transform.GetChild(0).gameObject.SetActive(true);
                    sic.transform.GetChild(1).gameObject.SetActive(true);
                    sic.OnClick = () =>
                    {
                        if (NN_Helper.Text2SceneHandler != null) NN_Helper.Text2SceneHandler.DisplaySentence(sic.SentenceIndex);
                    };
                    SentenceIndices.Add(sic);
                }
                    
            }
            for (int i=0; i<SentenceIndices.Count; i++)
            {
                SentenceIndices[i].gameObject.SetActive(i < _sentencePerSite);
                SentenceIndices[i].transform.SetParent(_annotationSurface.transform);
            }
                
            SentencePointer = (SentencePointer / _sentencePerSite) * _sentencePerSite;
        }
    }

    // Calculation variables
    private float HeightWithoutSurface
    {
        //                                | ==> Padding (Top)
        //                          [x]   | ==> Close Button 
        //                                | ==> Padding
        // [<] [>]                        | ==> Controlfield
        //                               _| ==> Padding
        // +--------------------------+   |
        // |                          |   |
        // |                          |   |
        // |                          |   | ==> Annotation-surface which is not included
        // |                          |   |
        // |                          |   |
        // +--------------------------+  _|
        //                               _| ==> Padding   
        // ------ Empty entities ------   |
        //         [] [] [] []            | ==> Empty-entity-controlfield    
        // [<] [                  ] [>]  _|
        //                                | ==> Padding
        // [<][site][>] [-][sen/site][+]  | ==> Site Control
        //                                | ==> Padding (Bottom)
        get { return 6 * Padding + _closeButton.GetComponent<BoxCollider>().size.y + _siteSettingsToolbarSize.y + _controlFieldSize.y + _emptyEntityControlHeight; }
    }

    // Magnifier
    private GameObject Magnifier;
    private GameObject MagnifierBackground;
    private GameObject MagnifierOutliner;
    private TextMeshPro Text;
    private LineRenderer LineRenderer;
    public TokenObject MagnifiedToken;
    private bool _magnifierActive = true;
    public Vector3 MagnifierTargetPosition;
    private bool _magnifierAnimOn;
    private float _magnifierAnimLerp;
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
                _magnifierAnimLerp = 0;
                _magnifierAnimOn = true;
            }
            else
            {
                Magnifier.transform.localPosition = Vector3.forward * -0.1f;
                Magnifier.transform.localRotation = Quaternion.identity;
                Magnifier.transform.localScale = Vector3.one * 0.01f;
                MagnifiedToken = null;
            }
        }
    }

    private Vector3 _localLeftPos;
    private Vector3 _localRightPos;
    private float _lastHandDistX;
    private float _lastHandDistY;
    private float _handDistX;
    private float _handDistY;
    private Vector3 _middle;
    private Plane _spanningPlane;
    private Vector3 a, b, c;
    private Vector3 _leftHandPos { get { return StolperwegeHelper.LeftFist.transform.position; } }
    private Vector3 _rightHandPos { get { return StolperwegeHelper.RightFist.transform.position; } }

    public void Initialize()
    {
        base.Start();

        // generals
        _collider = GetComponent<BoxCollider>();
        if (_collider == null) _collider = gameObject.AddComponent<BoxCollider>();
        _textSizeTester = transform.Find("TextSizeTester").GetComponent<TextMeshPro>();
        _outliner = transform.Find("Outliner").gameObject;
        _background = transform.Find("Background").gameObject;
        _closeButton = transform.Find("Close").GetComponent<InteractiveButton>();
        _tokenDummy = (GameObject)Instantiate(Resources.Load("Prefabs/Objects/TokenObject"));
        _tokenDummy.SetActive(false);

        // controlfield
        _controlField = transform.Find("Controlfield").gameObject;
        _undo = _controlField.transform.Find("Undo").GetComponent<InteractiveButton>();
        _undo.OnClick = SceneController.GetInterface<TextAnnotatorInterface>().Undo;

        _redo = _controlField.transform.Find("Redo").GetComponent<InteractiveButton>();
        _redo.OnClick = SceneController.GetInterface<TextAnnotatorInterface>().Redo;

        ObjectPlacing = transform.Find("ObjectPlacing").GetComponent<InteractiveButton>();
        ObjectPlacing.OnClick = () =>
        {
             NN_Helper.TextInputInteractionInScene.PlaceModels(0, NN_Helper.isoSpatialEntities.Count);
        };

        //_addEmptyQTN = _controlField.transform.Find("AddEmptyQTN").GetComponent<InteractiveButton>();
        //_addEmptyQTN.OnClick = () =>
        //{
        //    SceneController.GetInterface<TextAnnotatorInterface>().CreateSpatialEntity("emptyobject", Vector3.zero, Quaternion.identity, Vector3.zero);           
        //};

        // annotation surface
        _annotationSurface = transform.Find("AnnotationSurface").gameObject;
        _annotationBackground = _annotationSurface.transform.Find("Background").gameObject;

        SentenceIndices = new List<SentenceIndexContainer>();
        _indexDummy = ((GameObject)Instantiate(Resources.Load("Prefabs/Objects/SentenceIndex"))).GetComponent<SentenceIndexContainer>();
        _indexDummy.Initialize();
        _indexDummy.GetComponent<BoxCollider>().enabled = true;
        _indexDummy.transform.GetChild(0).gameObject.SetActive(false);
        _indexDummy.transform.GetChild(1).gameObject.SetActive(false);
        _indexDummy.transform.SetParent(transform);

        // empty entity control
        _emptyEntityControl = transform.Find("EmptyEntityControl").gameObject;
        _label = _emptyEntityControl.transform.Find("Label").GetComponent<TextMeshPro>();
        _entity = _emptyEntityControl.transform.Find("EntityCheckbox").GetComponent<InteractiveCheckbox>();
        _entity.OnClick = () =>
        {
            _entity.ButtonOn = !_entity.ButtonOn;
            //UpdateTokenContainer();
        };
        _entity.transform.Find("Background").GetComponent<MeshRenderer>().material.SetColor("_BaseColor", IsoSpatialEntity.ClassColor);
        _entity.transform.Find("Label").GetComponent<TextMeshPro>().color = Color.black;
        _entity.ButtonOn = true;

        _path = _emptyEntityControl.transform.Find("PathCheckbox").GetComponent<InteractiveCheckbox>();
        _path.OnClick = () =>
        {
            _path.ButtonOn = !_path.ButtonOn;
            //UpdateTokenContainer();
        };
        _path.transform.Find("Background").GetComponent<MeshRenderer>().material.SetColor("_BaseColor", IsoLocationPath.ClassColor);
        _path.transform.Find("Label").GetComponent<TextMeshPro>().color = Color.black;
        _path.ButtonOn = true;

        _place = _emptyEntityControl.transform.Find("PlaceCheckbox").GetComponent<InteractiveCheckbox>();
        _place.OnClick = () =>
        {
            _place.ButtonOn = !_place.ButtonOn;
            //UpdateTokenContainer();
        };
        _place.transform.Find("Background").GetComponent<MeshRenderer>().material.SetColor("_BaseColor", IsoLocationPlace.ClassColor);
        _place.transform.Find("Label").GetComponent<TextMeshPro>().color = Color.black;
        _place.ButtonOn = true;

        _eventPath = _emptyEntityControl.transform.Find("EventPathCheckbox").GetComponent<InteractiveCheckbox>();
        _eventPath.OnClick = () =>
        {
            _eventPath.ButtonOn = !_eventPath.ButtonOn;
            //UpdateTokenContainer();
        };
        _eventPath.transform.Find("Background").GetComponent<MeshRenderer>().material.SetColor("_BaseColor", IsoEventPath.ClassColor);
        _eventPath.transform.Find("Label").GetComponent<TextMeshPro>().color = Color.black;
        _eventPath.ButtonOn = true;

        _emptyEntityBackground = _emptyEntityControl.transform.Find("Background").gameObject;

        _prevEntities = _emptyEntityControl.transform.Find("PreviousEntities").GetComponent<InteractiveButton>();
        _prevEntities.OnClick = () =>
        {
            EmptyTokenSitePointer -= 1;
            UpdateEmptyTokenContainer(false);
        };
        _nextEntities = _emptyEntityControl.transform.Find("NextEntities").GetComponent<InteractiveButton>();
        _nextEntities.OnClick = () =>
        {
            EmptyTokenSitePointer += 1;
            UpdateEmptyTokenContainer(false);
        };

        // site control
        _siteSettingsToolbar = transform.Find("SiteSettingsToolbar").gameObject;
        _siteSwitcher = _siteSettingsToolbar.transform.Find("SiteSwitcher").gameObject;
        _sentenceDisplay = _siteSwitcher.transform.Find("Display/Text").GetComponent<TextMeshPro>();
        _prevSite = _siteSwitcher.transform.Find("Previous").GetComponent<InteractiveButton>();
        _prevSite.OnClick = () => 
        { 
            SentencePointer = Mathf.Max(0, SentencePointer - SentencePerSite);
            UpdateTokenContainer();
        };
        _nextSite = _siteSwitcher.transform.Find("Next").GetComponent<InteractiveButton>();
        _nextSite.OnClick = () => 
        { 
            SentencePointer += SentencePerSite;
            UpdateTokenContainer();
        };

        _sentencesPerSite = _siteSettingsToolbar.transform.Find("SentencesPerSite").gameObject;
        _display = _sentencesPerSite.transform.Find("Display/Text").GetComponent<TextMeshPro>();
        _lessSentences = _sentencesPerSite.transform.Find("Minus").GetComponent<InteractiveButton>();
        _lessSentences.OnClick = () =>
        {
            SentencePerSite -= 1;
            UpdateTokenContainer();
        };
        _moreSentences = _sentencesPerSite.transform.Find("Plus").GetComponent<InteractiveButton>();
        _moreSentences.OnClick = () =>
        {
            SentencePerSite += 1;
            UpdateTokenContainer();
        };

        _siteSwitcher.transform.localPosition = new Vector3(-_siteSettingsToolbarSize.x / 2, 0, 0);
        _sentencesPerSite.transform.localPosition = new Vector3(_siteSettingsToolbarSize.x / 2, 0, 0);

        OnGrabed += (collider) =>
        {
            if (Grabbing.Count == 2)
            {
                _localLeftPos = transform.InverseTransformPoint(_leftHandPos);
                _localRightPos = transform.InverseTransformPoint(_rightHandPos);
                _lastHandDistX = Mathf.Abs(_localRightPos.x - _localLeftPos.x);
                _lastHandDistY = Mathf.Abs(_localRightPos.y - _localLeftPos.y);
            }
        };

        MinX = Mathf.Max(_siteSettingsToolbarSize.x, _controlFieldSize.x);

        // Magnifier
        Magnifier = (GameObject)Instantiate(Resources.Load("Prefabs/Objects/TokenMagnifier"));
        Magnifier.transform.SetParent(transform);
        MagnifierBackground = Magnifier.transform.Find("Background").gameObject;
        MagnifierOutliner = Magnifier.transform.Find("Outliner").gameObject;
        MagnifierOutliner.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION_ON");
        MagnifierOutliner.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0, 0.627451f, 5.992157f) * 3);
        Text = Magnifier.transform.Find("Text").GetComponent<TextMeshPro>();
        LineRenderer = gameObject.AddComponent<LineRenderer>();        
        Material lineMat = (Material)Instantiate(Resources.Load("Materials/UI/MenuButtonMaterial"));
        lineMat.EnableKeyword("_EMISSION_ON");
        lineMat.SetColor("_EmissionColor", new Color(0, 0.627451f, 5.992157f) * 3);
        LineRenderer.material = lineMat;
        LineRenderer.widthMultiplier = 0.001f;
        LineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        LineRenderer.receiveShadows = false;
        LineRenderer.enabled = false;
        LineRenderer.useWorldSpace = false;
        MagnifierActive = false;
    }

    public void LoadDocument(AnnotationDocument doc)
    {     
        Document = doc;
        Document.OnCanUndo += ActualizeControls;
        Document.OnCanRedo += ActualizeControls;
        Sentences = new List<Sentence>(Document.GetElementsOfType<Sentence>());
        DetermineMaxSentenceLength();
        SentencePerSite = Mathf.Min(5, Sentences.Count);
        SentencePointer = 0;
        SizeX = 0;
        SizeY = 0;
        ActualizeControls();
        UpdateTokenContainer();
        UpdateEmptyTokenContainer(true);
    }

    public void ActualizeControls()
    {
        _undo.Active = Document.CanUndo;
        _redo.Active = Document.CanRedo;
    }

    float windowSizeX, windowSizeY, surfaceSizeX, surfaceSizeY, xPos, yPos, xTrans, yTrans, maxHeight; int maxRows;
    private void ChangeSize(float xDiff, float yDiff)
    {
        // actual position
        this.xPos = transform.localPosition.x;
        this.yPos = transform.localPosition.y;

        // the maximum of possible rows is the minimum sentences/site and no of sentences
        maxRows = Mathf.Min(SentencePerSite, Sentences.Count);
        maxHeight = maxRows * TokenHeight + (maxRows + 1) * TokenPadding + HeightWithoutSurface;

        // calculate the new size of the annotation surface
        surfaceSizeX = Mathf.Min(MaxX - 2 * Padding, Mathf.Max(MinX - 2 * Padding, MaxSentenceLength, SizeX - 2 * Padding + xDiff));
        surfaceSizeY = Mathf.Min(MaxY - HeightWithoutSurface, Mathf.Max(MinY - HeightWithoutSurface, maxHeight - HeightWithoutSurface, (SizeY - HeightWithoutSurface) + yDiff));

        // calculate the new total size of the window
        windowSizeX = surfaceSizeX + 3 * Padding + MaxIndexWidth;
        windowSizeY = surfaceSizeY + HeightWithoutSurface;

        // calculate the difference between actual and new size
        xTrans = (windowSizeX - SizeX) / 2f;
        yTrans = (windowSizeY - SizeY) / 2f;

        // store new window size
        SizeX = windowSizeX;
        SizeY = windowSizeY;


        //if (IsGrabbed)
        //    transform.localPosition = new Vector3(xPos + xDiff / 2, yPos + yDiff / 2, transform.localPosition.z);

        // change collider, background and outliner size
        _collider.size = new Vector3(SizeX, SizeY, 0.01f);
        _outliner.transform.localScale = new Vector3(SizeX + 2 * Padding, SizeY + 2 * Padding, 0.001f);
        _outliner.transform.localPosition = Vector3.back * 0.0005f;
        _background.transform.localScale = new Vector3(SizeX, SizeY, 0.001f);

        // calculate the y-position set the localposition of all elements top-down
        float yPos = SizeY / 2 - Padding - _closeButton.GetComponent<BoxCollider>().size.y / 2;
        float xPos = -SizeX / 2 + Padding + _closeButton.GetComponent<BoxCollider>().size.x / 2;
        _closeButton.transform.localPosition = new Vector3(xPos, yPos, 0.01f);
        xPos += (Padding + ObjectPlacing.GetComponent<BoxCollider>().size.x);
        ObjectPlacing.transform.localPosition = new Vector3(xPos, yPos - 0.025f, 0.01f);
        yPos -= (_closeButton.GetComponent<BoxCollider>().size.y / 2 + Padding + _controlFieldSize.y / 2);

        // positioning of the control field and its buttons
        _controlField.transform.localPosition = new Vector3(0, yPos, 0.01f);
        //float xPos = SizeX / 2 - Padding - _addEmptyQTN.GetComponent<BoxCollider>().size.x / 2;
        //_addEmptyQTN.transform.localPosition = new Vector3(xPos, 0, 0);
        xPos = SizeX / 2 - Padding - _undo.GetComponent<BoxCollider>().size.x / 2;
        //xPos -= (_addEmptyQTN.GetComponent<BoxCollider>().size.x / 2 + Padding + _undo.GetComponent<BoxCollider>().size.x / 2);
        _undo.transform.localPosition = new Vector3(xPos, 0, 0);
        xPos -= (Padding + _undo.GetComponent<BoxCollider>().size.x);
        _redo.transform.localPosition = new Vector3(xPos, 0, 0);

        // set annotation surface size and position
        xPos = SizeX / 2 - 2 * Padding - MaxIndexWidth - surfaceSizeX / 2;
        yPos -= (_controlFieldSize.y / 2 + Padding + surfaceSizeY / 2);
        _annotationBackground.transform.localScale = new Vector3(surfaceSizeX, surfaceSizeY, 0.01f);
        _annotationSurface.transform.localPosition = new Vector3(xPos, yPos, 0.01f);        

        // adjust TokenObject and SentenceIndexContainer positions
        foreach (TokenObject tokenObject in TokenObjects)
            tokenObject.transform.localPosition += new Vector3(xTrans, yTrans, 0);
        foreach (SentenceIndexContainer sic in SentenceIndices)
            if (sic.gameObject.activeInHierarchy) sic.transform.localPosition += new Vector3(xTrans, yTrans, 0);

        // set the empty token container position and size
        yPos -= (surfaceSizeY / 2 + Padding + _emptyEntityControlHeight / 2);        
        _emptyEntityControl.transform.localPosition = new Vector3(0, yPos, 0.01f);
        float originalY = _prevEntities.transform.localPosition.y;
        xPos = SizeX / 2 - Padding - _prevEntities.GetComponent<BoxCollider>().size.x / 2;
        _prevEntities.transform.localPosition = new Vector3(xPos, originalY, 0);
        _nextEntities.transform.localPosition = new Vector3(-xPos, originalY, 0);
        originalY = _emptyEntityBackground.transform.localScale.y;
        _emptyEntityBackground.transform.localScale = new Vector3(SizeX - 4 * Padding - 2 * _prevEntities.GetComponent<BoxCollider>().size.x, originalY, 0.005f);

        // set site toolbar position
        yPos -= (_emptyEntityControlHeight / 2 + Padding + _siteSettingsToolbarSize.y / 2);
        _siteSettingsToolbar.transform.localPosition = new Vector3(0, yPos, 0.01f);
    }

    public override void Update()
    {
        base.Update();

        if (Grabbing.Count == 2)
        {
            _localLeftPos = transform.InverseTransformPoint(_leftHandPos);
            _localRightPos = transform.InverseTransformPoint(_rightHandPos);
            _handDistX = Mathf.Abs(_localRightPos.x - _localLeftPos.x);
            _handDistY = Mathf.Abs(_localRightPos.y - _localLeftPos.y);
            ChangeSize(2 * (_handDistX - _lastHandDistX), 2 * (_handDistY - _lastHandDistY));
            _lastHandDistX = _handDistX;
            _lastHandDistY = _handDistY;
            _middle = (_leftHandPos + _rightHandPos) / 2;
            transform.position = _middle;
            a = _leftHandPos;
            if (_leftHandPos.y > _rightHandPos.y)
            {
                b = _rightHandPos;                
                c = _leftHandPos;
                c.y = _rightHandPos.y;
            } else
            {
                b = _rightHandPos;
                c = _rightHandPos;
                c.y = _leftHandPos.y;
            }
            _spanningPlane = new Plane(a, b, c);
            transform.forward = _spanningPlane.normal;
        }

        if (_magnifierAnimOn) ShowMagnifier();

        if (NN_Helper.TextInputInteractionInScene.objectsPlaced) ObjectPlacing.Active = false;
        else ObjectPlacing.Active = true;
    }

    private void ShowMagnifier()
    {
        if (MagnifiedToken == null) return;
        Magnifier.transform.position = Vector3.Lerp(Magnifier.transform.position, MagnifierTargetPosition, _magnifierAnimLerp);
        Magnifier.transform.localScale = Vector3.Lerp(Magnifier.transform.localScale, Vector3.one, _magnifierAnimLerp);
        Magnifier.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
        LineRenderer.enabled = (Magnifier.transform.position - MagnifiedToken.transform.position).magnitude > 0.05f;
        if (LineRenderer.enabled)
        {
            Vector3 elemScale = MagnifierOutliner.transform.localScale * 0.98f;
            LineRenderer.positionCount = 4;
            LineRenderer.SetPosition(0, Magnifier.transform.localPosition + Magnifier.transform.localRotation * new Vector3(-elemScale.x / 2, elemScale.y / 2, 0));
            LineRenderer.SetPosition(3, Magnifier.transform.localPosition + Magnifier.transform.localRotation * new Vector3(-elemScale.x / 2, -elemScale.y / 2, 0));
            LineRenderer.SetPosition(1, transform.InverseTransformPoint(MagnifiedToken.transform.position) + Vector3.up * TokenHeight / 2);
            LineRenderer.SetPosition(2, transform.InverseTransformPoint(MagnifiedToken.transform.position) + Vector3.down * TokenHeight / 2);
        }
        if (Magnifier.transform.position == MagnifierTargetPosition) _magnifierAnimOn = false;
        else _magnifierAnimLerp += Time.deltaTime;
    }

    public void SetupMagnifier(TokenObject token)
    {
        MagnifiedToken = token;
        Vector2 targetSize = new Vector2(0.4f, 0.1f);
        Text.rectTransform.sizeDelta = targetSize;
        Text.text = token.QuickTreeNode != null ? token.QuickTreeNode.TextContent :
                    token.Entity != null ? token.Entity.TextContent : "";
        Text.ForceMeshUpdate();
        Vector2 size = Text.GetRenderedValues();
        size.x = (Mathf.CeilToInt((int)(size.x * 100)) + 1) / 100f;
        Text.rectTransform.sizeDelta = size;
        Text.ForceMeshUpdate();
        MagnifierBackground.transform.localScale = new Vector3(size.x + 0.01f, size.y + 0.01f, MagnifierBackground.transform.localScale.z);
        MagnifierOutliner.transform.localScale = new Vector3(size.x + 0.02f, size.y + 0.02f, MagnifierOutliner.transform.localScale.z);
        MagnifierActive = true;
    }

    GameObject _tokenDummy; GameObject _tokenObject; TokenObject _tokenObjectScript;
    List<IsoEntity> _spatialEntities; IsoEntity _spatialEntity;
    public void UpdateTokenContainer()
    {

        // Clear the containers first
        if (TokenObjects == null) TokenObjects = new List<TokenObject>();
        else
        {
            foreach (TokenObject t in TokenObjects)
                if (t.gameObject != null) Destroy(t.gameObject);
            TokenObjects.Clear();
        }

        // instantiate the dummy if is null
        _tokenDummy.SetActive(true);

        MaxIndexWidth = _indexDummy.GetComponent<SentenceIndexContainer>().Label.GetPreferredValues("" + (SentencePointer + SentencePerSite)).x;        
        float annoSurfaceHeight = _annotationBackground.transform.localScale.y;
        float maxHeight = Mathf.Min(SentencePerSite, Sentences.Count) * TokenHeight + (Mathf.Min(SentencePerSite, Sentences.Count) + 1) * TokenPadding;

        float diffX = MaxSentenceLength - (SizeX - 2 * Padding);
        float diffY = maxHeight - annoSurfaceHeight;
        if (diffX > 0 || diffY > 0) ChangeSize(Mathf.Max(0, diffX), Mathf.Max(0, diffY));

        annoSurfaceHeight = _annotationBackground.transform.localScale.y;
        float tokenXPos, tokenYPos = annoSurfaceHeight / 2f;
        float sIndexXPos = SizeX / 2 - Padding - MaxIndexWidth / 2;
        for (int i = 0; i < SentencePerSite; i++)
        {
            if (SentencePointer + i >= Sentences.Count) break;
            tokenYPos -= (TokenHeight / 2 + TokenPadding);
            SentenceIndices[i].Setup(SentencePointer + i, MaxIndexWidth, TokenHeight);
            SentenceIndices[i].transform.SetParent(transform);
            SentenceIndices[i].transform.localPosition = new Vector3(sIndexXPos, _annotationSurface.transform.localPosition.y + tokenYPos, 0.01f);
            SentenceIndices[i].transform.localRotation = Quaternion.identity;
            tokenXPos = _annotationBackground.transform.localScale.x / 2;
            foreach (QuickTreeNode qtn in Sentences[SentencePointer + i].GetVisibleQuickTreeNodes())
            {
                _tokenObject = Instantiate(_tokenDummy);
                _tokenObject.name = qtn.TextContent;
                _tokenObjectScript = _tokenObject.GetComponent<TokenObject>();
                _tokenObjectScript.Init(qtn, this);
                _tokenObject.transform.SetParent(_annotationSurface.transform);
                _tokenObject.transform.localRotation = Quaternion.identity;
                _tokenObjectScript.TablePosition = new Vector3(tokenXPos - (_tokenObjectScript.Width / 2 + TokenPadding), tokenYPos, 0.01f);
                tokenXPos -= (_tokenObjectScript.Width + TokenPadding);
                TokenObjects.Add(_tokenObjectScript);
            }
            tokenYPos -= TokenHeight / 2f;
        }

        LockTokenColliders(false);
        _tokenDummy.SetActive(false);
    }

    public void UpdateEmptyTokenContainer(bool recalc)
    {
        if (EmptyTokenObjects == null) EmptyTokenObjects = new List<TokenObject>();
        else
        {
            foreach (TokenObject t in EmptyTokenObjects)
                if (t.gameObject != null) Destroy(t.gameObject);
            EmptyTokenObjects.Clear();
        }

        if (VisibleEmptyTokens == null) VisibleEmptyTokens = new List<int>();        

        _tokenDummy.SetActive(true);

        // get all empty spatial entities
        _spatialEntities = new List<IsoEntity>(Document.GetElementsOfTypeFromTo<IsoEntity>(0, 0, true));
        float xAvailable = _emptyEntityBackground.transform.localScale.x;

        if (recalc)
        {
            if (VisibleEmptyTokens.Count > 0) VisibleEmptyTokens.Clear();
            float tokenSize;
            float xSize = TokenPadding;
            int visibleEntities = 0;
            for (int i=0; i<_spatialEntities.Count; i++)
            {
                tokenSize = _tokenObjectScript.Label.GetPreferredValues(_spatialEntities[i].TextContent).x;
                if (xSize + tokenSize + TokenPadding > xAvailable)
                {
                    VisibleEmptyTokens.Add(visibleEntities);
                    xSize = 2 * TokenPadding + tokenSize;
                    visibleEntities = 1;
                    if (i == _spatialEntities.Count - 1) VisibleEmptyTokens.Add(visibleEntities);
                } else
                {
                    xSize += tokenSize + TokenPadding;
                    visibleEntities += 1;
                    if (i == _spatialEntities.Count - 1) VisibleEmptyTokens.Add(visibleEntities);
                }
            }

            string counts = "";
            foreach (int count in VisibleEmptyTokens)
            {
                counts += count + " ";
            }
            EmptyTokenSitePointer = Math.Max(0, Math.Min(EmptyTokenSitePointer, VisibleEmptyTokens.Count - 1));
        }

        int emptyTokenStart = 0;
        // Get the start index for the actual empty-token site
        for (int i = 0; i < EmptyTokenSitePointer; i++)
            emptyTokenStart += VisibleEmptyTokens[i];        

        float tokenXPos = xAvailable / 2;
        float tokenYPos = _emptyEntityBackground.transform.localPosition.y;

        for (int i = 0; i < VisibleEmptyTokens[EmptyTokenSitePointer]; i++)
        {
            _spatialEntity = _spatialEntities[emptyTokenStart + i];
            _tokenObject = Instantiate(_tokenDummy);
            _tokenObject.name = _spatialEntity.TextContent;
            _tokenObjectScript = _tokenObject.GetComponent<TokenObject>();
            _tokenObjectScript.Init(_spatialEntity, this);
            _tokenObjectScript.Label.faceColor = (Color)_spatialEntity.GetType().GetProperty("ClassColor").GetValue(null);
            _tokenObject.transform.SetParent(_emptyEntityControl.transform);
            _tokenObject.transform.localRotation = Quaternion.identity;
            _tokenObjectScript.TablePosition = new Vector3(tokenXPos - (_tokenObjectScript.Width / 2 + TokenPadding), tokenYPos, 0.01f);
            tokenXPos -= (_tokenObjectScript.Width + TokenPadding);
            EmptyTokenObjects.Add(_tokenObjectScript);
        }
        _prevEntities.Active = EmptyTokenSitePointer > 0;
        _nextEntities.Active = emptyTokenStart + VisibleEmptyTokens[EmptyTokenSitePointer] < _spatialEntities.Count;

        LockEmptyTokenColliders(false);
        _tokenDummy.SetActive(false);
    }

    public void LockTokenColliders(bool lockOn)
    {
        foreach (TokenObject to in TokenObjects)
            to.GetComponent<Collider>().enabled = !lockOn;
    }

    public void LockEmptyTokenColliders(bool lockOn)
    {
        foreach (TokenObject to in EmptyTokenObjects)
            to.GetComponent<Collider>().enabled = !lockOn;
    }


    public void DetermineMaxSentenceLength()
    {
        MaxSentenceLength = 0;
        for (int i = 0; i < Sentences.Count; i++)
        {
            float length = TokenPadding;
            foreach (QuickTreeNode qtn in Sentences[i].GetBaseQuickTreeNodes())
                length += _textSizeTester.GetPreferredValues(qtn.TextContent).x + TokenPadding;
            if (length > MaxSentenceLength) MaxSentenceLength = length;
        }
    }

    public override GameObject OnGrab(Collider other)
    {
        GameObject res = base.OnGrab(other);
        Vector3 localPos = transform.localPosition;
        transform.localPosition = new Vector3(0, localPos.y, localPos.z);
        return res;
    }

    private void SetSentenceIndexColliderStatus(bool status)
    {
        for (int i = 0; i < SentenceIndices.Count; i++)
            SentenceIndices[i].Collider.enabled = status;
    }

    private void SetTokenObjectColliderStatus(bool status)
    {
        foreach (TokenObject to in TokenObjects)
            to.GetComponent<Collider>().enabled = status;
    }

}

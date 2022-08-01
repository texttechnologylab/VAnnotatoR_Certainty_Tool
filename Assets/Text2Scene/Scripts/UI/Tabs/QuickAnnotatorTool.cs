using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[PrefabInterface(PrefabPath + "QuickAnnotatorTool")]
public class QuickAnnotatorTool : BuilderTab
{

    public AnnotationWindow AnnotationWindow;
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
    private float _animLerp;
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
                _animLerp = 0;
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

    protected float TokenPadding { get; private set; }
    protected float ToolSize { get; private set; }
    protected float TokenHeight { get; private set; }
    protected float TokenThickness { get; private set; }
    protected float FontSize { get; private set; }
    public GameObject MultiTokenContainer { get; private set; }
    public List<TokenObject> MultiTokens { get; private set; }
    public GameObject SentenceSwitcher { get; private set; }
    private TextMeshPro SentenceText;
    private InteractiveButton SentenceLeft;
    private InteractiveButton SentenceRight;

    public InteractiveButton AnnotationWindowOpener;

    public List<Sentence> Sentences { get; private set; }

    private int _sentencePointer;
    public int SentencePointer
    {
        get { return _sentencePointer; }
        set
        {
            _sentencePointer = value;
            Sentences[_sentencePointer].QuickAnnotatorTool = this;
            SentenceLeft.Active = _sentencePointer > 0;
            SentenceRight.Active = _sentencePointer < Sentences.Count - 1;
            SentenceText.text = "Sentence " + (_sentencePointer + 1) + " of " + Sentences.Count;
            UpdateTokenContainer();
        }
    }

    public Sentence ActualSentence
    {
        get
        {
            if (SentencePointer == -1) return null;
            return Sentences[SentencePointer];
        }
    }


    // Start is called before the first frame update
    public override void Initialize(SceneBuilder builder)
    {
        base.Initialize(builder);
        Name = "Annotations";
        UsableWithoutDocument = false;
        ShowOnToolbar = true;

        ToolSize = 0.48f;
        TokenPadding = 0.015f;
        TokenHeight = 0.03f;
        TokenThickness = 0.005f;
        FontSize = 0.2f;

        SentenceSwitcher = transform.Find("SentenceSwitcher").gameObject;
        SentenceText = SentenceSwitcher.transform.Find("Text").GetComponent<TextMeshPro>();

        SentenceLeft = SentenceSwitcher.transform.Find("Left").GetComponent<InteractiveButton>();
        SentenceLeft.OnClick = () =>
        {
            if (Sentences != null && SentencePointer < Sentences.Count)
                (Sentences[SentencePointer]).QuickAnnotatorTool = null;
            SentencePointer -= 1;
        };

        SentenceRight = SentenceSwitcher.transform.Find("Right").GetComponent<InteractiveButton>();
        SentenceRight.OnClick = () =>
        {
            if (Sentences != null && SentencePointer < Sentences.Count)
                (Sentences[SentencePointer]).QuickAnnotatorTool = null;
            SentencePointer += 1;
        };

        AnnotationWindow = ((GameObject)Instantiate(Resources.Load("Prefabs/AnnotationWindow"))).GetComponent<AnnotationWindow>();
        AnnotationWindow.Initialize();
        AnnotationWindow.SetVisible(false);

        AnnotationWindowOpener = transform.Find("AnnotationWindowOpener").GetComponent<InteractiveButton>();
        AnnotationWindowOpener.OnClick = () =>
        {
            AnnotationWindowOpener.ButtonOn = !AnnotationWindowOpener.ButtonOn;
            AnnotationWindow.Active = !AnnotationWindow.Active;
            if (AnnotationWindow.Active && AnnotationWindow.Document == null ||
                !AnnotationWindow.Document.Equals(Builder.GetTab<DocumentTab>().Document))
                AnnotationWindow.LoadDocument(Builder.GetTab<DocumentTab>().Document);
        };
        AnnotationWindow._closeButton.OnClick = AnnotationWindowOpener.OnClick;

        if (MultiTokenContainer == null)
        {
            MultiTokenContainer = new GameObject("MultiTokenContainer");
            MultiTokenContainer.transform.parent = transform;
            MultiTokenContainer.transform.localRotation = Quaternion.identity;
        }

        // Magnifier
        Magnifier = (GameObject)Instantiate(Resources.Load("Prefabs/Objects/TokenMagnifier"));
        Magnifier.transform.SetParent(transform);
        MagnifierBackground = Magnifier.transform.Find("Background").gameObject;
        MagnifierOutliner = Magnifier.transform.Find("Outliner").gameObject;
        MagnifierOutliner.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION_ON");
        MagnifierOutliner.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0, 0.627451f, 5.992157f) * 3);
        Text = Magnifier.transform.Find("Text").GetComponent<TextMeshPro>();
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
    }

    private void Update()
    {
        if (_magnifierAnimOn) ShowMagnifier();
    }

    private void ShowMagnifier()
    {
        if (MagnifiedToken == null) return;
        Magnifier.transform.position = Vector3.Lerp(Magnifier.transform.position, MagnifierTargetPosition, _animLerp);
        Magnifier.transform.localScale = Vector3.Lerp(Magnifier.transform.localScale, Vector3.one, _animLerp);
        Magnifier.transform.LookAt(StolperwegeHelper.CenterEyeAnchor.transform);
        LineRenderer.enabled = (Magnifier.transform.position - MagnifiedToken.transform.position).magnitude > 0.05f;
        if (LineRenderer.enabled)
        {
            Vector3 elemScale = MagnifierOutliner.transform.localScale * 0.98f;
            LineRenderer.SetPosition(0, Magnifier.transform.localPosition + Magnifier.transform.localRotation * new Vector3(-elemScale.x / 2, elemScale.y / 2, 0));
            LineRenderer.SetPosition(3, Magnifier.transform.localPosition + Magnifier.transform.localRotation * new Vector3(-elemScale.x / 2, -elemScale.y / 2, 0));
            LineRenderer.SetPosition(1, transform.InverseTransformPoint(MagnifiedToken.transform.position) + Vector3.up * TokenHeight / 2);
            LineRenderer.SetPosition(2, transform.InverseTransformPoint(MagnifiedToken.transform.position) + Vector3.down * TokenHeight / 2);
        }
        if (Magnifier.transform.position == MagnifierTargetPosition) _magnifierAnimOn = false;
        else _animLerp += Time.deltaTime;
    }

    public void SetupMagnifier(TokenObject token)
    {
        MagnifiedToken = token;
        Vector2 targetSize = new Vector2(0.4f, 0.1f);
        Text.rectTransform.sizeDelta = targetSize;        
        Text.text = token.QuickTreeNode.TextContent;
        Text.ForceMeshUpdate();
        Vector2 size = Text.GetRenderedValues();
        size.x = (Mathf.CeilToInt((int)(size.x * 100)) + 1) / 100f;
        Text.rectTransform.sizeDelta = size;
        Text.ForceMeshUpdate();
        MagnifierBackground.transform.localScale = new Vector3(size.x + 0.01f, size.y + 0.01f, MagnifierBackground.transform.localScale.z);
        MagnifierOutliner.transform.localScale = new Vector3(size.x + 0.02f, size.y + 0.02f, MagnifierOutliner.transform.localScale.z);
        MagnifierActive = true;
    }

    GameObject toDummy; GameObject to; TokenObject toScript; List<QuickTreeNode> qTNs;
    public virtual void UpdateTokenContainer()
    {
        if (MultiTokens == null) MultiTokens = new List<TokenObject>();
        else
        {

            foreach (TokenObject t in MultiTokens)
                if (t.gameObject != null)
                    Destroy(t.gameObject);

            MultiTokens.Clear();
        }

        if (toDummy == null)
            toDummy = ((GameObject)Instantiate(Resources.Load("Prefabs/Objects/TokenObject")));
        else
            toDummy.SetActive(true);

        float actualXPos = ToolSize / 2f; float actualYPos = ToolSize / 2f - TokenPadding - TokenHeight / 2f;
        float availableSpace = ToolSize - 2 * TokenPadding;
        qTNs = (Sentences[SentencePointer]).GetVisibleQuickTreeNodes();
        for (int i = 0; i < qTNs.Count; i++)
        {
            to = Instantiate(toDummy);
            to.name = qTNs[i].TextContent;
            toScript = to.GetComponent<TokenObject>();
            // try
            if (qTNs[i].Tokens.Count > 1 && availableSpace > ToolSize / 5)
                toScript.Init(qTNs[i], this, availableSpace, FontSize, TokenHeight, TokenThickness);
            else toScript.Init(qTNs[i], this, -1, FontSize, TokenHeight, TokenThickness);
            to.transform.SetParent(MultiTokenContainer.transform);
            to.transform.localEulerAngles = Vector3.zero;
            if (toScript.Width <= ToolSize)
            {
                if (actualXPos - toScript.Width - TokenPadding <= -ToolSize / 2f + TokenPadding)
                {
                    actualXPos = ToolSize / 2f;
                    actualYPos -= TokenPadding + TokenHeight;
                    availableSpace = ToolSize - 2 * TokenPadding;
                }
                actualXPos -= toScript.Width / 2 + TokenPadding;
            }
            availableSpace -= (toScript.Width + TokenPadding);
            toScript.TablePosition = new Vector3(actualXPos, actualYPos, 0);
            actualXPos -= toScript.Width / 2;
            MultiTokens.Add(toScript);
        }
        LockColliders(false);
        MultiTokenContainer.transform.localPosition = new Vector3(0, -0.05f, 0.02f);
        toDummy.SetActive(false);
    }

    public void LockColliders(bool lockOn)
    {
        foreach (TokenObject to in MultiTokens)
            to.GetComponent<Collider>().enabled = !lockOn;
    }

    protected override void UpdateTab()
    {
        base.UpdateTab();
        Sentences = new List<Sentence>(Builder.GetTab<DocumentTab>().Document.GetElementsOfType<Sentence>());
        SentencePointer = 0;
    }

    public override void ResetTab()
    {
        
    }


}

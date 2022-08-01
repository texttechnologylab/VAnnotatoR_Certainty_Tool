using System.Collections;
using System.Collections.Generic;
using Text2Scene;
using Text2Scene.NeuralNetwork;
using TMPro;
using UnityEngine;

/// <summary>
/// Configurates the Text2Scene Window of the DataBrowser
/// </summary>
public class Text2SceneHandler : MonoBehaviour
{
    public bool Search { get; set; }
    public string SearchRequest { get; private set; }

    private Text2SceneInterface text2SceneInterface;
    private TextMeshPro Title;
    private TextMeshPro Text;
    private TextMeshPro Page;
    private DataBrowser Browser;

    private InteractiveButton PreviousWord;
    private InteractiveButton NextWord;
    private InteractiveButton ScrollDown;
    private InteractiveButton ScrollUp;
    private InteractiveButton ObjectPlacing;
    private int Index;

    private Transform _parent;
    private Vector3 _visiblePos;
    private Quaternion _visibleRot;
    private Quaternion _hiddenRot;
    private Vector3 _targetPos;
    private Quaternion _targetRot;
    private Vector3 _targetScale;
    private List<Sentence> Sentences = new List<Sentence>();
    private Sentence ActiveSentece;
    private int SentenceIndex = 0;
    private Dictionary<Sentence, List<SentenceObject>> SentenceObjects;

    private bool initComplete = false;

    private void Start()
    {
        NN_Helper.Text2SceneHandler = this;
        Init();
    }

    private string lastInterface = "";

    /*
    private void FixedUpdate()
    {
        if (NN_Helper.Word2VecInitialized)
        {
            if (Browser.SelectedInterface != null)
            {
                if (Browser.SelectedInterface.Name == "ShapeNet" && lastInterface != Browser.SelectedInterface.Name)
                {
                    Debug.Log("Update to visible");
                    StartCoroutine(Activate(true));
                    lastInterface = Browser.SelectedInterface.Name;
                }
                else if (Browser.SelectedInterface.Name != lastInterface)
                {
                    lastInterface = Browser.SelectedInterface.Name;
                    Debug.Log("Update to hidden");
                    StartCoroutine(Activate(false));
                }
            }
        }

        if (!(text2SceneInterface.ObjectInserted && initComplete && !text2SceneInterface.UseAutomatic))
        {
            if (Title.text != "Title")
            {
                Title.text = "Title";
                Text.text = "";
                Page.text = "";
            }
            if (NN_Helper.isoSpatialEntities.Count > 0)
            {
                initComplete = true;
                if (Title.text != text2SceneInterface.Title) Title.text = text2SceneInterface.Title;
                Sentences = new List<Sentence>(NN_Helper.Document.GetElementsOfType<Sentence>());
                SentenceObjects = OrderObjects(Sentences);
                DisplaySentence(SentenceIndex);
            }
            else initComplete = false;
        }
        if (NN_Helper.Document == null)
        {
            ResetWindow();
        }
    }*/

    public void ResetWindow()
    {
        Title.text = "Title";
        Text.text = "";
        Page.text = "";
        UpdateButtons();
        Sentences.Clear();
        SentenceIndex = 0;
        SentenceObjects.Clear();
    }

    /// <summary>
    /// Collects all IsoSpatialEntities and their status for the sentences
    /// </summary>
    /// <param name="sentences"></param>
    /// <returns></returns>
    private Dictionary<Sentence, List<SentenceObject>> OrderObjects(List<Sentence> sentences)
    {
        Dictionary<Sentence, List<SentenceObject>> sentenceObjects = new Dictionary<Sentence, List<SentenceObject>>();
        sentences.ForEach(sentence =>
        {
            sentenceObjects.Add(sentence, new List<SentenceObject>());
        });

        int startIndex = 0;
        foreach (Sentence sentence in sentences)
        {
            int begin = sentence.Begin;
            int end = sentence.End;
            for (int i = startIndex; i < NN_Helper.isoSpatialEntities.Count; i++)
            {
                ClassifiedObject classifiedObject = NN_Helper.isoSpatialEntities[i];
                if (classifiedObject.End > sentence.End)
                {
                    startIndex = i;
                    break;
                }
                else if (classifiedObject.Begin >= sentence.Begin) sentenceObjects[sentence].Add(new SentenceObject(classifiedObject, i));
            }
        }

        return sentenceObjects;
    }

    public void Init()
    {
        Index = 0;
        if (text2SceneInterface == null) text2SceneInterface = GameObject.Find("Text2Scene").GetComponent<Text2SceneInterface>();
        Title = transform.Find("Title").GetComponent<TextMeshPro>();
        Text = transform.Find("TextField").GetChild(0).GetComponent<TextMeshPro>();
        Page = transform.Find("SiteIndicator").GetComponent<TextMeshPro>();

        _parent = transform.parent;
        Browser = _parent.GetComponent<DataBrowser>();

        PreviousWord = transform.Find("PreviousWord").GetComponent<InteractiveButton>();
        PreviousWord.OnClick = () =>
        {
            Index--;
            ClassifiedObject obj = SentenceObjects[ActiveSentece][Index].ClassifiedObject;
            UpdateHighlight(false, SentenceObjects[ActiveSentece][Index + 1].ClassifiedObject);
            UpdateHighlight(true, SentenceObjects[ActiveSentece][Index].ClassifiedObject);
            UpdateButtons();
            SearchObject(obj);
        };
        PreviousWord.Active = false;

        NextWord = transform.Find("NextWord").GetComponent<InteractiveButton>();
        NextWord.OnClick = () =>
        {
            Index++;
            ClassifiedObject obj = SentenceObjects[ActiveSentece][Index].ClassifiedObject;
            UpdateHighlight(false, SentenceObjects[ActiveSentece][Index - 1].ClassifiedObject);
            UpdateHighlight(true, SentenceObjects[ActiveSentece][Index].ClassifiedObject);
            UpdateButtons();
            SearchObject(obj);
        };
        NextWord.Active = false;

        ScrollDown = transform.Find("NextSite").GetComponent<InteractiveButton>();
        ScrollDown.OnClick = () =>
        {
            DisplaySentence(++SentenceIndex);
        };
        ScrollDown.Active = false;

        ScrollUp = transform.Find("PreviousSite").GetComponent<InteractiveButton>();
        ScrollUp.OnClick = () =>
        {
            DisplaySentence(--SentenceIndex);
        };
        ScrollUp.Active = false;

        ObjectPlacing = transform.Find("ObjectPlacing").GetComponent<InteractiveButton>();
        ObjectPlacing.OnClick = () =>
        {
            bool isPlaced = text2SceneInterface.PlaceModels(SentenceObjects[ActiveSentece][0].Index, SentenceObjects[ActiveSentece][SentenceObjects[ActiveSentece].Count - 1].Index);
            SentenceObjects[ActiveSentece].ForEach(x => x.IsPlaced = isPlaced);
            UpdateButtons();
        };
        ObjectPlacing.Active = false;

        _visiblePos = transform.localPosition;
        _visibleRot = transform.localRotation;
        _hiddenRot = Quaternion.AngleAxis(-180, Vector3.right);

        transform.localPosition = Vector3.forward * -0.01f;
        transform.localRotation = _hiddenRot;
        transform.localScale = Vector3.one * 0.01f;
    }

    /// <summary>
    /// Highlights the actual IsoSpatialEntity in the text
    /// </summary>
    /// <param name="Highlight"></param>
    /// <param name="word"></param>
    private void UpdateHighlight(bool Highlight, ClassifiedObject word)
    {
        string higlightedWord;
        string textBefore;
        string textAfter;
        int Begin = word.Begin - ActiveSentece.Begin;
        int End = word.End - ActiveSentece.Begin;

        if (Highlight)
        {
            higlightedWord = Text.text.Substring(Begin, End - Begin);
            textBefore = Text.text.Substring(0, Begin);
            textAfter = Text.text.Substring(End);
            Text.text = textBefore + "<color=red>" + higlightedWord + "</color>" + textAfter;
        }
        else
        {
            higlightedWord = Text.text.Substring(Begin + 11, (End + 11) - (Begin + 11));
            textBefore = Text.text.Substring(0, Begin);
            textAfter = Text.text.Substring(End + 19);
            Text.text = textBefore + higlightedWord + textAfter;
        }
    }

    private void SearchObject(ClassifiedObject searchableObject)
    {
        SearchRequest = searchableObject.Praefix + " " + searchableObject.Holonym + " " + searchableObject.DisambiguationWord;
        Search = true;
        Browser.BrowserUpdater?.Invoke();
    }

    private void UpdateButtons()
    {
        if (Index == 0) PreviousWord.Active = false;
        else PreviousWord.Active = true;

        if (SentenceObjects.Count == 0) NextWord.Active = false;
        else if (Index == SentenceObjects[ActiveSentece].Count - 1 || SentenceObjects[ActiveSentece].Count < 2) NextWord.Active = false;
        else NextWord.Active = true;

        if (SentenceIndex >= Sentences.Count - 1) ScrollDown.Active = false;
        else ScrollDown.Active = true;

        if (SentenceIndex <= 0) ScrollUp.Active = false;
        else ScrollUp.Active = true;

        if (text2SceneInterface.ObjectInserted && NN_Helper.isoSpatialEntities.Count > 0)
        {
            if (SentenceObjects.Count == 0) ObjectPlacing.Active = false;
            else if (SentenceObjects[ActiveSentece].TrueForAll(x => x.IsPlaced)) ObjectPlacing.Active = false;
            else ObjectPlacing.Active = true;
        }
        else ObjectPlacing.Active = false;
    }

    /// <summary>
    /// Updates the window depending on th sentence
    /// </summary>
    /// <param name="sentenceIndex"></param>
    public void DisplaySentence(int sentenceIndex)
    {
        if (sentenceIndex >= Sentences.Count || sentenceIndex < 0) return;
        Index = 0;
        ActiveSentece = Sentences[sentenceIndex];
        Text.text = ActiveSentece.TextContent;

        if (SentenceObjects[ActiveSentece].Count > 0)
        {
            SearchObject(SentenceObjects[ActiveSentece][0].ClassifiedObject);
            UpdateHighlight(true, SentenceObjects[ActiveSentece][0].ClassifiedObject);
        }
        Page.text = "Sentence " + (SentenceIndex + 1) + " of " + Sentences.Count;
        UpdateButtons();
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
        text2SceneInterface.checkDocuments();
        IsActive = status;
    }
}

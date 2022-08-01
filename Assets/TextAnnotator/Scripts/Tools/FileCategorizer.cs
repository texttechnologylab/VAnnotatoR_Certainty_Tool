using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
using LitJson;
using HtmlAgilityPack;

public class FileCategorizer : Interface
{
    public const string XML_PATH = "Assets/FileExplorer/_cachedData.xml";
    public const string TEMP_FILE = "Assets/FileExplorer/temp.txt";
    public static string TI_DDC_TOOLS_API = "https://textimager.hucompute.org/DDC/";
    public static string TI_DDC3_MAP = "https://textimager.hucompute.org/DDC/ddc3_map.js";
    public static string TI_DDC_API = "https://textimager.hucompute.org/DDC/api/v3/ddc/classify?ddc=ddc3&language=de";
    public static string LANG_DETECTION = "detectLanguage";
    public static string PARSE = "parse?";
    public const string JSONPARAM_TEXT = "text";
    public const string JSONPARAM_LANG = "lang";
    public const string JSONPARAM_FILENAME = "filename";
    public const string JSONPARAM_DDC = "ddc";
    public const string JSONPARAM_RESULT = "result";
    public const string JSONPARAM_SUCCESS = "success";
    public const string JSONPARAM_LIMIT = "limit";

    public Dictionary<string, string> LANG_TOOL_MAP;
    public static Dictionary<string, string> ID_CATEGORY_MAP;
    public Dictionary<string, string> CATEGORY_ID_MAP;

    public static Dictionary<string, Material> FileFormatIcons = new Dictionary<string, Material>();
    public static Color[] CATEGORY_COLORS = new Color[]
    {
        new Color(31f / 255f, 119f / 255f, 180f / 255f), new Color(255f / 255f, 127f / 255f, 14f / 255f),
        new Color(44f / 255f, 160f / 255f, 44f / 255f), new Color(214f / 255f, 39f / 255f, 40f / 255f),
        new Color(148f / 255f, 103f / 255f, 189f / 255f), new Color(140f / 255f, 86f / 255f, 75f / 255f),
        new Color(227f / 255f, 119f / 255f, 194f / 255f), new Color(127f / 255f, 127f / 255f, 127f / 255f),
        new Color(188f / 255f, 189f / 255f, 34f / 255f), new Color(23f / 255f, 190f / 255f, 207f / 255f)
    };

    public Dictionary<string, bool> CATEGORY_NAMES;

    public Dictionary<string, TextFileInfo> TextFileInfos;
    public Dictionary<string, TextFileInfo> TextComponentInfos;
    public HashSet<string> TextFound;
    //private Queue<VRData> Queue;
    private Queue<AnnotationBase> Queue;

    public bool _parsing { get; private set; }
    public bool _diffDetected = false;
    public bool _savingTools = false;
    public bool _metaLoaded = false;
    public bool _toolsLoaded = false;
    public bool _toolsUpdated { get; private set; }
    private bool _queueChanged = false;
    private bool _catProc = false;
    private bool _queueProcEnd = false;
    private bool _relictsRemoved = false;
    private bool _removingRelicts = false;
    private bool _saved = false;
    private int _procFiles = 0;
    private string parsedText;
    private JsonData data;
    private string label;
    private UnityWebRequest request;
    private byte[] encodedText;
    private byte[] fileInBytes;
    private byte[] boundary;
    private byte[] formSections;
    private byte[] terminate;
    private byte[] body;
    private List<IMultipartFormSection> formData;
    private List<string> res;
    private XmlWriter writer;
    private WaitForEndOfFrame _wait;
    //private VRResourceData castedResource;
    private Thread CachingThread;


    protected override IEnumerator InitializeInternal()
    {
        //StolperwegeHelper.categorizer = this;
        TextFileInfos = new Dictionary<string, TextFileInfo>();
        TextComponentInfos = new Dictionary<string, TextFileInfo>();
        TextFound = new HashSet<string>();
        Queue = new Queue<AnnotationBase>();
        _wait = new WaitForEndOfFrame();
        yield return StartCoroutine(ParseAPI());
    }

    private void Update()
    {
        //if (_toolsUpdated && _metaLoaded && !StolperwegeHelper.cache.IsSavingDDC)
        //{
        //    _toolsUpdated = false;
        //    _savingTools = true;
        //    StolperwegeHelper.cache.CacheDDC();
        //}

        //if (_toolsLoaded && _metaLoaded && Queue.Count > 0 && !_catProc && !_savingTools && !StolperwegeHelper.fileExplorer.CreatingCity)
        //{
        //    _catProc = true;
        //    AnnotationBase data = Queue.Dequeue();
        //    StartCoroutine(GetCategorization(data));
        //}

        //if (_queueChanged && _queueProcEnd && !_catProc && !StolperwegeHelper.cache.IsSavingFiles && !_saved)
        //{
        //    _procFiles = 0;
        //    _saved = true;
        //    _queueChanged = false;
        //    StolperwegeHelper.cache.CacheCategorizedFiles();
        //}

        //if (_queueChanged && _procFiles == 5 && !StolperwegeHelper.cache.IsSavingFiles)
        //{
        //    _procFiles = 0;
        //    StolperwegeHelper.cache.CacheCategorizedFiles();
        //}

    }

    public void EnqueueTextFile(AnnotationBase annotationBase)
    {
        _relictsRemoved = false;
        Queue.Enqueue(annotationBase);
        _queueChanged = true;
    }

    public IEnumerator GetCategorization(AnnotationBase annoBase)
    {
        // parse the text of the file first to get the preview
        parsedText = annoBase.TextContent;
        //if (parsedText == "" && annoBase is VRResourceData)
        //{
        //    castedResource = (VRResourceData)annoBase;
        //    parsedText = TextParser.ParseTextFromVRResourceData(castedResource);
        //}

        // show first, if there is maybe already a metadata for the file
        // if the file could be found and the last change date is equals to the stored one
        // write the stored infos in the VRData and return

        if (annoBase.CategoryID == "")
        {
            TextFileInfo info = null;
            if (TextComponentInfos.ContainsKey("" + annoBase.ID))
                info = TextComponentInfos["" + annoBase.ID];

            if (info != null && info.LastChanged.ToString().Equals(annoBase.TextFileLastChangedOn.ToString()))
            {
                annoBase.Language = info.Language;
                annoBase.CategoryID = info.CategoryID;

                TextFound.Add("" + annoBase.ID);
                _catProc = false;


                yield break;
            }
            //if (annoBase is VRResourceData)
            //{
            //    castedResource = (VRResourceData)annoBase;
            //    TextFileInfo info = null;
            //    if (TextFileInfos.ContainsKey(castedResource.Path))
            //        info = TextFileInfos[castedResource.Path];

            //    if (info != null && info.LastChanged.ToString().Equals(castedResource.LastChange.ToString()))
            //    {
            //        annoBase.Language = info.Language;
            //        annoBase.CategoryID = info.CategoryID;

            //        TextFound.Add(castedResource.Path);
            //        _catProc = false;
            //        yield break;
            //    }
            //}
            //else
            //{

            //    castedAnnotationBase = (VRTextData)annoBase;

            //    TextFileInfo info = null;
            //    if (TextComponentInfos.ContainsKey("" + castedAnnotationBase.ID))
            //        info = TextComponentInfos["" + castedAnnotationBase.ID];

            //    if (info != null && info.LastChanged.ToString().Equals(castedAnnotationBase.TextFileLastChangedOn.ToString()))
            //    {
            //        UnityEngine.Debug.Log("Drin");
            //        annoBase.Language = info.Language;
            //        annoBase.CategoryID = info.CategoryID;

            //        TextFound.Add("" + castedAnnotationBase.ID);
            //        _catProc = false;


            //        yield break;
            //    }
            //}

        }


        if (parsedText == "")
        {
            _catProc = false;
            yield break;
        }

        annoBase.Language = "de";

        if (parsedText.Length > 100) parsedText = parsedText.Substring(0, 100);

        request = new UnityWebRequest(TI_DDC_API + "&inputText=" + parsedText);

        encodedText = Encoding.UTF8.GetBytes(parsedText);
        formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormFileSection("file", encodedText, "temp.txt", "multipart/form-data"));

        boundary = UnityWebRequest.GenerateBoundary();
        formSections = UnityWebRequest.SerializeFormSections(formData, boundary);
        terminate = Encoding.UTF8.GetBytes(string.Concat("\r\n--", Encoding.UTF8.GetString(boundary), "--"));

        body = new byte[formSections.Length + terminate.Length];
        Buffer.BlockCopy(formSections, 0, body, 0, formSections.Length);
        Buffer.BlockCopy(terminate, 0, body, formSections.Length, terminate.Length);

        request = new UnityWebRequest(TI_DDC_API + "&inputText=" + parsedText);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.uploadHandler.contentType = string.Concat("multipart/form-data; boundary=", Encoding.UTF8.GetString(boundary));
        request.method = "POST";
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.isHttpError || request.isNetworkError)
        {
            Debug.Log(request.error);
            Debug.Log(parsedText);
            _catProc = false;
            yield break;
        }


        data = null;
        label = "none";
        try
        {
            data = JsonMapper.ToObject(request.downloadHandler.text);
        }
        catch (JsonException) { Debug.Log("Json could not be loaded."); }

        Debug.Log(request.downloadHandler.text);

        if (data != null)
        {
            data = data[JSONPARAM_DDC][0];
            label = data["label"].ToString().Replace("__label_ddc__", "");
        }

        annoBase.CategoryID = label;

        //store the infos in the dictionary

        if (TextComponentInfos.ContainsKey("" + annoBase.ID))
            TextComponentInfos["" + annoBase.ID] = new TextFileInfo(annoBase.Language, annoBase.CategoryID, annoBase.TextFileLastChangedOn);
        else
            TextComponentInfos.Add("" + annoBase.ID, new TextFileInfo(annoBase.Language, annoBase.CategoryID, annoBase.TextFileLastChangedOn));

        TextFound.Add("" + annoBase.ID);

        //if (annoBase is VRResourceData)
        //{
        //    castedResource = (VRResourceData)annoBase;
        //    if (TextFileInfos.ContainsKey(castedResource.Path))
        //        TextFileInfos[castedResource.Path] = new TextFileInfo(annoBase.Language, annoBase.CategoryID, castedResource.LastChange);
        //    else
        //        TextFileInfos.Add(castedResource.Path, new TextFileInfo(annoBase.Language, annoBase.CategoryID, castedResource.LastChange));

        //    TextFound.Add(castedResource.Path);
        //}
        //else
        //{
        //    castedAnnotationBase = (VRTextData)annoBase;
        //    if (TextComponentInfos.ContainsKey("" + castedAnnotationBase.ID))
        //        TextComponentInfos["" + castedAnnotationBase.ID] = new TextFileInfo(annoBase.Language, annoBase.CategoryID, castedAnnotationBase.TextFileLastChangedOn);
        //    else
        //        TextComponentInfos.Add("" + castedAnnotationBase.ID, new TextFileInfo(annoBase.Language, annoBase.CategoryID, castedAnnotationBase.TextFileLastChangedOn));

        //    TextFound.Add("" + castedAnnotationBase.ID);
        //}

        _procFiles += 1;

        _diffDetected = true;
        _catProc = false;
    }

    public delegate void OnLanguageDetected(string res);
    public IEnumerator DetectTextLanguage(string text, OnLanguageDetected onDet)
    {

        UnityWebRequest request = new UnityWebRequest(TI_DDC_TOOLS_API + LANG_DETECTION, "POST");
        request.SetRequestHeader("Content-Type", "application/json");

        string jsonString = @"{""inputText"": """ + text + "\"}";

        byte[] bytes = Encoding.ASCII.GetBytes(jsonString);
        request.uploadHandler = new UploadHandlerRaw(bytes);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        JsonData data = JsonMapper.ToObject(request.downloadHandler.text);

        List<string> results = new List<string>(data.Keys);

        onDet(results[0]);
    }

    public IEnumerator ParseAPI()
    {
        _parsing = true;

        // get the tools
        UnityWebRequest www = new UnityWebRequest(TI_DDC_TOOLS_API);
        www.method = "GET";
        www.downloadHandler = new DownloadHandlerBuffer();

        ID_CATEGORY_MAP = new Dictionary<string, string>();
        CATEGORY_ID_MAP = new Dictionary<string, string>();
        LANG_TOOL_MAP = new Dictionary<string, string>();

        yield return www.SendWebRequest();

        if (www.isHttpError || www.isNetworkError)
        {
            Debug.Log("Could not load data from API.");
            Debug.Log(www.error);
            _parsing = false;
            yield break;
        }

        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(www.downloadHandler.text);

        HtmlNodeCollection scriptNodes = doc.DocumentNode.SelectNodes("//head/script");

        string infos = null;

        for (int n = 0; n < scriptNodes.Count; n++)
        {
            if (scriptNodes[n].InnerText.Contains("var fasttext_annotator_options = "))
            {
                infos = scriptNodes[n].InnerText;
                break;
            }
        }

        if (infos == null)
        {
            Debug.Log("Could not found the DDC Tools on the API.");
            _parsing = false;
            yield break;
        }

        int toolsInd = infos.IndexOf("var fasttext_annotator_options = ");

        string strBatch = infos.Substring(toolsInd);

        int ind1 = strBatch.IndexOf('{');
        int ind2 = strBatch.IndexOf("};");

        strBatch = strBatch.Substring(ind1 + 1, ind2 - (ind1 + 1));

        string[] tools = strBatch.Replace("},", "*").Replace("\n", "").Replace("\r", "").Replace("\t", "").Split('*');

        int counter = 0;
        foreach (string tool in tools)
        {

            if (tool.Substring(0, 10).Contains("ddc2"))
                continue;

            string pipeline = tool.Replace("\"pipeline\":", "*").Split('*')[1];

            pipeline = pipeline.Replace("[{", "").Replace("}]", "").Replace(" ", "");
            string[] keyValue = pipeline.Split(':');
            string lang = keyValue[0].Replace("\"", "");
            ind1 = keyValue[1].IndexOf("[");
            ind2 = keyValue[1].IndexOf("]");
            string toolList = keyValue[1].Substring(ind1, ind2 - ind1 + 1);
            LANG_TOOL_MAP.Add(lang, toolList);
            if (counter++ == 10)
            {
                counter = 0;
                yield return _wait;
            }
        }

        // get categories

        UnityWebRequest site = new UnityWebRequest(TI_DDC3_MAP);
        site.downloadHandler = new DownloadHandlerBuffer();

        yield return site.SendWebRequest();

        int catIDInd = infos.IndexOf("var ddc3_map = ");

        strBatch = site.downloadHandler.text;

        strBatch = strBatch.Split('{')[1].Split('}')[0].Replace("\n", "").Replace("\r", "").Replace("\t", "");

        yield return _wait;

        string[] categories = strBatch.Replace("\",", "*").Split('*');

        CATEGORY_NAMES = new Dictionary<string, bool>();

        foreach (string cat in categories)
        {
            if (cat == "") continue;
            string[] keyValue = cat.Replace("\"", "").Replace(": ", "*").Split('*');
            if (keyValue.Length < 2 || keyValue[1].Contains("Unassigned") ||
                CATEGORY_ID_MAP.ContainsKey(keyValue[1])) continue;
            string id = keyValue[0].Replace(" ", "");
            ID_CATEGORY_MAP.Add(id, keyValue[1]);
            CATEGORY_ID_MAP.Add(keyValue[1], id);
            if (id.EndsWith("00"))
                CATEGORY_NAMES.Add(keyValue[1], true);
        }

        yield return _wait;

        _parsing = false;
        _toolsLoaded = true;
        _toolsUpdated = true;
    }

    private void OnDestroy()
    {
        if (CachingThread != null && CachingThread.IsAlive)
            CachingThread.Abort();
    }
}

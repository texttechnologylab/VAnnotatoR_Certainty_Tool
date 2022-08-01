using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using LitJson;
using System.Linq;
using static StolperwegeHelper;
using Text2Scene;

public class ShapeNetInterface : Interface
{
    public const string WS = "http://shapenet.texttechnologylab.org/";
    private const string CACHE_DIR = "\\Documents\\text2scene\\";

    public const string MAIN_CATEGORIES = "Main categories";
    public const string SUB_CATEGORIES = "Subcategories";

    // MODELS
    public const string MODELS = WS + "loadedobjects";
    public const string OBJECT_THUMBNAILS = WS + "thumbnails";
    public const string OBJECT_THUMBNAIL_INFO = WS + "thumbnailsInfos";
    public const string OBJECT_TAXONOMY = WS + "taxonomy";
    public const string GET_OBJECT_ID = WS + "get?id=";
    public const string SEARCH_OBJECTS = WS + "search?search=";

    private const string CACHED_OBJECT_DIR = CACHE_DIR + "objects\\";
    private const string CACHED_OBJECT_FILES = CACHED_OBJECT_DIR + "models\\";
    private const string CACHED_OBJECT_THUMBNAILS = CACHED_OBJECT_DIR + "thumbnails\\";
    public const string FORMATTED_MODEL_NAME = "ShapeNet Models";
    

    private bool _objectListLoaded;
    private string _objectListError;
    private bool _objectTaxonomyLoaded;
    private string _objectTaxonomyError;
    private bool _objectThumbnailsActualized;
    private string _objectThumbnailError;

    public Dictionary<string, ShapeNetModel> ShapeNetModels;
    public Dictionary<string, ShapeNetTaxonomyEntry> ModelTaxonomies;    
    public Dictionary<string, string> ObjectSubCategoryMap;
    public Dictionary<string, InteractiveCheckbox.CheckboxStatus> ModelMainCategories;
    public Dictionary<string, InteractiveCheckbox.CheckboxStatus> ModelSubCategories;
    public Dictionary<string, string> CachedObjectPathMap { get; private set; }

    // PARTS
    //public const string LOADED_PARTS = WS + "loadedPartObjects";
    //public const string PART_THUMBNAILS = WS + "thumbnails";
    //public const string OBJECT_THUMBNAIL_TIMESTAMP = WS + "thumbnailsTimestamp";
    //public const string OBJECT_TAXONOMY = WS + "taxonomy";
    //public const string GET_OBJECT_ID = WS + "get?id=";

    //private const string CACHED_OBJECT_DIR = CACHE_DIR + "objects\\";
    //private const string CACHED_OBJECT_MODELS = CACHED_OBJECT_DIR + "models\\";
    //private const string CACHED_OBJECT_THUMBNAILS = CACHED_OBJECT_DIR + "thumbnails\\";

    //private bool _objectListLoaded;
    //private string _objectListError;
    //private bool _objectTaxonomyLoaded;
    //private string _objectTaxonomyError;
    //private bool _objectThumbnailsActualized;
    //private string _objectThumbnailError;

    //public Dictionary<string, ShapeNetModel> ShapeNetObjects;
    //public Dictionary<string, ShapeNetTaxonomyEntry> ObjectTaxonomies;
    //public Dictionary<string, bool> ObjectMainCategories;
    //public Dictionary<string, bool> ObjectSubCategories;
    //public Dictionary<string, string> CachedObjectPathMap { get; private set; }


    // TEXTURES
    public const string TEXTURES = WS + "loadedTextures";
    public const string TEXTURE_THUMBNAILS = WS + "textureThumbnails";
    public const string TEXTURE_THUMBNAIL_INFO = WS + "textureThumbnailsInfos";
    public const string TEXTURE_TAXONOMY = WS + "textureTaxonomy";
    public const string GET_TEXTURE_ID = WS + "getTexture?id=";

    private const string CACHED_TEXTURE_DIR = CACHE_DIR + "textures\\";
    private const string CACHED_TEXTURE_FILES = CACHED_TEXTURE_DIR + "textureFiles\\";
    private const string CACHED_TEXTURE_THUMBNAILS = CACHED_TEXTURE_DIR + "thumbnails\\";
    public const string FORMATTED_TEXTURE_NAME = "ShapeNet Textures";

    private bool _textureListLoaded;
    private string _textureListError;
    private bool _textureTaxonomyLoaded;
    private string _textureTaxonomyError;
    private bool _textureThumbnailsActualized;
    private string _textureThumbnailError;

    public Dictionary<string, ShapeNetTexture> ShapeNetTextures;
    public Dictionary<string, ShapeNetTaxonomyEntry> TextureTaxonomies;
    public Dictionary<string, string> TextureSubCategoryMap;
    public Dictionary<string, InteractiveCheckbox.CheckboxStatus> TextureMainCategories;
    public Dictionary<string, InteractiveCheckbox.CheckboxStatus> TextureSubCategories;
    
    public Dictionary<string, string> CachedTexturePathMap { get; private set; }


    private const string ThumbnailZip = "thumbnails.zip";
    private const string ThumbnailInfosOld = "thumbnailLastUpdate.txt";
    private const string ThumbnailInfos = "thumbnailInfos.json";
    private string _path;
    private byte[] bytes;

    private DirectoryInfo dir;
    private FileStream fileStream;
    private StreamReader streamReader;
    private UnityWebRequest request;
    private FastZip zip;
    private Thread _unzippingThread;
    private FileInfo[] ActualFiles;
    private long ActualSize;
    private long UnzippedSize;
    private long StoredSize;
    private long JSONSize;
    private long StoredTimestamp;
    private long JSONtimestamp;
    private JsonData storedInfos;
    private JsonData data;
    private JsonData objectList;
    private ShapeNetModel shapeNetModel;
    private ShapeNetTexture shapeNetTexture;
    public string InitStatus;
    private static Queue<TextureRequest> TextureQueue;
    //private bool _textureDownloadInProgress = false;

    private Text2SceneHandler handler;

    public bool Initialized { get; private set; }    

    public struct TextureRequest
    {

        public string ID { get; private set; }
        public OnObjectLoaded Event { get; private set; }
        public TextureRequest(string id, OnObjectLoaded onLoaded) 
        {
            ID = id;
            Event = onLoaded;
        }
    }

    public delegate void OnThumbnailLoaded();
    public delegate void OnObjectLoaded(string filePath);
    //public void Awake()
    //{
    //    StolperwegeHelper.shapeNetInterface = this;        
    //    StartCoroutine(Initialize());
    //}

    protected override IEnumerator InitializeInternal()
    {
        Name = "ShapeNet";
        OnSetupBrowser = SetupBrowser;
        TextureQueue = new Queue<TextureRequest>();

        ShapeNetModels = new Dictionary<string, ShapeNetModel>();
        ModelTaxonomies = new Dictionary<string, ShapeNetTaxonomyEntry>();
        ModelMainCategories = new Dictionary<string, InteractiveCheckbox.CheckboxStatus>();
        ModelSubCategories = new Dictionary<string, InteractiveCheckbox.CheckboxStatus>();
        ObjectSubCategoryMap = new Dictionary<string, string>();
        _objectListLoaded = false;
        _objectTaxonomyLoaded = false;
        _objectThumbnailsActualized = false;
        _objectListError = null;
        _objectTaxonomyError = null;
        _objectThumbnailError = null;

        ShapeNetTextures = new Dictionary<string, ShapeNetTexture>();
        TextureTaxonomies = new Dictionary<string, ShapeNetTaxonomyEntry>();
        TextureMainCategories = new Dictionary<string, InteractiveCheckbox.CheckboxStatus>();
        TextureSubCategories = new Dictionary<string, InteractiveCheckbox.CheckboxStatus>();
        TextureSubCategoryMap = new Dictionary<string, string>();
        _textureListLoaded = false;
        _textureTaxonomyLoaded = false;
        _textureThumbnailsActualized = false;
        _textureListError = null;
        _textureTaxonomyError = null;
        _textureThumbnailError = null;

        InitializeCache();

        if (!_objectListLoaded)
        {
            InitStatus = "Loading Shapenet-Model-List...";
            yield return StartCoroutine(LoadModelList());
            if (_objectListError == null) InitStatus = "ShapeNet objects loaded.";
            else InitStatus = "ShapeNet objects cannot be loaded: " + _objectListError;
            _objectListLoaded = _objectListError == null;
        }
        
        if (!_objectTaxonomyLoaded)
        {
            InitStatus = "Loading Shapenet-Model-Taxonomies...";
            yield return StartCoroutine(LoadObjectTaxonomy());
            if (_objectTaxonomyError == null) InitStatus = "ShapeNet object-taxonomy loaded.";
            else InitStatus = "ShapeNet object-taxonomy cannot be loaded: " + _objectTaxonomyError;
            _objectTaxonomyLoaded = _objectTaxonomyError == null;
        }
        
        if (!_objectThumbnailsActualized)
        {
            InitStatus = "Loading Shapenet-Model-Thumbnails...";
            yield return StartCoroutine(CheckThumbnails(CACHED_OBJECT_DIR, CACHED_OBJECT_THUMBNAILS, CACHED_OBJECT_FILES, OBJECT_THUMBNAIL_INFO, OBJECT_THUMBNAILS, "Object"));
            if (_objectThumbnailError == null) InitStatus = "ShapeNet object-thumbnails actualized.";
            else InitStatus = "ShapeNet object-thumbnails cannot be actualized: " + _objectThumbnailError;
            _objectThumbnailsActualized = _objectThumbnailError == null;
        }

        if (!_textureListLoaded)
        {
            InitStatus = "Loading Shapenet-Texture-List...";
            yield return StartCoroutine(LoadTextureList());
            if (_textureListError == null) InitStatus = "ShapeNet textures loaded.";
            else InitStatus = "ShapeNet textures cannot be loaded: " + _textureListError;
            _textureListLoaded = _textureListError == null;
        }

        if (!_textureTaxonomyLoaded)
        {
            InitStatus = "Loading Shapenet-Texture-Taxonomies...";
            yield return StartCoroutine(LoadTextureTaxonomy());
            if (_textureTaxonomyError == null) InitStatus = "ShapeNet texture-taxonomy loaded.";
            else InitStatus = "ShapeNet texture-taxonomy cannot be loaded: " + _textureTaxonomyError;
            _textureTaxonomyLoaded = _textureTaxonomyError == null;
        }

        if (!_textureThumbnailsActualized)
        {
            InitStatus = "Loading Shapenet-Texture-Thumbnails...";
            yield return StartCoroutine(CheckThumbnails(CACHED_TEXTURE_DIR, CACHED_TEXTURE_THUMBNAILS, CACHED_TEXTURE_FILES, TEXTURE_THUMBNAIL_INFO,TEXTURE_THUMBNAILS, "Texture"));
            if (_textureThumbnailError == null) InitStatus = "ShapeNet texture-thumbnails actualized.";
            else InitStatus = "ShapeNet texture-thumbnails cannot be actualized: " + _textureThumbnailError;
            _textureThumbnailsActualized = _textureThumbnailError == null;
        }

        if (_objectListError == null && _objectTaxonomyError == null && _objectThumbnailError == null &&
            _textureListError == null && _textureTaxonomyError == null && _textureThumbnailError == null)
        {
            InitStatus = "Init done.";
            Initialized = true;
        } else
        {
            InitStatus = "Init failed. Retry...";
            StartCoroutine(InitializeInternal());
        }
    }

    private void InitializeCache()
    {
        InitStatus = "Initializing cached model map...";
        _path = UserFolder + CACHED_OBJECT_FILES;
        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);
        CachedObjectPathMap = new Dictionary<string, string>();
        string[] paths = Directory.GetDirectories(_path);
        for (int i = 0; i < paths.Length; i++)
        {
            dir = new DirectoryInfo(paths[i]);
            CachedObjectPathMap.Add(dir.Name, dir.FullName);
        }

        InitStatus = "Initializing cached texture map...";
        _path = UserFolder + CACHED_TEXTURE_FILES;
        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);
        CachedTexturePathMap = new Dictionary<string, string>();
        paths = Directory.GetDirectories(_path);
        for (int i = 0; i < paths.Length; i++)
        {
            dir = new DirectoryInfo(paths[i]);
            CachedTexturePathMap.Add(dir.Name, dir.FullName);
        }
    }

    /*
    public void Update()
    {
        //Debug.Log((TextureQueue != null) + ((TextureQueue != null) ? " " + TextureQueue.Count : ""));
        if (TextureQueue != null && TextureQueue.Count > 0 && !_textureDownloadInProgress)
            StartCoroutine(GetTexture());        
            
    }*/

    private IEnumerator LoadModelList()
    {
        request = UnityWebRequest.Get(MODELS);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            _objectListError = request.error;
        else
        {
            data = JsonMapper.ToObject(request.downloadHandler.text);
            if (!data.Keys.Contains("success") || !bool.Parse(data["success"].ToString()) ||
                !data.Keys.Contains("ShapeNetObj"))
            {
                _objectListError = "Downloading object list failed.";
                yield break;
            }
            objectList = data["ShapeNetObj"];
            for (int i = 0; i < objectList.Count; i++)
            {
                if (!objectList[i].Keys.Contains("id"))
                    Debug.Log("Object without id.");
                else
                {
                    shapeNetModel = new ShapeNetModel(objectList[i]);
                    ShapeNetModels.Add((string)shapeNetModel.ID, shapeNetModel);
                }

            }
        }
    }

    private IEnumerator LoadTextureList()
    {
        request = UnityWebRequest.Get(TEXTURES);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            _textureListError = request.error;
        else
        {
            data = JsonMapper.ToObject(request.downloadHandler.text);
            if (!data.Keys.Contains("success") || !bool.Parse(data["success"].ToString()) ||
                !data.Keys.Contains("Textures"))
            {
                _textureListError = "Downloading texture list failed.";
                yield break;
            }
            objectList = data["Textures"];
            for (int i = 0; i < objectList.Count; i++)
            {
                if (!objectList[i].Keys.Contains("id"))
                    Debug.Log("Object without id.");
                else
                {
                    shapeNetTexture = new ShapeNetTexture(objectList[i]);
                    ShapeNetTextures.Add((string)shapeNetTexture.ID, shapeNetTexture);
                }

            }
        }
    }

    string taxonomyName; JsonData taxonomyObject; List<string> keys;
    private IEnumerator LoadObjectTaxonomy()
    {
        request = UnityWebRequest.Get(OBJECT_TAXONOMY);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            _objectTaxonomyError = request.error;
        else
        {            
            data = JsonMapper.ToObject(request.downloadHandler.text);
            if (!data.Keys.Contains("success") || !bool.Parse(data["success"].ToString()) ||
                !data.Keys.Contains("taxonomy"))
            {
                _objectTaxonomyError = "Downloading object taxonomy failed.";
                yield break;
            }
            objectList = data["taxonomy"];
            for (int i = 0; i < objectList.Count; i++)
            {
                taxonomyObject = objectList[i];
                keys = new List<string>(taxonomyObject.Keys);
                taxonomyName = keys[0];
                if (!ModelMainCategories.ContainsKey(taxonomyName))
                {
                    ModelTaxonomies.Add(taxonomyName, new ShapeNetTaxonomyEntry(taxonomyName, ShapeNetTaxonomyEntry.TaxonomyType.Object, taxonomyObject[taxonomyName], this));
                    ModelMainCategories.Add(taxonomyName, InteractiveCheckbox.CheckboxStatus.AllChecked);
                }
            }
        }
    }

    private IEnumerator LoadTextureTaxonomy()
    {
        request = UnityWebRequest.Get(TEXTURE_TAXONOMY);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            _textureTaxonomyError = request.error;
        else
        {
            data = JsonMapper.ToObject(request.downloadHandler.text);
            if (!data.Keys.Contains("success") || !bool.Parse(data["success"].ToString()) ||
                !data.Keys.Contains("taxonomy"))
            {
                _textureTaxonomyError = "Downloading texture taxonomy failed.";
                yield break;
            }
            objectList = data["taxonomy"];
            for (int i=0; i<objectList.Count; i++)
            {
                taxonomyObject = objectList[i];
                keys = new List<string>(taxonomyObject.Keys);
                taxonomyName = keys[0];
                if (!TextureMainCategories.ContainsKey(taxonomyName))
                {
                    TextureTaxonomies.Add(taxonomyName, new ShapeNetTaxonomyEntry(taxonomyName, ShapeNetTaxonomyEntry.TaxonomyType.Texture, taxonomyObject[taxonomyName], this));
                    TextureMainCategories.Add(taxonomyName, InteractiveCheckbox.CheckboxStatus.AllChecked);
                }
            }
            
        }
    }

    public static IEnumerator LoadThumbnail(ShapeNetObject sObj, OnThumbnailLoaded onThumbnail)
    {
        string url = "file://" + UserFolder;
        if (sObj is ShapeNetModel) url += CACHED_OBJECT_THUMBNAILS + sObj.ID + "-7.png";
        if (sObj is ShapeNetTexture) url += CACHED_TEXTURE_THUMBNAILS + ((ShapeNetTexture)sObj).ThumbnailFileName;
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.error == null)
        {
            sObj.Thumbnail = DownloadHandlerTexture.GetContent(request);
            onThumbnail?.Invoke();
        }      
    }

    private IEnumerator CheckThumbnails(string cacheFolder, string thumbnailCacheFolder, string dataCacheFolder, string infoURL, string thumbnailURL, string thumbnailType)
    {
        _path = UserFolder + thumbnailCacheFolder;
        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);

        bool update = false;
        request = UnityWebRequest.Get(infoURL);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            _objectThumbnailError = request.error;
        else
        {

            // getting the timestamp of thumbnails from server
            data = JsonMapper.ToObject(request.downloadHandler.text);
            if (!data.Keys.Contains("success") || !bool.Parse(data["success"].ToString()) ||
                !data.Keys.Contains("timestamp") || !data.Keys.Contains("size"))
            {
                _objectThumbnailError = "Downloading " + thumbnailType.ToLower() + " timestamp failed.";
                yield break;
            }

            JSONtimestamp = long.Parse(data["timestamp"].ToString());
            JSONSize = long.Parse(data["size"].ToString());

            // get rid of deprecated file
            _path = UserFolder + cacheFolder + ThumbnailInfosOld;
            if (File.Exists(_path))
                File.Delete(_path);
            // creating the timestamp file if needed, otherwise comparing the timestamps
            _path = UserFolder + cacheFolder + ThumbnailInfos;
            if (!File.Exists(_path))
            {
                Debug.Log(thumbnailType + " timestamp-file missing. Updating " + thumbnailType.ToLower() + " thumbnails...");
                fileStream = new FileStream(_path, FileMode.Create);
                update = true;
            } else
            {
                try { 
                    fileStream = new FileStream(_path, FileMode.Open);
                    streamReader = new StreamReader(fileStream);
                    storedInfos = JsonMapper.ToObject(streamReader.ReadToEnd());
                    streamReader.Close();
                    fileStream.Close();
                    ActualSize = 0;
                    ActualFiles = new DirectoryInfo(UserFolder + thumbnailCacheFolder).GetFiles();
                    for (int i = 0; i < ActualFiles.Length; i++)
                        ActualSize += ActualFiles[i].Length;
                    update = !long.TryParse(storedInfos["timestamp"].ToString(), out StoredTimestamp) || JSONtimestamp != StoredTimestamp ||
                             !long.TryParse(storedInfos["size"].ToString(), out StoredSize) || JSONSize != StoredSize ||
                             !long.TryParse(storedInfos["unzippedSize"].ToString(), out UnzippedSize) || UnzippedSize != ActualSize;

                    if (update) Debug.Log(thumbnailType + " thumbnails out of date. Updating " + thumbnailType.ToLower() + " thumbnails...");
                    else Debug.Log(thumbnailType + " thumbnails up-to-date.");
                }
                catch (Exception e)
                {
                    Debug.Log("Could not parse Thumbnails. Updating " + thumbnailType.ToLower() + " thumbnails...");
                    fileStream = new FileStream(_path, FileMode.Create);
                    update = true;
                }
            }

            if (update)
            {
                _path = UserFolder + thumbnailCacheFolder;
                request = UnityWebRequest.Get(thumbnailURL);
                request.SendWebRequest();
                while (!request.isDone)
                {
                    InitStatus = "Downloading " + thumbnailType.ToLower() + " thumbnails:\n" +
                                     (int)(request.downloadedBytes / Mathf.Pow(10, 6) * 100) / 100f + " MB of " +
                                     (int)(JSONSize / Mathf.Pow(10, 6) * 100) / 100f + " MB";
                    yield return null;
                }
                InitStatus = "Downloading " + thumbnailType.ToLower() + " thumbnails:\n" +
                                     (int)(request.downloadedBytes / Mathf.Pow(10, 6) * 100) / 100f + " MB of " +
                                     (int)(JSONSize / Mathf.Pow(10, 6) * 100) / 100f + " MB";
                if (request.isNetworkError || request.isHttpError)
                    _objectThumbnailError = request.error;
                else
                {
                    InitStatus = "Unzipping " + thumbnailType.ToLower() + " thumbnails...";
                    fileStream = new FileStream(_path + ThumbnailZip, FileMode.Create);
                    fileStream.Write(request.downloadHandler.data, 0, request.downloadHandler.data.Length);
                    fileStream.Close();
                    _unzippingThread = new Thread(() => { UnzipFile(_path + ThumbnailZip, _path); });
                    _unzippingThread.Start();
                    while (_unzippingThread.IsAlive)
                        yield return null;
                    File.Delete(_path + ThumbnailZip);
                }
                ActualSize = 0;
                ActualFiles = new DirectoryInfo(UserFolder + thumbnailCacheFolder).GetFiles();
                for (int i = 0; i < ActualFiles.Length; i++)
                    ActualSize += ActualFiles[i].Length;
                storedInfos = new JsonData();
                storedInfos["timestamp"] = data["timestamp"];
                storedInfos["size"] = data["size"];
                storedInfos["unzippedSize"] = ActualSize;
                bytes = System.Text.Encoding.UTF8.GetBytes(storedInfos.ToJson());
                _path = UserFolder + cacheFolder + ThumbnailInfos;
                fileStream = new FileStream(_path, FileMode.Open);
                fileStream.SetLength(0);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Close();
            }

            if (streamReader != null) streamReader.Close();
            if (fileStream != null) fileStream.Close();
        }
        
    }

    private void UnzipFile(string filePath, string targetDir)
    {
        FastZip zip = new FastZip();
        zip.ExtractZip(filePath, targetDir, null);        
    }

    public IEnumerator GetModel(string id, OnObjectLoaded onLoaded)
    {
        _path = UserFolder + CACHED_OBJECT_FILES;
        
        if (CachedObjectPathMap.ContainsKey(id))
        {
            onLoaded(CachedObjectPathMap[id]);
            Debug.Log("Searched model was cached.");
            yield break;

        }

        Debug.Log("Searched model was not cached, downloading it...");
        request = UnityWebRequest.Get(GET_OBJECT_ID + id);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
            Debug.Log(request.error);
        else
        {
            if (!Directory.Exists(_path + id))
                Directory.CreateDirectory(_path + id);
            string zipFile = _path + id + "\\" + id + ".zip";
            fileStream = new FileStream(zipFile, FileMode.Create); 
            fileStream.Write(request.downloadHandler.data, 0, request.downloadHandler.data.Length);
            fileStream.Close();
            _unzippingThread = new Thread(() => { UnzipFile(zipFile, _path + id); });
            _unzippingThread.Start();
            while (_unzippingThread.IsAlive)
                yield return null;
            try
            {
                CachedObjectPathMap.Add(id, _path + id);
            }
            catch
            {

            }
            //File.Delete(zipFile);
            onLoaded(CachedObjectPathMap[id]);
        }
    }

    public static void GetModel()
    {

    }

    public static void RequestTexture(string id, OnObjectLoaded onLoaded)
    {
        TextureRequest req = new TextureRequest(id, onLoaded);
        Debug.Log("Enqueued: " + id);
        TextureQueue.Enqueue(req);
    }

    /*
    TextureRequest tRequest;
    private IEnumerator GetTexture()
    {
        Debug.Log("LOADING TEXTURE");
        _textureDownloadInProgress = true;
        tRequest = TextureQueue.Dequeue();
        _path = UserFolder + CACHED_TEXTURE_FILES;

        if (CachedTexturePathMap.ContainsKey(tRequest.ID))
        {
            tRequest.Event?.Invoke(CachedTexturePathMap[tRequest.ID]);
            Debug.Log("Searched texture was cached.");
            _textureDownloadInProgress = false;
            yield break;

        }

        Debug.Log("Searched texture was not cached, downloading it...");
        request = UnityWebRequest.Get(GET_TEXTURE_ID + tRequest.ID);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
            Debug.Log(request.error);
        else
        {
            if (!Directory.Exists(_path + tRequest.ID))
                Directory.CreateDirectory(_path + tRequest.ID);
            string zipFile = _path + tRequest.ID + "\\" + tRequest.ID + ".zip";
            fileStream = new FileStream(zipFile, FileMode.Create);
            fileStream.Write(request.downloadHandler.data, 0, request.downloadHandler.data.Length);
            fileStream.Close();
            _unzippingThread = new Thread(() => { UnzipFile(zipFile, _path + tRequest.ID); });
            _unzippingThread.Start();
            while (_unzippingThread.IsAlive)
                yield return null;
            CachedTexturePathMap.Add(tRequest.ID, _path + tRequest.ID);
            File.Delete(zipFile);
            tRequest.Event?.Invoke(CachedTexturePathMap[tRequest.ID]);
            Debug.Log(tRequest.ID + " loaded.");
        }
        _textureDownloadInProgress = false;
    }*/

    private IEnumerator SetupBrowser(DataBrowser browser)
    {
        // Close any panels animated
        if (browser.FilterPanel.IsActive)
        {
            if (browser.SearchPanel.IsActive) StartCoroutine(browser.FilterPanel.Activate(false));
            else yield return StartCoroutine(browser.FilterPanel.Activate(false));
        }
        if (browser.SearchPanel.IsActive) yield return StartCoroutine(browser.SearchPanel.Activate(false));        

        // ============================= FILTER PANEL SETUP ============================

        // Define filter update event
        browser.FilterPanel.FilterUpdater = () =>
        {
            VRResourceData actualSpace = (VRResourceData)browser.LastBrowserStateMap[Name];
            for (int i = 0; i < browser.FilterPanel.Checkboxes.Length; i++)
            {
                browser.FilterPanel.Checkboxes[i].gameObject.SetActive((browser.FilterPanel.TypePointer + i) < browser.FilterPanel.TypeList.Count);
                if (browser.FilterPanel.Checkboxes[i].gameObject.activeInHierarchy)
                {
                    string label = browser.FilterPanel.TypeList[browser.FilterPanel.TypePointer + i];
                    browser.FilterPanel.Checkboxes[i].ButtonValue = label;
                    browser.FilterPanel.Checkboxes[i].Status = browser.FilterPanel.Types[browser.FilterPanel.TypeList[browser.FilterPanel.TypePointer + i]];
                    bool hasSubcategories = !browser.FilterPanel.ShowingSubTypes &&
                                            ((actualSpace.Name.Equals(FORMATTED_MODEL_NAME) && ModelTaxonomies[label].SubCategories.Count > 0) ||
                                             (actualSpace.Name.Equals(FORMATTED_TEXTURE_NAME) && TextureTaxonomies[label].SubCategories.Count > 0));
                    browser.FilterPanel.Openers[i].gameObject.SetActive(hasSubcategories);
                }
            }
        };

        // Define event for subcategory opener button        
        for (int i=0; i<browser.FilterPanel.Openers.Length; i++)
        {
            InteractiveButton opener = browser.FilterPanel.Openers[i];
            InteractiveCheckbox cb = browser.FilterPanel.Checkboxes[i];
            opener.OnClick = () =>
            {
                browser.FilterPanel.ShowingSubTypes = true;
                VRResourceData actualSpace = (VRResourceData)browser.LastBrowserStateMap[Name];
                Dictionary<string, InteractiveCheckbox.CheckboxStatus> filters = new Dictionary<string, InteractiveCheckbox.CheckboxStatus>();
                if (actualSpace.Name.Equals(FORMATTED_MODEL_NAME))
                {
                    foreach (string subCategorie in ModelTaxonomies[(string)cb.ButtonValue].SubCategories)
                        filters.Add(subCategorie, ModelSubCategories[subCategorie]);
                    browser.FilterPanel.Init(SUB_CATEGORIES, filters);
                }
                else
                {
                    foreach (string subCategorie in TextureTaxonomies[(string)cb.ButtonValue].SubCategories)
                        filters.Add(subCategorie, TextureSubCategories[subCategorie]);
                    browser.FilterPanel.Init(SUB_CATEGORIES, filters);
                }
            };
        }

        // Define event for back-button on subcategory page
        browser.FilterPanel.Back.OnClick = () =>
        {
            SetupFilterPanel(browser, ((VRResourceData)browser.LastBrowserStateMap[Name]).Name, false);
        };

        // Set event for changing checkboxes
        browser.FilterPanel.CheckboxUpdater = (type, status) => 
        {
            VRResourceData actualSpace = (VRResourceData)browser.LastBrowserStateMap[Name];
            string _mainCat; int checkedSubCategories; InteractiveCheckbox.CheckboxStatus _mainCatStatus;
            if (browser.FilterPanel.ShowingSubTypes)
            {
                if (actualSpace.Name.Equals(FORMATTED_MODEL_NAME))
                {
                    ModelSubCategories[type] = status;
                    _mainCat = ObjectSubCategoryMap[type];
                    checkedSubCategories = 0;
                    foreach (string subCat in ModelTaxonomies[_mainCat].SubCategories)
                        if (ModelSubCategories[subCat] == InteractiveCheckbox.CheckboxStatus.AllChecked)
                            checkedSubCategories += 1;
                    _mainCatStatus = (checkedSubCategories == 0) ? InteractiveCheckbox.CheckboxStatus.NoneChecked :
                                     (checkedSubCategories == ModelTaxonomies[_mainCat].SubCategories.Count) ?
                                     InteractiveCheckbox.CheckboxStatus.AllChecked : InteractiveCheckbox.CheckboxStatus.PartsChecked;
                    ModelMainCategories[_mainCat] = _mainCatStatus;
                }
                else
                {
                    TextureSubCategories[type] = status;
                    _mainCat = TextureSubCategoryMap[type];
                    checkedSubCategories = 0;
                    foreach (string subCat in TextureTaxonomies[_mainCat].SubCategories)
                        if (TextureSubCategories[subCat] == InteractiveCheckbox.CheckboxStatus.AllChecked)
                            checkedSubCategories += 1;
                    _mainCatStatus = (checkedSubCategories == 0) ? InteractiveCheckbox.CheckboxStatus.NoneChecked :
                                     (checkedSubCategories == TextureTaxonomies[_mainCat].SubCategories.Count) ?
                                     InteractiveCheckbox.CheckboxStatus.AllChecked : InteractiveCheckbox.CheckboxStatus.PartsChecked;
                    TextureMainCategories[_mainCat] = _mainCatStatus;
                }
            } else
            {
                if (actualSpace.Name.Equals(FORMATTED_MODEL_NAME))
                {
                    ModelMainCategories[type] = status;
                    if (ModelTaxonomies.ContainsKey(type))
                    {
                        foreach (string sub in ModelTaxonomies[type].SubCategories)
                            ModelSubCategories[sub] = status;
                    }
                } else
                {
                    TextureMainCategories[type] = status;
                    if (TextureTaxonomies.ContainsKey(type))
                    {
                        foreach (string sub in TextureTaxonomies[type].SubCategories)
                            TextureSubCategories[sub] = status;
                    }
                }
            }
            browser.FilterPanel.Types[type] = status; 
        };

        // ============================= DATA PANEL SETUP ============================

        //Parent button functionality
        browser.DataPanel.ParentDir.OnClick = () =>
        {
            VRResourceData actualDir = null;
            if (browser.LastBrowserStateMap.ContainsKey(Name) && browser.LastBrowserStateMap[Name] != null) actualDir = (VRResourceData)browser.LastBrowserStateMap[Name];
            if (actualDir == null) return;
            browser.SetActualState(Name, null);
            StartCoroutine(browser.FilterPanel.Activate(false));
            StartCoroutine(browser.SearchPanel.Activate(false));
            SetupMainMenu(browser);
        };

        // Define browser update event
        browser.BrowserUpdater = () =>
        {
            VRResourceData actualDir = null;
            if (browser.LastBrowserStateMap.ContainsKey(Name) && browser.LastBrowserStateMap[Name] != null) actualDir = (VRResourceData)browser.LastBrowserStateMap[Name];
            if (actualDir == null) return;
            if (actualDir.Name.Equals(FORMATTED_MODEL_NAME))
            {
                try
                {
                    if (handler == null)
                    {
                        handler = browser.transform.Find("Text2ScenePanel").GetComponent<Text2SceneHandler>();
                        //handler.Init();
                    }

                    if (handler.Search)
                    {
                        StartCoroutine(ObjectSearchRequest(handler.SearchRequest, browser, actualDir.Name));
                        handler.Search = false;
                    }
                    else StartCoroutine(ObjectSearchRequest(browser.SearchPanel.SearchPattern.ToLower(), browser, actualDir.Name));
                }
                catch (NullReferenceException e)
                {
                    Debug.LogError(e);
                    StartCoroutine(ObjectSearchRequest(browser.SearchPanel.SearchPattern.ToLower(), browser, actualDir.Name));
                }
            }
            else if (actualDir.Name.Equals(FORMATTED_TEXTURE_NAME))
                browser.DataPanel.Init(actualDir.Name, GetObjectList(ShapeNetTextures.Values, browser.SearchPanel.SearchPattern.ToLower()));
            browser.DataPanel.ParentDir.Active = true;
            browser.DataPanel.Root.gameObject.SetActive(false);
        };

        // ============================= LOADING LAST STATE ============================ 

        if (!browser.LastBrowserStateMap.ContainsKey(Name) || browser.LastBrowserStateMap[Name] == null)
        {
            SetupMainMenu(browser);
        }
    }

    private void SetupMainMenu(DataBrowser browser)
    {
        browser.DataPanel.Init(Name, GetShapeNetSpaces());
        browser.DataPanel.Root.gameObject.SetActive(false);
        browser.DataPanel.ParentDir.Active = false;
        foreach (DataContainer dc in browser.DataPanel.DataContainers)
        {
            dc.OnClick = () =>
            {
                VRResourceData resource = (VRResourceData)dc.Resource;
                if (resource != null && resource.Type != VRResourceData.DataType.File)
                {
                    browser.SetActualState(Name, resource);
                    if (resource.Name.Equals(FORMATTED_MODEL_NAME)) { 
                        browser.DataPanel.Init(resource.Name, GetObjectList(ShapeNetModels.Values, browser.SearchPanel.SearchPattern.ToLower()));
                        //StartCoroutine(ObjectSearchRequest(browser.SearchPanel.SearchPattern.ToLower(), browser, resource.Name));
                    }
                    else if (resource.Name.Equals(FORMATTED_TEXTURE_NAME)) { 
                        browser.DataPanel.Init(resource.Name, GetObjectList(ShapeNetTextures.Values, browser.SearchPanel.SearchPattern.ToLower()));
                    }
                    SetupFilterPanel(browser, resource.Name, false);
                    browser.DataPanel.Root.gameObject.SetActive(false);
                    browser.DataPanel.ParentDir.Active = true;
                    foreach (DataContainer dCont in browser.DataPanel.DataContainers)
                        dCont.OnClick = null;

                    StartCoroutine(browser.FilterPanel.Activate(true));
                    StartCoroutine(browser.SearchPanel.Activate(true));
                }
            };
            dc.ExecuteBeforeLongClick = () =>
            {
                return !dc.Resource.Name.Equals(FORMATTED_MODEL_NAME) && !dc.Resource.Name.Equals(FORMATTED_TEXTURE_NAME);
            };
        }
    }

    private void SetupFilterPanel(DataBrowser browser, string space, bool showingSubTypes)
    {
        browser.FilterPanel.ShowingSubTypes = showingSubTypes;
        if (space.Equals(FORMATTED_MODEL_NAME)) 
            browser.FilterPanel.Init(MAIN_CATEGORIES, ModelMainCategories);
        else
            browser.FilterPanel.Init(MAIN_CATEGORIES, TextureMainCategories);
    }

    private IEnumerator ObjectSearchRequest(string search, DataBrowser browser, string name)
    {
        Debug.Log("Searchrequest: " + search);
        if (search.Equals(""))
        {
            browser.DataPanel.Init(name, GetObjectList(ShapeNetModels.Values, ""));
        }
        else { 
            List<string> foundObjects = new List<string>();
            request = UnityWebRequest.Get(SEARCH_OBJECTS + search);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
                _objectTaxonomyError = request.error;
            else
            {
                data = JsonMapper.ToObject(request.downloadHandler.text);
                Debug.Log("Search return: " + data);
                if (!data.Keys.Contains("success") || !bool.Parse(data["success"].ToString()) ||
                    !data.Keys.Contains("term") || !data.Keys.Contains("results"))
                {
                    _objectTaxonomyError = "Downloading object taxonomy failed.";
                    yield break;
                }

                foreach (JsonData obj in data["results"])
                {
                    foundObjects.Add(obj["id"].ToString());
                }
                Debug.Log("Search Final: " + foundObjects);
            }

            if (foundObjects != null)
            {
                browser.DataPanel.Init(name, GetObjectList(ShapeNetModels, foundObjects));
            }
        }
    }

    public static IEnumerator ObjectSearchRequest(string search, Action<List<string>> callback)
    {
        Debug.Log("Searchrequest: " + search);
        if (search.Equals(""))
        {
            search = "CellPhone";
        }
        
        List<string> foundObjects = new List<string>();
        UnityWebRequest request = UnityWebRequest.Get(SEARCH_OBJECTS + search);
        yield return request.SendWebRequest();
        string _objectTaxonomyError;
        if (request.isNetworkError || request.isHttpError)
            _objectTaxonomyError = request.error;
        else
        {
            JsonData data = JsonMapper.ToObject(request.downloadHandler.text);
            Debug.Log("Search return: " + data);
            if (!data.Keys.Contains("success") || !bool.Parse(data["success"].ToString()) ||
                !data.Keys.Contains("term") || !data.Keys.Contains("results"))
            {
                _objectTaxonomyError = "Downloading object taxonomy failed.";
                yield break;
            }

            foreach (JsonData obj in data["results"])
            {
                foundObjects.Add(obj["id"].ToString());
            }
            Debug.Log("Search Final: " + foundObjects);
        }

        if (foundObjects != null)
        {
            callback(foundObjects);
        }
    }

    private List<ShapeNetObject> GetObjectList(IEnumerable<ShapeNetObject> objects, string pattern)
    {
        List<ShapeNetObject> res = new List<ShapeNetObject>();

        foreach (ShapeNetObject obj in objects)
            if (CheckPatternMatch(obj, pattern) && ProofCategories(obj))
                res.Add(obj);
        return res;
    }

    private List<ShapeNetObject> GetObjectList(Dictionary<string, ShapeNetModel> ShapeNetModels, List<string> filterlist)
    {

        List<ShapeNetObject> res = new List<ShapeNetObject>();
        Debug.Log("Filterlist: " + filterlist.ToString());
        foreach (string obj in filterlist)
            if (ShapeNetModels.ContainsKey(obj))
                res.Add(ShapeNetModels[obj]);

        Debug.Log("Final found List: " + res);
        return res;
    }

    private List<VRResourceData> GetShapeNetSpaces()
    {
        List<VRResourceData> spaces = new List<VRResourceData>();
        spaces.Add(new VRResourceData(FORMATTED_MODEL_NAME, MODELS, null, "", DateTime.MinValue, DateTime.MinValue, VRData.SourceType.Remote));
        spaces.Add(new VRResourceData(FORMATTED_TEXTURE_NAME, TEXTURES, null, "", DateTime.MinValue, DateTime.MinValue, VRData.SourceType.Remote));
        return spaces;
    }

    private bool CheckPatternMatch(ShapeNetObject snO, string pattern)
    {
        if (snO is ShapeNetModel)
        {
            ShapeNetModel snM = (ShapeNetModel)snO;
            if (pattern == "") return true;
            if (snM.Name.ToLower().Contains(pattern)) return true;
            if (snM.Lemmas.Contains(pattern)) return true;
            return false;
        }
        else if (snO is ShapeNetTexture)
        {
            ShapeNetTexture snT = (ShapeNetTexture)snO;
            if (pattern == "") return true;
            if (snT.Name.ToLower().Contains(pattern)) return true;
            return false;
        }
        else return false;
    }

    private bool ProofCategories(ShapeNetObject snObj)
    {
        foreach (string category in snObj.Categories)
        {

            if (snObj is ShapeNetModel)
            {
                if (!ModelSubCategories.ContainsKey(category)) continue;
                if (ModelSubCategories[category] == InteractiveCheckbox.CheckboxStatus.AllChecked)
                    return true;
            }
            if (snObj is ShapeNetTexture)
            {
                if (!TextureSubCategories.ContainsKey(category)) continue;
                if (TextureSubCategories[category] == InteractiveCheckbox.CheckboxStatus.AllChecked)
                    return true;
            }
        }
        return false;
    }

}

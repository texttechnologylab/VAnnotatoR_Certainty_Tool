using System;
using UnityEngine;
using System.Collections;
using System.Text;
//using System.Diagnostics;
using MessageLibrary;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SimpleWebBrowser
{


    

    public class WebBrowser : MonoBehaviour
    {

        #region General

        [Header("General settings")] public int Width = 1024;

        public int Height = 768;

        public string MemoryFile = "MainSharedMem";

        public bool RandomMemoryFile = true;

        [Range(8000f, 9000f)] public int Port = 8885;

        public bool RandomPort = true;

        public string InitialURL = "http://www.google.com";

        public bool EnableWebRTC = false;

        [Multiline]
        public string JSInitializationCode = "";

        //public List<GameObject> AdditionalBrowserObjects

        #endregion



        [Header("UI settings")]
        [SerializeField]
        public BrowserUI mainUIPanel;

        public bool KeepUIVisible = false;

        public Camera MainCamera;

        [Header("Dialog settings")]
        [SerializeField]
        public Canvas DialogCanvas;
        [SerializeField]
        public Text DialogText;
        [SerializeField]
        public Button OkButton;
        [SerializeField]
        public Button YesButton;
        [SerializeField]
        public Button NoButton;
        [SerializeField]
        public InputField DialogPrompt;

        //dialog states - threading
        private bool _showDialog = false;
        private string _dialogMessage = "";
        private string _dialogPrompt = "";
        private DialogEventType _dialogEventType;
        //query - threading
        private bool _startQuery = false;
        private string _jsQueryString = "";
        
        //status - threading
        private bool _setUrl = false;
        public string _setUrlString = "";
       

        #region JS Query events

        public delegate void JSQuery(string query);

        public event JSQuery OnJSQuery;

        #endregion


        private Material _mainMaterial;


        private float width, height;
        private Transform topLeft, bottomLeft, bottomRight;


        private BrowserEngine _mainEngine;


        private int posX = 0;
        private int posY = 0;

        public string selectedText = "";
        public string clickedHTML = "";


        //why Unity does not store the links in package?
        void InitPrefabLinks()
        {
            //if (mainUIPanel == null)
            //    mainUIPanel = gameObject.transform.Find("MainUI").gameObject.GetComponent<BrowserUI>();
            //if (DialogCanvas == null)
            //    DialogCanvas = gameObject.transform.Find("MessageBox").gameObject.GetComponent<Canvas>();
            //if (DialogText == null)
            //    DialogText = DialogCanvas.transform.Find("MessageText").gameObject.GetComponent<Text>();
            //if (OkButton == null)
            //    OkButton = DialogCanvas.transform.Find("OK").gameObject.GetComponent<Button>();
            //if (YesButton == null)
            //    YesButton = DialogCanvas.transform.Find("Yes").gameObject.GetComponent<Button>();
            //if (NoButton == null)
            //    NoButton = DialogCanvas.transform.Find("No").gameObject.GetComponent<Button>();
            //if (DialogPrompt == null)
            //    DialogPrompt = DialogCanvas.transform.Find("Prompt").gameObject.GetComponent<InputField>();

        }

        void Awake()
        {
            _mainEngine = new BrowserEngine();

            if (RandomMemoryFile)
            {
                Guid memid = Guid.NewGuid();
                MemoryFile = memid.ToString();
            }
            if (RandomPort)
            {
                System.Random r = new System.Random();
                Port = 8000 + r.Next(1000);
            }

            topLeft = new GameObject("TopLeft").transform;
            topLeft.transform.parent = transform;
            topLeft.localPosition = new Vector3(-5, 0, 5);
            bottomLeft = new GameObject("BottomLeft").transform;
            bottomLeft.transform.parent = transform;
            bottomLeft.localPosition = new Vector3(-5, 0, -5);
            bottomRight = new GameObject("BottomRight").transform;
            bottomRight.transform.parent = transform;
            bottomRight.localPosition = new Vector3(5, 0, -5);

            OnJSQuery += MainBrowser_OnJSQuery;


            _mainEngine.InitPlugin(Width, Height, MemoryFile, Port, InitialURL,EnableWebRTC);
            //run initialization
            if (JSInitializationCode.Trim() != "")
                _mainEngine.RunJSOnce(JSInitializationCode);

        }

    // Use this for initialization
    void Start()
        {
            InitPrefabLinks();
            //mainUIPanel.InitPrefabLinks();

            if (MainCamera == null)
            {
                MainCamera = Camera.main;
                if (MainCamera == null)
                    Debug.LogError("Error: can't find main camera");
            }

            _mainMaterial = GetComponent<MeshRenderer>().material;
            _mainMaterial.SetTexture("_MainTex", _mainEngine.BrowserTexture);
            _mainMaterial.SetTextureScale("_MainTex", new Vector2(-1, 1));





            // _mainInput = MainUrlInput.GetComponent<Input>();

            //attach dialogs and queries
            _mainEngine.OnJavaScriptDialog += _mainEngine_OnJavaScriptDialog;
            _mainEngine.OnJavaScriptQuery += _mainEngine_OnJavaScriptQuery;
            _mainEngine.OnPageLoaded += _mainEngine_OnPageLoaded;


            //DialogCanvas.worldCamera = MainCamera;
            //DialogCanvas.gameObject.SetActive(false);

            gameObject.layer = 2;
            transform.parent.GetComponentInChildren<ExpandView>().clickable = false;
            transform.parent.GetComponentInChildren<ExpandView>().OnLongClick =  () => {

                StolperwegeHelper.VRWriter.Interface.DoneClicked += (string str) => {
                    foreach (char c in str)
                        SendKey(c);
                };

                StolperwegeHelper.VRWriter.Active = true;

            };

        }

        

        private void _mainEngine_OnPageLoaded(string url)
        {
            _setUrl = true;
            _setUrlString = url;
           
        }

        //make it thread-safe
        private void _mainEngine_OnJavaScriptQuery(string message)
        {
            _jsQueryString = message;
            _startQuery = true;
        }

        public void RespondToJSQuery(string response)
        {
            _mainEngine.SendQueryResponse(response);
        }

        private void _mainEngine_OnJavaScriptDialog(string message, string prompt, DialogEventType type)
        {
            _showDialog = true;
            _dialogEventType = type;
            _dialogMessage = message;
            _dialogPrompt = prompt;

        }

        private void ShowDialog()
        {

            switch (_dialogEventType)
            {
                case DialogEventType.Alert:
                {
                    DialogCanvas.gameObject.SetActive(true);
                    OkButton.gameObject.SetActive(true);
                    YesButton.gameObject.SetActive(false);
                    NoButton.gameObject.SetActive(false);
                    DialogPrompt.text = "";
                    DialogPrompt.gameObject.SetActive(false);
                    DialogText.text = _dialogMessage;
                    break;
                }
                case DialogEventType.Confirm:
                {
                    DialogCanvas.gameObject.SetActive(true);
                    OkButton.gameObject.SetActive(false);
                    YesButton.gameObject.SetActive(true);
                    NoButton.gameObject.SetActive(true);
                    DialogPrompt.text = "";
                    DialogPrompt.gameObject.SetActive(false);
                    DialogText.text = _dialogMessage;
                    break;
                }
                case DialogEventType.Prompt:
                {
                    DialogCanvas.gameObject.SetActive(true);
                    OkButton.gameObject.SetActive(false);
                    YesButton.gameObject.SetActive(true);
                    NoButton.gameObject.SetActive(true);
                    DialogPrompt.text = _dialogPrompt;
                    DialogPrompt.gameObject.SetActive(true);
                    DialogText.text = _dialogMessage;
                    break;
                }
            }
            _showDialog = false;
        }

        #region UI

        public void OnNavigate()
        {
            // MainUrlInput.isFocused
            _mainEngine.SendNavigateEvent(mainUIPanel.UrlField.text, false, false);

        }

        public void RunJavaScript(string js)
        {
            _mainEngine.SendExecuteJSEvent(js);
        }

        public void GoBackForward(bool forward)
        {
            if (forward)
                _mainEngine.SendNavigateEvent("", false, true);
            else
                _mainEngine.SendNavigateEvent("", true, false);
        }

        #endregion

        #region Dialogs

        public void DialogResult(bool result)
        {
            DialogCanvas.gameObject.SetActive(false);
            _mainEngine.SendDialogResponse(result, DialogPrompt.text);

        }

        #endregion


        #region Events (3D)

        //void OnMouseEnter()
        //{
        //    _focused = true;
        //}

        //void OnMouseExit()
        //{
        //    _focused = false;
        //}

        void OnMouseDown()
        {

            if (_mainEngine.Initialized)
            {
                Vector2 pixelUV = GetScreenCoords();
                print(pixelUV);

                if (pixelUV.x > 0)
                {
                    SendMouseButtonEvent((int)pixelUV.x, (int)pixelUV.y, MouseButton.Left, MouseEventType.ButtonDown);

                }
            }

        }        

        private Vector2 GetScreenCoords()
        {


            RaycastHit hit;
            if (
                !Physics.Raycast(
                    MainCamera.ScreenPointToRay(Input.mousePosition), out hit))
                return new Vector2(-1f, -1f);
            Texture tex = _mainMaterial.mainTexture;


            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x = (1 - pixelUV.x) * tex.width;
            pixelUV.y *= tex.height;
            return pixelUV;
        }

        private bool switched = false;
        private Vector3 handEnterPos; // never used => lastHandPos;

        private const string selectQuery = "window.cefQuery({request: '' + document.getSelection(),onSuccess: function(response) {'Response: ' + response;},onFailure: function(error_code, error_message) { }});";

        private string getClickQuery(Vector2 uv)
        {
            return "window.cefQuery({request: 'click:'+ document.elementFromPoint(" + (int)uv.x + "," + (int)uv.y + ").outerHTML, onSuccess: function(response) { 'Response: ' + response; },onFailure: function(error_code, error_message) { }}); ";
        }

        private bool armEntered = false;
        private IEnumerator clearSelectedText()
        {
            armEntered = false;
            yield return new WaitForSeconds(0.5f);

            if (!armEntered)
            {
                Debug.Log("clear");
                selectedText = "";
            }
                
        }

        private Vector3 lastFingerPos;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Contains("Finger"))
            {
               // StolperwegeHelper.keyboard.Active = true ;
               // StolperwegeHelper.keyboard.CurrentBrowser = this;
                lastFingerPos = other.transform.position;
                if(selectedText.Length>0)
                    StartCoroutine(clearSelectedText());
                if (_mainEngine.Initialized)
                {
                    Vector2 pixelUV = GetScreenCoords(other);

                    if (pixelUV.x > 0)
                    {
                        print("enter" + pixelUV);
                        SendMouseButtonEvent((int)pixelUV.x, (int)pixelUV.y, MouseButton.Left, MouseEventType.ButtonDown);

                    }
                }

            }

            if (other.gameObject.name.Contains("Arm"))
            {
                armEntered = true;
                handEnterPos = other.bounds.center;
                // never used
                //lastHandPos = other.bounds.center;
                switched = false;

                Vector2 pixelUV = GetScreenCoords(other);

                if (_mainEngine.Initialized)
                {
                    Debug.Log(pixelUV);
                    _mainEngine.RunJSOnce(getClickQuery(pixelUV));
                }

            }

            
        }


        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.name.Contains("Finger"))
            {
                Vector3 diff = lastFingerPos - other.transform.position;
                lastFingerPos = other.transform.position;
                if (_mainEngine.Initialized)
                {
                    Vector2 pixelUV = GetScreenCoords(other);

                    if (pixelUV.x > 0)
                    {


                        int px = (int)pixelUV.x;
                        int py = (int)pixelUV.y;

                        //ProcessScrollInput(px, py);
                        if (posX != px || posY != py)
                        {
                            MouseMessage msg = new MouseMessage
                            {
                                Type = MouseEventType.Move,
                                X = px,
                                Y = py,
                                GenericType = MessageLibrary.BrowserEventType.Mouse,
                                // Delta = e.Delta,
                                Button = MouseButton.None
                            };

                            msg.Button = MouseButton.Left;
                            if (Input.GetMouseButton(1))
                                msg.Button = MouseButton.Right;
                            if (Input.GetMouseButton(1))
                                msg.Button = MouseButton.Middle;

                            posX = px;
                            posY = py;
                            _mainEngine.SendMouseEvent(msg);
                        }

                        //check other buttons...
                        if (Input.GetMouseButtonDown(1))
                            SendMouseButtonEvent(px, py, MouseButton.Right, MouseEventType.ButtonDown);
                        if (Input.GetMouseButtonUp(1))
                            SendMouseButtonEvent(px, py, MouseButton.Right, MouseEventType.ButtonUp);
                        if (Input.GetMouseButtonDown(2))
                            SendMouseButtonEvent(px, py, MouseButton.Middle, MouseEventType.ButtonDown);
                        if (Input.GetMouseButtonUp(2))
                            SendMouseButtonEvent(px, py, MouseButton.Middle, MouseEventType.ButtonUp);
                    }
                }
            }

            if (other.gameObject.name.Contains("Arm"))
            {

                if (!switched)
                {
                    Vector3 diff = Quaternion.Inverse(transform.rotation) * (handEnterPos - other.bounds.center);
                    if (diff.x < -0.3)
                    {
                        GoBackForward(false);
                        switched = true;
                    }
                    if (diff.x > 0.3)
                    {
                        GoBackForward(true);
                        switched = true;
                    }

                    Vector2 pixelUV = GetScreenCoords(other);

                    if (pixelUV.x > 0)
                    {


                        //int px = (int)pixelUV.x;
                        //int py = (int)pixelUV.y;

                        //ProcessScrollInput(px, py, diff.y);
                    }

                        
                }

            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.Contains("Finger"))
            {
                _mainEngine.RunJSOnce(selectQuery);
                if (_mainEngine.Initialized)
                {
                    Vector2 pixelUV = GetScreenCoords(other);
                    if (pixelUV.x > 0)
                    {
                        print("exit"+pixelUV);
                        SendMouseButtonEvent((int)pixelUV.x, (int)pixelUV.y, MouseButton.Left, MouseEventType.ButtonUp);
                    }
                }
            }

            if (other.gameObject.name.Contains("Arm"))
            {
                // never used
                //DiscourseReferent createdRef = null;
                Debug.Log("CreateTExt "+ other.name + selectedText + clickedHTML);

                StolperwegeInterface.OnCreated onCreated = (StolperwegeElement e) => {
                    e.Object3D.GetComponent<StolperwegeObject>().OnGrab(other);
                    other.GetComponent<DragFinger>().GrabedObject = e.StolperwegeObject;
                    e.AddStolperwegeRelation(getStolperwegeUri(),true);
                };


                if (!selectedText.Equals(""))
                {
                    Debug.Log("create " + selectedText);
                    //StolperwegeText text = new StolperwegeText("" + Time.time, selectedText, null);
                    //text.draw();
                    //text.Object3D.transform.forward = Helper.centerEyeAnchor.transform.forward;
                    //text.Object3D.onGrab(other);

                    //Debug.Log("created " + selectedText);
                    

                    //createdRef = text;

                    Hashtable keys = new Hashtable
                        {
                            {"value",selectedText }
                        };
                    selectedText = "";
                    StartCoroutine(SceneController.GetInterface<StolperwegeInterface>().CreateElement("text", keys, onCreated,true));
                }
                else if(!clickedHTML.Equals(""))
                {
                    if (clickedHTML.StartsWith("<img"))
                    {
                        string imageURI = getPropertyFromTag(clickedHTML, "src");

                        if (imageURI.StartsWith("/") && !imageURI.StartsWith("//"))
                        {
                            imageURI = getBaseURI() + imageURI;
                        }

                        if (!imageURI.Contains("http"))
                            imageURI = "https:" + imageURI;

                        print(imageURI);

                        //StolperwegeImage image = new StolperwegeImage(""+Time.time, imageURI, null);

                        //image.draw();

                        //image.Object3D.transform.forward = -Helper.centerEyeAnchor.transform.forward;

                        //image.Object3D.GetComponent<StolperwegeObject>().onGrab(other);

                        clickedHTML = "";

                        //createdRef = image;

                        Hashtable keys = new Hashtable
                        {
                            {"value",imageURI }
                        };

                        StartCoroutine(SceneController.GetInterface<StolperwegeInterface>().CreateElement("image", keys, onCreated,true));
                    }
                    if(clickedHTML.StartsWith("<a "))
                    {
                        string uri = getPropertyFromTag(clickedHTML, "href");
                        string name = getSubelementFromTag(clickedHTML);

                        if(uri.StartsWith("/") && !uri.StartsWith("//"))
                        {
                            uri = getBaseURI() + uri;
                        }

                        print(uri + " " + name);

                        clickedHTML = "";

                        Hashtable keys = new Hashtable
                        {
                            {"value",uri }
                        };

                        StartCoroutine(SceneController.GetInterface<StolperwegeInterface>().CreateElement("uri", keys, onCreated,true));
                    }

                    if(clickedHTML.StartsWith("<p class=\"text\">"))
                    {

                        Hashtable keys = new Hashtable
                        {
                            {"value",new System.Text.RegularExpressions.Regex("<[^>]*>").Replace(clickedHTML,"") }
                        };

                        StartCoroutine(SceneController.GetInterface<StolperwegeInterface>().CreateElement("text", keys, onCreated, true));
                    }
                }

                
                
            }


        }

        private DiscourseReferent getStolperwegeUri()
        {
            return (DiscourseReferent)transform.parent.parent.GetComponentInChildren<StolperwegeURIObject>().Referent;
        }

        private string getBaseURI()
        {
            int i = 0;
            string baseURI = "";

            foreach(char c in _setUrlString)
            {
                if (c.Equals('/')) i++;
                if (i >= 3) break;

                baseURI += c;
            }

            return baseURI;
        }

        void OnMouseUp()
        {

            //string query = "window.cefQuery({request: 'mark:' + document.getSelection(),onSuccess: function(response) {'Response: ' + response;},onFailure: function(error_code, error_message) { }});";

            Debug.Log("sdad");
            if (_mainEngine.Initialized)
            {
                Vector2 pixelUV = GetScreenCoords();
                print(pixelUV);
                string query3 = "window.cefQuery({request: 'click:'+ document.elementFromPoint(500,100).outerHTML, onSuccess: function(response) { 'Response: ' +  response; },onFailure: function(error_code, error_message) { }}); ";

            _mainEngine.RunJSOnce(query3);

            if (pixelUV.x > 0)
                {
                    //SendMouseButtonEvent((int)pixelUV.x, (int)pixelUV.y, MouseButton.Left, MouseEventType.ButtonUp);
                }
            }
        }

        ////void OnMouseOver()
        ////{
        ////    if (_mainEngine.Initialized)
        ////    {
        ////        Vector2 pixelUV = GetScreenCoords();

        ////        if (pixelUV.x > 0)
        ////        {


        ////            int px = (int)pixelUV.x;
        ////            int py = (int)pixelUV.y;

        ////            ProcessScrollInput(px, py);

        ////            if (posX != px || posY != py)
        ////            {
        ////                MouseMessage msg = new MouseMessage
        ////                {
        ////                    Type = MouseEventType.Move,
        ////                    X = px,
        ////                    Y = py,
        ////                    GenericType = MessageLibrary.BrowserEventType.Mouse,
        ////                    Delta = e.Delta,
        ////                    Button = MouseButton.None
        ////                };

        ////                if (Input.GetMouseButton(0))
        ////                    msg.Button = MouseButton.Left;
        ////                if (Input.GetMouseButton(1))
        ////                    msg.Button = MouseButton.Right;
        ////                if (Input.GetMouseButton(1))
        ////                    msg.Button = MouseButton.Middle;

        ////                posX = px;
        ////                posY = py;
        ////                _mainEngine.SendMouseEvent(msg);
        ////            }

        ////            check other buttons...
        ////            if (Input.GetMouseButtonDown(1))
        ////                SendMouseButtonEvent(px, py, MouseButton.Right, MouseEventType.ButtonDown);
        ////            if (Input.GetMouseButtonUp(1))
        ////                SendMouseButtonEvent(px, py, MouseButton.Right, MouseEventType.ButtonUp);
        ////            if (Input.GetMouseButtonDown(2))
        ////                SendMouseButtonEvent(px, py, MouseButton.Middle, MouseEventType.ButtonDown);
        ////            if (Input.GetMouseButtonUp(2))
        ////                SendMouseButtonEvent(px, py, MouseButton.Middle, MouseEventType.ButtonUp);
        ////        }
        ////    }


        ////}

        #endregion

        #region Helpers

        private Vector2 GetScreenCoords(Collider other)
        {
            width = (bottomRight.position - bottomLeft.position).magnitude;
            height = (bottomLeft.position - topLeft.position).magnitude;

            Quaternion rot = Quaternion.Inverse(transform.rotation);

            Vector3 diff = rot * (other.bounds.center - topLeft.position);

            diff.x /= width;
            diff.z /= height;

            diff.x *= Width;
            diff.z *= Height;

            Vector2 textCoords = new Vector2(diff.x, - diff.z);


            return textCoords;

        }

        private string getPropertyFromTag(string tag, string property)
        {

            string propertyResult = "";
            bool src = false;

            while (tag.Length > 0)
            {
                if (src && tag.StartsWith("\"")) break;
                if (src)
                {
                    propertyResult += tag[0];
                }
                if (tag.StartsWith(property+"=\""))
                {
                    tag = tag.Replace(property + "=\"", "");
                    src = true;
                }
                else
                    tag = tag.Remove(0, 1);
            }

            return propertyResult;
        }

        private string getSubelementFromTag(string tag)
        {

            string propertyResult = "";
            bool src = false;

            while (tag.Length > 0)
            {
                if (src && tag.StartsWith("<")) break;
                if (src)
                {
                    propertyResult += tag[0];
                }
                if (tag.StartsWith(">"))
                    src = true;

                tag = tag.Remove(0, 1);
            }

            return propertyResult;
        }

        private void SendMouseButtonEvent(int x, int y, MouseButton btn, MouseEventType type)
        {
            MouseMessage msg = new MouseMessage
            {
                Type = type,
                X = x,
                Y = y,
                GenericType = MessageLibrary.BrowserEventType.Mouse,
                // Delta = e.Delta,
                Button = btn
            };
            _mainEngine.SendMouseEvent(msg);
        }

        private void ProcessScrollInput(int px, int py, float delta)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            scroll = delta*_mainEngine.BrowserTexture.height;

            int scInt = (int) scroll;

            if (scInt != 0)
            {
                MouseMessage msg = new MouseMessage
                {
                    Type = MouseEventType.Wheel,
                    X = px,
                    Y = py,
                    GenericType = MessageLibrary.BrowserEventType.Mouse,
                    Delta = scInt,
                    Button = MouseButton.None
                };

                if (Input.GetMouseButton(0))
                    msg.Button = MouseButton.Left;
                if (Input.GetMouseButton(1))
                    msg.Button = MouseButton.Right;
                if (Input.GetMouseButton(1))
                    msg.Button = MouseButton.Middle;

                _mainEngine.SendMouseEvent(msg);
            }
        }

        #endregion

        bool first = true;

        // Update is called once per frame
        void Update()
        {

            _mainEngine.UpdateTexture();



            //Dialog
            if (_showDialog)
            {
                ShowDialog();
            }

            //Query
            if (_startQuery)
            {
                _startQuery = false;
                if (OnJSQuery != null)
                    OnJSQuery(_jsQueryString);

            }

            //Status
            if (_setUrl)
            {
                _setUrl = false;

                if (!first)
                {
                    Hashtable keys = new Hashtable
                        {
                            {"value",_setUrlString.Replace("&","%26") }
                        };
                    StartCoroutine(SceneController.GetInterface<StolperwegeInterface>().CreateElement("uri", keys, (StolperwegeElement se) => { SetDiscourseReferent((StolperwegeUri)se); }, true));
                }
                else
                    first = false;
                
               //mainUIPanel.UrlField.text = _setUrlString;

            }
           


            }

        public void SendKey(Char c)
        {
            Debug.Log("send" + c);
            int code = (c == ' ') ? (int)KeyCode.Space : (int)Enum.Parse(typeof(KeyCode), ("" + c).ToUpper());
            _mainEngine.SendCharEvent(code, KeyboardEventType.CharKey);
        }

        private void SetDiscourseReferent(StolperwegeUri uri)
        {
            StolperwegeObject oldUri = getStolperwegeUri().StolperwegeObject;

            uri.Object3D.transform.parent = oldUri.transform.parent;
            uri.Object3D.transform.localPosition = oldUri.transform.localPosition;
            uri.Object3D.transform.localRotation = oldUri.transform.localRotation;
            uri.Object3D.transform.localScale = oldUri.transform.localScale;

            GetComponentInParent<ExpandView>().StObject = uri.StolperwegeObject;

            oldUri.transform.parent = null;
            oldUri.HideRelatedObjects();
            oldUri.gameObject.SetActive(false);

            uri.StolperwegeObject.ShowAllRelatedObjects();

        }

        #region Keys

        private void ProcessKeyEvents()
        {
            foreach (KeyCode k in Enum.GetValues(typeof (KeyCode)))
            {
                CheckKey(k);
            }

        }

        private void CheckKey(KeyCode code)
        {
            if (Input.GetKeyDown(code))
                _mainEngine.SendCharEvent((int) code, KeyboardEventType.Down);
            if (Input.GetKeyUp(KeyCode.Backspace))
                _mainEngine.SendCharEvent((int) code, KeyboardEventType.Up);
        }

        #endregion

        void OnDisable()
        {
            _mainEngine.Shutdown();
        }

        private void MainBrowser_OnJSQuery(string query)
        {
            Debug.Log("Javascript query:" + query);
            this.RespondToJSQuery("My response: OK");
            if (query == null || query.Equals("")) return;
            if (query.StartsWith("click:"))
                clickedHTML = query.Replace("click:", "");
            else
                selectedText = query;
        }

        // never used
        //public event BrowserEngine.PageLoaded OnPageLoaded;
    }

    
}
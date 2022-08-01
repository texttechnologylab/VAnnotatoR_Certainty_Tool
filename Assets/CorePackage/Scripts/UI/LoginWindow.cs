using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using TMPro;
using Valve.VR;

public class LoginWindow : AnimatedWindow
{

    private InteractiveButton Left;
    private InteractiveButton Right;
    private InteractiveCheckbox Remember;
    private InteractiveButton ShowPasswordBtn;
    private InteractiveButton Login;
    private InteractiveButton Reset;

    private KeyboardEditText Username;
    private KeyboardEditText Password;
    private LoginData CachedLogin;

    private TextMeshPro SpaceDisplay;
    private TextMeshPro StatusDisplay;

    private List<Interface> Interfaces;
    public int InterfaceCount {  get { return Interfaces.Count; } }

    private int _interfacePointer;
    public int InterfacePointer
    {
        get { return _interfacePointer; }
        set
        {
            _interfacePointer = (value < 0) ? Interfaces.Count - 1 : value % Interfaces.Count;
            CachedLogin = LocalCacheHandler.GetCredentials(Interfaces[_interfacePointer].Name);
            Username.Text = CachedLogin == null ? "" : CachedLogin.Username;
            Password.Text = CachedLogin == null ? "" : "Nice try ;)";
            Left.Active = InterfaceCount > 1;
            Right.Active = InterfaceCount > 1;
            SpaceDisplay.text = Interfaces[_interfacePointer].Name;
            StatusDisplay.text = "";
        }
    }

    private bool _initialized;

    private void BaseInit()
    {
        Start();
        name = "Login";
        Interfaces = new List<Interface>();
        foreach (Interface iFace in SceneController.Interfaces)
            if (iFace.OnLogin != null) Interfaces.Add(iFace);

        Left = transform.Find("Left").GetComponent<InteractiveButton>();
        Left.OnClick = () => 
        {
            StatusDisplay.text = "";
            InterfacePointer -= 1;
        };

        Right = transform.Find("Right").GetComponent<InteractiveButton>();
        Right.OnClick = () => 
        {
            StatusDisplay.text = "";
            InterfacePointer += 1;
        };

        Remember = transform.Find("RememberCheckbox").GetComponent<InteractiveCheckbox>();
        Remember.OnClick = () => { Remember.ButtonOn = !Remember.ButtonOn; };
        Remember.ButtonOn = false;

        ShowPasswordBtn = transform.Find("ShowPassword").GetComponent<InteractiveButton>();
        ShowPasswordBtn.AsyncClick = ShowPassword;

        Login = transform.Find("Login").GetComponent<InteractiveButton>();
        Login.AsyncClick = StartLogin;

        Reset = transform.Find("Reset").GetComponent<InteractiveButton>();
        Reset.OnClick = () => 
        {
            Username.Text = "";
            Password.Text = "";
            StatusDisplay.text = "";
        };

        Username = transform.Find("UserNameInput").GetComponent<KeyboardEditText>();
        Username = transform.Find("UserNameInput").GetComponent<KeyboardEditText>();
        Username.Text = Username.Description;
        Username.OnClick = () => 
        { 
            StatusDisplay.text = "";
            Username.ActivateWriter();
        };

        Password = transform.Find("PasswordInput").GetComponent<KeyboardEditText>();
        Password = transform.Find("PasswordInput").GetComponent<KeyboardEditText>();
        Password.Text = Password.Description;
        Password.OnClick = () => 
        {
            StatusDisplay.text = "";
            Password.ActivateWriter();
        };

        SpaceDisplay = transform.Find("SpaceDisplay/Display").GetComponent<TextMeshPro>();
        StatusDisplay = transform.Find("StatusDisplay").GetComponent<TextMeshPro>();

        DestroyOnObjectRemover = false;
        OnRemove = () => { Active = false; };

        InterfacePointer = 0;
        _active = true;
        Active = false;

        _initialized = true;
    }

    public void Init(Interface iFace)
    {
        if (!_initialized) BaseInit();
        for (int i=0; i<Interfaces.Count; i++)
        {
            if (Interfaces[i].GetType().Equals(iFace.GetType()))
            {
                InterfacePointer = i;
                return;
            }                
        }
    }

    public void Init()
    {
        if (!_initialized) BaseInit();
        InterfacePointer = 0;
    }

    private float _loginRequestTimer;
    public override void Update()
    {
        base.Update();

        if (Password != null && !Password.Private && !ShowPasswordBtn.Highlight) SetPasswortVisibility(false);

        if (loginRequestEnded)
        {
            _loginRequestTimer += Time.deltaTime;
            if (_loginRequestTimer > 3)
            {
                _loginRequestTimer = 0;
                loginRequestEnded = false;
                StatusDisplay.text = "";
            }
        }

    }

    private void SetPasswortVisibility(bool status)
    {
        Password.Private = !status;
        Password.Text = Password.Text;
    }

    private IEnumerator ShowPassword()
    {
        SetPasswortVisibility(true);
        while (SteamVR_Actions.default_trigger.GetState(StolperwegeHelper.User.PointerHandType))
            yield return null;
        SetPasswortVisibility(false);

    }

    private LoginData login; private bool loginRunning; private float _loadingStatusTimer; private bool loginRequestEnded;
    private string[] statusArray = new string[] { "\xf251", "\xf252", "\xf253" }; private string actualStatus; bool loginSuccess;
    private IEnumerator StartLogin()
    {
        if (Password.Text == "" || Username.Text == "") yield break;

        LoginData loginData;

        if (CachedLogin != null && Username.Text.Equals(CachedLogin.Username) &&
            Password.Text.Equals("Nice try ;)"))
            loginData = CachedLogin;
        else 
            loginData = new LoginData(Username.Text, StolperwegeHelper.Md5Sum(Password.Text));

        loginRunning = true;
        SetInterfaceStatus(false);
        loginSuccess = false;

        Interfaces[InterfacePointer].OnLogin.Invoke(loginData, (bool success, string message) => { OnLoginResult(success, message); });

        _loadingStatusTimer = 0;
        while (loginRunning)
        {
            _loadingStatusTimer += Time.deltaTime;
            if (_loadingStatusTimer >= 3) _loadingStatusTimer = 0;
            actualStatus = statusArray[(int)_loadingStatusTimer];
            if (!actualStatus.Equals(StatusDisplay.text)) StatusDisplay.text = actualStatus;            
            yield return null;
        }

        SetInterfaceStatus(true);

        if (loginSuccess)
        {
            if (Remember.ButtonOn) LocalCacheHandler.StoreCredentials(Interfaces[InterfacePointer].Name, loginData);
            yield return new WaitForSeconds(2);
            Active = false;
        }
        
    }

    public void OnLoginResult(bool success, string message)
    {
        StatusDisplay.text = message;
        loginRunning = false;
        loginRequestEnded = true;
        loginSuccess = success;
    }

    private void SetInterfaceStatus(bool status)
    {
        Left.Active = status && Interfaces.Count > 1;
        Right.Active = status && Interfaces.Count > 1;
        Login.Active = status;
        Reset.Active = status;
        Username.GetComponent<Collider>().enabled = status;
        Password.GetComponent<Collider>().enabled = status;
    }
}

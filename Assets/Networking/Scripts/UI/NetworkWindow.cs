using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class NetworkWindow : AnimatedWindow
{

    private NetworkModule _networkModule;
    private InteractiveButton _closeButton;
    private InteractiveButton _general;
    private InteractiveButton _communication;

    // General tab variables
    #region
    private GameObject _generalTab;
    private InteractiveButton _offlineButton;
    private InteractiveButton _hostButton;
    private InteractiveButton _clientButton;
    public TextMeshPro GeneralStatusDisplay { get; private set; }
    public KeyboardEditText PortInputfield { get; private set; }
    #endregion

    // Communication tab variables
    #region
    private GameObject _communicationTab;
    private InteractiveButton _vivoxLoginButton;
    private KeyboardEditText _channelInputfield;
    private InteractiveButton _joinChButton;
    private InteractiveButton _disconnectButton;
    public TextMeshPro CommunicationStatusDisplay { get; private set; }
    #endregion

    public override bool Active 
    {
        get => base.Active;
        set
        {
            base.Active = value;
            ActualizeStatus();
            ActiveTab = ActiveTab;
        }
    }

    private int _activeTab;
    public int ActiveTab
    {
        get { return _activeTab; }
        set
        {
            _activeTab = value;
            _general.ButtonOn = _activeTab == 0;
            _communication.ButtonOn = _activeTab == 1;
            _generalTab.SetActive(_general.ButtonOn);
            _communicationTab.SetActive(_communication.ButtonOn);
        }
    }

    public override void Start()
    {
        base.Start();
        _networkModule = SceneController.GetInterface<NetworkModule>();
        SpawnDistanceMultiplier = 0.9f;

        _general = transform.Find("General").GetComponent<InteractiveButton>();
        _general.OnClick = () =>
        {
            if (_general.ButtonOn) return;
            ActiveTab = 0;
        };

        _communication = transform.Find("Communication").GetComponent<InteractiveButton>();
        _communication.OnClick = () =>
        {
            if (_communication.ButtonOn) return;
            ActiveTab = 1;
        };

        _closeButton = transform.Find("CloseButton").GetComponent<InteractiveButton>();
        _closeButton.OnClick = () => { Active = !Active; };

        // General tab variables
        _generalTab = transform.Find("GeneralTab").gameObject;
        GeneralStatusDisplay = _generalTab.transform.Find("StatusDisplay/Display").GetComponent<TextMeshPro>();

        PortInputfield = _generalTab.transform.Find("Port/PortInputfield").GetComponent<KeyboardEditText>();
        PortInputfield.IsNumberField = true;
        PortInputfield.EnableNegativeNumber = false;
        PortInputfield.EnableFloatingPoint = false;
        PortInputfield.OnCommit = (text, gameObject) =>
        {
            while (text.Length > 0 && text[0] == '0')
                text = text.Substring(1);
            if (text.Length == 0) text = "" + NetworkModule.DefaultPort;
            PortInputfield.Text = text;
            _networkModule.Port = ushort.Parse(text);
        };

        _offlineButton = _generalTab.transform.Find("OfflineButton").GetComponent<InteractiveButton>();
        _offlineButton.OnClick = () =>
        {
            if (_networkModule.NetworkManager.isNetworkActive)
            {
                if (_networkModule.NetworkManager.mode == NetworkManagerMode.ClientOnly)
                    _networkModule.NetworkManager.StopClient();
                if (_networkModule.NetworkManager.mode == NetworkManagerMode.Host)
                    _networkModule.NetworkManager.StopHost();
            }
        };

        _hostButton = _generalTab.transform.Find("HostButton").GetComponent<InteractiveButton>();
        _hostButton.OnClick = () =>
        {
            _networkModule.NetworkManager.StartHost();
            StartCoroutine(WaitForConnection());
        };

        _clientButton = _generalTab.transform.Find("ClientButton").GetComponent<InteractiveButton>();
        _clientButton.OnClick = () =>
        {
            _networkModule.NetworkManager.StartClient();
            StartCoroutine(WaitForConnection());
        };

        // Communication tab variables
        _communicationTab = transform.Find("CommunicationTab").gameObject;
        _vivoxLoginButton = _communicationTab.transform.Find("LoginButton").GetComponent<InteractiveButton>();
        _vivoxLoginButton.ChangeText("Login");
        _vivoxLoginButton.OnClick = () =>
        {
            _vivoxLoginButton.Active = false;
            if (_networkModule.VoiceManager.LoginState == VivoxUnity.LoginState.LoggedOut)
            {
                _networkModule.VoiceManager.OnUserLoggedInEvent += OnVivoxLogin;
                _networkModule.VoiceManager.Login();                
            }
            else if (_networkModule.VoiceManager.LoginState == VivoxUnity.LoginState.LoggedIn)
            {
                _networkModule.VoiceManager.OnUserLoggedOutEvent += OnVivoxLogout;
                _networkModule.VoiceManager.Logout();
            }
        };

        _channelInputfield = _communicationTab.transform.Find("Channel/ChannelInputfield").GetComponent<KeyboardEditText>();
        _channelInputfield.Text = "";
        _channelInputfield.OnCommit = (text, go) => { _joinChButton.Active = text != null && text.Length > 0; };

        _joinChButton = _communicationTab.transform.Find("JoinButton").GetComponent<InteractiveButton>();
        _joinChButton.OnClick = () =>
        {
            if (_channelInputfield.Text != null && _channelInputfield.Text.Length > 0)
            {
                _networkModule.VoiceManager.JoinChannel(_channelInputfield.Text, VivoxUnity.ChannelType.NonPositional, VivoxVoiceManager.ChatCapability.TextAndAudio);
                _joinChButton.Active = false;
                _disconnectButton.Active = true;
            }
        };
        _joinChButton.Active = false;

        _disconnectButton = _communicationTab.transform.Find("DisconnectButton").GetComponent<InteractiveButton>();
        _disconnectButton.OnClick = () =>
        {
            _networkModule.VoiceManager.DisconnectAllChannels();
            _joinChButton.Active = _channelInputfield.Text != null && _channelInputfield.Text.Length > 0;
            _disconnectButton.Active = false;

        };
        _disconnectButton.Active = false;
    }

    public void ActualizeStatus()
    {
        _offlineButton.Active = _networkModule.NetworkManager.isNetworkActive;
        _hostButton.Active = !_networkModule.NetworkManager.isNetworkActive;
        _clientButton.Active = !_networkModule.NetworkManager.isNetworkActive;
        if (_networkModule.NetworkManager.isNetworkActive)
        {
            GeneralStatusDisplay.text = "You are using VAnnotatoR online. Role: ";
            GeneralStatusDisplay.text += (_networkModule.NetworkManager.mode == NetworkManagerMode.ClientOnly) ? "client." : "host.";
        } else
            GeneralStatusDisplay.text = "You are using VAnnotatoR offline.";
    }

    private IEnumerator WaitForConnection()
    {
        while (!NetworkClient.isConnected) 
            yield return null;

        ClientScene.Ready(NetworkClient.connection);

        if (ClientScene.localPlayer == null)
            ClientScene.AddPlayer(NetworkClient.connection);
    }

    private void OnVivoxLogin()
    {
        _networkModule.VoiceManager.OnUserLoggedInEvent -= OnVivoxLogin;
        _vivoxLoginButton.Active = true;
        _vivoxLoginButton.ChangeText("Logout");
    }

    private void OnVivoxLogout()
    {
        _networkModule.VoiceManager.OnUserLoggedInEvent -= OnVivoxLogout;
        _vivoxLoginButton.Active = true;
        _vivoxLoginButton.ChangeText("Login");
    }

}

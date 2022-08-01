using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Mirror;

public class NetworkModule : Interface
{

    public const ushort DefaultPort = 7777;

    private ushort _port;
    public ushort Port
    {
        get { return _port; }
        set
        {
            _port = value;
            TelepathyTransport.port = _port;
        }
    }

    public ExtendedNetworkManager NetworkManager;
    public TelepathyTransport TelepathyTransport;
    public NetworkWindow NetworkWindow { get; private set; }
    public VivoxVoiceManager VoiceManager { get; private set; }

    protected override IEnumerator InitializeInternal()
    {
        Name = "Networking";
        while (!SceneController.Initialized) yield return null;
        NetworkManager = ((GameObject)Instantiate(Resources.Load("Prefabs/NetworkManager"))).GetComponent<ExtendedNetworkManager>();        
        NetworkWindow = ((GameObject)Instantiate(Resources.Load("Prefabs/NetworkWindow"))).GetComponent<NetworkWindow>();
        VoiceManager = ((GameObject)Instantiate(Resources.Load("Prefabs/VivoxVoiceManager"))).GetComponent<VivoxVoiceManager>();
        NetworkWindow.Start();
        NetworkWindow.gameObject.transform.SetParent(transform);
        NetworkWindow.SetVisible(false);        
        TelepathyTransport = NetworkManager.GetComponent<TelepathyTransport>();
        _port = TelepathyTransport.port;
        NetworkWindow.PortInputfield.Text = "" + Port;
        //StolperwegeHelper.User.gameObject.AddComponent<NetworkIdentity>();
        //StolperwegeHelper.User.gameObject.AddComponent<NetworkTransform>();
        //while (OpenVR.System.GetTrackedDeviceActivityLevel(0) !=
        //       EDeviceActivityLevel.k_EDeviceActivityLevel_UserInteraction)
        //    yield return null;
        //NetworkWindow.Active = true;
    }
}

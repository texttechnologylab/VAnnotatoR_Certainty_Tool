using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Rendering.Universal;

public class ExtendedNetworkManager : NetworkManager
{

    private NetworkModule NetworkModule;

    public override void Awake()
    {
        base.Awake();
        autoCreatePlayer = false;
        playerPrefab = (GameObject)Resources.Load("Prefabs/Avatar/Avatar");
        AddNetworkComponentsToAvatar(playerPrefab);
        NetworkModule = SceneController.GetInterface<NetworkModule>();
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
        GameObject avatar = (playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, avatar);

    }

    public override void OnStartClient()
    {
        if (NetworkModule.NetworkWindow.Active)
            NetworkModule.NetworkWindow.ActualizeStatus();        
    }

    public override void OnStopClient()
    {
        if (NetworkModule.NetworkWindow.Active)
            NetworkModule.NetworkWindow.ActualizeStatus();
    }

    public override void OnStartHost()
    {
        if (NetworkModule.NetworkWindow.Active)
            NetworkModule.NetworkWindow.ActualizeStatus();
    }

    public override void OnStopHost()
    {
        if (NetworkModule.NetworkWindow.Active)
            NetworkModule.NetworkWindow.ActualizeStatus();
    }

    public override void OnStartServer()
    {
        if (NetworkModule.NetworkWindow.Active)
            NetworkModule.NetworkWindow.ActualizeStatus();
    }

    public override void OnStopServer()
    {
        if (NetworkModule.NetworkWindow.Active)
            NetworkModule.NetworkWindow.ActualizeStatus();
    }

    private void AddNetworkComponentsToAvatar(GameObject avatar)
    {
        // Add a network identity to the avatar, if it does not have one
        if (avatar.GetComponent<NetworkIdentity>() == null)
            avatar.AddComponent<NetworkIdentity>();

        // Add a network transform to the avatar, if it does not have one
        if (avatar.GetComponent<NetworkTransform>() == null)
            avatar.AddComponent<NetworkTransform>();

        // Add a network behaviour to the avatar, if it does not have one
        if (avatar.GetComponent<NetworkAvatarController>() == null)
            avatar.AddComponent<NetworkAvatarController>();

        // Check if the avatar has any of network transform child components
        // and destroy them
        NetworkTransformChild[] ntcs = avatar.GetComponents<NetworkTransformChild>();
        for (int i = 0; i < ntcs.Length; i++)
            DestroyImmediate(ntcs[i], true);

        // Add and setup network transform child components for 
        // the left and right hand and the head
        NetworkTransformChild leftHand = avatar.AddComponent<NetworkTransformChild>();
        leftHand.target = avatar.transform.Find("LeftHand");
        NetworkTransformChild rightHand = avatar.AddComponent<NetworkTransformChild>();
        rightHand.target = avatar.transform.Find("RightHand");
        NetworkTransformChild head = avatar.AddComponent<NetworkTransformChild>();
        head.target = avatar.transform.Find("CenterEyeAnchor");

        // Add a network animator component for each hand
        NetworkAnimator leftHandAnim = avatar.AddComponent<NetworkAnimator>();
        leftHandAnim.animator = avatar.transform.Find("LeftHand").GetComponent<Animator>();
        leftHandAnim.clientAuthority = true;
        NetworkAnimator rightHandAnim = avatar.AddComponent<NetworkAnimator>();
        rightHandAnim.animator = avatar.transform.Find("RightHand").GetComponent<Animator>();
        rightHandAnim.clientAuthority = true;
    }

    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        base.OnDestroy();

        DestroyImmediate(playerPrefab.GetComponent<NetworkTransform>(), true);
        DestroyImmediate(playerPrefab.GetComponent<NetworkAvatarController>(), true);

        NetworkTransformChild[] ntcs = playerPrefab.GetComponents<NetworkTransformChild>();
        for (int i = 0; i < ntcs.Length; i++)
            DestroyImmediate(ntcs[i], true);

        NetworkAnimator[] animators = playerPrefab.GetComponents<NetworkAnimator>();
        for (int i = 0; i < animators.Length; i++)
            DestroyImmediate(animators[i], true);

        DestroyImmediate(playerPrefab.GetComponent<NetworkIdentity>(), true);
    }


}

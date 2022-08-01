using System;

[Menu(PrefabPath + "StandardMenuItem", true)]
public class NetworkMenu : MenuScript
{


    public override void Start()
    {
        base.Start();
        SetNameAndIcon("Network", "\xf6ff");
        OnClick = SetNetworkWindowsStatus;
    }

    private void SetNetworkWindowsStatus()
    {
        if (SceneController.GetInterface<NetworkModule>() == null) throw new Exception("Network module is missing.");
        SceneController.GetInterface<NetworkModule>().NetworkWindow.Active = !SceneController.GetInterface<NetworkModule>().NetworkWindow.Active;
        MainMenuCallback();
    }
}

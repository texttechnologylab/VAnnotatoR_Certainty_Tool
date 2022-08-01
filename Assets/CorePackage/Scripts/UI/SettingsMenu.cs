using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[Menu(PrefabPath + "StandardMenuItem", true)]
public class SettingsMenu : MenuScript
{

    private SettingsWindow SettingsWindow;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        SetNameAndIcon("Settings", "\xf013");
        OnClick = SetSettingsWindowsStatus;
    }

    private void SetSettingsWindowsStatus()
    {
        if (SettingsWindow == null)
        {
            SettingsWindow = ((GameObject)Instantiate(Resources.Load("Prefabs/UI/SettingsWindow"))).GetComponent<SettingsWindow>();
            SettingsWindow.Start();
        }
        SettingsWindow.Active = !SettingsWindow.Active;
    }
}

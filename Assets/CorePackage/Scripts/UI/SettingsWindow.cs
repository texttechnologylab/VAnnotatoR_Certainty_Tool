using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SettingsWindow : AnimatedWindow
{

    private InteractiveButton _closeButton;
    private InteractiveButton _general;

    // General tab variables
    #region
    private GameObject _generalTab;
    private InteractiveButton _pointerHandButton;
    private InteractiveButton _emptyCacheButton;
    #endregion

    public override bool Active
    {
        get => base.Active;
        set
        {
            base.Active = value;            
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
            _generalTab.SetActive(_general.ButtonOn);
            if (_generalTab.activeInHierarchy)
            {
                _pointerHandButton.ButtonValue = LocalCacheHandler.GetDefaultPointerSide();
                _pointerHandButton.ChangeText((SteamVR_Input_Sources)_pointerHandButton.ButtonValue == SteamVR_Input_Sources.LeftHand ? "Left" : "Right");
            }
                
        }
    }

    public override void Start()
    {
        base.Start();
        SpawnDistanceMultiplier = 0.9f;

        _general = transform.Find("General").GetComponent<InteractiveButton>();
        _general.OnClick = () =>
        {
            if (_general.ButtonOn) return;
            ActiveTab = 0;
        };

        _closeButton = transform.Find("CloseButton").GetComponent<InteractiveButton>();
        _closeButton.OnClick = () => { Active = !Active; };

        // General tab variables
        _generalTab = transform.Find("GeneralTab").gameObject;
        _pointerHandButton = _generalTab.transform.Find("DefaultPointerHand/PointerHandButton").GetComponent<InteractiveButton>();
        _pointerHandButton.OnClick = () =>
        {
            if (_pointerHandButton.ButtonValue.Equals(SteamVR_Input_Sources.RightHand))
            {
                _pointerHandButton.ButtonValue = SteamVR_Input_Sources.LeftHand;
                _pointerHandButton.ChangeText("Left");
            }
            else
            {
                _pointerHandButton.ButtonValue = SteamVR_Input_Sources.RightHand;
                _pointerHandButton.ChangeText("Right");
            }
            LocalCacheHandler.SetDefaultPointingHand((SteamVR_Input_Sources)_pointerHandButton.ButtonValue);
        };

        _emptyCacheButton = _generalTab.transform.Find("EmptyCache/DeleteButton").GetComponent<InteractiveButton>();
        _emptyCacheButton.OnLongClick = LocalCacheHandler.DeleteCredentials;
        _emptyCacheButton.LoadingText = "Stored credentials will be removed, are you sure?";
    }


}

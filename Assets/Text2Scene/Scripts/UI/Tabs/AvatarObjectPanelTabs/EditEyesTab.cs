using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EditEyesTab : AvatarObjectPanelTab
{
    private InteractiveSlider EarSizeSlider;
    private InteractiveSlider EarRotationSlider;
    private InteractiveSlider EarPositionSlider;
    private InteractiveSlider EyeSizeSlider;
    private InteractiveSlider EyeRotationSlider;
    private InteractiveSlider EyeSpacingSlider;
    private InteractiveSlider CheekSizeSlider;
    private InteractiveSlider CheekPositionSlider;
    private InteractiveSlider LowCheekPositionSlider;
    private UMAController AvatarController;

    public override AvatarObjectPanelTab Initialize(AvatarObjectPanelTab parent)
    {
        _parentTab = parent;
        iShapeObject = parent.iShapeObject;

        TabButton = GameObject.Find("EyesButton").GetComponent<InteractiveButton>();
        if (TabButton == null) Debug.LogWarning("Warning! TabButton for tab 'EditEyes' could not be found");
        else
        {
            TabButton.OnClick = () =>
            {
                _parentTab.ActiveTab = this;
            };
        }
        AvatarController = iShapeObject.transform.GetComponentInChildren<UMAController>();

        EarSizeSlider = transform.Find("EditEarSize").GetComponentInChildren<InteractiveSlider>();
        EarRotationSlider = transform.Find("EditEarRotation").GetComponentInChildren<InteractiveSlider>();
        EarPositionSlider = transform.Find("EditEarPosition").GetComponentInChildren<InteractiveSlider>();
        EyeSizeSlider = transform.Find("EditEyeSize").GetComponentInChildren<InteractiveSlider>();
        EyeRotationSlider = transform.Find("EditEyeRotation").GetComponentInChildren<InteractiveSlider>();
        EyeSpacingSlider = transform.Find("EditEyeSpacing").GetComponentInChildren<InteractiveSlider>();
        CheekSizeSlider = transform.Find("EditCheekSize").GetComponentInChildren<InteractiveSlider>();
        CheekPositionSlider = transform.Find("EditCheekPosition").GetComponentInChildren<InteractiveSlider>();
        LowCheekPositionSlider = transform.Find("EditLowCheekPosition").GetComponentInChildren<InteractiveSlider>();

        EarSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.EARS_SIZE));
        EarRotationSlider.Initialize(AvatarController.GetValue(UMAProperty.EARS_ROTATION));
        EarPositionSlider.Initialize(AvatarController.GetValue(UMAProperty.UPPER_WEIGHT));
        EyeSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.EYE_SIZE));
        EyeRotationSlider.Initialize(AvatarController.GetValue(UMAProperty.EYE_ROTATION));
        EyeSpacingSlider.Initialize(AvatarController.GetValue(UMAProperty.EYE_SPACING));
        CheekSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.CHEEK_SIZE));
        CheekPositionSlider.Initialize(AvatarController.GetValue(UMAProperty.CHEEK_POSITION));
        LowCheekPositionSlider.Initialize(AvatarController.GetValue(UMAProperty.LOW_CHEEK_POSITION));

        EarSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeEarsSize(n); };
        EarRotationSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeEarsRotation(n); };
        EarPositionSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeEarsPosition(n); };
        EyeSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeEyeSize(n); };
        EyeRotationSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeEyeRotation(n); };
        EyeSpacingSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeEyeSpacing(n); };
        CheekSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeCheekSize(n); };
        CheekPositionSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeCheekPositon(n); };
        LowCheekPositionSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeLowCheekPosition(n); }; 

        SetActive(false);
        return this;
    }

    public override void Save(UMAISOObject umaISOObject)
    {
        umaISOObject.Ears.SetProperty(UMAProperty.EARS_SIZE, AvatarController.GetValue(UMAProperty.EARS_SIZE).ToString());
        umaISOObject.Ears.SetProperty(UMAProperty.EARS_ROTATION, AvatarController.GetValue(UMAProperty.EARS_ROTATION).ToString());
        umaISOObject.Ears.SetProperty(UMAProperty.EARS_POSITION, AvatarController.GetValue(UMAProperty.UPPER_WEIGHT).ToString());
        umaISOObject.Eyes.SetProperty(UMAProperty.EYE_SIZE, AvatarController.GetValue(UMAProperty.EYE_SIZE).ToString());
        umaISOObject.Eyes.SetProperty(UMAProperty.EYE_ROTATION, AvatarController.GetValue(UMAProperty.EYE_ROTATION).ToString());
        umaISOObject.Eyes.SetProperty(UMAProperty.EYE_SPACING, AvatarController.GetValue(UMAProperty.EYE_SPACING).ToString());
        umaISOObject.Face.SetProperty(UMAProperty.CHEEK_SIZE, AvatarController.GetValue(UMAProperty.CHEEK_SIZE).ToString());
        umaISOObject.Face.SetProperty(UMAProperty.CHEEK_POSITION, AvatarController.GetValue(UMAProperty.CHEEK_POSITION).ToString());
        umaISOObject.Face.SetProperty(UMAProperty.LOW_CHEEK_POSITION, AvatarController.GetValue(UMAProperty.LOW_CHEEK_POSITION).ToString());
        base.Save(umaISOObject);
    }
}
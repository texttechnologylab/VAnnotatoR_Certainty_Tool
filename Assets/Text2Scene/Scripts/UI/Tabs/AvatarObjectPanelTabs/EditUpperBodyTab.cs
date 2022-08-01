using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EditUpperBodyTab : AvatarObjectPanelTab
{
    private InteractiveSlider NeckThicknessSlider;
    private InteractiveSlider BreastSizeSlider;
    private InteractiveSlider ArmLengthSlider;
    private InteractiveSlider ArmThicknessSlider;
    private InteractiveSlider ForeArmSizeSlider;
    private InteractiveSlider ForeArmPositionSlider;
    private InteractiveSlider HandSizeSlider;

    private UMAController AvatarController;
    public override AvatarObjectPanelTab Initialize(AvatarObjectPanelTab parent)
    {
        _parentTab = parent;
        iShapeObject = parent.iShapeObject;

        TabButton = GameObject.Find("UpperBodyButton").GetComponent<InteractiveButton>();
        if (TabButton == null) Debug.LogWarning("Warning! TabButton for tab 'EditUpperBody' could not be found");
        else
        {
            TabButton.OnClick = () =>
            {
                _parentTab.ActiveTab = this;
            };
        }
        AvatarController = iShapeObject.transform.GetComponentInChildren<UMAController>();

        NeckThicknessSlider = transform.Find("EditNeckThickness").GetComponentInChildren<InteractiveSlider>();
        BreastSizeSlider = transform.Find("EditBreastSize").GetComponentInChildren<InteractiveSlider>();
        ArmLengthSlider = transform.Find("EditArmLength").GetComponentInChildren<InteractiveSlider>();
        ArmThicknessSlider = transform.Find("EditArmThickness").GetComponentInChildren<InteractiveSlider>();
        ForeArmSizeSlider = transform.Find("EditForeArmSize").GetComponentInChildren<InteractiveSlider>();
        ForeArmPositionSlider = transform.Find("EditForeArmPosition").GetComponentInChildren<InteractiveSlider>();
        HandSizeSlider = transform.Find("EditHandSize").GetComponentInChildren<InteractiveSlider>();

        NeckThicknessSlider.Initialize(AvatarController.GetValue(UMAProperty.NECK_THICKNESS));
        BreastSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.BREAST_SIZE));
        ArmLengthSlider.Initialize(AvatarController.GetValue(UMAProperty.ARM_LENGTH));
        ArmThicknessSlider.Initialize(AvatarController.GetValue(UMAProperty.ARM_WIDTH));
        ForeArmSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.FOREARM_LENGTH));
        ForeArmPositionSlider.Initialize(AvatarController.GetValue(UMAProperty.FOREARM_POSTION));
        HandSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.HAND_SIZE));

        NeckThicknessSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeNeckThickness(n); };
        BreastSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeBreastSize(n); };
        ArmLengthSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeArmLength(n); };
        ArmThicknessSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeArmWidth(n); };
        ForeArmSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeForearmLength(n); };
        ForeArmPositionSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeForearmWidth(n); };
        HandSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeHandsSize(n); };

        SetActive(false);
        return this;
    }

    public override void Save(UMAISOObject umaISOObject)
    {
        umaISOObject.Head.SetProperty(UMAProperty.NECK_THICKNESS, AvatarController.GetValue(UMAProperty.NECK_THICKNESS).ToString());
        umaISOObject.Torso.SetProperty(UMAProperty.BREAST_SIZE, AvatarController.GetValue(UMAProperty.BREAST_SIZE).ToString());
        umaISOObject.Arms.SetProperty(UMAProperty.ARM_LENGTH, AvatarController.GetValue(UMAProperty.ARM_LENGTH).ToString());
        umaISOObject.Arms.SetProperty(UMAProperty.ARM_WIDTH, AvatarController.GetValue(UMAProperty.ARM_WIDTH).ToString());
        umaISOObject.Arms.SetProperty(UMAProperty.FOREARM_LENGTH, AvatarController.GetValue(UMAProperty.FOREARM_LENGTH).ToString());
        umaISOObject.Arms.SetProperty(UMAProperty.FOREARM_POSTION, AvatarController.GetValue(UMAProperty.FOREARM_POSTION).ToString());
        umaISOObject.Arms.SetProperty(UMAProperty.HAND_SIZE, AvatarController.GetValue(UMAProperty.HAND_SIZE).ToString());
        base.Save(umaISOObject);
    }
}
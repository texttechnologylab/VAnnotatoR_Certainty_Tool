using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EditLowerBodyTab : AvatarObjectPanelTab
{
    private InteractiveSlider GluteusMaximusSizeSlider;
    private InteractiveSlider LegSpacingSlider;
    private InteractiveSlider LegLengthSlider;
    private InteractiveSlider FeetSizeSlider;

    private UMAController AvatarController;
    public override AvatarObjectPanelTab Initialize(AvatarObjectPanelTab parent)
    {
        _parentTab = parent;
        iShapeObject = parent.iShapeObject;

        TabButton = GameObject.Find("LowerBodyButton").GetComponent<InteractiveButton>();
        if (TabButton == null) Debug.LogWarning("Warning! TabButton for tab 'EditLowerBody' could not be found");
        else
        {
            TabButton.OnClick = () =>
            {
                _parentTab.ActiveTab = this;
            };
        }
        AvatarController = iShapeObject.transform.GetComponentInChildren<UMAController>();

        GluteusMaximusSizeSlider = transform.Find("EditGluteusMaximusSize").GetComponentInChildren<InteractiveSlider>();
        LegSpacingSlider = transform.Find("EditLegSpacing").GetComponentInChildren<InteractiveSlider>();
        LegLengthSlider = transform.Find("EditLegLength").GetComponentInChildren<InteractiveSlider>();
        FeetSizeSlider = transform.Find("EditFeetSize").GetComponentInChildren<InteractiveSlider>();

        GluteusMaximusSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.GLUTEUS_SIZE));
        LegSpacingSlider.Initialize(AvatarController.GetValue(UMAProperty.LEG_SEPERATION));
        LegLengthSlider.Initialize(AvatarController.GetValue(UMAProperty.LEGS_SIZE));
        FeetSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.FEET_SIZE));

        GluteusMaximusSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeGluteusSize(n); };
        LegSpacingSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeLegSeparation(n); };
        LegLengthSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeLegsSize(n); };
        FeetSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeFeetSize(n); };

        SetActive(false);
        return this;
    }

    public override void Save(UMAISOObject umaISOObject)
    {
        umaISOObject.Torso.SetProperty(UMAProperty.GLUTEUS_SIZE, AvatarController.GetValue(UMAProperty.GLUTEUS_SIZE).ToString());
        umaISOObject.Legs.SetProperty(UMAProperty.LEG_SEPERATION, AvatarController.GetValue(UMAProperty.LEG_SEPERATION).ToString());
        umaISOObject.Legs.SetProperty(UMAProperty.LEGS_SIZE, AvatarController.GetValue(UMAProperty.LEGS_SIZE).ToString());
        umaISOObject.Legs.SetProperty(UMAProperty.FEET_SIZE, AvatarController.GetValue(UMAProperty.FEET_SIZE).ToString());
        base.Save(umaISOObject);
    }
}
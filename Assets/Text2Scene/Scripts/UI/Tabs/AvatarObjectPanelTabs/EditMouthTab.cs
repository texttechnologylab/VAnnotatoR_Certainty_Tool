using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EditMouthTab : AvatarObjectPanelTab
{
    private InteractiveSlider MouthSizeSlider;
    private InteractiveSlider LipSizeSlider;
    private InteractiveSlider JawPositionSlider;
    private InteractiveSlider JawSizeSlider;
    private InteractiveSlider MandibleSizeSlider;
    private InteractiveSlider ChinPositionSlider;
    private InteractiveSlider ChinSizeSlider;
    private InteractiveSlider ChinPronouncedSlider;
    private UMAController AvatarController;

    public override AvatarObjectPanelTab Initialize(AvatarObjectPanelTab parent)
    {
        _parentTab = parent;
        iShapeObject = parent.iShapeObject;

        TabButton = GameObject.Find("MouthButton").GetComponent<InteractiveButton>();
        if (TabButton == null) Debug.LogWarning("Warning! TabButton for tab 'EditMouth' could not be found");
        else
        {
            TabButton.OnClick = () =>
            {
                _parentTab.ActiveTab = this;
            };
        }
        AvatarController = iShapeObject.transform.GetComponentInChildren<UMAController>();

        MouthSizeSlider = transform.Find("EditMouthSize").GetComponentInChildren<InteractiveSlider>();
        LipSizeSlider = transform.Find("EditLipSize").GetComponentInChildren<InteractiveSlider>();
        JawPositionSlider = transform.Find("EditJawPosition").GetComponentInChildren<InteractiveSlider>();
        JawSizeSlider = transform.Find("EditJawSize").GetComponentInChildren<InteractiveSlider>();
        MandibleSizeSlider = transform.Find("EditMandibleSize").GetComponentInChildren<InteractiveSlider>();
        ChinPositionSlider = transform.Find("EditChinPosition").GetComponentInChildren<InteractiveSlider>();
        ChinSizeSlider = transform.Find("EditChinSize").GetComponentInChildren<InteractiveSlider>();
        ChinPronouncedSlider = transform.Find("EditChinPronounced").GetComponentInChildren<InteractiveSlider>();

        MouthSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.MOUTH_SIZE));
        LipSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.LIPS_SIZE));
        JawPositionSlider.Initialize(AvatarController.GetValue(UMAProperty.JAWS_POSITION));
        JawSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.JAWS_SIZE));
        MandibleSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.MANDIBLE_SIZE));
        ChinPositionSlider.Initialize(AvatarController.GetValue(UMAProperty.CHIN_POSITION));
        ChinSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.CHEEK_SIZE));
        ChinPronouncedSlider.Initialize(AvatarController.GetValue(UMAProperty.CHIN_PRONOUNCED));

        MouthSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeMouthSize(n);};
        LipSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeLipsSize(n);};
        JawPositionSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeJawsPosition(n);};
        JawSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeJawsSize(n);};
        MandibleSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeMandibleSize(n);};
        ChinPositionSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeChinPosition(n);};
        ChinSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeChinSize(n);};
        ChinPronouncedSlider.OnSelectionChange = (p ,n) => { AvatarController.ChangeChinPronounced(n);  };

        SetActive(false);
        return this;
    }

    public override void Save(UMAISOObject umaISOObject)
    {
        umaISOObject.Mouth.SetProperty(UMAProperty.MOUTH_SIZE, AvatarController.GetValue(UMAProperty.MOUTH_SIZE).ToString());
        umaISOObject.Mouth.SetProperty(UMAProperty.LIPS_SIZE, AvatarController.GetValue(UMAProperty.LIPS_SIZE).ToString());
        umaISOObject.Mouth.SetProperty(UMAProperty.JAWS_POSITION, AvatarController.GetValue(UMAProperty.JAWS_POSITION).ToString());
        umaISOObject.Mouth.SetProperty(UMAProperty.JAWS_SIZE, AvatarController.GetValue(UMAProperty.JAWS_SIZE).ToString());
        umaISOObject.Mouth.SetProperty(UMAProperty.MANDIBLE_SIZE, AvatarController.GetValue(UMAProperty.MANDIBLE_SIZE).ToString());
        umaISOObject.Mouth.SetProperty(UMAProperty.CHIN_POSITION, AvatarController.GetValue(UMAProperty.CHIN_POSITION).ToString());
        umaISOObject.Mouth.SetProperty(UMAProperty.CHIN_SIZE, AvatarController.GetValue(UMAProperty.CHIN_SIZE).ToString());
        umaISOObject.Mouth.SetProperty(UMAProperty.CHIN_PRONOUNCED, AvatarController.GetValue(UMAProperty.CHIN_PRONOUNCED).ToString());
        base.Save(umaISOObject);
    }
}
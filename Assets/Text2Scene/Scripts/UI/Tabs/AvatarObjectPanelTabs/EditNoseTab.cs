using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EditNoseTab : AvatarObjectPanelTab
{
    private InteractiveSlider NoseCurveSlider;
    private InteractiveSlider NoseFlattenSlider;
    private InteractiveSlider NoseInclinationSlider;
    private InteractiveSlider NosePositionSlider;
    private InteractiveSlider NosePronouncedSlider;
    private InteractiveSlider NoseSizeSlider;
    private InteractiveSlider NoseWidthSlider;
    private UMAController AvatarController;

    public override AvatarObjectPanelTab Initialize(AvatarObjectPanelTab parent)
    {
        _parentTab = parent;
        iShapeObject = parent.iShapeObject;

        TabButton = GameObject.Find("NoseButton").GetComponent<InteractiveButton>();
        if (TabButton == null) Debug.LogWarning("Warning! TabButton for tab 'EditNose' could not be found");
        else
        {
            TabButton.OnClick = () =>
            {
                _parentTab.ActiveTab = this;
            };
        }
        AvatarController = iShapeObject.transform.GetComponentInChildren<UMAController>();

        NoseCurveSlider = transform.Find("EditNoseCurve").GetComponentInChildren<InteractiveSlider>();
        NoseFlattenSlider = transform.Find("EditNoseFlatten").GetComponentInChildren<InteractiveSlider>();
        NoseInclinationSlider = transform.Find("EditNoseInclination").GetComponentInChildren<InteractiveSlider>();
        NosePositionSlider = transform.Find("EditNosePosition").GetComponentInChildren<InteractiveSlider>();
        NosePronouncedSlider = transform.Find("EditNosePronounced").GetComponentInChildren<InteractiveSlider>();
        NoseSizeSlider = transform.Find("EditNoseSize").GetComponentInChildren<InteractiveSlider>();
        NoseWidthSlider = transform.Find("EditNoseWidth").GetComponentInChildren<InteractiveSlider>();

        NoseCurveSlider.Initialize(AvatarController.GetValue(UMAProperty.NOSE_CURVE));
        NoseFlattenSlider.Initialize(AvatarController.GetValue(UMAProperty.NOSE_FLATTEN));
        NoseInclinationSlider.Initialize(AvatarController.GetValue(UMAProperty.NOSE_INCLANATION));
        NosePositionSlider.Initialize(AvatarController.GetValue(UMAProperty.NOSE_POSITION));
        NosePronouncedSlider.Initialize(AvatarController.GetValue(UMAProperty.NOSE_PRONOUNCED));
        NoseSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.NOSE_SIZE));
        NoseWidthSlider.Initialize(AvatarController.GetValue(UMAProperty.NOSE_WIDTH));

        NoseCurveSlider.OnSelectionChange = (n, p) => { AvatarController.ChangeNoseCurve(n); };
        NoseFlattenSlider.OnSelectionChange = (n, p) => { AvatarController.ChangeNoseFlatten(n); };
        NoseInclinationSlider.OnSelectionChange = (n, p) => { AvatarController.ChangeNoseInclination(n); };
        NosePositionSlider.OnSelectionChange = (n, p) => { AvatarController.ChangeNosePosition(n); };
        NosePronouncedSlider.OnSelectionChange = (n, p) => { AvatarController.ChangeNosePronounced(n); };
        NoseSizeSlider.OnSelectionChange = (n, p) => { AvatarController.ChangeNoseSize(n); };
        NoseWidthSlider.OnSelectionChange = (n, p) => { AvatarController.ChangeNoseWidth(n); };

        SetActive(false);
        return this;
    }

    public override void Save(UMAISOObject umaISOObject)
    {
        umaISOObject.Nose.SetProperty(UMAProperty.NOSE_CURVE, AvatarController.GetValue(UMAProperty.NOSE_CURVE).ToString());
        umaISOObject.Nose.SetProperty(UMAProperty.NOSE_FLATTEN, AvatarController.GetValue(UMAProperty.NOSE_FLATTEN).ToString());
        umaISOObject.Nose.SetProperty(UMAProperty.NOSE_INCLANATION, AvatarController.GetValue(UMAProperty.NOSE_INCLANATION).ToString());
        umaISOObject.Nose.SetProperty(UMAProperty.NOSE_POSITION, AvatarController.GetValue(UMAProperty.NOSE_POSITION).ToString());
        umaISOObject.Nose.SetProperty(UMAProperty.NOSE_PRONOUNCED, AvatarController.GetValue(UMAProperty.NOSE_PRONOUNCED).ToString());
        umaISOObject.Nose.SetProperty(UMAProperty.NOSE_SIZE, AvatarController.GetValue(UMAProperty.NOSE_SIZE).ToString());
        umaISOObject.Nose.SetProperty(UMAProperty.NOSE_WIDTH, AvatarController.GetValue(UMAProperty.NOSE_WIDTH).ToString());
        base.Save(umaISOObject);
    }
}
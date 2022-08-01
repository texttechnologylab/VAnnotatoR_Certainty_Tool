using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EditGeneralHeadTab : AvatarObjectPanelTab
{
    private InteractiveSlider HeadSizeSlider;
    private InteractiveSlider HeadWidthSlider;
    private InteractiveSlider ForeHeadSizeSlider;
    private InteractiveSlider ForeHeadPositionSlider;
    private UMAController AvatarController;

    public override AvatarObjectPanelTab Initialize(AvatarObjectPanelTab parent)
    {
        _parentTab = parent;
        iShapeObject = parent.iShapeObject;

        TabButton = GameObject.Find("GeneralHeadButton").GetComponent<InteractiveButton>();
        if (TabButton == null) Debug.LogWarning("Warning! TabButton for tab 'EditGeneralHead' could not be found");
        else
        {
            TabButton.OnClick = () =>
            {
                _parentTab.ActiveTab = this;

            };
        }
        AvatarController = iShapeObject.transform.GetComponentInChildren<UMAController>();

        HeadSizeSlider = transform.Find("EditHeadSize").GetComponentInChildren<InteractiveSlider>();
        HeadWidthSlider = transform.Find("EditHeadWidth").GetComponentInChildren<InteractiveSlider>();
        ForeHeadSizeSlider = transform.Find("EditForeHeadSize").GetComponentInChildren<InteractiveSlider>();
        ForeHeadPositionSlider = transform.Find("EditForeHeadPosition").GetComponentInChildren<InteractiveSlider>();

        HeadSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.HEAD_SIZE));
        HeadWidthSlider.Initialize(AvatarController.GetValue(UMAProperty.HEAD_WIDTH));
        ForeHeadSizeSlider.Initialize(AvatarController.GetValue(UMAProperty.FOREHEAD_SIZE));
        ForeHeadPositionSlider.Initialize(AvatarController.GetValue(UMAProperty.FOREHEAD_POSTION));

        HeadSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeHeadSize(n); };
        HeadWidthSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeHeadWidth(n); };
        ForeHeadSizeSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeForeheadSize(n); };
        ForeHeadPositionSlider.OnSelectionChange = (p, n) => { AvatarController.ChangeForeheadPosition(n); };

        SetActive(false);
        return this;
    }

    public override void Save(UMAISOObject umaISOObject)
    {
        umaISOObject.Head.SetProperty(UMAProperty.HEAD_SIZE, AvatarController.GetValue(UMAProperty.HEAD_SIZE).ToString());
        umaISOObject.Head.SetProperty(UMAProperty.HEAD_WIDTH, AvatarController.GetValue(UMAProperty.HEAD_WIDTH).ToString());
        umaISOObject.Head.SetProperty(UMAProperty.FOREHEAD_SIZE, AvatarController.GetValue(UMAProperty.FOREHEAD_SIZE).ToString());
        umaISOObject.Head.SetProperty(UMAProperty.FOREHEAD_POSTION, AvatarController.GetValue(UMAProperty.FOREHEAD_POSTION).ToString());
        base.Save(umaISOObject);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GeneralTab : AvatarObjectPanelTab
{
    private InteractiveSelectionMenu RaceMenu;
    private InteractiveSlider LowerMuscleSlider;
    private InteractiveSlider UpperMuscleSlider;
    private InteractiveSlider UpperWeightSlider;
    private InteractiveButton Color1Button;
    private InteractiveButton Color2Button;
    private InteractiveButton Color3Button;
    private InteractiveButton Color4Button;
    private InteractiveButton Color5Button;

    private UMAController AvatarController;

    private string Color1 = "#8D5524";
    private string Color2 = "#C68642";
    private string Color3 = "#E0AC69";
    private string Color4 = "#F1C27D";
    private string Color5 = "#FFDBAC";
        
    public override AvatarObjectPanelTab Initialize(AvatarObjectPanelTab parent)
    {
        _parentTab = parent;
        iShapeObject = parent.iShapeObject;

        TabButton = GameObject.Find("GeneralButton").GetComponent<InteractiveButton>();
        if (TabButton == null) Debug.LogWarning("Warning! TabButton for tab 'General' could not be found");
        else
        {
            TabButton.OnClick = () =>
            {
                _parentTab.ActiveTab = this;
            };
        }

        AvatarController = iShapeObject.GetComponentInChildren<UMAController>();

        RaceMenu = transform.Find("SelectRace").GetComponentInChildren<InteractiveSelectionMenu>();
        LowerMuscleSlider = transform.Find("LowerMuscles").GetComponentInChildren<InteractiveSlider>();
        UpperMuscleSlider = transform.Find("UpperMuscles").GetComponentInChildren<InteractiveSlider>();
        UpperWeightSlider = transform.Find("UpperWeight").GetComponentInChildren<InteractiveSlider>();

        Color1Button = transform.Find("Color1").GetComponent<InteractiveButton>();
        Color2Button = transform.Find("Color2").GetComponent<InteractiveButton>();
        Color3Button = transform.Find("Color3").GetComponent<InteractiveButton>();
        Color4Button = transform.Find("Color4").GetComponent<InteractiveButton>();
        Color5Button = transform.Find("Color5").GetComponent<InteractiveButton>();

        RaceMenu.Initialize(UMAModel.StringModelList, AvatarController.Model.Value);
        LowerMuscleSlider.Initialize(AvatarController.GetValue(UMAProperty.LOWER_MUSCLE));
        UpperMuscleSlider.Initialize(AvatarController.GetValue(UMAProperty.UPPER_MUSCLE));
        UpperWeightSlider.Initialize(AvatarController.GetValue(UMAProperty.UPPER_WEIGHT));

        RaceMenu.OnSelectionChange = (p, n) => AvatarController.ChangeGender(UMAModel.GetModel(n));
        LowerMuscleSlider.OnSelectionChange = (p, n) => AvatarController.ChangeLowerMuscle(n);
        UpperMuscleSlider.OnSelectionChange = (p, n) => AvatarController.ChangeUpperMuscle(n);
        UpperWeightSlider.OnSelectionChange = (p, n) => AvatarController.ChangeUpperWeight(n);

        if (ColorUtility.TryParseHtmlString(Color1, out Color newCol1)) Color1Button.OnClick = () => AvatarController.ChangeSkinColor(newCol1);
        if (ColorUtility.TryParseHtmlString(Color2, out Color newCol2)) Color2Button.OnClick = () => AvatarController.ChangeSkinColor(newCol2);
        if (ColorUtility.TryParseHtmlString(Color3, out Color newCol3)) Color3Button.OnClick = () => AvatarController.ChangeSkinColor(newCol3);
        if (ColorUtility.TryParseHtmlString(Color4, out Color newCol4)) Color4Button.OnClick = () => AvatarController.ChangeSkinColor(newCol4);
        if (ColorUtility.TryParseHtmlString(Color5, out Color newCol5)) Color5Button.OnClick = () => AvatarController.ChangeSkinColor(newCol5);

        SetActive(false);
        return this;
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);
    }

    public override void Save(UMAISOObject umaISOObject)
    {
        umaISOObject.Root.SetProperty(UMAProperty.MODEL, AvatarController.Model.Value);
        umaISOObject.Root.SetProperty(UMAProperty.LOWER_MUSCLE, AvatarController.GetValue(UMAProperty.LOWER_MUSCLE).ToString());
        umaISOObject.Root.SetProperty(UMAProperty.UPPER_MUSCLE, AvatarController.GetValue(UMAProperty.UPPER_MUSCLE).ToString());
        umaISOObject.Root.SetProperty(UMAProperty.UPPER_WEIGHT, AvatarController.GetValue(UMAProperty.UPPER_WEIGHT).ToString());
        umaISOObject.Root.SetProperty(UMAProperty.SKIN_COLOR, "#" + ColorUtility.ToHtmlStringRGB(AvatarController.GetSkinColor()));
    }
}
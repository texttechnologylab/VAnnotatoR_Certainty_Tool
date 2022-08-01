using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayMenu : MonoBehaviour
{
    public CertaintyToolInterface Interface { get; private set; }
    public GameObject Menu { get; private set; }
    public SelectionSlider SelectionSlider { get; private set; }
    public SelectionMenu SelectionMenu { get; private set; }
    public Spinbox SpinboxMin { get; private set; }
    public Spinbox SpinboxMax { get; private set; }
    public BuildingNameDisplay BuildingNameDisplay { get; private set; }
    public int MaxLenghtBuildingString { get; private set; } = 20;
    public InteractiveButton ReferenceModeButton { get; private set; }

    public void Init(CertaintyToolInterface CTInterface)
    {
        Menu = transform.gameObject;
        Interface = CTInterface;

        BuildingNameDisplay = transform.Find("BuildingNameDisplay").gameObject.GetComponent<BuildingNameDisplay>();
        SelectionSlider = transform.Find("SliderMenu").gameObject.GetComponent<SelectionSlider>();
        SelectionMenu = transform.Find("SelectionMenu").gameObject.GetComponent<SelectionMenu>();

        SpinboxMin = transform.Find("SpinboxMin").gameObject.GetComponent<Spinbox>();
        SpinboxMax = transform.Find("SpinboxMax").gameObject.GetComponent<Spinbox>();

        SpinboxMin.Init(0, 0, 5);
        SpinboxMax.Init(5, 0, 5);

        SpinboxMin.OnValueChange = SpinboxOnValueChange;
        SpinboxMax.OnValueChange = SpinboxOnValueChange;

        BuildingNameDisplay.Init(Interface);
        SelectionSlider.Init(Interface);
        SelectionMenu.Init(Interface);

        ReferenceModeButton = transform.Find("ReferenceModeCheckbox/ButtonCheck").gameObject.GetComponent<InteractiveButton>();
        ReferenceModeButton.ButtonOn = false;
        ReferenceModeButton.OnClick = ReferenceModeButtonOnClick;
    }

    public void GUIUpdate()
    {
        SpinboxColorUpdate();
        SpinboxBoundaryUpdate();
    }
    public void SetBuildingDisplayTag(string text)
    {
        if (text.Length > MaxLenghtBuildingString) text = text.Substring(0, MaxLenghtBuildingString) + "...";
        BuildingNameDisplay.SetText(text);
    }

    private void SpinboxOnValueChange()
    {
        if (Interface.BuildingSelected)
        {
            Interface.SelectedBuilding.SetShowBoundaries(SpinboxMin.Value, SpinboxMax.Value);
        }

        GUIUpdate();
    }
    private void SpinboxColorUpdate()
    {
        SpinboxMin.SetOutlinerColor(CertaintyToolInterface.CalcColor(SpinboxMin.Value, Interface.MinRating, Interface.MaxRating));
        SpinboxMax.SetOutlinerColor(CertaintyToolInterface.CalcColor(SpinboxMax.Value, Interface.MinRating, Interface.MaxRating));
    }

    private void SpinboxBoundaryUpdate()
    {
        SpinboxMin.SetBoundaries(Interface.MinRating, SpinboxMax.Value);
        SpinboxMax.SetBoundaries(SpinboxMin.Value, Interface.MaxRating);
    }

    private void ReferenceModeButtonOnClick()
    {
        ReferenceModeButton.ButtonOn = !ReferenceModeButton.ButtonOn;
        Interface.SelectedBuilding.SetReferenceMode(ReferenceModeButton.ButtonOn);
    }
}

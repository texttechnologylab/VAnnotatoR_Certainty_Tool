using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RatingSettingsMenu : MonoBehaviour
{
    public CertaintyToolInterface Interface { get; private set; }
    public GameObject Menu { get; private set; }

    public Spinbox SpinboxMin { get; private set; }
    public Spinbox SpinboxMax { get; private set; }
    public InteractiveButton ButtonApply { get; private set; }
    public TextMeshPro NumStepsTag { get; private set; }


    public void Init(CertaintyToolInterface CTInterface)
    {
        Menu = transform.gameObject;
        Interface = CTInterface;

        SpinboxMin = transform.Find("SpinboxMin").gameObject.GetComponent<Spinbox>();
        SpinboxMin.Init(0, 0, 0);
        SpinboxMin.OnValueChange = SpinboxOnChange;

        SpinboxMax = transform.Find("SpinboxMax").gameObject.GetComponent<Spinbox>();
        SpinboxMax.Init(0, 0, 0);
        SpinboxMax.OnValueChange = SpinboxOnChange;

        NumStepsTag = transform.Find("TextfieldNumSteps/Tag").gameObject.GetComponent<TextMeshPro>();

        InitButtons();
        Show(false);
    }

    private void InitButtons()
    {
        ButtonApply = transform.Find("ButtonApply").gameObject.GetComponent<InteractiveButton>();
        ButtonApply.OnClick = OnClickButtonApply;
    }

    public void Show(bool b)
    {
        Menu.SetActive(b);
    }

    public void GUIUpdate()
    {
        SpinboxColorUpdate();
        SpinboxBoundaryUpdate();
        UpdateNumStepsPanel();
    }

    private void OnClickButtonApply()
    {
        if(Interface.BuildingSelected)
        {
            Interface.SelectedBuilding.SetRatingBoundaries(SpinboxMin.Value, SpinboxMax.Value);

            // Rating Update in case of out-of-bounds alteration
            if (Interface.SelectedSubBuilding != null && Interface.SelectedSubBuilding.Rating != null)
                Interface.MainMenu.SetRatingMenu.Spinbox.SetValue((int)Interface.SelectedSubBuilding.Rating);
        }

        // Update Display Menu Spinbox to new Rating boundaries
        Interface.MainMenu.DisplayMenu.SpinboxMin.SetValue(SpinboxMin.Value);
        Interface.MainMenu.DisplayMenu.SpinboxMax.SetValue(SpinboxMax.Value);

        Interface.MainMenu.GUIUpdate();
    }

    private void SpinboxOnChange()
    {
        Interface.MainMenu.GUIUpdate();
    }

    private void SpinboxColorUpdate()
    {
        if (Interface.BuildingSelected)
        {
            //Debug.Log("SpinMinVal: " + SpinboxMin.Value);
            //Debug.Log("MinRating: " + Interface.SelectedBuilding.MinRating);
            //Debug.Log(Interface.SelectedBuilding.MinRating != SpinboxMin.Value);
            //Debug.Log("SpinMaxVal: " + SpinboxMax.Value);
            //Debug.Log("MaxRating: " + Interface.SelectedBuilding.MaxRating);
            //Debug.Log(Interface.SelectedBuilding.MaxRating != SpinboxMax.Value);

            if (Interface.SelectedBuilding.RatingMin != SpinboxMin.Value)
            {
                SpinboxMin.SetOutlinerColor(new Color(1f, 1f, 0f));
            }
            else
            {
                SpinboxMin.SetOutlinerColor(SpinboxMin.OriginalOutlinerColor);
            }
            if (Interface.SelectedBuilding.RatingMax != SpinboxMax.Value)
            {
                SpinboxMax.SetOutlinerColor(new Color(1f, 1f, 0f));
            }
            else
            {
                SpinboxMax.SetOutlinerColor(SpinboxMax.OriginalOutlinerColor);
            }
        }
    }

    private void SpinboxBoundaryUpdate()
    {
        SpinboxMin.SetBoundaries(CertaintyToolInterface.LowestPossibleRating, SpinboxMax.Value);
        SpinboxMax.SetBoundaries(SpinboxMin.Value, CertaintyToolInterface.HighestPossibleRating);
    }

    private void UpdateNumStepsPanel()
    {
        int steps = SpinboxMax.Value - SpinboxMin.Value + 1;
        NumStepsTag.SetText(steps.ToString());
    }
}

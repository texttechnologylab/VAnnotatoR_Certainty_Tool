using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionSettingsMenu : MonoBehaviour
{
    public CertaintyToolInterface Interface { get; private set; }
    public GameObject Menu { get; private set; }
    public InteractiveButton ButtonHide { get; private set; }
    public InteractiveButton ButtonMultiSelect { get; private set; }
    public InteractiveButton ButtonSelectAll { get; private set; }
    public InteractiveButton ButtonSelectParent { get; private set; }

    public void Init(CertaintyToolInterface CTInterface)
    {
        Menu = transform.gameObject;
        Interface = CTInterface;

        InitButtons();
        Show(false);
    }
    private void InitButtons()
    {
        ButtonHide = transform.Find("CheckboxHide/ButtonCheck").gameObject.GetComponent<InteractiveButton>();
        ButtonHide.OnClick = OnClickButtonHide;

        ButtonHide.ActiveButtonOnColor = new Color(0f, 1f, 0f);
        ButtonHide.ActiveButtonOffColor = new Color(0.4f, 0.4f, 0.4f);

        ButtonMultiSelect = transform.Find("CheckboxMultiSelect/ButtonCheck").gameObject.GetComponent<InteractiveButton>();
        ButtonMultiSelect.OnClick = OnClickButtonMultiSelect;

        ButtonMultiSelect.ActiveButtonOnColor = new Color(0f, 1f, 0f);
        ButtonMultiSelect.ActiveButtonOffColor = new Color(0.4f, 0.4f, 0.4f);

        ButtonSelectAll = transform.Find("ButtonSelectAll").gameObject.GetComponent<InteractiveButton>();
        ButtonSelectAll.OnClick = OnClickButtonButtonSelectAll;
        ButtonSelectParent = transform.Find("ButtonSelectParent").gameObject.GetComponent<InteractiveButton>();
        ButtonSelectParent.OnClick = OnClickButtonButtonSelectParent;
    }

    public void Show(bool b)
    {
        Menu.SetActive(b);
    }
    private void OnClickButtonHide()
    {
        ButtonHide.ButtonOn = !ButtonHide.ButtonOn;
        if (Interface.BuildingSelected) Interface.SelectedBuilding.SetEditMode(ButtonHide.ButtonOn);
    }
    private void OnClickButtonMultiSelect()
    {
        ButtonMultiSelect.ButtonOn = !ButtonMultiSelect.ButtonOn;
        Interface.MultiSelect = ButtonMultiSelect.ButtonOn;
    }

    private void OnClickButtonButtonSelectAll()
    {
        Interface.SelectBuildingController();
    }
    private void OnClickButtonButtonSelectParent()
    {
        if (Interface.BuildingSelected) Interface.SelectParent();
    }
}

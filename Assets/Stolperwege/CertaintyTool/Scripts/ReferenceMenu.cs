using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReferenceMenu : MonoBehaviour
{
    public CertaintyToolInterface Interface { get; private set; }
    public GameObject Menu { get; private set; }
    public TextMeshPro ObjectDisplay { get; private set; }
    public InteractiveButton ButtonCreate { get; private set; }
    public InteractiveButton ButtonDelete { get; private set; }
    public InteractiveButton ButtonReflexive { get; private set; }
    public KeyboardEditText Textfield { get; private set; }
    public bool IsKeyBoardActive { get; private set; }


    private string OriginalObjectDisplayText = "No Annotation Object selected";

    public void Init(CertaintyToolInterface CTInterface)
    {
        Menu = transform.gameObject;
        Interface = CTInterface;

        ObjectDisplay = transform.Find("TextfieldDisplay/Tag").gameObject.GetComponent<TextMeshPro>();

        InitButtons();
        Show(false);
    }
    private void InitButtons()
    {
        ButtonCreate = transform.Find("ButtonCreate").gameObject.GetComponent<InteractiveButton>();
        ButtonCreate.OnClick = OnClickButtonCreate;
        ButtonDelete = transform.Find("ButtonDelete").gameObject.GetComponent<InteractiveButton>();
        ButtonDelete.OnClick = OnClickButtonDelete;
        ButtonReflexive = transform.Find("CheckBoxReflexive/ButtonCheck").gameObject.GetComponent<InteractiveButton>();
        ButtonReflexive.OnClick = OnClickButtonReflexive;
        Textfield = transform.Find("TextfieldInput").gameObject.GetComponent<KeyboardEditText>();
        Textfield.OnCommit = OnCommitTextfield;

        ButtonReflexive.ButtonOn = false;
    }
    public void Show(bool b)
    {
        Menu.SetActive(b);
    }
    public void SetObjectDisplayText(string text)
    {
        ObjectDisplay.SetText(text);
    }

    public void ResetObjectDisplayText()
    {
        ObjectDisplay.SetText(OriginalObjectDisplayText);
    }

    private void OnClickButtonCreate()
    {
        SubBuildingController MainObject = Interface.SelectedSubBuilding;
        List<SubBuildingController> SecObjects = Interface.SelectedSubBuildings;
        bool reflexive = ButtonReflexive.ButtonOn;

        Interface.SelectedBuilding?.CreateReferences(MainObject, SecObjects, reflexive);
        Interface.MainMenu.DisplayMenu.ReferenceModeButton.ButtonOn = true;
        Interface.SelectedBuilding?.SetReferenceMode(true);
    }

    private void OnClickButtonDelete()
    {
        Interface.DeleteSelectedReferences();
    }

    private void OnClickButtonReflexive()
    {
        ButtonReflexive.ButtonOn = !ButtonReflexive.ButtonOn;
    }

    private void UpdateObjectDisplay()
    {
        SubBuildingController MainObject = Interface.SelectedSubBuilding;
        List<SubBuildingController> SecObjects = Interface.SelectedSubBuildings;

        string textMain = MainObject == null ? "(None)" : MainObject.Name;
        string textOther = SecObjects.Count == 0 ? "(None)" : SecObjects.Count == 1 ? SecObjects[0].Name : "(Multiple)";
        string text = "Main: " + textMain + "\b Other: " + textOther;

        ObjectDisplay.SetText(text);
    }

    private void OnCommitTextfield(string text, GameObject go)
    {
        Debug.LogError("OnTextfieldChange: " + text);
        Interface.SelectedBuilding?.UpdateReferenceTags(text);
    }
}

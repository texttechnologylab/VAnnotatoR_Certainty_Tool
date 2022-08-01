using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetRatingMenu : MonoBehaviour
{
    public CertaintyToolInterface Interface { get; private set; }
    public GameObject Menu { get; private set; }

    public TextMeshPro ObjectDisplayTag { get; private set; }

    private string OriginalObjectDisplayTagText = "No Object selected";

    public Spinbox Spinbox { get; private set; }

    public InteractiveButton ButtonClear { get; private set; }
    public InteractiveButton ButtonApply { get; private set; }
    public InteractiveButton ButtonClearRating { get; private set; }

    // Checkbox Button for "Apply to subobjects" option
    public InteractiveButton ButtonCheck { get; private set; }
    public int MaxLenghtObjectString { get; private set; } = 17;

    public void Init(CertaintyToolInterface CTInterface)
    {
        Menu = transform.gameObject;
        Interface = CTInterface;
        ObjectDisplayTag = transform.Find("ObjectNameDisplay/Textfield/Tag").gameObject.GetComponent<TextMeshPro>();
        Spinbox = transform.Find("Spinbox").gameObject.GetComponent<Spinbox>();
        Spinbox.Init(0, 0, 5);
        Spinbox.OnValueChange = SpinboxColorUpdate;
        Spinbox.OnBoundaryChange = SpinboxColorUpdate;

        InitButtons();
        Show(false);
    }

    private void InitButtons()
    {
        ButtonClear = transform.Find("ObjectNameDisplay/ButtonClear").gameObject.GetComponent<InteractiveButton>();
        ButtonClear.OnClick = OnClickButtonClear;

        ButtonApply = transform.Find("ButtonApply").gameObject.GetComponent<InteractiveButton>();
        ButtonApply.OnClick = OnClickButtonApply;

        ButtonCheck = transform.Find("CheckboxSubobjects/ButtonCheck").gameObject.GetComponent<InteractiveButton>();
        ButtonCheck.OnClick = OnClickButtonCheck;

        ButtonClearRating = transform.Find("ButtonClearRating").gameObject.GetComponent<InteractiveButton>();
        ButtonClearRating.OnClick = OnClickButtonClearRating;

        ButtonCheck.ActiveButtonOnColor = new Color(0f, 1f, 0f);
        ButtonCheck.ActiveButtonOffColor = new Color(0.4f, 0.4f, 0.4f);
    }

    public void GUIUpdate()
    {
        SpinboxBoundaryUpdate();
    }

    public void SetObjectDisplayTag(string text)
    {
        if (text.Length > MaxLenghtObjectString) text = text.Substring(0, MaxLenghtObjectString) + "...";
        ObjectDisplayTag.SetText(text);
    }

    public void Show(bool b)
    {
        Menu.SetActive(b);
    }

    private void OnClickButtonClear()
    {
        Interface.ClearSelectedObject();
        ObjectDisplayTag.SetText(OriginalObjectDisplayTagText);
    }

    private void OnClickButtonClearRating()
    {
        Interface.ClearRatingObj(ButtonCheck.ButtonOn);
    }

    public void ClearObjectDisplayTagText()
    {
        ObjectDisplayTag.SetText(OriginalObjectDisplayTagText);
    }

    private void OnClickButtonApply()
    {
        Interface.SetRatingObj(Spinbox.Value, ButtonCheck.ButtonOn);
    }

    private void OnClickButtonCheck()
    {
        ButtonCheck.ButtonOn = !ButtonCheck.ButtonOn;
    }

    private void SpinboxColorUpdate()
    {
        Spinbox.SetOutlinerColor(CertaintyToolInterface.CalcColor(Spinbox.Value, Spinbox.LowerBoundary, Spinbox.UpperBoundary));
    }

    private void SpinboxBoundaryUpdate()
    {
        Spinbox.SetBoundaries(Interface.MinRating, Interface.MaxRating);
    }
}

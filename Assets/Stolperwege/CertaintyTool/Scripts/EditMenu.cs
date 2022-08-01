using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditMenu : MonoBehaviour
{
    public CertaintyToolInterface Interface { get; private set; }
    public GameObject Menu { get; private set; }
    public InteractiveButton ButtonEditRatings { get; private set; }
    public InteractiveButton ButtonEditSettings { get; private set; }
    public InteractiveButton ButtonEditAnnos { get; private set; }
    public InteractiveButton ButtonSelection { get; private set; }
    public InteractiveButton ButtonAnalytics { get; private set; }
    public InteractiveButton ButtonReferences { get; private set; }
    private string OriginalObjectDisplayText = "No Annotation Object selected";

    public void Init(CertaintyToolInterface CTInterface)
    {
        Menu = transform.gameObject;
        Interface = CTInterface;

        InitButtons();
        Show(false);
    }
    private void InitButtons()
    {
        ButtonEditRatings = transform.Find("ButtonEditRatings").gameObject.GetComponent<InteractiveButton>();
        ButtonEditRatings.OnClick = OnClickButtonEditRatings;
        ButtonEditSettings = transform.Find("ButtonEditSettings").gameObject.GetComponent<InteractiveButton>();
        ButtonEditSettings.OnClick = OnClickButtonEditSettings;
        ButtonEditAnnos = transform.Find("ButtonEditAnnos").gameObject.GetComponent<InteractiveButton>();
        ButtonEditAnnos.OnClick = OnClickButtonEditAnnos;
        ButtonSelection = transform.Find("ButtonSelection").gameObject.GetComponent<InteractiveButton>();
        ButtonSelection.OnClick = OnClickButtonButtonSelection;
        ButtonAnalytics = transform.Find("ButtonAnalytics").gameObject.GetComponent<InteractiveButton>();
        ButtonAnalytics.OnClick = OnClickButtonButtonAnalytics;
        ButtonReferences = transform.Find("ButtonReferences").gameObject.GetComponent<InteractiveButton>();
        ButtonReferences.OnClick = OnClickButtonReferences;
    }
    public void Show(bool b)
    {
        Menu.SetActive(b);
    }

    private void OnClickButtonEditRatings()
    {
        ButtonEditRatings.ButtonOn = !ButtonEditRatings.ButtonOn;
        Interface.MainMenu.SetRatingMenu.Show(ButtonEditRatings.ButtonOn);
    }
    private void OnClickButtonEditSettings()
    {
        ButtonEditSettings.ButtonOn = !ButtonEditSettings.ButtonOn;
        Interface.MainMenu.RatingSettingsMenu.Show(ButtonEditSettings.ButtonOn);
    }
    private void OnClickButtonEditAnnos()
    {
        ButtonEditAnnos.ButtonOn = !ButtonEditAnnos.ButtonOn;
        Interface.MainMenu.AnnotationMenu.Show(ButtonEditAnnos.ButtonOn);
    }
    private void OnClickButtonButtonSelection()
    {
        ButtonSelection.ButtonOn = !ButtonSelection.ButtonOn;
        Interface.MainMenu.SelectionSettingsMenu.Show(ButtonSelection.ButtonOn);
    }
    private void OnClickButtonButtonAnalytics()
    {
        ButtonAnalytics.ButtonOn = !ButtonAnalytics.ButtonOn;
        Interface.MainMenu.AnalyticsMenu.Show(ButtonAnalytics.ButtonOn);
    }
    private void OnClickButtonButtonLegend()
    {
        ButtonAnalytics.ButtonOn = !ButtonAnalytics.ButtonOn;
        Interface.MainMenu.AnalyticsMenu.Show(ButtonAnalytics.ButtonOn);
    }
    private void OnClickButtonReferences()
    {
        ButtonReferences.ButtonOn = !ButtonReferences.ButtonOn;
        Interface.MainMenu.ReferenceMenu.Show(ButtonReferences.ButtonOn);
    }
}

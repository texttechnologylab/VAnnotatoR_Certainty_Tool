using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CertaintyToolMainMenu : MonoBehaviour
{
    public CertaintyToolInterface Interface { get; private set; }
    public GameObject Menu { get; private set; }
    public DisplayMenu DisplayMenu { get; private set; }
    public RatingSettingsMenu RatingSettingsMenu { get; private set; }
    public SetRatingMenu SetRatingMenu { get; private set; }
    public SelectionSettingsMenu SelectionSettingsMenu { get; private set; }
    public AnnotationMenu AnnotationMenu { get; private set; }
    public EditMenu EditMenu { get; private set; }
    public AnalyticsMenu AnalyticsMenu { get; private set; }
    public ReferenceMenu ReferenceMenu { get; private set; }

    public void Init(CertaintyToolInterface CTInterface)
    {
        Menu = transform.gameObject;
        Interface = CTInterface;

        DisplayMenu = transform.Find("DisplayMenu").gameObject.GetComponent<DisplayMenu>();
        RatingSettingsMenu = transform.Find("RatingSettingsMenu").gameObject.GetComponent<RatingSettingsMenu>();
        SetRatingMenu = transform.Find("SetRatingMenu").gameObject.GetComponent<SetRatingMenu>();
        SelectionSettingsMenu = transform.Find("SelectionSettingsMenu").gameObject.GetComponent<SelectionSettingsMenu>();
        AnnotationMenu = transform.Find("AnnotationMenu").gameObject.GetComponent<AnnotationMenu>();
        EditMenu = transform.Find("EditMenu").gameObject.GetComponent<EditMenu>();
        AnalyticsMenu = transform.Find("AnalyticsMenu").gameObject.GetComponent<AnalyticsMenu>();
        ReferenceMenu = transform.Find("ReferenceMenu").gameObject.GetComponent<ReferenceMenu>();


        DisplayMenu.Init(Interface);
        RatingSettingsMenu.Init(Interface);
        SetRatingMenu.Init(Interface);
        SelectionSettingsMenu.Init(Interface);
        AnnotationMenu.Init(Interface);
        EditMenu.Init(Interface);
        AnalyticsMenu.Init(Interface);
        ReferenceMenu.Init(Interface);
    }

    public void GUIUpdate ()
    {
        DisplayMenu.GUIUpdate();
        SetRatingMenu.GUIUpdate();
        RatingSettingsMenu.GUIUpdate();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EditHeadTab : AvatarObjectPanelTab
{
    public override AvatarObjectPanelTab Initialize(AvatarObjectPanelTab parent)
    {
        _parentTab = parent;
        iShapeObject = parent.iShapeObject;

        TabButton = GameObject.Find("HeadButton").GetComponent<InteractiveButton>();
        if (TabButton == null) Debug.LogWarning("Warning! TabButton for tab 'EditHead' could not be found");
        else
        {
            TabButton.OnClick = () =>
            {
                _parentTab.ActiveTab = this;

            };
        }

        SubTabs.Add(transform.Find("EditGeneralHeadTab").GetComponent<EditGeneralHeadTab>().Initialize(this));
        SubTabs.Add(transform.Find("EditEyesTab").GetComponent<EditEyesTab>().Initialize(this));
        SubTabs.Add(transform.Find("EditNoseTab").GetComponent<EditNoseTab>().Initialize(this));
        SubTabs.Add(transform.Find("EditMouthTab").GetComponent<EditMouthTab>().Initialize(this));

        ActiveTab = SubTabs[0];

        SetActive(false);
        return this;
    }

    public override void Save(UMAISOObject umaISOObject)
    {
        base.Save(umaISOObject);
    }
}
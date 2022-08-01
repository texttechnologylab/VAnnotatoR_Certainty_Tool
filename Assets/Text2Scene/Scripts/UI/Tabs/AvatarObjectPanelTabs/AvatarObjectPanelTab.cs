using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class AvatarObjectPanelTab : InteractiveObject
{
    public InteractiveButton TabButton;
    public List<AvatarObjectPanelTab> SubTabs = new List<AvatarObjectPanelTab>();
    public InteractiveShapeNetObject iShapeObject;
    protected AvatarObjectPanelTab _parentTab;
    protected AvatarObjectPanelTab _activeTab;
    protected bool _hasChanges = false;
    public AvatarObjectPanelTab ActiveTab
    {
        get { return _activeTab; }
        set
        {
            if(value == _activeTab) return;
            if(_activeTab != null) _activeTab.SetActive(false);
            value.SetActive(true);
            _activeTab = value;
        }
    }

    public virtual void SetActive(bool active)
    {
        if(TabButton != null) TabButton.ButtonOn = active;
        if(!active)
        {
            SubTabs.ForEach(T => T.SetActive(active));
        }
        gameObject.SetActive(active);
        if (ActiveTab != null) ActiveTab.SetActive(active);
    }
    
    public virtual void Save(UMAISOObject umaISOObject)
    {
        _hasChanges = false;
        if (SubTabs?.Count > 0) SubTabs.ForEach(t => t.Save(umaISOObject));
    }

    public bool ChildrenHaveChanges(bool includeSelf = true)
    {
        return includeSelf && _hasChanges || SubTabs.Exists(t => t.ChildrenHaveChanges());
    }

    public abstract AvatarObjectPanelTab Initialize(AvatarObjectPanelTab parent);
}